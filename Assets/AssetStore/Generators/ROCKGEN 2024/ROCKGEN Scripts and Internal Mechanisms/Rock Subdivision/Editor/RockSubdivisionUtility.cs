using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RockGenerator
{
    public class RockSubdivisionUtility : EditorWindow
    {
        public RockSubdivisionUtility() {
            titleContent.text = "ROCKGEN 2024/Rock Subdivision";
        }

        Vector2 selectionScroll = Vector2.zero;
        int iterations = 1;
        CatmullClark.Options.BoundaryInterpolation boundaryInterpolation;

        void OnGUI() {

            EditorGUIUtility.labelWidth = 80;

            Transform[] selection = Selection.transforms;

            // Selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Selection", GUILayout.Width(80));
            selectionScroll = EditorGUILayout.BeginScrollView(selectionScroll);
            foreach (Transform t in selection) {
                EditorGUILayout.LabelField(t.name);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

            // Iterations
            iterations = (int)EditorGUILayout.Slider("Iterations", iterations, 1, 3);

            // Boundaries
            boundaryInterpolation = (CatmullClark.Options.BoundaryInterpolation)
                EditorGUILayout.EnumPopup("Boundaries", boundaryInterpolation);

            // Button
            if (GUILayout.Button("Subdivide")) {
                if (selection.Length == 0) throw new System.Exception("Nothing selected to subdivide");
                var options = new CatmullClark.Options {
                    boundaryInterpolation = boundaryInterpolation,
                };
                foreach (Transform t in selection) {
                    // Add Undo record
                    MeshFilter mf = CatmullClark.CheckMeshFilter(t.gameObject);
                    Undo.RecordObject(mf, "Subdivide " + t.name);
                    // Subdivide
                    CatmullClark.Subdivide(t.gameObject, iterations, options);
                }
                if (selection.Length > 1) {
                    Undo.SetCurrentGroupName(string.Format("Subdivide {0} objects", selection.Length));
                }
            }
        }

        void OnInspectorUpdate() {
            Repaint();
        }

        [MenuItem("Window/ROCKGEN 2024/Rock Subdivision")]
        public static void ShowRockSubdivisionUtility() {
            if (window == null) {
                window = ScriptableObject.CreateInstance<RockSubdivisionUtility>();
            }
            window.ShowUtility();
        }
        static private RockSubdivisionUtility window = null;
    }
}
