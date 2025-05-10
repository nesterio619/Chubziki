// LoadBiomeSettingsWindow.cs
// Editor window for loading and applying biome settings in TerraForge.
// TerraForge 2.0.0

using UnityEngine;
using UnityEditor;
using TerraForge2.Scripts.Generators;

/// <summary>
/// Editor window for loading and applying biome settings in TerraForge.
/// </summary>
public class LoadBiomeSettingsWindow : EditorWindow
{
    /// <summary>
    /// The terrain generator to which the settings will be applied.
    /// </summary>
    [Tooltip("The terrain generator to which the settings will be applied.")]
    private TerraForgeTerrainGenerator terrainGenerator;

    /// <summary>
    /// The biome settings file to load the settings from.
    /// </summary>
    [Tooltip("The biome settings file to load the settings from.")]
    public BiomeSettings biomeSettings;

    /// <summary>
    /// Shows the Load Biome Settings window.
    /// </summary>
    [MenuItem("Tools/TerraForge 2/Load Biome Settings")]
    public static void ShowWindow()
    {
        GetWindow<LoadBiomeSettingsWindow>("Load Biome Settings");
    }

    private void OnGUI()
    {
        GUILayout.Label("Load Biome Settings", EditorStyles.boldLabel);

        terrainGenerator = (TerraForgeTerrainGenerator)EditorGUILayout.ObjectField("Terrain Generator", terrainGenerator, typeof(TerraForgeTerrainGenerator), true);
        biomeSettings = (BiomeSettings)EditorGUILayout.ObjectField("Biome Settings", biomeSettings, typeof(BiomeSettings), false);

        if (GUILayout.Button("Load and Apply Biome Settings"))
        {
            LoadAndApplyBiomeSettings();
        }
    }

    /// <summary>
    /// Loads the biome settings from the file and applies them to the terrain generator.
    /// </summary>
    private void LoadAndApplyBiomeSettings()
    {
        if (terrainGenerator == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Terrain Generator.", "OK");
            return;
        }

        if (biomeSettings == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Biome Settings file.", "OK");
            return;
        }

        // Apply the settings to the terrain generator
        terrainGenerator.generalSettings.terrainHeight = biomeSettings.terrainHeight;
        terrainGenerator.terrainLayers = new TerrainLayerSettings[biomeSettings.terrainLayers.Length];
        for (int i = 0; i < biomeSettings.terrainLayers.Length; i++)
        {
            terrainGenerator.terrainLayers[i] = new TerrainLayerSettings(biomeSettings.terrainLayers[i]);
        }
        terrainGenerator.hydraulicErosionLayerSettings = new HydraulicErosionLayerSettings(biomeSettings.hydraulicErosionLayerSettings);

        EditorUtility.SetDirty(terrainGenerator);
        EditorUtility.DisplayDialog("Success", "Biome Settings applied successfully.", "OK");
    }
}
