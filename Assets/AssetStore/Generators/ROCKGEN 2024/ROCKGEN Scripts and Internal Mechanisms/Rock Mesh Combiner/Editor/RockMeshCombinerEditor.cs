using UnityEngine;
using UnityEditor;

namespace RockGenerator
{
    [CustomEditor(typeof(RockMeshCombiner))]
    public class RockMeshCombinerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RockMeshCombiner rockMeshCombiner = (RockMeshCombiner)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Combine Meshes"))
            {
                rockMeshCombiner.RockMeshCombine();
            }
        }
    }
}
