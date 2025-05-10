// BiomeSettings.cs
// Used to define and manage settings for different biomes in the terrain generation system.
// It allows you to configure biome-specific properties, apply these settings to terrain generators, and preview the resulting terrain in Unity.
// TerraForge 2.0.0

using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEditor.Experimental.SceneManagement;
#endif
using System.Collections.Generic;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.Generators
{
    /// <summary>
    /// Represents the settings for a specific biome in the TerraForge terrain generation system.
    /// </summary>
    [CreateAssetMenu(fileName = "BiomeSettings", menuName = "TerraForge 2/Biome Settings", order = 1)]
    public class BiomeSettings : ScriptableObject
    {
        /// <summary>
        /// The name of the biome.
        /// </summary>
        [Tooltip("The name of the biome.")]
        public string biomeName = "Biome 1";

        /// <summary>
        /// The height of the terrain for this biome.
        /// </summary>
        [Tooltip("The height of the terrain for this biome.")]
        public float terrainHeight;

        /// <summary>
        /// The layers of terrain settings for this biome.
        /// </summary>
        [Tooltip("The layers of terrain settings for this biome.")]
        public TerrainLayerSettings[] terrainLayers;

        /// <summary>
        /// Settings for terrain hydraulic erosion.
        /// </summary>
        [Tooltip("Settings for terrain hydraulic erosion.")]
        public HydraulicErosionLayerSettings hydraulicErosionLayerSettings;

        /// <summary>
        /// The prefab used for previewing the biome.
        /// </summary>
        [HideInInspector]
        public GameObject biomePreviewTerrainPrefab;

        #if UNITY_EDITOR
            // Variables for the Custom Editor:
            public bool showTerrainLayersSettings = true;
            public bool showHydraulicErosionLayerSettings;

        #endif

        /// <summary>
        /// Applies the biome settings to the specified terrain generator and terrain game object.
        /// </summary>
        /// <param name="generator">The terrain generator to apply the settings to.</param>
        /// <param name="terrainGameObject">The terrain game object to apply the settings to.</param>
        /// <param name="isEmptyBiome">Indicates if the biome is empty.</param>
        /// <param name="emptyBiomesHeight">The height for empty biomes.</param>
        public void ApplyingBiomeSettings(TerraForgeTerrainGenerator generator, GameObject terrainGameObject, bool isEmptyBiome, float emptyBiomesHeight)
        {
            if (!isEmptyBiome)
            {
                if (terrainGameObject != null)
                {
                    // Update the name of the terrain game object to include the biome name.
                    terrainGameObject.name = $"{biomeName}_{terrainGameObject.name}";
                }

                // Disable auto-update for terrain generation and adaptation to avoid conflicts during manual updates.
                generator.generalSettings.autoUpdateTerrainGeneration = false;
            
                // Apply the biome-specific terrain height.
                generator.generalSettings.terrainHeight = terrainHeight;

                // Copy the terrain layers.
                generator.terrainLayers = new TerrainLayerSettings[terrainLayers.Length];
                for (int i = 0; i < terrainLayers.Length; i++)
                {
                    generator.terrainLayers[i] = new TerrainLayerSettings(terrainLayers[i]);
                    generator.terrainLayers[i].seed = UnityEngine.Random.Range(0, 1000);
                }

                generator.hydraulicErosionLayerSettings = new HydraulicErosionLayerSettings(hydraulicErosionLayerSettings);
            }
            else
            {
                // Update the name of the terrain game object to include the biome name.
                terrainGameObject.name = $"{biomeName}_{terrainGameObject.name}";

                // Disable auto-update for terrain generation and adaptation to avoid conflicts during manual updates.
                generator.generalSettings.autoUpdateTerrainGeneration = false;
            
                // Apply the biome-specific terrain height.
                generator.generalSettings.terrainHeight = emptyBiomesHeight;

                // Copy the terrain layers.
                generator.terrainLayers = new TerrainLayerSettings[0];

                generator.hydraulicErosionLayerSettings = hydraulicErosionLayerSettings;
            }
        }

    #if UNITY_EDITOR
        /// <summary>
        /// Shows a preview of the biome.
        /// </summary>
        public void ShowPreviewBiome()
        {
            // Instantiate the preview terrain prefab and apply biome settings.
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(TerraForgeGlobalSettings.Instance.biomePreviewTerraForgeTerrain);
            TerraForgeTerrainGenerator generator = instance.GetComponent<TerraForgeTerrainGenerator>();

            if (generator == null)
            {
                Debug.LogError("The instantiated prefab does not contain a TerraForgeTerrainGenerator component.");
                DestroyImmediate(instance);
                return;
            }

            generator.generalSettings.terrainHeight = terrainHeight;
            generator.terrainLayers = terrainLayers;
            generator.hydraulicErosionLayerSettings = hydraulicErosionLayerSettings;
            generator.generalSettings.autoUpdateTerrainGeneration = false;
            generator.generalSettings.terrainResolution = TerrainResolution.Resolution513;
            generator.ChangeTerrainResolution(true);

            generator.biomeSettings = this;

            generator.generalSettings.autoUpdateTerrainGeneration = true;

            instance.GetComponent<Terrain>().terrainData.size = new Vector3(1000, 1000 * 0.6f, 1000);

            // Apply changes to the prefab and destroy the temporary instance.
            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.UserAction);
            DestroyImmediate(instance);

            // Open the prefab in the editor based on user confirmation settings.
            AssetDatabase.OpenAsset(TerraForgeGlobalSettings.Instance.biomePreviewTerraForgeTerrain);

            /*var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var autoSaveProperty = typeof(PrefabStage).GetProperty("autoSave", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (autoSaveProperty != null)
            {
                autoSaveProperty.SetValue(prefabStage, false, null);
            }*/
        }

        /// <summary>
        /// Closes the prefab view if a prefab stage is currently open.
        /// </summary>
        public void ClosePrefabView()
        {
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null)
                {
                    StageUtility.GoBackToPreviousStage();
                }
            }
            else
            {
                Debug.LogWarning("No prefab stage is currently open.");
            }
        }
    #endif
    }
}
