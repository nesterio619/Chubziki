using Actors.EditorActors;
using Actors.Molds;
using Core.ObjectPool;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Actors.Constructors
{
    [ExecuteInEditMode]
    public class EditorQuestElementConstructor : ObjectConstructor<GameObject>
    {
        public const string POSTFIX_FOR_EDITOR = "_EditorQuestElement";

        private static EditorQuestElementConstructor _instance = new();
        public static EditorQuestElementConstructor Instance
        {
            get => _instance;

            private set
            {
                if (_instance == null)
                    _instance = value;
            }

        }

        private readonly List<System.Type> _requiredComponentsTypes = new List<System.Type>
        {
            typeof(Transform),
            typeof(RectTransform),
            typeof(MeshRenderer),
            typeof(MeshFilter),
            typeof(SkinnedMeshRenderer),
            typeof(ProBuilderMesh),
            typeof(Rigidbody),
            typeof(TextMeshPro)
        };

        public override GameObject Load(Mold moldType, Transform transform)
        {
            return Create(moldType.PrefabPoolInfoGetter, transform, "");
        }

        public GameObject Create(PrefabPoolInfo poolInfo, Transform transform, string questName)
        {
            GameObject instance = InstantiateFromPool(poolInfo, transform);

            instance.name += POSTFIX_FOR_EDITOR;
            instance.name += $"({questName})";
            instance.tag = "EditorOnly";
#if UNITY_EDITOR
            RemoveUnnecessaryComponents(instance);
            instance.AddComponent<EditorQuestElement>();
#endif
            return instance;
        }

        private void RemoveUnnecessaryComponents(GameObject instance)
        {
            Component[] components = instance.GetComponentsInChildren<Component>();

            foreach (var component in components)
            {
                if (!_requiredComponentsTypes.Contains(component.GetType()))
                {
                    Object.DestroyImmediate(component);
                }
            }

            if(instance.TryGetComponent(out Rigidbody rigidbody))
                Object.DestroyImmediate(rigidbody);
        }
    }
}