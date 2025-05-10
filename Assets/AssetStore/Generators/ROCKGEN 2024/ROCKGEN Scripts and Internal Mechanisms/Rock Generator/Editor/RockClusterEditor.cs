using UnityEngine;
using UnityEditor;

namespace RockGenerator
{
    [CustomEditor(typeof(RockCluster))]
    public class RockClusterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            RockCluster rockCluster = (RockCluster)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Child Stones"))
            {
                rockCluster.CreateChildStones();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Delete Child Stones"))
            {
                rockCluster.DeleteChildStones();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add Rock Mesh Combiner Component"))
            {
                rockCluster.AddRockMeshCombinerComponent();
            }
        }
    }
}
