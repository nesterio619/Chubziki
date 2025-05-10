using UnityEngine;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;

namespace RockGenerator
{
    [CustomEditor(typeof(RockGen))]
    public class RockGenEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RockGen rockGen = (RockGen)target;

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Add Cluster Component"))
            {
                rockGen.AddClusterComponent();
            }
        }
    }
}
