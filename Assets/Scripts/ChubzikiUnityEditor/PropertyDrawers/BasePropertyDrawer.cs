#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChubzikiUnityEditor.PropertyDrawers
{
    public class BasePropertyDrawer : PropertyDrawer
    {
        protected bool _isFoldoutOpen = true;

        protected virtual IEnumerable<string> PropertyNames { get; }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var totalHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (!_isFoldoutOpen) 
                return totalHeight;

            foreach (var propName in PropertyNames)
            {
                var prop = property.FindPropertyRelative(propName);
                totalHeight += EditorGUI.GetPropertyHeight(prop) + EditorGUIUtility.standardVerticalSpacing;
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _isFoldoutOpen = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                _isFoldoutOpen, label, true);

            if (!_isFoldoutOpen) return;

            EditorGUI.indentLevel++;
            var yOffset = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            foreach (var propName in PropertyNames)
            {
                var serializedProperty = property.FindPropertyRelative(propName);
                var propHeight = EditorGUI.GetPropertyHeight(serializedProperty);
                var fieldRect = new Rect(position.x, yOffset, position.width, propHeight);
                EditorGUI.PropertyField(fieldRect, serializedProperty, true);
                yOffset += propHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            EditorGUI.indentLevel--;
        }
    }
}
#endif