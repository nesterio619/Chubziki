#if UNITY_EDITOR
using Components.Mechanism;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChubzikiUnityEditor.Actors
{
    [CustomEditor(typeof(BridgeActor))]
    public class BridgeActorEditor : Editor
    {
        private SerializedProperty _zSpacing;

        private void OnEnable()
        {
            _zSpacing = serializedObject.FindProperty("zSpacing");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            BridgeActor bridge = (BridgeActor)target;

            DrawDefaultInspector();

            var buttonText = bridge.HasMirroredPart ? "Update mirrored part" : "Copy and mirror moveable part";

            GUI.enabled = PrefabStageUtility.GetCurrentPrefabStage();
            if (GUILayout.Button(buttonText))
            {
                bridge.MirrorMoveablePart();
                EditorUtility.SetDirty(bridge.gameObject);
            }
            GUI.enabled = true;

            if (bridge.HasMirroredPart)
                EditorGUILayout.PropertyField(_zSpacing);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif