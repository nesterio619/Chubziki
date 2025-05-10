using Actors;
using Actors.Constructors;
using Actors.Molds;
using Components.Mechanism;
using Core.Extensions;
using Core.Interfaces;
using Core.SaveSystem;
using Core.Utilities;
using QuestsSystem;
using QuestsSystem.Base;
using QuestsSystem.QuestConfig;
using Regions.BoundsInEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Regions
{
    [Serializable]
    public struct ActorSpawnPreset
    {
        [HideInInspector] public string Name;

        [SerializeField] internal Transform[] transforms;

        [SerializeField] internal Mold mold;

        // Button config, only visible if mold is PressureButtonMold
        [SerializeField] internal Transform activatedObject;
        [SerializeField] internal UnityEvent OnPress;
        [SerializeField] internal UnityEvent OnRelease;


        public bool Equals(ActorSpawnPreset obj)
        {
            if (transforms.Length != obj.transforms.Length)
                return false;

            if (mold != obj.mold)
                return false;

            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] != obj.transforms[i])
                    return false;

            return true;
        }

        public bool HasNullTransform => transforms.Any(x => x == null);
    }

    public class Location : MonoBehaviour, IDisposable
    {
        //=-- Serialized in editor --=//
        // Contains all graphic elements in location
        private Transform _lastEnvironment;

        [SerializeField] protected Transform environmentParent;

        private readonly List<ActorSpawnPreset> _lastActorPresets = new();

        [SerializeField] private List<ActorSpawnPreset> actorPresets = new();

        [SerializeField] protected Material boundsMaterial;

        [SerializeField] protected BoundsSceneElement LocationBounds;

        public Color boundsColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        /// Bounds of the location. Defines both size and position
        internal Bounds Bounds { get; set; }

        internal Dictionary<Transform, Actor> _actorsDictionary = new();

        private List<Renderer> _renderersList = new List<Renderer>();

        protected bool isInitialized = false;

        public bool IsLoaded { get; protected set; } = false;

        internal Sector _mySector;

        private Color _lastBoundsColor;

        private Material _lastMaterial;

        public Action AttackLocation;

        [HideInInspector] [SerializeField] private bool showLocation = false;

        internal string _locationPath;
        internal Dictionary<string, Pose> _savedActorPoses;

        public bool ShowLocation
        {
            get => showLocation;
            set
            {
                showLocation = value;
#if UNITY_EDITOR
                ToggleDisplayBounds(value);
#endif
            }
        }

        #region events

        protected internal event Action OnLoad;
        protected internal event Action OnUnload;
        protected internal event Action<Location> OnLocationDestroy;

        protected internal event Action OnEnter;
        protected internal event Action OnExit;

        internal event Action<bool> OnSwitchLogic;
        internal event Action<bool> OnSwitchGraphics;

        #endregion


        private void OnEnable()
        {
            CopyLastActorsTransformsAndMolds();
        }

        private void CopyLastActorsTransformsAndMolds()
        {

            _lastActorPresets.Clear();
            foreach (var item in actorPresets)
            {
                var copiedActorPreset = new ActorSpawnPreset();

                copiedActorPreset.mold = item.mold;
                copiedActorPreset.transforms = new Transform[item.transforms.Length];

                for (int i = 0; i < item.transforms.Length; i++)
                    copiedActorPreset.transforms[i] = item.transforms[i];

                _lastActorPresets.Add(copiedActorPreset);
            }
        }

        private void OnValidate()
        {
            if (environmentParent != null)
            {
                if (_lastEnvironment != environmentParent)
                {
                    FindAllEnvironmentMeshes();
                }
            }
        }

        private void FindAllEnvironmentMeshes()
        {
            if (environmentParent == null) return;

            foreach (var item in environmentParent.GetComponentsInChildren<Renderer>())
            {
                if (!_renderersList.Contains(item))
                    _renderersList.Add(item);
            }
        }

        protected virtual string MaterialPath => "Assets/Materials/EditorLocationBounds/LocationMaterial.mat";

        public void Initialize(Sector sector)
        {
            if (isInitialized)
            {
                Debug.LogWarning("Location already initialized");
                return;
            }
            isInitialized = true;

            Bounds = MeshUtilities.TransformBounds(LocationBounds.MyMeshFilter.sharedMesh.bounds, transform);

            SwitchGraphics(false);
            SwitchLogic(false);

            _mySector = sector;
            _locationPath = UtilitiesProvider.GetGameObjectPath(gameObject, 2);

            SaveManager.Progress.TryGetLocationObjectPoses(_locationPath, out _savedActorPoses);
        }

#if UNITY_EDITOR

        private void Reset()
        {
            ReloadForEditor();
        }

        protected virtual void ReloadForEditor()
        {
            CreateActorsParent();

            CreateEnvironmentParent();

            CalculateBounds();
        }

        private void CreateActorsParent()
        {
            foreach (var item in GetComponentsInChildren<Transform>())
                if (item.name == "Actors")
                    return;

            GameObject actors = new GameObject("Actors");
            actors.transform.SetParent(transform);
        }

        protected void CreateEnvironmentParent()
        {
            foreach (var item in GetComponentsInChildren<Transform>())
                if (item.name == "Environment")
                    return;

            GameObject environment = new GameObject("Environment");
            environment.transform.SetParent(transform);
            this.environmentParent = environment.transform;
        }

        public void RefreshEditorActors()
        {
            bool isNeedRefresh = false;

            if (_lastActorPresets.Count != actorPresets.Count)
                isNeedRefresh = true;
            else
                for (int i = 0; i < _lastActorPresets.Count; i++)
                {
                    if (_lastActorPresets[i].Equals(actorPresets[i]))
                        continue;

                    isNeedRefresh = true;
                    break;
                }

            if (!isNeedRefresh)
                return;

            EditorActorConstructor.Instance.RefreshLocation(this, actorPresets);

            CopyLastActorsTransformsAndMolds();
        }

        public void ForceRefreshEditorActors()
        {
            EditorActorConstructor.Instance.RefreshLocation(this, actorPresets);
            CopyLastActorsTransformsAndMolds();
        }

        public void RefreshBoundsColor()
        {
            if (_lastBoundsColor == boundsColor)
                return;
            _lastBoundsColor = boundsColor;

            if (LocationBounds == null || boundsMaterial == null)
                return;

            //We cannot change LocationBounds.MyMeshRenderer.material.color in EditorMode. Need to create new Material;
            Material material = new Material(boundsMaterial);
            material.color = boundsColor;

            if (_lastMaterial != null) DestroyImmediate(_lastMaterial);
            _lastMaterial = material;

            LocationBounds.MyMeshRenderer.material = material;
            ToggleDisplayBounds(true);
        }
#endif

        public virtual void Enter() // Called when we enter enter location type bounds
        {
            if (!IsLoaded)
                Load();

            OnEnter?.Invoke();
        }

        public virtual void Exit() // Called when we exit any location type bounds
        {
            OnExit?.Invoke();
        }

        #region Load or Unload

        protected internal virtual void Load() // Load location assets
        {
            if (IsLoaded) return;

            LoadActorsFromPresets();
            IsLoaded = true;
            OnLoad?.Invoke();
        }

        public virtual void Dispose() // Unload location assets
        {
            this.UnloadActors(_actorsDictionary.Values.ToArray());

            RemoveSectorSwitchLogicEvent(SwitchLogic);

            OnUnload?.Invoke();

            IsLoaded = false;
        }

        public void SwitchLogic(bool enable) => OnSwitchLogic?.Invoke(enable);

        private void OnDestroy() => OnLocationDestroy?.Invoke(this);

        #endregion

        #region Control location content

        public virtual void SwitchGraphics(bool stateToSet)
        {
            foreach (var rendererFromList in _renderersList)
                rendererFromList.enabled = stateToSet;

            OnSwitchGraphics?.Invoke(stateToSet);
        }

        void LoadActorsFromPresets()
        {
            foreach (var actorPreset in actorPresets)
                this.CreateActors(actorPreset);

            AddSectorSwitchLogicEvent(SwitchLogic);
        }


        private void AddSectorSwitchLogicEvent(Action<bool> action)
            => _mySector.OnSwitchLogic += action;

        private void RemoveSectorSwitchLogicEvent(Action<bool> action)
        {
            if (_mySector == null)
                return;
            _mySector.OnSwitchLogic -= action;
        }

        public void SaveActorPose(GameObject gameObject)
        {
            var name = gameObject.transform.parent.name;
            SaveObjectPose(name, gameObject.transform.GetPose());
        }
        public void SaveObjectPose(string objectName, Pose pose)
        {
            if (_savedActorPoses == null)
                SaveManager.Progress.AddLocationObjectPoses(_locationPath, out _savedActorPoses);

            if (_savedActorPoses.ContainsKey(objectName))
                _savedActorPoses[objectName] = pose;

            else _savedActorPoses.Add(objectName, pose);
        }
        public bool TryGetObjectPose(string objectName, out Pose pose)
        {
            if(_savedActorPoses != null && _savedActorPoses.ContainsKey(objectName))
            {
                pose = _savedActorPoses[objectName];
                return true;
            }    

            pose = default;
            return false;
        }

        #endregion

        #region  Bounds And Gizmos

        public bool IsInsideBounds(Vector3 position) => Bounds.Contains(position);

        public virtual void CalculateBounds(bool displayBounds = true)
        {
            if (LocationBounds == null)
            {
#if UNITY_EDITOR
                AssetUtils.TryLoadUnityAsset(MaterialPath, out boundsMaterial);
#endif
                Material material = new Material(boundsMaterial);
                material.color = boundsColor;


                LocationBounds = BoundsSceneElement.Create<LocationBounds>(gameObject.name, this, material, transform);
            }

            if (LocationBounds != null && environmentParent.childCount != 0)
            {
                LocationBounds.CreateMeshBounds();

                Bounds = MeshUtilities.TransformBounds(LocationBounds.MyMeshFilter.sharedMesh.bounds, transform);
            }

#if UNITY_EDITOR
            ToggleDisplayBounds(displayBounds);
#endif
        }

#if UNITY_EDITOR
        public void ToggleDisplayBounds(bool enable) => LocationBounds.SwitchVisible(enable);
#endif

        public virtual List<Bounds> GetAllBounds()
        {
            List<Bounds> locationBounds = new();

            List<Transform> environmentTransforms = new List<Transform>();

            if (environmentParent != null) environmentTransforms.AddRange(environmentParent.GetComponentsInChildren<Transform>());
            actorPresets.ForEach(x => environmentTransforms.AddRange(x.transforms));

            foreach (Transform item in environmentTransforms)
            {
                if (item == null) continue;
                if (item == environmentParent && environmentTransforms.Count != 1) continue;

                var bounds = BoundsUtilities.GetValidBounds(item.gameObject);
                if (bounds != default) locationBounds.Add(bounds);
            }

#if UNITY_EDITOR
            foreach (var gameObject in locationBounds)
                BoundsUtilities.DrawBounds(gameObject, 2);
#endif

            return locationBounds;
        }

        public bool IsVisibleBounds()
        {
            return LocationBounds.IsVisibleBounds;
        }

        public bool IsLocationBoundsExist()
        {
            return LocationBounds != null;
        }

        public void BakeReflectionProbe()
        {
            var probe = GetComponentInChildren<ReflectionProbe>();
            var bounds = GetUnmodifiedBounds();

            if (probe == null)
            {
                var probeGameObject = new GameObject("ReflectionProbe");
                probeGameObject.transform.SetParent(transform);

                probe = probeGameObject.AddComponent<ReflectionProbe>();
                probe.mode = ReflectionProbeMode.Realtime;
                probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                probe.boxProjection = true;
            }

            probe.size = bounds.size;
            probe.transform.localPosition = bounds.center;

            probe.RenderProbe();
        }

        public bool HasEmptyReferences()
        {
            foreach (var actor in actorPresets)
            {
                if (actor.mold == null
                    || actor.transforms.Length == 0
                    || actor.transforms.Contains(null))
                    return true;
            }

            return false;
        }

        public Bounds GetUnmodifiedBounds()
        {
            var sharedMesh = LocationBounds.MyMeshFilter.sharedMesh;
            if (sharedMesh == null) return default;

            return sharedMesh.bounds;
        }

        #endregion

        public void ClearOnSwitchLogicEvent()
        {
            OnSwitchLogic = null;
        }
    }

    public static class LocationExtension
    {

        public static Actor[] CreateActors(this Location location, ActorSpawnPreset actorSpawnPreset, bool returnCreatedActors = false, bool enableLogic = true, bool fromCompletedQuest = false)
        {
            var createdActors = returnCreatedActors
                ? new List<Actor>()
                : null;

            foreach (var actorTransform in actorSpawnPreset.transforms)
            {
                if (actorTransform == null)
                {
                    Debug.LogWarning($"Transform is null in Location : {location.name} , Mold : {actorSpawnPreset.mold}");
                    continue;
                }

                if (actorTransform.gameObject.activeSelf == false
                    || actorTransform.childCount > 0 && actorTransform.GetChild(0).gameObject.activeSelf == false)
                    continue;

                bool isGetValue = false;

                location._actorsDictionary.TryGetValue(actorTransform, out var actor);
                if (actor == null)
                {
                    actor = ActorConstructor.Instance.Load(actorSpawnPreset.mold, actorTransform, location);

                    location._actorsDictionary[actorTransform] = actor;

                    if (location._savedActorPoses != null && location._savedActorPoses.ContainsKey(actorTransform.name))
                        actor.transform.ApplyPose(location._savedActorPoses[actorTransform.name]);

                    if (actor is ISetupForCompletedQuest setup && fromCompletedQuest)
                        setup.SetupForCompletedQuest();

                }
                else
                {
                    isGetValue = true;

                }

                if (returnCreatedActors)
                    createdActors.Add(actor);

                if (enableLogic)
                    location.OnSwitchLogic += actor.ToggleLogic;

                Transform actorTransformDispose = actorTransform;
                actor.OnDispose += () => location._actorsDictionary[actorTransformDispose] = null;

                if (actor is IMoving movingActor)
                {
                    movingActor.AddVisibleObject();
                    movingActor.SetSector(location._mySector);
                }


                if (!isGetValue)
                {
                    actor.ToggleLogic(false);
                    actor.SwitchGraphic(true);
                }

                if (actorSpawnPreset.mold is PressureButtonMold)
                {
                    var button = actor.GetComponent<PressureButtonActor>();
                    button.Initialize(
                        actorSpawnPreset.activatedObject,
                        () => actorSpawnPreset.OnPress?.Invoke(),
                        () => actorSpawnPreset.OnRelease?.Invoke()
                    );
                }
            }

            return returnCreatedActors
                ? createdActors.ToArray()
                : null;
        }

        public static void UnloadActors(this Location location, Actor[] actorsToUnload)
        {
            foreach (var actorToUnload in actorsToUnload)
                UnloadActor(location, actorToUnload);

            location.ClearOnSwitchLogicEvent();
        }

        public static void UnloadActor(this Location location, Actor actorToUnload)
        {
            if (actorToUnload == null)
                return;

            if (actorToUnload is IMoving moving)
                if (moving.IsVisible && moving.IsOutOfSector)
                    return;

            location._actorsDictionary.Remove(actorToUnload.transform.parent);

            UtilitiesProvider.WaitAndRun(() =>
            {
                if (actorToUnload != null)
                    actorToUnload.ReturnToPool();
            }, true);
        }
    }
}
