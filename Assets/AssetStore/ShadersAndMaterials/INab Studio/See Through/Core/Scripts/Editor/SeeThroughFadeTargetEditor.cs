using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Progress;

namespace INab.WorldAlchemy
{
    [CustomEditor(typeof(SeeThroughFadeTarget)), CanEditMultipleObjects]
    public class SeeThroughFadeTargetEditor : Editor
    {
        private SerializedProperty duration;
        private SerializedProperty triggerGameObjects;

        private SerializedProperty useOpacity;
        private SerializedProperty opacityCurve;
        private SerializedProperty opacityRenderers;

        private SerializedProperty useMaskTransform; 
        private SerializedProperty maskTransform; 

        private SerializedProperty useScale;
        private SerializedProperty startScale;
        private SerializedProperty endScale;

        private SerializedProperty usePosition;
        private SerializedProperty startPosition;
        private SerializedProperty endPosition;

        private SerializedProperty useRotation;
        private SerializedProperty startRotation;
        private SerializedProperty endRotation;

        private void OnEnable()
        {
            duration = serializedObject.FindProperty("duration");
            triggerGameObjects = serializedObject.FindProperty("triggerGameObjects"); 
            useOpacity = serializedObject.FindProperty("useOpacity");
            opacityCurve = serializedObject.FindProperty("opacityCurve");
            opacityRenderers = serializedObject.FindProperty("opacityRenderers");
            useMaskTransform = serializedObject.FindProperty("useMaskTransform");
            maskTransform = serializedObject.FindProperty("maskTransform");
            useScale = serializedObject.FindProperty("useScale");
            startScale = serializedObject.FindProperty("startScale");
            endScale = serializedObject.FindProperty("endScale");
            usePosition = serializedObject.FindProperty("usePosition");
            startPosition = serializedObject.FindProperty("startPosition");
            endPosition = serializedObject.FindProperty("endPosition");
            useRotation = serializedObject.FindProperty("useRotation");
            startRotation = serializedObject.FindProperty("startRotation");
            endRotation = serializedObject.FindProperty("endRotation");
        }

        public override void OnInspectorGUI()
        {
            var fadeTarget = (SeeThroughFadeTarget)target;

            serializedObject.Update();

            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(duration);
                EditorGUILayout.Space();

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(triggerGameObjects);
                EditorGUI.indentLevel--;

                if (fadeTarget.triggerGameObjects.Count == 0)
                {
                    EditorGUILayout.HelpBox("Add game objects with colliders that will trigger the fade effect when raycasted. You can add the object itself.", MessageType.Warning);
                }

                EditorGUILayout.Space();

                
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Opacity", EditorStyles.boldLabel);
            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(useOpacity);
                EditorGUILayout.Space();

                if (useOpacity.boolValue)
                {
                    EditorGUILayout.PropertyField(opacityCurve);
                    EditorGUILayout.Space();

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(opacityRenderers);
                    EditorGUI.indentLevel--;

                    if (fadeTarget.opacityRenderers.Count == 0)
                    {
                        EditorGUILayout.HelpBox("Add renderers to fade opacity of. These renderers need to have materials with the 'See Through Objects Fade' shader.", MessageType.Warning);
                    }

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Find Renderers"))
                    {
                        fadeTarget.FindRenderersOnTarget();
                    }

                    if (GUILayout.Button("Find Renderers in Children"))
                    {
                        fadeTarget.FindRenderersInChildren();
                    }

                    if (GUILayout.Button("Clear Renderers"))
                    {
                        fadeTarget.ClearRenderersList();
                    }
                }

                EditorGUILayout.Space();
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Mask Transform Manipulation", EditorStyles.boldLabel);
            using (var verticalScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(useMaskTransform);
                EditorGUILayout.Space();

                if (useMaskTransform.boolValue)
                {
                    EditorGUILayout.PropertyField(maskTransform);
                    if (fadeTarget.maskTransform == null)
                    {
                        EditorGUILayout.HelpBox("Set the mask Transform.", MessageType.Warning);
                    }

                    EditorGUILayout.PropertyField(useScale);
                    if (useScale.boolValue)
                    {
                        EditorGUILayout.PropertyField(startScale);
                        EditorGUILayout.PropertyField(endScale);
                    }
                    EditorGUILayout.PropertyField(usePosition);
                    if (usePosition.boolValue)
                    {
                        EditorGUILayout.PropertyField(startPosition);
                        EditorGUILayout.PropertyField(endPosition);
                    }
                    EditorGUILayout.PropertyField(useRotation);
                    if (useRotation.boolValue)
                    {
                        EditorGUILayout.PropertyField(startRotation);
                        EditorGUILayout.PropertyField(endRotation);
                    }
                }

                
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}
