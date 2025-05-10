#if UNITY_EDITOR
using Core.SceneControl;
using UnityEngine;
using UnityEditor;

namespace Regions.Editors
{

    [CustomEditor(typeof(SceneBaker))]

    public class SceneBakerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var baker = (SceneBaker)target;

            GUILayout.Space(7);
            GUILayout.BeginHorizontal();

            if(GUILayout.Button("Disable all"))
            {
                baker.ToggleAllBakingOptions(false);
                EditorUtility.SetDirty(baker);
            }

            if (GUILayout.Button("Enable all"))
            {
                baker.ToggleAllBakingOptions(true);
                EditorUtility.SetDirty(baker);
            }

            GUILayout.EndHorizontal();
            
            GUILayout.Space(7);
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Bake with defined settings"))
            {
                baker.BakeAll();
                EditorUtility.SetDirty(baker);
            }
            
            GUILayout.EndHorizontal();
        }
        
    }
}
#endif
