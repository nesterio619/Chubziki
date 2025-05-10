using UnityEngine;
using System.Collections.Generic;
using Regions;
using Actors.EditorActors;
using Actors.EditorElements;
using Actors.Molds;
using TMPro;
using UnityEngine.ProBuilder;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Constructors
{
    [ExecuteInEditMode]  // Attribute for exicuting in editor mode
    public class EditorActorConstructor : ObjectConstructor<GameObject>
    {
        private const string POSTFIX_FOR_EDITOR = "_EDITOR-ACTOR";

        private static EditorActorConstructor instance = new();

        private static List<System.Type> _requiredComponentsTypes = new List<System.Type>
        {
            typeof(Transform),
            typeof(RectTransform),
            typeof(MeshRenderer),
            typeof(EditorActor),
            typeof(MeshFilter),
            typeof(SkinnedMeshRenderer),
            typeof(ProBuilderMesh),
            typeof(TextMeshPro)
            #if UNITY_EDITOR
            ,typeof(BridgeEditorActor),
            typeof(TowerEditorActor),
            typeof(RBLibraBridgeEditorActor)
            #endif
        };

        public static EditorActorConstructor Instance
        {
            get
            {
                return instance;
            }

            private set
            {
                if (instance == null)
                {
                    instance = value;
                }
                else
                {
                    Debug.LogWarning("Trying create EditorActorConstructor");
                }
            }
        }

        private static Dictionary<Location, List<GameObject>> _createdElements = new();

        private Location _transferLocation;

#if UNITY_EDITOR
        private void CreateElements(Location location, List<ActorSpawnPreset> transformsAndMolds)
        {
            _transferLocation = location;

            foreach (var element in transformsAndMolds)
            {
                foreach (var parentTransform in element.transforms)
                {
                    GameObject instance = Load(element.mold, parentTransform);
                    if (instance == null) break;
                }
            }

            AssetDatabase.SaveAssets();
        }
#endif

        public override GameObject Load(Mold moldType, Transform transform)
        {
            if (moldType == null)
            {
                Debug.LogError("Mold type is null!");
                return null;
            }

#if UNITY_EDITOR

            GameObject instance = InstantiateFromPool(moldType.PrefabPoolInfoGetter, transform);

            InitializeInstance(instance, moldType);

            instance.name = moldType.name + POSTFIX_FOR_EDITOR;
            instance.tag = "EditorOnly";

            RemoveUnnecessaryComponents(instance);

            _createdElements.TryGetValue(_transferLocation, out List<GameObject> editorGameObjects);

            if (!_createdElements.ContainsKey(_transferLocation))
            {
                editorGameObjects = new List<GameObject>();
                _createdElements.Add(_transferLocation, editorGameObjects);
            }

            editorGameObjects.Add(instance);

            return instance;
#else
            Debug.LogWarning("EditorActorConstructor.Create is not supported outside of the Unity Editor.");
            return null;
#endif
        }

        private static void RemoveUnnecessaryComponents(GameObject instance)
        {
            Component[] components = instance.GetComponentsInChildren<Component>();

            foreach (var component in components)
            {
                if (!_requiredComponentsTypes.Contains(component.GetType()))
                {
                    Object.DestroyImmediate(component);
                }
            }
        }

        private void InitializeInstance(GameObject instance, Mold mold)
        {
#if UNITY_EDITOR
            switch (mold)
            {
                case BridgeMold bridgeMold:
                    instance.AddComponent<BridgeEditorActor>().Initialize(_transferLocation, bridgeMold);
                    break;
                case RBLibraBridgeMold rbLibraBridgeMold:
                    instance.AddComponent<RBLibraBridgeEditorActor>().Initialize(_transferLocation, rbLibraBridgeMold);
                    break;
                case TowerMold towerMold:
                    instance.AddComponent<TowerEditorActor>().Initialize(_transferLocation);
                    break;
                case ChubzikMold chubzikMold:
                    {
                        var armor = InstantiateFromPool(chubzikMold.ArmorMold.PrefabPoolInfoGetter, instance.transform).GetComponent<ChubzikModel>();
                        if (chubzikMold.WeaponPrefabPool.PrefabPoolInfoGetter != null)
                            ChubzikConstructor.CreateWeaponModel(chubzikMold.WeaponPrefabPool.PrefabPoolInfoGetter, armor.LeftHand, armor.RightHand);

                        instance.AddComponent<EditorActor>().Initialize(_transferLocation);
                        break;
                    }
                default:
                    instance.AddComponent<EditorActor>().Initialize(_transferLocation);
                    break;
            }
#endif
        }

#if UNITY_EDITOR
        public void AddExistingElement(Location location, GameObject gameObject)
        {
            if (location == null)
            {
                return;
            }

            List<GameObject> editorGameObjects;
            _createdElements.TryGetValue(location, out editorGameObjects);

            if (editorGameObjects == null)
            {
                editorGameObjects = new();
                _createdElements.Add(location, editorGameObjects);
            }

            if (!editorGameObjects.Contains(gameObject))
                editorGameObjects.Add(gameObject);
        }

        public void RefreshLocation(Location location, List<ActorSpawnPreset> elements)
        {
            ClearAllGameobjectElements(location);
            CreateElements(location, elements);
        }

        private void ClearAllGameobjectElements(Location location)
        {
            if (_createdElements.TryGetValue(location, out List<GameObject> editorGameObjects))
            {
                foreach (var item in editorGameObjects)
                {
                    Object.DestroyImmediate(item);
                }

            }
        }

        public List<GameObject> GetObjectsOfLocation(Location location)
        {
            _createdElements.TryGetValue(location, out List<GameObject> list);
            return list;
        }

        public void DestroyAllEditorActors()
        {
            foreach (var location in _createdElements.Keys)
                ClearAllGameobjectElements(location);

            _createdElements.Clear();
        }

        private static bool _showEditorActors = true;

        [MenuItem("Chubziki/EditorActors/Show all EditorActors", false, 2)]
        public static void ToggleAllEditorActors()
        {
            _showEditorActors = !_showEditorActors;

            foreach (var locationEditorActors in _createdElements.Values)
                foreach (var editorActor in locationEditorActors)
                    if (editorActor != null)
                        editorActor.SetActive(_showEditorActors);
        }
        [MenuItem("Chubziki/EditorActors/Show all EditorActors", true)]
        public static bool ToggleAllEditorActorsValidate()
        {
            Menu.SetChecked("Chubziki/EditorActors/Show all EditorActors", _showEditorActors);
            return true;
        }
        public static void TryHideAllEditorActors()
        {
            if (_showEditorActors) return;

            foreach (var locationEditorActors in _createdElements.Values)
                foreach (var editorActor in locationEditorActors)
                    if (editorActor != null)
                        editorActor.SetActive(false);
        }
#endif
    }
}