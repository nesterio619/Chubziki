#if UNITY_EDITOR
using Actors.Molds;
using Components.Mechanism;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChubzikiUnityEditor.Molds
{
    [CustomEditor(typeof(BridgeMold))]
    public class BridgeMoldEditor : Editor
    {
        private SerializedProperty _displayGizmos;

        private void OnEnable()
        {
            _displayGizmos =  serializedObject.FindProperty("DisplayGizmos");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            BridgeMold mold = (BridgeMold)target;

            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                if(property.name == "m_Script")
                    GUI.enabled = false;

                if (property.name.Contains("PoolInfo"))
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(property);

                    if(GUILayout.Button("Apply prefab values",GUILayout.Width(130)))
                        mold.ApplyPrefabValues();

                    EditorGUILayout.EndHorizontal();
                }
                else
                    EditorGUILayout.PropertyField(property);

                GUI.enabled = true;
                enterChildren = false;
            }

            EditorGUILayout.Space(12);

            if (GUILayout.Button("Mirror moveable part"))
            {
                var confirm = EditorUtility.DisplayDialog(
                    "Mirror prefab",
                    "Are you sure you want to mirror this bridge? The prefab of this bridge will be changed",
                    "Yes",
                    "No"
                );

                if (!confirm) return;

                var prefab = Resources.Load(mold.PrefabPoolInfoGetter.ObjectPath);
                if (prefab==null) return;

                AssetDatabase.OpenAsset(prefab);
                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                var bridge = stage.FindComponentOfType<BridgeActor>();

                bridge.MirrorMoveablePart();
                Debug.Log(bridge.name + " mirrored.");
                EditorUtility.SetDirty(bridge);
            }

            EditorGUILayout.PropertyField(_displayGizmos);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif