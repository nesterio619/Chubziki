using UnityEngine;
using UnityEditor;

namespace RSM
{
    [CustomPropertyDrawer(typeof(RSMAttribute), true)]
    public class HideDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}