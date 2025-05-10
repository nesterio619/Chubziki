// TerraForgeGlobalSettings.cs
// TerraForge 2.0.0

using UnityEngine;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.TerraForgeEditor
{
    /// <summary>
    /// ScriptableObject containing global settings for TerraForge.
    /// </summary>
    [CreateAssetMenu(fileName = "TerraForgeGlobalSettings", menuName = "TerraForge 2/Global Settings (Recommended in a single copy)", order = 3)]
    public class TerraForgeGlobalSettings : ScriptableObject
    {
        /// <summary>
        /// Flag to enable confirmation dialogs.
        /// </summary>
        public bool enableConfirmation = true;

        /// <summary>
        /// Affects the delay between automatic generations (System.Threading.Tasks.Task.Delay(X)).
        /// </summary>
        [Range(100, 1000)]
        public int delayBetweenAutomaticGeneratingOperations;

        /// <summary>
        /// Default TerraForge terrain prefab.
        /// </summary>
        public GameObject defaultTerraForgeTerrain;

        /// <summary>
        /// Default TerraForge terrain prefab.
        /// </summary>
        public GameObject defaultTerraForgeTerrainForGrid;
        
        /// <summary>
        /// Default TerraForge terrain data.
        /// </summary>
        public TerrainData defaultTerraForgeTerrainData;

        /// <summary>
        /// TerraForge biome preview terrain prefab.
        /// </summary>
        public GameObject biomePreviewTerraForgeTerrain;

        /// <summary>
        /// TerraForge biome preview terrain data.
        /// </summary>
        public TerrainData biomePreviewTerraForgeTerrainData;

        /// <summary>
        /// Default TerraForge terrain grid prefab.
        /// </summary>
        public GameObject defaultTerraForgeTerrainGrid;

        /// <summary>
        /// Path to Save Terrain Data [in the Editor].
        /// </summary>
        public string editorPathToSaveTerrainData = "Assets/TerraForge 2/TerrainsData"; 

        /// <summary>
        /// Compute shader for hydraulic erosion.
        /// </summary>
        public ComputeShader hydraulicErosionComputeShader;
        
        private static TerraForgeGlobalSettings instance;

        /// <summary>
        /// Singleton instance of TerraForgeGlobalSettings.
        /// </summary>
        public static TerraForgeGlobalSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    // Attempt to load the settings from the Resources folder
                    instance = Resources.Load<TerraForgeGlobalSettings>("TerraForgeGlobalSettings");

                    // If not found, create a new instance and log a warning
                    if (instance == null)
                    {
                        instance = CreateInstance<TerraForgeGlobalSettings>();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Method to force reload the instance from Resources.
        /// </summary>
        public static void Reload()
        {
            instance = Resources.Load<TerraForgeGlobalSettings>("TerraForgeGlobalSettings");
            if (instance == null)
            {
                instance = CreateInstance<TerraForgeGlobalSettings>();
                Debug.LogWarning("TerraForgeGlobalSettings asset not found. A new instance has been created.");
            }
        }
    }
}