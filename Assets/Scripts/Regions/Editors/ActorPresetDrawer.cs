using Actors.Molds;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Regions.Editors
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ActorSpawnPreset))]
    public class ActorPresetDrawer : PropertyDrawer
    {
        private const string FoldoutKey = "ActorPresetFoldout";
        private bool isFoldout = true;

        private const string ButtonFoldoutKey = "ButtonFoldout";
        private bool isButtonFoldout = true;

        private Color WarningColor = new Color(1f, 0.3f, 0.3f);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Color defaultGUIColor = GUI.color;

            EditorGUI.BeginProperty(position, label, property);

            string foldoutKey = $"{FoldoutKey}_{property.propertyPath}";
            isFoldout = EditorPrefs.GetBool(foldoutKey, true);

            string buttonFoldoutKey = $"{ButtonFoldoutKey}_{property.propertyPath}";
            isButtonFoldout = EditorPrefs.GetBool(buttonFoldoutKey, true);

            var transformsProp = property.FindPropertyRelative("transforms");
            var moldProp = property.FindPropertyRelative("mold");
            var activatedObjectProp = property.FindPropertyRelative("activatedObject");
            var onPressProp = property.FindPropertyRelative("OnPress");
            var onReleaseProp = property.FindPropertyRelative("OnRelease");

            bool isButton = moldProp.objectReferenceValue is PressureButtonMold;

            string elementName = "Element";
            if (transformsProp.arraySize > 0)
            {
                var transform = transformsProp.GetArrayElementAtIndex(0).objectReferenceValue;
                if (transform != null)
                    elementName = transform.name;

                if (transformsProp.arraySize > 1)
                    elementName += $"   and {transformsProp.arraySize - 1} others";
            }

            bool isTransformEmpty = transformsProp.arraySize == 0 || ContainsNull(transformsProp);
            bool isMoldEmpty = moldProp.objectReferenceValue == null;

            if (isTransformEmpty || isMoldEmpty) GUI.color = WarningColor;

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            isFoldout = EditorGUI.Foldout(foldoutRect, isFoldout, elementName, true);
            EditorPrefs.SetBool(foldoutKey, isFoldout);

            GUI.color = defaultGUIColor;

            if (isFoldout)
            {
                EditorGUI.indentLevel++;

                float lineHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                Rect currentRect = new Rect(position.x, position.y + lineHeight, position.width, EditorGUIUtility.singleLineHeight);

                if (isTransformEmpty) GUI.color = WarningColor;

                EditorGUI.PropertyField(currentRect, transformsProp, true);
                currentRect.y += EditorGUI.GetPropertyHeight(transformsProp);

                GUI.color = defaultGUIColor;

                currentRect.y += EditorGUIUtility.standardVerticalSpacing;

                if (isMoldEmpty) GUI.color = WarningColor;

                EditorGUI.PropertyField(currentRect, moldProp, new GUIContent("Mold"));
                currentRect.y += lineHeight;

                GUI.color = defaultGUIColor;

                if (isButton)
                {
                    currentRect.y += EditorGUIUtility.standardVerticalSpacing;
                    isButtonFoldout = EditorGUI.Foldout(currentRect, isButtonFoldout, "Button Config", true);
                    currentRect.y += lineHeight;

                    EditorPrefs.SetBool(buttonFoldoutKey, isButtonFoldout);

                    if (isButtonFoldout)
                    {
                        EditorGUI.PropertyField(currentRect, activatedObjectProp);
                        currentRect.y += lineHeight;

                        EditorGUI.PropertyField(currentRect, onPressProp);
                        currentRect.y += EditorGUI.GetPropertyHeight(onPressProp);

                        currentRect.y += EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(currentRect, onReleaseProp);
                        currentRect.y += EditorGUI.GetPropertyHeight(onReleaseProp);
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            string foldoutKey = $"{FoldoutKey}_{property.propertyPath}";
            bool isFoldout = EditorPrefs.GetBool(foldoutKey, true);

            string buttonFoldoutKey = $"{ButtonFoldoutKey}_{property.propertyPath}";
            isButtonFoldout = EditorPrefs.GetBool(buttonFoldoutKey, true);

            if (isFoldout)
            {
                var transformsProp = property.FindPropertyRelative("transforms");
                height += EditorGUI.GetPropertyHeight(transformsProp);

                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var moldProp = property.FindPropertyRelative("mold");
                if (moldProp.objectReferenceValue is PressureButtonMold)
                {
                    height += EditorGUIUtility.singleLineHeight * 0.5f;
                    if (isButtonFoldout)
                    {
                        height += EditorGUIUtility.singleLineHeight;
                        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OnPress"));
                        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("OnRelease"));
                    }
                    height += EditorGUIUtility.singleLineHeight * 0.65f;
                }

                height += EditorGUIUtility.singleLineHeight * 0.5f;
            }

            return height;
        }

        private bool ContainsNull(SerializedProperty array)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                if (array.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    return true;
            }
            return false;
        }
    }
#endif
}