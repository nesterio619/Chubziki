using System.Collections.Generic;
using Regions;
using UnityEngine;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

#endif

namespace Core.SceneControl
{
    [ExecuteInEditMode]
    public class SceneBaker : MonoBehaviour
    {
        [Space(5)] [Header("Baking options")]
        [Space(5)]
        [SerializeField] private bool autoSave;
        [SerializeField] private bool checkForErrors;

        [Space(10)] [Header("Components to bake")]
        [SerializeField] private bool recalculateBounds;
        [SerializeField] private bool bakeNavMesh;
        [SerializeField] private bool bakeReflections;

        public static bool IsBaking { get; private set; }

        private void Awake()
        {
            if (Application.isPlaying)
                Destroy(this);
        }
        
        public void ToggleAllBakingOptions(bool enable)
        {
            recalculateBounds = enable;
            bakeNavMesh = enable;
            bakeReflections = enable;
            checkForErrors = enable;
        }
        
        public void BakeAll()
        {
            ConsoleLogHelper.TryStartNewLog(true);
            
            IsBaking = true;

            ConsoleLogHelper.Log("SceneBaker logs:");
#if UNITY_EDITOR
            if(checkForErrors)
                if (SceneExceptionFinder.Instance.CheckForExceptions())
                {
                    EditorApplication.isPlaying = false;
                    ConsoleLogHelper.Log("\nStopped entering play mode.");
                }
#endif
            BakeRegions();
            
            ConsoleLogHelper.EndLog();

            IsBaking = false;
        }

        #region events that start baking

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorSceneManager.sceneSaving += OnSceneSaving;
            EditorSceneManager.sceneClosing += OnSceneClosing;
#endif
        }


        private void OnDisable()
        {
#if UNITY_EDITOR  
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            EditorSceneManager.sceneClosing -= OnSceneClosing;
#endif
        }

        #if UNITY_EDITOR
        private void OnSceneClosing(Scene scene, bool removingScene)
        {
            if (!autoSave) return;
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;

            BakeAll();
            EditorSceneManager.SaveScene(scene);
        }

        private void OnSceneSaving(Scene scene, string path)
        {
            if(!SceneManager.IsChangingScene && !EditorApplication.isPlayingOrWillChangePlaymode) 
                BakeAll(); 
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
                BakeAll();
        }
#endif
        #endregion

        #region Bake regions

        private void BakeRegions()
        {
            ConsoleLogHelper.Log("\nStarting baking...");

            foreach (var region in FindObjectsOfType<Region>())
            {
                ConsoleLogHelper.Log($"\nBaking region [{region.gameObject.name}]...");

                if (recalculateBounds)
                {
                    ConsoleLogHelper.Log($" - Recalculating bounds...", false);
                    region.CalculateBounds(displayBounds: false);
                    ConsoleLogHelper.Log($" -> Success");
                }

                BakeSectors(region.Sectors, region.name);
            }

            ConsoleLogHelper.Log("\nBaking finished.");
        }

        private void BakeSectors(List<Sector> sectors, string regionName)
        {
            if(!bakeNavMesh && !bakeReflections) return;

            ConsoleLogHelper.Log($" - Baking sectors...");
            foreach (var sector in sectors)
            {
                ConsoleLogHelper.LogIndent(1,$"Sector [{sector.name}]");
                if (bakeNavMesh)
                {
                    ConsoleLogHelper.LogIndent(1,$"- Baking NavMesh...", false);
                    sector.navMeshBaker.BakeNavMesh();
                    ConsoleLogHelper.Log($" -> Success");
                }

                if (!bakeReflections) continue;

                ConsoleLogHelper.LogIndent(1,$"- Baking reflection probes...");
                foreach (var location in sector.Locations)
                {
                    ConsoleLogHelper.LogIndent(2,$"- Location [{location.name}] baking reflection probes...", false);
                    location.BakeReflectionProbe();
                    ConsoleLogHelper.Log($" -> Success");
                }
                ConsoleLogHelper.LogIndent(1,$"- Reflection probes baked.\n");
            }
            ConsoleLogHelper.Log($" - Sectors baked");
        }

        #endregion
        
    }
}

