// TerraForgeTerrainGenerator.cs
// Responsible for terrain generation.
// TerraForge 2.0.0

using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using System;
using System.IO;
using Unity.Collections;
using System.Collections.Generic;
#if UNITY_BURST
    using Unity.Burst;
    using Unity.Jobs;
#endif
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.Generators
{
    /// <summary>
    /// Generates terrain using TerraForge settings.
    /// </summary>
    [System.Serializable] [ExecuteInEditMode]
    [AddComponentMenu("TerraForge 2/Terrain Generator")]
    public class TerraForgeTerrainGenerator : MonoBehaviour, IGenerator
    {
        /// <summary>
        /// General settings for the terrain generation.
        /// </summary>
        [Tooltip("General settings for the terrain generation.")]
        public GeneralTerrainSettings generalSettings;

        /// <summary>
        /// Array of settings for individual terrain layers.
        /// </summary>
        [Tooltip("Array of settings for individual terrain layers.")]
        public TerrainLayerSettings[] terrainLayers;

        /// <summary>
        /// Settings for terrain hydraulic erosion.
        /// </summary>
        [Tooltip("Settings for terrain hydraulic erosion.")]
        public HydraulicErosionLayerSettings hydraulicErosionLayerSettings;

        /// <summary>
        /// Settings for terrain edge smoothing.
        /// </summary>
        //[HideInInspector]
        public TerrainEdgeSmoothingSettings terrainEdgeSmoothingSettings;

        /// <summary>
        /// Previous settings for terrain hydraulic erosion, used to detect changes.
        /// </summary>
        private HydraulicErosionLayerSettings previousHydraulicErosionLayerSettings;

        /// <summary>
        /// Settings for contact heights generators.
        /// </summary>
        [HideInInspector]
        public ContactHeightsGeneratorsSettings contactHeightsGenerators;

        /// <summary>
        /// Reference to the Unity Terrain component.
        /// </summary>
        [HideInInspector]
        public Terrain terrain;

        /// <summary>
        /// 2D array representing the blended height map.
        /// </summary>
        private float[,] _blendedHeightMap;

        /// <summary>
        /// 2D array representing the height map after hydraulic erosion.
        /// </summary>
        private float[,] _hydraulicErodedHeightMap;

        /// <summary>
        /// Array of cached height maps.
        /// </summary>
        private float[][,] _cachedHeightMaps;

        /// <summary>
        /// Flags indicating whether each layer has been modified.
        /// </summary>
        private bool[] _layerModifiedFlags;

        /// <summary>
        /// Array of the previous terrain layer settings.
        /// </summary>
        private TerrainLayerSettings[] _previousTerrainLayers;

        /// <summary>
        /// Settings for the biome applied to the terrain.
        /// </summary>
        public BiomeSettings biomeSettings;

        /// <summary>
        /// PlayerPrefs keys for saving and loading component data.
        /// </summary>
        private const string GeneralSettingsKey = "GeneralSettings";
        private const string TerrainLayersKey = "TerrainLayers";
        private const string HydraulicErosionSettingsKey = "HydraulicErosionSettings";

        /// <summary>
        /// Event triggered when terrain generation is complete.
        /// </summary>
        public event System.Action OnGenerationComplete;

    #if UNITY_EDITOR
        // Variables for the Custom Editor:
        [HideInInspector] public bool showTerrainSettings = true;
        [HideInInspector] public bool showTerrainNoiseLayersSettings;
        [HideInInspector] public bool showHydraulicErosionLayerSettings;
    #endif

        /// <summary>
        /// Saves component data (general settings, terrain layers, hydraulic erosion settings) using PlayerPrefs.
        /// </summary>
        public void SaveComponentData()
        {
            PlayerPrefs.SetString(GeneralSettingsKey, JsonUtility.ToJson(generalSettings));
            PlayerPrefs.SetString(TerrainLayersKey, JsonHelper.ToJson(terrainLayers)); // Custom JsonHelper to serialize array
            PlayerPrefs.SetString(HydraulicErosionSettingsKey, JsonUtility.ToJson(hydraulicErosionLayerSettings));

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads component data (general settings, terrain layers, hydraulic erosion settings) from PlayerPrefs.
        /// </summary>
        public void LoadComponentData()
        {
            if (PlayerPrefs.HasKey(GeneralSettingsKey))
            {
                generalSettings = JsonUtility.FromJson<GeneralTerrainSettings>(PlayerPrefs.GetString(GeneralSettingsKey));
            }
            if (PlayerPrefs.HasKey(TerrainLayersKey))
            {
                terrainLayers = JsonHelper.FromJson<TerrainLayerSettings>(PlayerPrefs.GetString(TerrainLayersKey)); // Custom JsonHelper to deserialize array
            }
            if (PlayerPrefs.HasKey(HydraulicErosionSettingsKey))
            {
                hydraulicErosionLayerSettings = JsonUtility.FromJson<HydraulicErosionLayerSettings>(PlayerPrefs.GetString(HydraulicErosionSettingsKey));
            }
        }

        /// <summary>
        /// Helper class for serializing and deserializing arrays using JsonUtility.
        /// </summary>
        public static class JsonHelper
        {
            public static T[] FromJson<T>(string json)
            {
                Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                return wrapper.items;
            }

            public static string ToJson<T>(T[] array)
            {
                Wrapper<T> wrapper = new Wrapper<T>();
                wrapper.items = array;
                return JsonUtility.ToJson(wrapper);
            }

            [Serializable]
            private class Wrapper<T>
            {
                public T[] items;
            }
        }

        /// <summary>
        /// Validates the component settings when the script is loaded or a value changes in the inspector.
        /// </summary>
        private void OnValidate()
        {
            // Check if auto-update for terrain generation is enabled
            if (generalSettings.autoUpdateTerrainGeneration)
            {
                AutoUpdateGenerate();
            }
        }
        
        /// <summary>
        /// Generates the terrain.
        /// </summary>
        public async void TerrainGenerate()
        {
            // Check if there is a new generation waiting
            if (generalSettings.isNewGenerationWaiting)
                return;

            // Set flag to indicate new generation is waiting
            generalSettings.isNewGenerationWaiting = true;

            // Add a small delay to ensure smooth execution
            await System.Threading.Tasks.Task.Delay(100);

            // Check if the component has been destroyed during the delay
            if (this == null)
            {
                generalSettings.isNewGenerationWaiting = false;
                return;
            }

            // Check if terrain material has changed
            if (generalSettings.bufferTerrainMaterial != generalSettings.terrainMaterial)
            {
                ChangeTerrainMaterial();
            }

            // Check if terrain resolution has changed
            if (generalSettings.bufferTerrainResolution != generalSettings.terrainResolution)
            {
                ChangeTerrainResolution(true);
            }
            {
                // Generate terrain
                Generate();
            }

            // Perform hydraulic erosion if enabled
            if (hydraulicErosionLayerSettings.isEnabled)
            {
                if (_hydraulicErodedHeightMap != null && !IsHydraulicErosionGenerationLayerModified(hydraulicErosionLayerSettings, previousHydraulicErosionLayerSettings) && !hydraulicErosionLayerSettings.isHydraulicErosionLayerModified)
                {
                    ApplicationHydraulicErosionLayer();
                }
                else
                {
                    HydraulicErodeGenerate();
                }
            }

            // Perform terrain edge smoothing if enabled
            if (terrainEdgeSmoothingSettings.isEnabled)
            {
                TerrainEdgeSmoothing();
            }

            OnGenerationComplete?.Invoke();
        }

        /// <summary>
        /// Automatically generates the terrain.
        /// </summary>
        private async void AutoUpdateGenerate()
        {
            // Check if the terrain component exists
            if (terrain == null)
            {
                // Attempt to get the terrain component from the current GameObject
                terrain = this.GetComponent<Terrain>();

                // Check again if the terrain component exists
                if (terrain == null)
                {
                    Debug.LogWarning("Terrain component not found. Please ensure it is available on the GameObject!");
                    return;
                }
            }
            
            // Check if there is a new generation waiting
            if (generalSettings.isNewGenerationWaiting)
                return;

            // Check if terrain resolution is too high for automatic generation
            if (terrain != null && ((terrain.terrainData.heightmapResolution > 513 && (int)generalSettings.terrainResolution > 513) || (int)generalSettings.terrainResolution > 513))
            {
                Debug.LogWarning("Terrain resolution is greater than 513. Automatic terrain update is disabled.");
                generalSettings.autoUpdateTerrainGeneration = false;
                return;
            }

            // Set flag to indicate new generation is waiting
            generalSettings.isNewGenerationWaiting = true;

            // Add a small delay to ensure smooth execution
            await System.Threading.Tasks.Task.Delay(TerraForgeGlobalSettings.Instance.delayBetweenAutomaticGeneratingOperations);

            // Check if the component has been destroyed during the delay
            if (this == null)
            {
                generalSettings.isNewGenerationWaiting = false;
                return;
            }

            // Check if terrain resolution has changed
            if (generalSettings.bufferTerrainResolution != generalSettings.terrainResolution)
            {
                ChangeTerrainResolution(true);
            }

            // Check if terrain material has changed
            if (generalSettings.bufferTerrainMaterial != generalSettings.terrainMaterial)
            {
                ChangeTerrainMaterial();
            }

            // Generate terrain
            Generate();

            // Perform hydraulic erosion if enabled
            if (hydraulicErosionLayerSettings.isEnabled)
            {
                if (_hydraulicErodedHeightMap != null && !IsHydraulicErosionGenerationLayerModified(hydraulicErosionLayerSettings, previousHydraulicErosionLayerSettings) && !hydraulicErosionLayerSettings.isHydraulicErosionLayerModified)
                {
                    ApplicationHydraulicErosionLayer();
                }
                else
                {
                    HydraulicErodeGenerate();
                }
            }

            // Perform terrain edge smoothing if enabled
            if (terrainEdgeSmoothingSettings.isEnabled)
            {
                TerrainEdgeSmoothing();
            }
        }

        /// <summary>
        /// Checks if a terrain layer has been modified.
        /// </summary>
        /// <param name="currentLayer">The current terrain layer settings.</param>
        /// <param name="previousLayer">The previous terrain layer settings.</param>
        /// <returns>True if the layer has been modified, false otherwise.</returns>
        private bool IsLayerModified(TerrainLayerSettings currentLayer, TerrainLayerSettings previousLayer)
        {
            // Check for null references
            if (currentLayer == null || previousLayer == null)
                return true;

            // Compare current layer settings with previous settings to determine if modified
            return 
                currentLayer.isEnabled != previousLayer.isEnabled ||  
                currentLayer.blurRadius != previousLayer.blurRadius ||  
                currentLayer.noiseType != previousLayer.noiseType || 
                currentLayer.fractalType != previousLayer.fractalType ||  
                currentLayer.depth != previousLayer.depth || 
                currentLayer.scale != previousLayer.scale || 
                currentLayer.octaves != previousLayer.octaves || 
                currentLayer.lacunarity != previousLayer.lacunarity || 
                currentLayer.persistence != previousLayer.persistence || 
                currentLayer.seed != previousLayer.seed || 
                currentLayer.inversion != previousLayer.inversion || 
                !AreAnimationCurvesEqual(currentLayer.heightCurve, previousLayer.heightCurve) || 
                currentLayer.useFalloffMap != previousLayer.useFalloffMap || 
                currentLayer.falloffAngleFactor != previousLayer.falloffAngleFactor || 
                currentLayer.falloffRange != previousLayer.falloffRange;
        }

        /// <summary>
        /// Checks if a terrain layer has been modified.
        /// </summary>
        /// <param name="currentLayer">The current terrain layer settings.</param>
        /// <param name="previousLayer">The previous terrain layer settings.</param>
        /// <returns>True if the layer has been modified, false otherwise.</returns>
        private bool IsLayerModified1(TerrainLayerSettings currentLayer, TerrainLayerSettings previousLayer)
        {
            // Check for null references
            if (currentLayer == null || previousLayer == null)
                return true;

            // Compare current layer settings with previous settings to determine if modified
            return 
                currentLayer.blurRadius != previousLayer.blurRadius ||  
                currentLayer.depth != previousLayer.depth || 
                currentLayer.useFalloffMap != previousLayer.useFalloffMap || 
                currentLayer.falloffAngleFactor != previousLayer.falloffAngleFactor || 
                currentLayer.falloffRange != previousLayer.falloffRange;
        }

        /// <summary>
        /// Checks if two AnimationCurves are equal.
        /// </summary>
        /// <param name="curve1">The first AnimationCurve to compare.</param>
        /// <param name="curve2">The second AnimationCurve to compare.</param>
        /// <returns>True if the AnimationCurves are equal, false otherwise.</returns>
        private bool AreAnimationCurvesEqual(AnimationCurve curve1, AnimationCurve curve2)
        {
            // Compare AnimationCurves based on their keyframe count and keyframe values
            if (curve1 == null || curve2 == null)
                return false;

            Keyframe[] keys1 = curve1.keys;
            Keyframe[] keys2 = curve2.keys;

            if (keys1.Length != keys2.Length)
                return false;

            for (int i = 0; i < keys1.Length; i++)
            {
                if (keys1[i].time != keys2[i].time || keys1[i].value != keys2[i].value)
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Changes the terrain material to the one specified in the general settings.
        /// </summary>
        public void ChangeTerrainMaterial()
        {
            // Check if the terrain component exists
            if (terrain == null)
            {
                // Attempt to get the terrain component from the current GameObject
                terrain = this.GetComponent<Terrain>();

                // Check again if the terrain component exists
                if (terrain == null)
                {
                    Debug.LogWarning("Terrain component not found. Please ensure it is available on the GameObject!");
                    return;
                }
            }
            // Ensure the terrain material is not null
            if (generalSettings.terrainMaterial == null)
            {
                // Set the terrain material to the default material template if it's null
                generalSettings.terrainMaterial = terrain.materialTemplate;
            }
            else if (terrain.materialTemplate != generalSettings.terrainMaterial)
            {
                // Assign the specified terrain material if it's different from the current material template
                terrain.materialTemplate = generalSettings.terrainMaterial;
            }

            // Update the buffer terrain material
            generalSettings.bufferTerrainMaterial = generalSettings.terrainMaterial;
        }

        /// <summary>
        /// Changes the terrain resolution based on the resolution specified in the general settings.
        /// </summary>
        /// <param name="refreshGeneration">Flag indicating whether to refresh terrain generation after resolution change.</param>
        public void ChangeTerrainResolution(bool refreshGeneration)
        {
            // Check if the terrain component exists
            if (terrain == null)
            {
                // Attempt to get the terrain component from the current GameObject
                terrain = this.GetComponent<Terrain>();

                // Check again if the terrain component exists
                if (terrain == null)
                {
                    Debug.LogWarning("Terrain component not found. Please ensure it is available on the GameObject!");
                    return;
                }
            }
            
            // Create a copy of the existing terrain data to modify
            TerrainData terrainData = terrain.terrainData;

            // Adjust heightmap resolution
            terrainData.heightmapResolution = (int)generalSettings.terrainResolution - 1;

            // Adjust alphamap resolution
            terrainData.alphamapResolution = (int)generalSettings.terrainResolution - 1;

            // Adjust base map resolution
            terrainData.baseMapResolution = (int)generalSettings.terrainResolution - 1;

            // Adjust detail resolution
            terrainData.SetDetailResolution((int)generalSettings.terrainResolution, terrainData.detailResolutionPerPatch);

            // Set terrain size
            if (generalSettings.terrainSize <= 0f)
            {
                generalSettings.terrainSize = 1000f;
            }

            terrainData.size = new Vector3(generalSettings.terrainSize, generalSettings.terrainSize * 0.6f, generalSettings.terrainSize);
            generalSettings.bufferTerrainSize = generalSettings.terrainSize;

            // Assign the modified terrain data back to the terrain
            terrain.terrainData = terrainData;

            // Notify that the terrain data has changed
            terrain.Flush();

            // Update the buffer terrain resolution
            generalSettings.bufferTerrainResolution = generalSettings.terrainResolution;

            if (refreshGeneration)
            {
                // Refresh generation for all layers if specified
                RefreshGenerationAllLayers();
            }
        }


        /// <summary>
        /// Applies the changes made to the biome settings.
        /// </summary>
        public void ApplyTheChangesToBiome()
        {
            // Update biome settings with terrain height and layers
            biomeSettings.terrainHeight = generalSettings.terrainHeight;
            biomeSettings.terrainLayers = terrainLayers;

    #if UNITY_EDITOR
            // Close the prefab view
            TerraForgeEditorUtilities.ClosePrefabView();
    #endif

            // Clear the reference to biome settings
            biomeSettings = null;
        }


        /// <summary>
        /// Generates the terrain based on the configured terrain layers.
        /// </summary>
        public void Generate()
        {
            // Check if the terrain component exists
            if (terrain == null)
            {
                // Attempt to get the terrain component from the current GameObject
                terrain = this.GetComponent<Terrain>();

                // Check again if the terrain component exists
                if (terrain == null)
                {
                    Debug.LogWarning("Terrain component not found. Please ensure it is available on the GameObject!");
                    return;
                }
            }

            // Get the terrain data
            TerrainData terrainData = terrain.terrainData;

            // Initialize the blended height map with the general terrain height
            _blendedHeightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    _blendedHeightMap[x, y] = generalSettings.terrainHeight * 0.01f;
                }
            }
            
            if (terrainLayers != null)
            {
                // Initialize cached height maps and modification flags if not already initialized
                if (_cachedHeightMaps == null || _cachedHeightMaps.Length != terrainLayers.Length)
                {
                    _cachedHeightMaps = new float[terrainLayers.Length][,];
                    _layerModifiedFlags = new bool[terrainLayers.Length];
                    _previousTerrainLayers = new TerrainLayerSettings[terrainLayers.Length];
                }

                // Generate height maps for each layer and blend them into the final height map
                for (int i = 0; i < terrainLayers.Length; i++)
                {
                    TerrainLayerSettings layer = terrainLayers[i];
                    
                    // Skip this layer if it's not enabled
                    if (!layer.isEnabled)
                    {
                        if (_previousTerrainLayers[i].isEnabled != layer.isEnabled)
                        {
                            hydraulicErosionLayerSettings.isHydraulicErosionLayerModified = true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    // Check if the layer has been modified since the last generation
                    bool layerModified = IsLayerModified(layer, _previousTerrainLayers[i]);

                    if (!layerModified && _cachedHeightMaps[i] != null && !hydraulicErosionLayerSettings.isHydraulicErosionLayerModified)
                    {
                        // Use cached height map if the layer hasn't been modified
                        BlendHeightMaps(_cachedHeightMaps[i], layer.depth * 0.01f);
                    }
                    else
                    {
                        // Generate and cache height map for the modified layer
                        float[,] layerHeightMap = GenerateLayerHeightMap(layer, terrainData);
                        if (layer.isEnabled)
                        {
                            BlendHeightMaps(layerHeightMap, layer.depth * 0.01f);
                        }
                        else
                        {
                            BlendHeightMaps(layerHeightMap, layer.depth * 0f);
                        }
                        _cachedHeightMaps[i] = layerHeightMap;
                        _layerModifiedFlags[i] = true; // Mark the layer as modified
                        hydraulicErosionLayerSettings.isHydraulicErosionLayerModified = true;
                    }

                    // Update previous state for comparison in the next generation
                    _previousTerrainLayers[i] = new TerrainLayerSettings(layer);
                }
            }
            // Apply the blended height map to the terrain
            terrainData.SetHeights(0, 0, _blendedHeightMap);
            
            // Reset the new generation flag
            generalSettings.isNewGenerationWaiting = false;
        }

        /// <summary>
        /// Refreshes the generation of all terrain layers.
        /// </summary>
        public void RefreshGenerationAllLayers()
        {
            // Check if the terrain component exists
            if (terrain == null)
            {
                // Attempt to get the terrain component from the current GameObject
                terrain = this.GetComponent<Terrain>();

                // Check again if the terrain component exists
                if (terrain == null)
                {
                    Debug.LogWarning("Terrain component not found. Please ensure it is available on the GameObject!");
                    return;
                }
            }

            // Get the terrain data
            TerrainData terrainData = terrain.terrainData;

            // Initialize the blended height map with the general terrain height
            _blendedHeightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    _blendedHeightMap[x, y] = generalSettings.terrainHeight * 0.01f;
                }
            }

            if (terrainLayers != null)
            {
                // Initialize cached height maps and modification flags if not already initialized
                if (_cachedHeightMaps == null || _cachedHeightMaps.Length != terrainLayers.Length)
                {
                    _cachedHeightMaps = new float[terrainLayers.Length][,];
                    _layerModifiedFlags = new bool[terrainLayers.Length];
                    _previousTerrainLayers = new TerrainLayerSettings[terrainLayers.Length];
                }

                // Generate height maps for each layer and blend them into the final height map
                for (int i = 0; i < terrainLayers.Length; i++)
                {
                    TerrainLayerSettings layer = terrainLayers[i];

                    // Generate and cache height map for the modified layer
                    float[,] layerHeightMap = GenerateLayerHeightMap(layer, terrainData);
                    if (layer.isEnabled)
                    {
                        BlendHeightMaps(layerHeightMap, layer.depth * 0.01f);
                    }
                    else
                    {
                        BlendHeightMaps(layerHeightMap, layer.depth * 0f);
                    }
                    _cachedHeightMaps[i] = layerHeightMap;
                    _layerModifiedFlags[i] = true;
                    _previousTerrainLayers[i] = new TerrainLayerSettings(layer);
                }
            }

            // Mark hydraulic erosion layer as modified
            hydraulicErosionLayerSettings.isHydraulicErosionLayerModified = true;

            // Apply the blended height map to the terrain
            terrainData.SetHeights(0, 0, _blendedHeightMap);

            // Reset the new generation flag
            generalSettings.isNewGenerationWaiting = false;

            // Perform hydraulic erosion if enabled
            if (hydraulicErosionLayerSettings.isEnabled)
            {
                HydraulicErodeGenerate();
            }

            // Perform terrain edge smoothing if enabled
            if (terrainEdgeSmoothingSettings.isEnabled)
            {
                TerrainEdgeSmoothing();
            }
        }

        /// <summary>
        /// Generates the height map for a terrain layer.
        /// </summary>
        /// <param name="layer">The terrain layer settings.</param>
        /// <param name="terrainData">The terrain data.</param>
        /// <returns>The generated height map.</returns>
        private float[,] GenerateLayerHeightMap(TerrainLayerSettings layer, TerrainData terrainData)
        {
            // Generate falloff map if enabled
            float[,] falloff = null;
            if (layer.useFalloffMap)
            {
                falloff = new FalloffMap
                {
                    falloffAngleFactor = layer.falloffAngleFactor,
                    falloffRange = layer.falloffRange,
                    Size = terrainData.heightmapResolution
                }.Generate();
            }

            // Generate noise map for the current layer
            float[,] noiseMap = GenerateNoise(layer, falloff, terrainData);

            // Apply blur to the noise map
            noiseMap = ApplyBlur(noiseMap, layer.blurRadius);

            return noiseMap;
        }

        /// <summary>
        /// Blends a layer's height map into the overall height map.
        /// </summary>
        /// <param name="_blendedHeightMap">The overall height map.</param>
        /// <param name="layerHeightMap">The height map of the layer to blend.</param>
        /// <param name="depth">The depth of the layer.</param>
        private void BlendHeightMaps(float[,] layerHeightMap, float depth)
        {
            // Check if _blendedHeightMap is null
            if (_blendedHeightMap == null)
            {
                Debug.LogError("_blendedHeightMap is null");
                return; // Exit the method to avoid further processing
            } 
            
            // Blend the height maps using layer strength as transparency
            for (int y = 0; y < _blendedHeightMap.GetLength(1); y++)
            {
                for (int x = 0; x < _blendedHeightMap.GetLength(0); x++)
                {
                    _blendedHeightMap[x, y] += Mathf.Lerp(0f, layerHeightMap[x, y], depth) * depth;
                }
            }
        }

        /// <summary> 
        /// Generates the noise map for a terrain layer.
        /// </summary>
        /// <param name="layer">The terrain layer settings.</param>
        /// <param name="falloffMap">The falloff map.</param>
        /// <param name="terrainData">The terrain data.</param>
        /// <returns>The generated noise map.</returns>
        private float[,] GenerateNoise(TerrainLayerSettings layer, float[,] falloffMap, TerrainData terrainData)
        {
            // Generate noise map using fast noise algorithm
            return GenerateFastNoise(layer, falloffMap, terrainData);
        }

        /// <summary>
        /// Generates noise using the FastNoise algorithm.
        /// </summary>
        /// <param name="layer">The terrain layer settings.</param>
        /// <param name="falloffMap">The falloff map.</param>
        /// <param name="terrainData">The terrain data.</param>
        /// <returns>The generated noise map.</returns>
        private float[,] GenerateFastNoise(TerrainLayerSettings layer, float[,] falloffMap, TerrainData terrainData)
        {
            // Create a copy of the height curve and optionally invert it
            AnimationCurve heightCurve = layer.inversion ? InvertKeysRelativeToZero(layer.heightCurve) : new AnimationCurve(layer.heightCurve.keys);

            // Generate noise map using Perlin noise
            float maxLocalNoiseHeight;
            float minLocalNoiseHeight;
            float[,] noiseMap = new PerlinMap()
            {
                Size = terrainData.heightmapResolution,
                octaves = layer.octaves,
                scale = layer.scale,
                seed = layer.seed,
                persistence = layer.persistence,
                lacunarity = layer.lacunarity,
                noiseType = layer.noiseType,
                fractalType = layer.fractalType
            }.Generate(out maxLocalNoiseHeight, out minLocalNoiseHeight);

            // Apply height curve and falloff map to the noise map
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    var lerp = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);

                    // Subtract falloff value if provided
                    if (falloffMap != null)
                    {
                        lerp -= falloffMap[x, y];
                    }

                    // Evaluate height curve
                    noiseMap[x, y] = (lerp >= 0) ? heightCurve.Evaluate(lerp) : 0;
                }
            }

            return noiseMap;
        }
        
        /// <summary>
        /// Generates a square falloff map based on the terrain height map.
        /// </summary>
        /// <param name="heightMap">The terrain height map.</param>
        /// <param name="transitionWidth">The width of the transition area.</param>
        /// <param name="falloffRange">The range of the falloff.</param>
        /// <param name="fromCenter">Determines if the falloff is from the center or not.</param>
        /// <returns>The generated falloff map.</returns>
        public float[,] GenerateSquareFalloffMap(float[,] heightMap, float transitionWidth, float falloffRange, bool fromCenter)
        {
            // Calculate the center of the map
            int centerX = heightMap.GetLength(0) / 2;
            int centerY = heightMap.GetLength(1) / 2;

            float[,] falloffMap = new float[heightMap.GetLength(0), heightMap.GetLength(1)];

            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                for (int x = 0; x < heightMap.GetLength(0); x++)
                {
                    // Calculate the falloff distance as the maximum of horizontal and vertical distances from the center
                    float falloffDistanceX = Mathf.Abs(x - centerX);
                    float falloffDistanceY = Mathf.Abs(y - centerY);
                    float falloffDistance = Mathf.Max(falloffDistanceX, falloffDistanceY);

                    // Calculate the falloff value based on the direction
                    float falloffValue;
                    if (fromCenter)
                    {
                        falloffValue = Mathf.Clamp01((falloffDistance - falloffRange + transitionWidth) / (2 * transitionWidth));
                    }
                    else
                    {
                        falloffValue = Mathf.Clamp01((falloffRange - falloffDistance + transitionWidth) / (2 * transitionWidth));
                    }
                    falloffMap[x, y] = Mathf.Clamp01(1f - falloffValue);
                }
            }

            return falloffMap;
        }

        /// <summary>
        /// Applies a blur effect to a given noise map.
        /// </summary>
        /// <param name="noiseMap">The input noise map.</param>
        /// <param name="blurRadius">The blur radius.</param>
        /// <returns>The blurred noise map.</returns>
    #if UNITY_BURST
        [BurstCompile]
    #endif
        public float[,] ApplyBlur(float[,] noiseMap, int blurRadius)
        {
    #if UNITY_BURST
            int width = noiseMap.GetLength(0);
            int height = noiseMap.GetLength(1);

            NativeArray<float> sourceMap = new NativeArray<float>(width * height, Allocator.TempJob);
            NativeArray<float> blurredMap = new NativeArray<float>(width * height, Allocator.TempJob);

            // Copy the noise map data to the sourceMap NativeArray
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    sourceMap[y * width + x] = noiseMap[x, y];
                }
            }

            // Create a blur job and schedule it
            var blurJob = new BlurJob
            {
                sourceMap = sourceMap,
                blurredMap = blurredMap,
                width = width,
                height = height,
                blurRadius = blurRadius
            };

            JobHandle jobHandle = blurJob.Schedule(width * height, 64); // 64 is the batch count

            // Complete the job
            jobHandle.Complete();

            float[,] result = new float[width, height];

            // Copy the blurred map data back to the result array
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[x, y] = blurredMap[y * width + x];
                }
            }

            // Release the NativeArrays
            sourceMap.Dispose();
            blurredMap.Dispose();

            return result;
        #else
            int width = noiseMap.GetLength(0);
            int height = noiseMap.GetLength(1);
            float[,] blurredMap = new float[width, height];

            if (blurRadius == 0)
            {
                return noiseMap;
            }

            // Apply blur to the noise map using a simple box blur algorithm
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float sum = 0f;
                    int count = 0;

                    for (int offsetY = -blurRadius; offsetY <= blurRadius; offsetY++)
                    {
                        for (int offsetX = -blurRadius; offsetX <= blurRadius; offsetX++)
                        {
                            int neighborX = Mathf.Clamp(x + offsetX, 0, width - 1);
                            int neighborY = Mathf.Clamp(y + offsetY, 0, height - 1);
                            sum += noiseMap[neighborX, neighborY];
                            count++;
                        }
                    }

                    blurredMap[x, y] = sum / count;
                }
            }

            return blurredMap;
        #endif
        }

        /// <summary>
        /// Applies terrain edge smoothing.
        /// </summary>
        public void TerrainEdgeSmoothing()
        {
            float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
            float[,] smoothedHeightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
            smoothedHeightMap = ApplyBlur(heightMap, terrainEdgeSmoothingSettings.blurRadius);

            // Generate falloff map
            float[,] falloffMap = GenerateSquareFalloffMap(smoothedHeightMap, smoothedHeightMap.GetLength(1) * terrainEdgeSmoothingSettings.falloffTransitionWidth, smoothedHeightMap.GetLength(1) * terrainEdgeSmoothingSettings.falloffRange, false);

            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                for (int x = 0; x < heightMap.GetLength(0); x++)
                {
                    // Apply erosion strength with falloff factor
                    heightMap[x, y] = Mathf.Lerp(heightMap[x, y], smoothedHeightMap[x, y], falloffMap[x, y]);
                }
            }

            terrain.terrainData.SetHeights(0, 0, heightMap);
        }

        /// <summary>
        /// Inverts the keys of an animation curve relative to zero.
        /// </summary>
        /// <param name="curve">The animation curve to invert.</param>
        /// <returns>The inverted animation curve.</returns>
        public static AnimationCurve InvertKeysRelativeToZero(AnimationCurve curve)
        {
            Keyframe[] keys = curve.keys;
            float firstKeyValue = keys[0].value;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].value = firstKeyValue - (keys[i].value - firstKeyValue);
            }
            return new AnimationCurve(keys);
        }


        /// <summary>
        /// Generates the hydraulic erosion effect on the terrain.
        /// </summary>
        public void HydraulicErodeGenerate()
        {
            if (terrainLayers != null)
            {
                if (terrain == null)
                {
                    // Attempt to get the terrain component from the current GameObject
                    terrain = this.GetComponent<Terrain>();

                    // Check again if the terrain component exists
                    if (terrain == null)
                    {
                        Debug.LogWarning("Terrain component not found. Please ensure it is available on the GameObject!");
                        return;
                    }
                }

                if (_blendedHeightMap == null)
                {
                    _blendedHeightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
                }
                _hydraulicErodedHeightMap = HydraulicErode((float[,])_blendedHeightMap.Clone());
                previousHydraulicErosionLayerSettings = new HydraulicErosionLayerSettings(hydraulicErosionLayerSettings);
                hydraulicErosionLayerSettings.isHydraulicErosionLayerModified = false;
                ApplicationHydraulicErosionLayer();

                if (generalSettings.autoUpdateTerrainGeneration)
                {
                    AutoUpdateGenerate();
                }
            }
        }

        /// <summary>
        /// Generates the hydraulic erosion effect on the provided height map.
        /// </summary>
        /// <param name="heightMap">The height map to apply hydraulic erosion to.</param>
        /// <returns>The height map after applying hydraulic erosion.</returns> 
        public float[,] HydraulicErode(float[,] heightMap)
        {
            int heightmapResolution = terrain.terrainData.heightmapResolution;
            int iterations = hydraulicErosionLayerSettings.iterations;
            int resolution = (int)hydraulicErosionLayerSettings.erosionResolution - 1;
            for (int i = 0; i < hydraulicErosionLayerSettings.erosionSteps; i++)
            {
                heightMap = HydraulicsSystem.GenerateHydraulicErosion(TerraForgeGlobalSettings.Instance.hydraulicErosionComputeShader, hydraulicErosionLayerSettings, iterations, heightMap, heightmapResolution, resolution + 1);

                iterations *= 2;
                resolution *= 2;
            }

            return heightMap;
        }

        /// <summary>
        /// Applies the hydraulic erosion layer to the terrain.
        /// </summary>
        public void ApplicationHydraulicErosionLayer()
        {
            if (_blendedHeightMap == null)
            {
                _blendedHeightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
            }
            float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);

            if (hydraulicErosionLayerSettings.useFalloffMap)
            {
                // Generate falloff map
                float[,] falloffMap = GenerateSquareFalloffMap(heightMap, heightMap.GetLength(1) * hydraulicErosionLayerSettings.falloffTransitionWidth, heightMap.GetLength(1) * hydraulicErosionLayerSettings.falloffRange, true);

                for (int y = 0; y < heightMap.GetLength(1); y++)
                {
                    for (int x = 0; x < heightMap.GetLength(0); x++)
                    {
                        // Apply erosion strength with falloff factor
                        heightMap[x, y] = Mathf.Lerp(_blendedHeightMap[x, y], _hydraulicErodedHeightMap[x, y], hydraulicErosionLayerSettings.strength * falloffMap[x, y]);
                    }
                }
            }
            else
            {
                for (int y = 0; y < heightMap.GetLength(1); y++)
                {
                    for (int x = 0; x < heightMap.GetLength(0); x++)
                    {
                        // Apply erosion strength without falloff factor
                        heightMap[x, y] = Mathf.Lerp(_blendedHeightMap[x, y], _hydraulicErodedHeightMap[x, y], hydraulicErosionLayerSettings.strength);
                    }
                }
            }
            
            // Apply the modified height map to the terrain
            terrain.terrainData.SetHeights(0, 0, heightMap);
        }

        /// <summary>
        /// Checks if the hydraulic erosion layer settings have been modified.
        /// </summary>
        /// <param name="currentLayer">The current hydraulic erosion layer settings.</param>
        /// <param name="previousLayer">The previous hydraulic erosion layer settings.</param>
        /// <returns>True if the layer settings have been modified, false otherwise.</returns>
        private bool IsHydraulicErosionGenerationLayerModified(HydraulicErosionLayerSettings currentLayer, HydraulicErosionLayerSettings previousLayer)
        {
            return currentLayer.iterations != previousLayer.iterations ||
                    currentLayer.erosionResolution != previousLayer.erosionResolution ||
                    currentLayer.erosionSteps != previousLayer.erosionSteps ||
                    currentLayer.erosionRadius != previousLayer.erosionRadius ||
                    currentLayer.inertia != previousLayer.inertia ||
                    currentLayer.minSedimentCapacity != previousLayer.minSedimentCapacity ||
                    currentLayer.maxDropletLifetime != previousLayer.maxDropletLifetime ||
                    currentLayer.initialWaterVolume != previousLayer.initialWaterVolume;
        }

        /// <summary>
        /// Generates the hydraulic erosion effect on the terrain (Editor).
        /// </summary>
        public void HydraulicErodeGenerateEditor()
        {
            if (hydraulicErosionLayerSettings.isEnabled)
            {
                if (_hydraulicErodedHeightMap != null && !IsHydraulicErosionGenerationLayerModified(hydraulicErosionLayerSettings, previousHydraulicErosionLayerSettings) && !hydraulicErosionLayerSettings.isHydraulicErosionLayerModified)
                {
                    ApplicationHydraulicErosionLayer();
                }
                else
                {
                    HydraulicErodeGenerate();
                }
            }
        }


        /// <summary>
        /// Manually changes the TerrainData of the terrain to the currentTerrainData stored in general settings.
        /// </summary>
        public void ManualChangeTerrainData()
        {
            if (generalSettings.currentTerrainData == null)
            {
                Debug.LogError("TerrainData is missing.");
                return;
            }

            // Assign the currentTerrainData to the terrain and TerrainCollider
            terrain.terrainData = generalSettings.currentTerrainData;
            terrain.GetComponent<TerrainCollider>().terrainData = generalSettings.currentTerrainData;
        }
        
        /// <summary>
        /// Creates and saves empty TerrainData based on default settings and properties of the associated Terrain.
        /// </summary>
        public void CreateAndSaveEmptyTerrainData()
        {
            // Create a new empty TerrainData instance
            TerrainData newTerrainData = new TerrainData();

            // Get the default Terrain object
            Terrain defaultTerrain = TerraForgeGlobalSettings.Instance.defaultTerraForgeTerrain.GetComponent<Terrain>();

            // Access the existing TerrainData from the default Terrain
            TerrainData existingTerrainData = defaultTerrain.terrainData;

            // Copy properties from the existing TerrainData to the new TerrainData
            newTerrainData.heightmapResolution = existingTerrainData.heightmapResolution;
            newTerrainData.size = existingTerrainData.size;
            newTerrainData.alphamapResolution = existingTerrainData.alphamapResolution;
            newTerrainData.baseMapResolution = existingTerrainData.baseMapResolution;
            newTerrainData.SetDetailResolution(existingTerrainData.detailResolution, existingTerrainData.detailResolutionPerPatch);
            newTerrainData.terrainLayers = existingTerrainData.terrainLayers;

            // Set heights of the new TerrainData to match the existing one
            newTerrainData.SetHeights(0, 0, existingTerrainData.GetHeights(0, 0, existingTerrainData.heightmapResolution, existingTerrainData.heightmapResolution));

            // Assign the new TerrainData to the terrain
            terrain.terrainData = newTerrainData;

            // Update the currentTerrainData reference in general settings
            generalSettings.currentTerrainData = terrain.terrainData;

            // Update the TerrainCollider with the new TerrainData
            terrain.GetComponent<TerrainCollider>().terrainData = generalSettings.currentTerrainData;

            // Refresh generation for all layers
            RefreshGenerationAllLayers();

            // Create and save the asset if saveTerrainData is true
            if (generalSettings.saveTerrainData)
            {
    #if UNITY_EDITOR
                SaveTerrainDataEditor(newTerrainData);
    #else
                SaveTerrainDataRuntime(newTerrainData);
    #endif
            }
        }

        /// <summary>
        /// Creates a clone of the current TerrainData and sets it to the terrain, optionally saving it as an asset.
        /// </summary>
        public void CreateAndSetCloneTerrainData()
        {
            // Check if the terrain component exists
            if (terrain == null)
            {
                // Attempt to get the terrain component from the current GameObject
                terrain = this.GetComponent<Terrain>();

                // Check again if the terrain component exists
                if (terrain == null)
                {
                    Debug.LogWarning("Terrain component not found. Please ensure it is available on the GameObject!");
                    return;
                }
            }

            TerrainData existingTerrainData = terrain.terrainData;

            if (existingTerrainData == null)
            {
                Debug.LogError("TerrainData is missing.");
                return;
            }

            TerrainData newTerrainData = new TerrainData();

            // Copy properties from the existing TerrainData to the new TerrainData
            newTerrainData.heightmapResolution = existingTerrainData.heightmapResolution;
            newTerrainData.size = existingTerrainData.size;
            newTerrainData.alphamapResolution = existingTerrainData.alphamapResolution;
            newTerrainData.baseMapResolution = existingTerrainData.baseMapResolution;
            newTerrainData.SetDetailResolution(existingTerrainData.detailResolution, existingTerrainData.detailResolutionPerPatch);
            newTerrainData.terrainLayers = existingTerrainData.terrainLayers;

            newTerrainData.SetHeights(0, 0, existingTerrainData.GetHeights(0, 0, existingTerrainData.heightmapResolution, existingTerrainData.heightmapResolution));

            // Assign the new TerrainData to the terrain
            terrain.terrainData = newTerrainData;

            // Update the currentTerrainData reference in general settings
            generalSettings.currentTerrainData = terrain.terrainData;

            // Update the TerrainCollider with the new TerrainData
            terrain.GetComponent<TerrainCollider>().terrainData = generalSettings.currentTerrainData;

            // Save the new TerrainData
            if (generalSettings.saveTerrainData)
            {
    #if UNITY_EDITOR
                SaveTerrainDataEditor(newTerrainData);
    #else
                SaveTerrainDataRuntime(newTerrainData);
    #endif
            }
        }

        /// <summary>
        /// Saves the terrain data in the editor environment.
        /// </summary>
        /// <param name="newTerrainData">The new terrain data to be saved.</param>
        private void SaveTerrainDataEditor(TerrainData newTerrainData)
        {
    #if UNITY_EDITOR
            string path = TerraForgeGlobalSettings.Instance.editorPathToSaveTerrainData;

            // Ensure the path ends with a forward slash
            if (!path.EndsWith("/"))
            {
                path += "/";
            }

            // Generate a unique asset name
            string assetName = "NewTerrainData.asset";
            string assetPath = path + assetName;

            int index = 1;
            while (AssetDatabase.LoadAssetAtPath<TerrainData>(assetPath) != null)
            {
                assetName = $"NewTerrainData_{index}.asset";
                assetPath = path + assetName;
                index++;
            }

            // Create and save the asset
            AssetDatabase.CreateAsset(newTerrainData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
    #endif
    }

        /// <summary>
        /// Saves the terrain data at runtime to a specified path.
        /// </summary>
        /// <param name="newTerrainData">The TerrainData object to be saved.</param>
        private void SaveTerrainDataRuntime(TerrainData newTerrainData)
        {
            // Path where the terrain data will be saved
            string path = Application.persistentDataPath + "/SavedTerrainData/";

            // Ensure the directory exists
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Generate a unique file name
            string fileName = "NewTerrainData.dat";
            generalSettings.currentTerrainDataFileRuntimePath = path + fileName;

            int index = 1;
            while (File.Exists(generalSettings.currentTerrainDataFileRuntimePath))
            {
                fileName = $"NewTerrainData_{index}.dat";
                generalSettings.currentTerrainDataFileRuntimePath = path + fileName;
                index++;
            }

            try
            {
                using (FileStream fs = new FileStream(generalSettings.currentTerrainDataFileRuntimePath, FileMode.Create))
                {
                    BinaryWriter writer = new BinaryWriter(fs);

                    // Save heightmap
                    int heightmapResolution = newTerrainData.heightmapResolution;
                    writer.Write(heightmapResolution);
                    float[,] heights = newTerrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);
                    for (int y = 0; y < heightmapResolution; y++)
                    {
                        for (int x = 0; x < heightmapResolution; x++)
                        {
                            writer.Write(heights[y, x]);
                        }
                    }

                    // Save detail layers
                    int detailResolution = newTerrainData.detailResolution;
                    writer.Write(detailResolution);
                    int detailLayerCount = newTerrainData.detailPrototypes.Length;
                    writer.Write(detailLayerCount);
                    for (int layer = 0; layer < detailLayerCount; layer++)
                    {
                        int[,] details = newTerrainData.GetDetailLayer(0, 0, detailResolution, detailResolution, layer);
                        for (int y = 0; y < detailResolution; y++)
                        {
                            for (int x = 0; x < detailResolution; x++)
                            {
                                writer.Write(details[y, x]);
                            }
                        }
                    }

                    // Save tree instances
                    TreeInstance[] trees = newTerrainData.treeInstances;
                    writer.Write(trees.Length);
                    foreach (TreeInstance tree in trees)
                    {
                        writer.Write(tree.position.x);
                        writer.Write(tree.position.y);
                        writer.Write(tree.position.z);
                        writer.Write(tree.widthScale);
                        writer.Write(tree.heightScale);
                        writer.Write(tree.color.r);
                        writer.Write(tree.color.g);
                        writer.Write(tree.color.b);
                        writer.Write(tree.color.a);
                        writer.Write(tree.lightmapColor.r);
                        writer.Write(tree.lightmapColor.g);
                        writer.Write(tree.lightmapColor.b);
                        writer.Write(tree.lightmapColor.a);
                        writer.Write(tree.prototypeIndex);
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Log error if saving fails
                Debug.LogError($"Failed to save TerrainData asset at: {generalSettings.currentTerrainDataFileRuntimePath}. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes the current TerrainData asset.
        /// </summary>
        public void DeleteCurrentTerrainData()
        {
            // Check if there is a TerrainData to delete
            if (generalSettings.currentTerrainData == null)
            {
                Debug.LogWarning("No TerrainData to delete.");
                return;
            }

            // Check if running in the Unity Editor
    #if UNITY_EDITOR
            DeleteTerrainDataEditor(generalSettings.currentTerrainData);
    #else
            DeleteTerrainDataRuntime();
    #endif

            // Clear the currentTerrainData reference
            generalSettings.currentTerrainData = null;
        }

        /// <summary>
        /// Deletes the specified TerrainData asset in the Unity Editor.
        /// </summary>
        /// <param name="terrainData">The TerrainData asset to delete.</param>
        private void DeleteTerrainDataEditor(TerrainData terrainData)
        {
    #if UNITY_EDITOR
            // Get the path of the TerrainData asset
            string assetPath = AssetDatabase.GetAssetPath(terrainData);

            // Check if the asset path is valid
            if (!string.IsNullOrEmpty(assetPath))
            {
                // Delete the asset
                bool success = AssetDatabase.DeleteAsset(assetPath);
                if (!success)
                {
                    // Log error if deletion fails
                    Debug.LogError($"Failed to delete TerrainData asset at: {assetPath}");
                }
            }
            else
            {
                // Log warning for invalid asset path
                Debug.LogWarning("Invalid asset path. Make sure the TerrainData is a valid asset in the project.");
            }
    #endif
        }

        /// <summary>
        /// Deletes the specified TerrainData file at runtime.
        /// </summary>
        private void DeleteTerrainDataRuntime()
        {
        if (File.Exists(generalSettings.currentTerrainDataFileRuntimePath))
        {
            try
            {
                File.Delete(generalSettings.currentTerrainDataFileRuntimePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete TerrainData file at: {generalSettings.currentTerrainDataFileRuntimePath}. Exception: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"TerrainData file does not exist at: {generalSettings.currentTerrainDataFileRuntimePath}");
        }
        }

        /// <summary>
        /// Blends the terrain with the terrain on the right side.
        /// </summary>
        public void BlendTerrains_Right()
        {
            // Check if the contact height generator for the right side is assigned
            if (contactHeightsGenerators.contactHeightsGenerator_Right.heightsGenerator == null)
            {
                return;
            }

            TerrainData terrainData1 = terrain.terrainData;
            TerrainData terrainData2 = contactHeightsGenerators.contactHeightsGenerator_Right.heightsGenerator.terrain.terrainData;

            // Check if both terrain data are available
            if (terrainData1 == null || terrainData2 == null)
            {
                Debug.LogError("Terrain data is missing.");
                return;
            }

            // Get the heightmap resolutions
            int resolutionX1 = terrainData1.heightmapResolution;
            int resolutionZ1 = terrainData1.heightmapResolution;
            int resolutionX2 = terrainData2.heightmapResolution;
            int resolutionZ2 = terrainData2.heightmapResolution;

            // Check if the resolutions match
            if (resolutionX1 != resolutionX2 || resolutionZ1 != resolutionZ2)
            {
                Debug.LogError("Terrain heightmap resolutions do not match.");
                return;
            }

            // Calculate transition samples based on transition width
            float transitionWidth = contactHeightsGenerators.contactHeightsGenerator_Right.transitionWidth;
            
            if (transitionWidth > terrainData1.size.x/2)
            {
                transitionWidth = transitionWidth / (1000f/terrainData1.size.x);
            }

            if (contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending)
            {
                transitionWidth = terrainData1.size.x/2;
            }

            int transitionSamples = Mathf.RoundToInt(transitionWidth / terrainData1.size.x * resolutionX1);

            // Get heightmaps
            float[,] heightmap1 = terrainData1.GetHeights(0, 0, resolutionX1, resolutionZ1);
            float[,] heightmap2 = terrainData2.GetHeights(0, 0, resolutionX2, resolutionZ2);

            // Blend the vertices from the edge towards the center with a smooth transition
            for (int i = 0; i <= transitionSamples; i++)
            {
                int x1 = resolutionX1 - 1 - i; // positions moving from edge to center on the first terrain
                int x2 = i; // positions moving from edge to center on the second terrain

                if (x1 < 0 || x2 >= resolutionX2) break;

                int z_Limits_Start = 0;
                int z_Limits_End = resolutionZ1;

                if (contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending && contactHeightsGenerators.contactHeightsGenerator_Right.bothHalves)
                {
                    if (contactHeightsGenerators.contactHeightsGenerator_Right.bothHalves)
                    {
                        z_Limits_Start = i;
                        z_Limits_End = resolutionZ1 - i;
                    }
                    else if (contactHeightsGenerators.contactHeightsGenerator_Right.firstHalf)
                    {
                        z_Limits_Start = i;
                        z_Limits_End = resolutionZ1;
                    }
                    else
                    {
                        z_Limits_Start = 0;
                        z_Limits_End = resolutionZ1 - i;
                    }
                }

                for (int z = z_Limits_Start; z < z_Limits_End; z++) // Adjust z value to create a diagonal transition
                {
                    float blendedHeight = default(float);

                    // Calculate blend factor based on distance from the edge
                    if (i == 0)
                    {
                        // Calculate blended height
                        blendedHeight = Mathf.Lerp(heightmap1[x1, z], heightmap2[x2, z], 1.0f);
                    }
                    else
                    {
                        float blendFactor = Mathf.SmoothStep(0, 1, (transitionSamples - i) / (float)transitionSamples);

                        if (contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending)
                        {
                            if (z <= transitionSamples && (contactHeightsGenerators.contactHeightsGenerator_Right.bothHalves || contactHeightsGenerators.contactHeightsGenerator_Right.firstHalf))
                            {
                                blendFactor = Mathf.SmoothStep(0, 1, (z - i) / (float)z);
                            }

                            if (z > transitionSamples && (contactHeightsGenerators.contactHeightsGenerator_Right.bothHalves || contactHeightsGenerators.contactHeightsGenerator_Right.secondHalf))
                            {
                                blendFactor = Mathf.SmoothStep(0, 1, ((transitionSamples * 2 - z) - i) / (float)(transitionSamples * 2 - z));
                            }
                        }

                        // Calculate blended height
                        blendedHeight = Mathf.Lerp(heightmap1[x1, z], heightmap2[x2, z], blendFactor * contactHeightsGenerators.contactHeightsGenerator_Right.transitionStrength);
                    }


                    // Apply blended height to the first terrain
                    heightmap1[x1, z] = blendedHeight;
                }
            }

            // Apply the modified heightmaps back to the terrains
            terrainData1.SetHeights(0, 0, heightmap1);

            // Refresh the terrains
            terrain.Flush();
        }

        /// <summary>
        /// Blends the terrain with the terrain on the left side.
        /// </summary>
        public void BlendTerrains_Left()
        {
            // Check if the contact height generator for the left side is assigned
            if (contactHeightsGenerators.contactHeightsGenerator_Left.heightsGenerator == null)
            {
                return;
            }

            TerrainData terrainData1 = terrain.terrainData;
            TerrainData terrainData2 = contactHeightsGenerators.contactHeightsGenerator_Left.heightsGenerator.terrain.terrainData;

            // Check if both terrain data are available
            if (terrainData1 == null || terrainData2 == null)
            {
                Debug.LogError("Terrain data is missing.");
                return;
            }

            // Get the heightmap resolutions
            int resolutionX1 = terrainData1.heightmapResolution;
            int resolutionZ1 = terrainData1.heightmapResolution;
            int resolutionX2 = terrainData2.heightmapResolution;
            int resolutionZ2 = terrainData2.heightmapResolution;

            // Check if the resolutions match
            if (resolutionX1 != resolutionX2 || resolutionZ1 != resolutionZ2)
            {
                Debug.LogError("Terrain heightmap resolutions do not match.");
                return;
            }

            // Calculate transition samples based on transition width
            float transitionWidth = contactHeightsGenerators.contactHeightsGenerator_Left.transitionWidth;
            
            if (transitionWidth > terrainData1.size.x/2)
            {
                transitionWidth = transitionWidth / (1000f/terrainData1.size.x);
            }

            if (contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending)
            {
                transitionWidth = terrainData1.size.x/2;
            }

            int transitionSamples = Mathf.RoundToInt(transitionWidth / terrainData1.size.x * resolutionX1);

            // Get heightmaps
            float[,] heightmap1 = terrainData1.GetHeights(0, 0, resolutionX1, resolutionZ1);
            float[,] heightmap2 = terrainData2.GetHeights(0, 0, resolutionX2, resolutionZ2);

            // Blend the vertices from the edge towards the center with a smooth transition
            for (int i = 0; i <= transitionSamples; i++)
            {
                int x1 = i; // positions moving from edge to center on the first terrain
                int x2 = resolutionX1 - 1 - i; // positions moving from edge to center on the second terrain

                if (x1 < 0 || x2 >= resolutionX2) break;

                int z_Limits_Start = 0;
                int z_Limits_End = resolutionZ1;

                if (contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending && contactHeightsGenerators.contactHeightsGenerator_Left.bothHalves)
                {
                    if (contactHeightsGenerators.contactHeightsGenerator_Left.bothHalves)
                    {
                        z_Limits_Start = i;
                        z_Limits_End = resolutionZ1 - i;
                    }
                    else if (contactHeightsGenerators.contactHeightsGenerator_Left.firstHalf)
                    {
                        z_Limits_Start = i;
                        z_Limits_End = resolutionZ1;
                    }
                    else
                    {
                        z_Limits_Start = 0;
                        z_Limits_End = resolutionZ1 - i;
                    }
                }

                for (int z = z_Limits_Start; z < z_Limits_End; z++) // Adjust z value to create a diagonal transition
                {
                    float blendedHeight = default(float);

                    // Calculate blend factor based on distance from the edge
                    if (i == 0)
                    {
                        // Calculate blended height
                        blendedHeight = Mathf.Lerp(heightmap1[x1, z], heightmap2[x2, z], 1.0f);
                    }
                    else
                    {
                        float blendFactor = Mathf.SmoothStep(0, 1, (transitionSamples - i) / (float)transitionSamples);

                        if (contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending)
                        {
                            if (z <= transitionSamples && (contactHeightsGenerators.contactHeightsGenerator_Left.bothHalves || contactHeightsGenerators.contactHeightsGenerator_Left.firstHalf))
                            {
                                blendFactor = Mathf.SmoothStep(0, 1, (z - i) / (float)z);
                            }

                            if (z > transitionSamples && (contactHeightsGenerators.contactHeightsGenerator_Left.bothHalves || contactHeightsGenerators.contactHeightsGenerator_Left.secondHalf))
                            {
                                blendFactor = Mathf.SmoothStep(0, 1, ((transitionSamples * 2 - z) - i) / (float)(transitionSamples * 2 - z));
                            }
                        }

                        // Calculate blended height
                        blendedHeight = Mathf.Lerp(heightmap1[x1, z], heightmap2[x2, z], blendFactor * contactHeightsGenerators.contactHeightsGenerator_Left.transitionStrength);
                    }


                    // Apply blended height to the first terrain
                    heightmap1[x1, z] = blendedHeight;
                }
            }

            // Apply the modified heightmaps back to the terrains
            terrainData1.SetHeights(0, 0, heightmap1);

            // Refresh the terrains
            terrain.Flush();
        }

        /// <summary>
        /// Blends the terrain with the terrain on the top side.
        /// </summary>
        public void BlendTerrains_Top()
        {
            // Check if the contact height generator for the top side is assigned
            if (contactHeightsGenerators.contactHeightsGenerator_Top.heightsGenerator == null)
            {
                return;
            }

            TerrainData terrainData1 = terrain.terrainData;
            TerrainData terrainData2 = contactHeightsGenerators.contactHeightsGenerator_Top.heightsGenerator.terrain.terrainData;

            // Check if both terrain data are available
            if (terrainData1 == null || terrainData2 == null)
            {
                Debug.LogError("Terrain data is missing.");
                return;
            }

            // Get the heightmap resolutions
            int resolutionX1 = terrainData1.heightmapResolution;
            int resolutionZ1 = terrainData1.heightmapResolution;
            int resolutionX2 = terrainData2.heightmapResolution;
            int resolutionZ2 = terrainData2.heightmapResolution;

            // Check if the resolutions match
            if (resolutionX1 != resolutionX2 || resolutionZ1 != resolutionZ2)
            {
                Debug.LogError("Terrain heightmap resolutions do not match.");
                return;
            }

            if (terrainData1.size.x != terrainData2.size.x)
            {
                Debug.LogError("The size of the reliefs do not match. Edge blending is interrupted!");
                return;
            }

            // Calculate transition samples based on transition width
            float transitionWidth = contactHeightsGenerators.contactHeightsGenerator_Top.transitionWidth;
            
            if (transitionWidth > terrainData1.size.x/2)
            {
                transitionWidth = transitionWidth / (1000f/terrainData1.size.x);
            }

            if (contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending)
            {
                transitionWidth = terrainData1.size.x/2;
            }

            int transitionSamples = Mathf.RoundToInt(transitionWidth / terrainData1.size.x * resolutionX1);
            // Get heightmaps
            float[,] heightmap1 = terrainData1.GetHeights(0, 0, resolutionX1, resolutionZ1);
            float[,] heightmap2 = terrainData2.GetHeights(0, 0, resolutionX2, resolutionZ2);

            // Blend the vertices from the edge towards the center with a smooth transition
            for (int i = 0; i <= transitionSamples; i++)
            {
                int x1 = i; // positions moving from edge to center on the first terrain
                int x2 = resolutionX1 - 1 - i; // positions moving from edge to center on the second terrain

                if (x1 < 0 || x2 >= resolutionX2) break;
                
                int z_Limits_Start = 0;
                int z_Limits_End = resolutionZ1;
                
                if (contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending && contactHeightsGenerators.contactHeightsGenerator_Top.bothHalves)
                {
                    if (contactHeightsGenerators.contactHeightsGenerator_Top.bothHalves)
                    {
                        z_Limits_Start = i;
                        z_Limits_End = resolutionZ1 - i;
                    }
                    else if (contactHeightsGenerators.contactHeightsGenerator_Top.firstHalf)
                    {
                        z_Limits_Start = i;
                        z_Limits_End = resolutionZ1;
                    }
                    else
                    {
                        z_Limits_Start = 0;
                        z_Limits_End = resolutionZ1 - i;
                    }
                }
                
                for (int z = z_Limits_Start; z < z_Limits_End; z++) // Adjust z value to create a diagonal transition
                {
                    float blendedHeight = default(float);

                    // Calculate blend factor based on distance from the edge
                    if (i == 0)
                    {
                        // Calculate blended height
                        blendedHeight = Mathf.Lerp(heightmap1[z, x1], heightmap2[z, x2], 1.0f);
                    }
                    else
                    {
                        float blendFactor = Mathf.SmoothStep(0, 1, (transitionSamples - i) / (float)transitionSamples);
                        
                        if (contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending)
                        {
                            if (z <= transitionSamples && (contactHeightsGenerators.contactHeightsGenerator_Top.bothHalves || contactHeightsGenerators.contactHeightsGenerator_Top.firstHalf))
                            {
                                blendFactor = Mathf.SmoothStep(0, 1, (z - i) / (float)z);
                            }
                            
                            if (z > transitionSamples && (contactHeightsGenerators.contactHeightsGenerator_Top.bothHalves || contactHeightsGenerators.contactHeightsGenerator_Top.secondHalf))
                            {
                                blendFactor = Mathf.SmoothStep(0, 1, ((transitionSamples*2-z) - i) / (float)(transitionSamples*2-z));
                            }
                        }

                        // Calculate blended height
                        blendedHeight = Mathf.Lerp(heightmap1[z, x1], heightmap2[z, x2], blendFactor * contactHeightsGenerators.contactHeightsGenerator_Top.transitionStrength);
                    }
                    

                    // Apply blended height to the first terrain
                    heightmap1[z, x1] = blendedHeight;
                }
            }

            // Apply the modified heightmaps back to the terrains
            terrainData1.SetHeights(0, 0, heightmap1);

            // Refresh the terrains
            terrain.Flush();
        }

        /// <summary>
        /// Blends the terrain with the terrain on the bottom side.
        /// </summary>
        public void BlendTerrains_Bottom()
        {
            // Check if the contact height generator for the bottom side is assigned
            if (contactHeightsGenerators.contactHeightsGenerator_Bottom.heightsGenerator == null)
            {
                return;
            }

            TerrainData terrainData1 = terrain.terrainData;
            TerrainData terrainData2 = contactHeightsGenerators.contactHeightsGenerator_Bottom.heightsGenerator.terrain.terrainData;

            // Check if both terrain data are available
            if (terrainData1 == null || terrainData2 == null)
            {
                Debug.LogError("Terrain data is missing.");
                return;
            }

            // Get the heightmap resolutions
            int resolutionX1 = terrainData1.heightmapResolution;
            int resolutionZ1 = terrainData1.heightmapResolution;
            int resolutionX2 = terrainData2.heightmapResolution;
            int resolutionZ2 = terrainData2.heightmapResolution;

            // Check if the resolutions match
            if (resolutionX1 != resolutionX2 || resolutionZ1 != resolutionZ2)
            {
                Debug.LogError("Terrain heightmap resolutions do not match.");
                return;
            }

            // Calculate transition samples based on transition width
            float transitionWidth = contactHeightsGenerators.contactHeightsGenerator_Bottom.transitionWidth;
            
            if (transitionWidth > terrainData1.size.x/2)
            {
                transitionWidth = transitionWidth / (1000f/terrainData1.size.x);
            }

            if (contactHeightsGenerators.contactHeightsGenerator_Bottom.diagonalBlending)
            {
                transitionWidth = terrainData1.size.x/2;
            }
            

            int transitionSamples = Mathf.RoundToInt(transitionWidth / terrainData1.size.x * resolutionX1);
            // Get heightmaps
            float[,] heightmap1 = terrainData1.GetHeights(0, 0, resolutionX1, resolutionZ1);
            float[,] heightmap2 = terrainData2.GetHeights(0, 0, resolutionX2, resolutionZ2);

            // Blend the vertices from the edge towards the center with a smooth transition
            for (int i = 0; i <= transitionSamples; i++)
            {
                int x1 = resolutionX1 - 1 - i; // positions moving from edge to center on the first terrain
                int x2 = i;// positions moving from edge to center on the second terrain

                if (x1 < 0 || x2 >= resolutionX2) break;
                
                int z_Limits_Start = 0;
                int z_Limits_End = resolutionZ1;
                
                if (contactHeightsGenerators.contactHeightsGenerator_Bottom.diagonalBlending && contactHeightsGenerators.contactHeightsGenerator_Bottom.bothHalves)
                {
                    if (contactHeightsGenerators.contactHeightsGenerator_Bottom.bothHalves)
                    {
                        z_Limits_Start = i;
                        z_Limits_End = resolutionZ1 - i;
                    }
                    else if (contactHeightsGenerators.contactHeightsGenerator_Bottom.firstHalf)
                    {
                        z_Limits_Start = i;
                        z_Limits_End = resolutionZ1;
                    }
                    else
                    {
                        z_Limits_Start = 0;
                        z_Limits_End = resolutionZ1 - i;
                    }
                }
                
                for (int z = z_Limits_Start; z < z_Limits_End; z++) // Adjust z value to create a diagonal transition
                {
                    float blendedHeight = default(float);

                    // Calculate blend factor based on distance from the edge
                    if (i == 0)
                    {
                        // Calculate blended height
                        blendedHeight = Mathf.Lerp(heightmap1[z, x1], heightmap2[z, x2], 1.0f);
                    }
                    else
                    {
                        float blendFactor = Mathf.SmoothStep(0, 1, (transitionSamples - i) / (float)transitionSamples);
                        
                        if (contactHeightsGenerators.contactHeightsGenerator_Bottom.diagonalBlending)
                        {
                            if (z <= transitionSamples && (contactHeightsGenerators.contactHeightsGenerator_Bottom.bothHalves || contactHeightsGenerators.contactHeightsGenerator_Bottom.firstHalf))
                            {
                                blendFactor = Mathf.SmoothStep(0, 1, (z - i) / (float)z);
                            }
                            
                            if (z > transitionSamples && (contactHeightsGenerators.contactHeightsGenerator_Bottom.bothHalves || contactHeightsGenerators.contactHeightsGenerator_Bottom.secondHalf))
                            {
                                blendFactor = Mathf.SmoothStep(0, 1, ((transitionSamples*2-z) - i) / (float)(transitionSamples*2-z));
                            }
                        }

                        // Calculate blended height
                        blendedHeight = Mathf.Lerp(heightmap1[z, x1], heightmap2[z, x2], blendFactor * contactHeightsGenerators.contactHeightsGenerator_Bottom.transitionStrength);
                    }
                    

                    // Apply blended height to the first terrain
                    heightmap1[z, x1] = blendedHeight;
                }
            }

            // Apply the modified heightmaps back to the terrains
            terrainData1.SetHeights(0, 0, heightmap1);

            // Refresh the terrains
            terrain.Flush();
        }
    }
}