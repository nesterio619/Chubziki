using System;
using System.Collections.Generic;
using System.Text;
using Actors.Constructors;
using Actors.Molds;
using Core.ObjectPool;
using Core.Utilities;
using QuestsSystem.Base;
using Regions;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace QuestsSystem.QuestConfig
{
    [Serializable]
    public struct QuestElementAndTransformPath
    {
        public string TransformPath; // Path in the scene hierarchy
        public PrefabPoolInfo _PrefabPoolInfo; // The ScriptableObject _PrefabPoolInfo
    }

    [Serializable]
    public struct ActorMoldAndTransformPath
    {
        public string TransformPath;
        public Mold Mold;
        public bool SavePosition;

        public static implicit operator ActorPresetWithPath(ActorMoldAndTransformPath param)
        {
            var preset = new ActorPresetWithPath();
            preset.TransformPaths = new () { param.TransformPath };
            preset.Mold = param.Mold;
            preset.SavePosition = param.SavePosition;
            return preset;
        }
    }

    [Serializable]
    public struct ActorSpawnInfo
    {
        [Serializable]
        public struct MoldCount 
        {
            public Mold Mold;
            public int Count; 
        }

        public string CenterTransformPath;
        public float Radius;
        public MoldCount[] MoldCounts;
    }

    [Serializable]
    public struct ActorPresetWithPath
    {
        public string ParentTransformPath;
        public List<string> TransformPaths;
        public Mold Mold;
        public bool SavePosition;

        public ActorPresetWithPath(string parentTransformPath, Mold mold, List<string> transformPaths = null)
        {
            ParentTransformPath = parentTransformPath;
            TransformPaths = transformPaths ?? new();
            Mold = mold;
            SavePosition = false;
        }

        public static implicit operator ActorSpawnPreset(ActorPresetWithPath param)
        {
            var transforms = new List<Transform>();
            foreach(var path in param.TransformPaths)
            {
                var transform = GameObject.Find(path).transform;
                if(transform != null) transforms.Add(transform);
            }

            var preset = new ActorSpawnPreset();
            preset.transforms = transforms.ToArray();
            preset.mold = param.Mold;

            return preset;
        }
    }

    [CreateAssetMenu(menuName = "Quests/Create basic quest", fileName = "Assets/Resources/Quests/NewBasicQuest")]
    public abstract class QuestConfig : ScriptableObject
    {
        [SerializeField] public string QuestName;
        [SerializeField] public string QuestDescription;
        [SerializeField] private bool ShowQuestElements;

        [SerializeField] public string UnlockName;
        [SerializeField] public int UpgradePoints;

        [field: SerializeField][HideInInspector] public List<QuestElementAndTransformPath> QuestElementsAndTransformsPaths { get; set; } = new List<QuestElementAndTransformPath>();
        [field: SerializeField][HideInInspector] public string StartPositionPath = null;
        [field: SerializeField][HideInInspector] public bool ShowDebugPopupOnStart = false;
        [field: SerializeField][HideInInspector] public bool ShowDebugPopupOnFinish = false;

        [field: SerializeField][HideInInspector] public string QuestLocationPath;
        [field: SerializeField][HideInInspector] public bool AcceptQuestOnLocationEnter = true;
        [field: SerializeField][HideInInspector] public bool FailQuestOnLocationExit = true;
        [field: SerializeField][HideInInspector] public List<ActorSpawnInfo> ActorSpawnInfo { get; set; } = new List<ActorSpawnInfo>();

        [SerializeField] public List<ActorPresetWithPath> ActorsToSpawn = new List<ActorPresetWithPath>();
        [SerializeField] public List<ActorMoldAndTransformPath> IndividualQuestActors = new List<ActorMoldAndTransformPath>();

        [SerializeField] public bool CanBeRestarted;

        private const string CONFIGS_PATH = "Quests/"; // Path in resources folder to find all quests configs

        private QuestLogic.QuestLogic _questLogic;
        public QuestLogic.QuestLogic QuestLogic
        {
            get
            {
                if (_questLogic != null) 
                    return _questLogic;
                
                _questLogic = GetQuestLogicType();
                if (_questLogic == null)
                {
                    Debug.LogError($"You need to put quest logic {QuestName} into namespace QuestsSystem.QuestLogic to work");
                    return null;
                }
                    
                InitializeQuestLogic(_questLogic);

                return _questLogic;
            }
        }

        protected abstract QuestLogic.QuestLogic GetQuestLogicType();

        public static QuestConfig GetConfig(string questName)
        {
            QuestConfig[] quests = Resources.LoadAll<QuestConfig>(CONFIGS_PATH); // Load all quests configs in the project

            foreach (var quest in quests)
                if (quest.QuestName == questName)
                    return quest;

            return null;
        }

        protected virtual void InitializeQuestLogic(QuestLogic.QuestLogic questLogic)
        {
            if (questLogic == null) return;

            questLogic.Initialize(this);
        }

        #region LocationInteraction
        public virtual void SetupLocationEvents()
        {
            if(string.IsNullOrEmpty(QuestLocationPath)) return;

            var locationObject = GameObject.Find(QuestLocationPath);
            if(locationObject == null) return;

            var location = locationObject.GetComponent<Location>();

            location.OnLocationDestroy += UnsubscribeLocationEvents;
            location.OnLoad += QuestLogic.SpawnAllActors;
            location.OnSwitchLogic += QuestLogic.ToggleActorLogic;

            if (AcceptQuestOnLocationEnter)
                location.OnEnter += StartQuest;

            if (FailQuestOnLocationExit)
                location.OnExit += EndQuest;
        }

        private void StartQuest() => QuestsManager.Instance.StartQuestImmediately(QuestName);
        private void EndQuest() => QuestsManager.Instance.RemoveQuest(QuestName);

        private void UnsubscribeLocationEvents(Location location)
        {
            location.OnLoad -= QuestLogic.SpawnAllActors;
            location.OnLocationDestroy -= UnsubscribeLocationEvents;
            location.OnSwitchLogic -= QuestLogic.ToggleActorLogic;
            location.OnEnter -= StartQuest;
            location.OnExit -= EndQuest;
        }
        #endregion
        
        public virtual ActorPresetWithPath[] GetActorsToSpawn()
        {
            var list = new List<ActorPresetWithPath>();

            list.AddRange(ActorsToSpawn);
            
            foreach(var actor in IndividualQuestActors)
                list.Add(actor);

            return list.ToArray();
        }

        #region EditorQuestElements

#if UNITY_EDITOR

        public void CreateEditorElements()
        {
            ForEachQuestElementChild(
                childAction: child => DestroyImmediate(child),
                elementAction: (transform, poolInfo) => EditorQuestElementConstructor.Instance.Create(poolInfo, transform, QuestName)
            );

            SetQuestElementsVisibility(ShowQuestElements);
        }

        public void SetQuestElementsVisibility(bool isVisible)
        {
            ForEachQuestElementChild(child => child.SetActive(isVisible));
            ShowQuestElements = isVisible;
        }

        public void ForEachQuestElementChild(Action<GameObject> childAction, Action<Transform, PrefabPoolInfo> elementAction = null)
        {
            foreach (var element in QuestElementsAndTransformsPaths)
            {
                if (string.IsNullOrEmpty(element.TransformPath)) continue;
                if (element._PrefabPoolInfo == null) 
                {
                    DestroyEditorElementAtPath(element.TransformPath);
                    continue;
                }

                Transform transform = UtilitiesProvider.GetTransformFromPath(element.TransformPath);
                if (transform == null) continue;
                foreach (Transform child in transform)
                {
                    if (child.name.Contains(QuestName))
                        childAction?.Invoke(child.gameObject);
                }

                elementAction?.Invoke(transform, element._PrefabPoolInfo);
            }
        }

        public void DestroyEditorElementAtPath(string transformPath)
        {
            if (string.IsNullOrEmpty(transformPath)) return;

            Transform transform = UtilitiesProvider.GetTransformFromPath(transformPath);
            foreach (Transform child in transform)
            {
                if (child.name.Contains(QuestName))
                    DestroyImmediate(child.gameObject);
            }
        }

        public void DestroyAllEditorElements() => ForEachQuestElementChild(childAction: child => DestroyImmediate(child));

        public void GenerateActorPositions( int index)
        {
            var info = ActorSpawnInfo[index];

            var center = UtilitiesProvider.GetTransformFromPath(info.CenterTransformPath);

            if (center == null)
            {
                Debug.LogError("The center transform is null or exists in other scene.");
                return;
            }

            ActorsToSpawn.RemoveAll(x => x.ParentTransformPath == info.CenterTransformPath);
            ActorsToSpawn.AddRange(QuestActorSpawner.GenerateSpawnPositions(center, info));

            EditorUtility.SetDirty(this);
        }
        public static string GenerateSpawnerName(ActorSpawnInfo info)
        {
            var name = new StringBuilder();

            for (int i = 0; i < info.MoldCounts.Length; i++)
            {
                var element = info.MoldCounts[i];

                if (element.Mold == null) continue;
                name.Append(element.Mold.name.Replace("Mold", ""));
                name.Append($"({element.Count})");

                if (i < info.MoldCounts.Length - 1) name.Append(", ");
            }

            if (name.Length == 0) name.Append("Spawner");

            return name.ToString();
        }
#endif
        #endregion
    }
}