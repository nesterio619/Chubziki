// SaveBiomeSettingsWindow.cs
// Editor window for saving biome settings in TerraForge.
// TerraForge 2.0.0

using UnityEngine;
using UnityEditor;
using System.IO;
using TerraForge2.Scripts.Generators;

/// <summary>
/// Editor window for saving biome settings in TerraForge.
/// </summary>
public class SaveBiomeSettingsWindow : EditorWindow
{
    /// <summary>
    /// The terrain generator from which to save the settings.
    /// </summary>
    [Tooltip("The terrain generator from which to save the settings.")]
    private TerraForgeTerrainGenerator terrainGenerator;

    /// <summary>
    /// The name of the file to save the settings as.
    /// </summary>
    [Tooltip("The name of the file to save the settings as.")]
    private string fileName = "BiomeSettings.asset";

    /// <summary>
    /// The folder path where the settings will be saved.
    /// </summary>
    [Tooltip("The folder path where the settings will be saved.")]
    private string folderPath = "Assets/TerraForge 2/Demo/Profiles/Biome Settings Profiles/";

    private const string EditorPrefsKey = "SaveBiomeSettingsWindow_FolderPath";

    /// <summary>
    /// Shows the Save Biome Settings window.
    /// </summary>
    [MenuItem("Tools/TerraForge 2/Save Biome Settings")]
    public static void ShowWindow()
    {
        GetWindow<SaveBiomeSettingsWindow>("Save Biome Settings");
    }

    private void OnEnable()
    {
        folderPath = EditorPrefs.GetString(EditorPrefsKey, folderPath);
    }

    private void OnDisable()
    {
        EditorPrefs.SetString(EditorPrefsKey, folderPath);
    }

    private void OnGUI()
    {
        GUILayout.Label("Save Biome Settings", EditorStyles.boldLabel);

        terrainGenerator = (TerraForgeTerrainGenerator)EditorGUILayout.ObjectField("Terrain Generator", terrainGenerator, typeof(TerraForgeTerrainGenerator), true);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        GUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Save Folder");
        if (GUILayout.Button("Select Folder", GUILayout.Width(100)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", folderPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                folderPath = "Assets" + selectedPath.Replace(Application.dataPath, "");
                EditorPrefs.SetString(EditorPrefsKey, folderPath);
            }
        }
        GUILayout.Label(folderPath, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Save Biome Settings"))
        {
            SaveBiomeSettings();
        }
    }

    /// <summary>
    /// Saves the biome settings from the assigned terrain generator to a file.
    /// </summary>
    private void SaveBiomeSettings()
    {
        if (terrainGenerator == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a Terrain Generator.", "OK");
            return;
        }

        string savePath = Path.Combine(folderPath, fileName);

        if (File.Exists(savePath))
        {
            if (!EditorUtility.DisplayDialog("File Exists", "A file with this name already exists. Do you want to overwrite it?", "Yes", "No"))
            {
                return;
            }
        }

        BiomeSettings biomeSettings = ScriptableObject.CreateInstance<BiomeSettings>();
        biomeSettings.biomeName = terrainGenerator.name;
        biomeSettings.terrainHeight = terrainGenerator.generalSettings.terrainHeight;
        biomeSettings.terrainLayers = new TerrainLayerSettings[terrainGenerator.terrainLayers.Length];
        for (int i = 0; i < terrainGenerator.terrainLayers.Length; i++)
        {
            biomeSettings.terrainLayers[i] = new TerrainLayerSettings(terrainGenerator.terrainLayers[i]);
        }
        biomeSettings.hydraulicErosionLayerSettings = new HydraulicErosionLayerSettings(terrainGenerator.hydraulicErosionLayerSettings);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        AssetDatabase.CreateAsset(biomeSettings, savePath);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Success", "Biome Settings saved successfully.", "OK");
    }
}