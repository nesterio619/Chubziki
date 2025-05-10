// CleanUpTerrainModifiers.cs
// Editor window for cleaning up terrain modifiers in TerraForge.
// TerraForge 2.0.0

using UnityEngine;
using UnityEditor;
using System.IO;
using TerraForge2.Scripts.TerrainPainter;

/// <summary>
/// Editor window for cleaning up terrain modifiers in TerraForge.
/// </summary>
public class CleanUpTerrainModifiers : EditorWindow
{
    /// <summary>
    /// Shows the Clean Up Terrain Modifiers window.
    /// </summary>
    [MenuItem("Tools/TerraForge 2/Clean Up Terrain Modifiers")]
    public static void ShowWindow()
    {
        GetWindow<CleanUpTerrainModifiers>("Clean Up Terrain Modifiers");
    }

    private void OnGUI()
    {
        GUILayout.Label("Clean Up Terrain Modifiers", EditorStyles.boldLabel);

        if (GUILayout.Button("Clean Up"))
        {
            CleanUpUnassignedTerrainData();
        }
    }

    /// <summary>
    /// Deletes all instances of TerraForgeTerrainPainterModifier with unassigned or empty terrainData.
    /// </summary>
    private void CleanUpUnassignedTerrainData()
    {
        string[] assetGuids = AssetDatabase.FindAssets("t:TerraForgeTerrainPainterModifier");
        int deleteCount = 0;

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TerraForgeTerrainPainterModifier modifier = AssetDatabase.LoadAssetAtPath<TerraForgeTerrainPainterModifier>(assetPath);

            if (modifier != null && (modifier.terrainData == null || modifier.terrainData.heightmapResolution == 0))
            {
                AssetDatabase.DeleteAsset(assetPath);
                deleteCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Clean Up Completed", $"{deleteCount} unassigned or empty terrainData modifiers deleted.", "OK");
    }
}