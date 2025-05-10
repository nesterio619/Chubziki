using UnityEngine;
using System.Collections.Generic;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;

/// <summary>
/// Manages the runtime generation of terrain using TerraForge and biome settings.
/// </summary>
public class RuntimeTerrainGenerator : MonoBehaviour
{
    /// <summary>
    /// List of biome settings to choose from for terrain generation.
    /// </summary>
    [Tooltip("List of biome settings to choose from for terrain generation.")]
    public List<BiomeSettings> biomes;

    /// <summary>
    /// Reference to the TerraForge terrain generator.
    /// </summary>
    [Tooltip("Reference to the TerraForge terrain generator.")]
    public TerraForgeTerrainGenerator terrainGenerator;

    /// <summary>
    /// Interval in seconds between each terrain generation.
    /// </summary>
    [Tooltip("Interval in seconds between each terrain generation.")]
    public float generationInterval = 10f;

    /// <summary>
    /// Determines whether to use a falloff map for the terrain layers.
    /// </summary>
    [Tooltip("Determines whether to use a falloff map for the terrain layers.")]
    public bool useFalloffMap;

    /// <summary>
    /// Initializes the terrain generator and starts the generation process.
    /// </summary>
    void Start()
    {
        if (biomes.Count > 0 && terrainGenerator != null)
        {
            // Start the first generation immediately.
            GenerateTerrain();

            // Set up repeated terrain generation at the specified interval.
            InvokeRepeating(nameof(GenerateTerrain), generationInterval, generationInterval);
        }
        else
        {
            Debug.LogError("Please ensure biomes, terrain generator, and terrain game object are assigned.");
        }
    }

    /// <summary>
    /// Generates the terrain by selecting a random biome and applying its settings.
    /// </summary>
    void GenerateTerrain()
    {
        // Randomly select a biome from the list.
        int randomBiomeIndex = Random.Range(0, biomes.Count);
        ChangeBiome(randomBiomeIndex);

        if (useFalloffMap)
        {
            for (int i = 0; i < terrainGenerator.terrainLayers.Length; i++)
            {
                TerrainLayerSettings layer = terrainGenerator.terrainLayers[i];
                layer.useFalloffMap = true;
            }
        }

        // Generate the terrain.
        terrainGenerator.TerrainGenerate();
    }

    /// <summary>
    /// Changes the current biome settings to the selected biome.
    /// </summary>
    /// <param name="biomeIndex">Index of the biome to switch to.</param>
    public void ChangeBiome(int biomeIndex)
    {
        if (biomeIndex < 0 || biomeIndex >= biomes.Count)
        {
            Debug.LogError("Invalid biome index.");
            return;
        }

        BiomeSettings selectedBiome = biomes[biomeIndex];
        selectedBiome.ApplyingBiomeSettings(terrainGenerator, null, false, 15);
    }
}