#if UNITY_EDITOR
using Actors.Molds;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RBLibraBridgeMold))]
public class RBLibraBridgeMoldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        RBLibraBridgeMold mold = (RBLibraBridgeMold)target;

        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            if (property.name == "m_Script")
                GUI.enabled = false;

            if (property.name.Contains("PoolInfo"))
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(property);

                if (GUILayout.Button("Apply prefab values", GUILayout.Width(130)))
                    mold.ApplyPrefabValues();

                EditorGUILayout.EndHorizontal();
            }
            else
                EditorGUILayout.PropertyField(property);

            GUI.enabled = true;
            enterChildren = false;
        }
    }
}
#endif