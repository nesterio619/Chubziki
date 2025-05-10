using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace INab.WorldAlchemy
{
    [CustomEditor(typeof(SeeThroughDetect)), CanEditMultipleObjects]
    public class SeeThroughDetectEditor : Editor
    {
        private SerializedProperty fadeTargets; 
        private SerializedProperty cameraTransform; 
        private SerializedProperty targetTransform; 

        private SerializedProperty detectionLayer; 
        private SerializedProperty targetOffsetDistance; 
        private SerializedProperty cameraOffsetDistance; 
        private SerializedProperty enableDebugRaycast;

        private SeeThroughDetect seeThroughDetect;

        private void OnEnable()
        {
            fadeTargets = serializedObject.FindProperty("fadeTargets");
            cameraTransform = serializedObject.FindProperty("cameraTransform");
            targetTransform = serializedObject.FindProperty("targetTransform");
            detectionLayer = serializedObject.FindProperty("detectionLayer");
            targetOffsetDistance = serializedObject.FindProperty("targetOffsetDistance");
            cameraOffsetDistance = serializedObject.FindProperty("cameraOffsetDistance");
            enableDebugRaycast = serializedObject.FindProperty("enableDebugRaycast");
        }

        public override void OnInspectorGUI()
        {
            seeThroughDetect = (SeeThroughDetect)target;

            serializedObject.Update();

            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Space();

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(fadeTargets);
                EditorGUI.indentLevel--;

                if(seeThroughDetect.fadeTargets.Count == 0)
                {
                    EditorGUILayout.HelpBox("There is no fade targets to perform obscruction detection on.", MessageType.Warning);
                }

                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(cameraTransform);
                if (seeThroughDetect.cameraTransform == null)
                {
                    EditorGUILayout.HelpBox("Set the camera to use for raycasting.", MessageType.Warning);
                }
                EditorGUILayout.PropertyField(targetTransform);
                if (seeThroughDetect.targetTransform == null)
                {
                    EditorGUILayout.HelpBox("Set the target object whose visibility is being checked.", MessageType.Warning);
                }
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(detectionLayer);
                if (seeThroughDetect.detectionLayer == LayerMask.GetMask())
                {
                    EditorGUILayout.HelpBox("Set the layer mask that determines which objects will be detected by raycasts.", MessageType.Warning);
                }

                EditorGUILayout.PropertyField(targetOffsetDistance);
                EditorGUILayout.PropertyField(cameraOffsetDistance);
                EditorGUILayout.PropertyField(enableDebugRaycast);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
