// TerraForgeEditorUtilities.cs
// Utility class for TerraForge editor-related functionalities.
// TerraForge 2.0.0

using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEditor.Experimental.SceneManagement;
#endif
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.TerraForgeEditor
{
    /// <summary>
    /// Utility class for TerraForge editor-related functionalities.
    /// </summary>
    public static class TerraForgeEditorUtilities
    {
        #if UNITY_EDITOR
            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to perform this action? It may take a while" - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_1()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to perform this action? It may take a while.",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to perform this action? Higher resolution (> 513) may take some time to generate the terrain!" - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_2()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to perform this action? Higher resolution (> 513) may take some time to generate the terrain!",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to reduce the resolution for this terrain? It will not be possible to undo the action." - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_3()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to reduce the resolution for this terrain? It will not be possible to undo the action.",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to keep the changes in this biome? It will not be possible to undo the action." - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_4()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to keep the changes in this biome? It will not be possible to undo the action.",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to make an auto-assignment for Global parameters? It will not be possible to undo the action." - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_5()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to make an auto-assignment for Global parameters? It will not be possible to undo the action.",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to change the TerrainData for the current terrain?" - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_6()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to change the TerrainData for the current terrain?",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to delete the Grid with the corresponding TerrainData files? It will not be possible to undo the action." - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_7()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to delete the Grid with the corresponding TerrainData files? It will not be possible to undo the action.",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to perform this action? Higher resolutions (> 513) can take a long time to create a terrain grid!" - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_8()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to perform this action? Higher resolutions (> 513) can take a long time to create a terrain grid!",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "The saving of the TerrainData file will now be disabled. Optional: Do you want to delete the currently saved TerrainData file?" - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_9()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "The saving of the TerrainData file will now be disabled. Optional: Do you want to delete the currently saved TerrainData file?",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to perform this action? If the number of cells is large (> 9), it may take longer to create a relief grid!" - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_10()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to perform this action? If the number of cells is large (> 9), it may take longer to create a relief grid!",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to perform this action? At high resolutions (> 513) it can take a long time to refresh the resolution of the terrain grid!" - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_11()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to perform this action? At high resolutions (> 513) it can take a long time to refresh the resolution of the terrain grid!",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to perform this action? If the number of cells is large (> 9), it may take longer to refresh a relief grid!" - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_12()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to perform this action? If the number of cells is large (> 9), it may take longer to refresh a relief grid!",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to perform this action? The previously generated grid (with higher resolution) will be deleted!" - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_13()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to perform this action? The previously generated grid (with higher resolution) will be deleted!",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to perform this action? At high resolutions (> 513) it can take a long time to refresh the resolution of the terrain!" - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_14()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to perform this action? At high resolutions (> 513) it can take a long time to refresh the resolution of the terrain!",
                    "Yes",
                    "No"
                );
            }

            /// <summary>
            /// Displays a confirmation dialog: "Are you sure you want to update the generation of all layers? Usually takes longer than normal generation." - "Yes"/"No"
            /// </summary>
            /// <returns>True if the user confirms, false otherwise.</returns>
            public static bool ShowConfirmationDialog_15()
            {
                // Display confirmation dialog with custom message and options
                return EditorUtility.DisplayDialog(
                    "Confirmation",
                    "Are you sure you want to update the generation of all layers? Usually takes longer than normal generation.",
                    "Yes",
                    "No"
                );
            }


            /// <summary>
            /// Closes the current prefab stage.
            /// </summary>
            public static void ClosePrefabView()
            {
                // Check if a prefab stage is currently open
                if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                {
                    // Get the current prefab stage
                    PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                    
                    // Close the current prefab stage
                    if (prefabStage != null)
                    {
                        StageUtility.GoBackToPreviousStage();
                    }
                }
                else
                {
                    // Log a warning if no prefab stage is open
                    Debug.LogWarning("No prefab stage is currently open.");
                }
            }

            /// <summary>
            /// Creates a TerraForge terrain object.
            /// </summary>
            /// <param name="menuCommand">The menu command that triggered this action.</param>
            [MenuItem("GameObject/TerraForge 2/TerraForge Terrain", false, 10)]
            private static void CreateTerraForgeTerrain(MenuCommand menuCommand)
            {
                // Ensure the TerraForgeGlobalSettings instance is valid
                if (TerraForgeGlobalSettings.Instance == null)
                {
                    Debug.LogError("TerraForgeGlobalSettings.Instance is null.");
                    return;
                }

                // Get the default TerraForge terrain prefab
                GameObject defaultTerrainPrefab = TerraForgeGlobalSettings.Instance.defaultTerraForgeTerrain;
                if (defaultTerrainPrefab == null)
                {
                    Debug.LogError("TerraForgeGlobalSettings.Instance.defaultTerraForgeTerrain is null.");
                    return;
                }

                // Instantiate the default TerraForge terrain prefab
                GameObject terraForgeTerrain = Object.Instantiate(defaultTerrainPrefab);
                terraForgeTerrain.name = "TerraForge Terrain";
                terraForgeTerrain.GetComponent<TerraForgeTerrainGenerator>().CreateAndSetCloneTerrainData();
                terraForgeTerrain.GetComponent<TerraForgeTerrainPainter>().RepaintAll();

                // Ensure it gets parented correctly to the selected GameObject in the hierarchy view
                GameObjectUtility.SetParentAndAlign(terraForgeTerrain, menuCommand.context as GameObject);

                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(terraForgeTerrain, "Create TerraForge Terrain");

                // Select the newly created GameObject
                Selection.activeObject = terraForgeTerrain;
            }

            /// <summary>
            /// Creates a TerraForge terrain grid object.
            /// </summary>
            /// <param name="menuCommand">The menu command that triggered this action.</param>
            [MenuItem("GameObject/TerraForge 2/TerraForge Terrains Grid", false, 10)]
            private static void CreateTerraForgeTerrainGrid(MenuCommand menuCommand)
            {
                // Ensure the TerraForgeGlobalSettings instance is valid
                if (TerraForgeGlobalSettings.Instance == null)
                {
                    Debug.LogError("TerraForgeGlobalSettings.Instance is null.");
                    return;
                }

                // Get the default TerraForge terrain grid prefab
                GameObject defaultTerrainGridPrefab = TerraForgeGlobalSettings.Instance.defaultTerraForgeTerrainGrid;
                if (defaultTerrainGridPrefab == null)
                {
                    Debug.LogError("TerraForgeGlobalSettings.Instance.defaultTerraForgeTerrainGrid is null.");
                    return;
                }

                // Instantiate the default TerraForge terrain grid prefab
                GameObject terraForgeTerrainGrid = Object.Instantiate(defaultTerrainGridPrefab);
                terraForgeTerrainGrid.name = "TerraForge Terrain Grid";

                // Ensure it gets parented correctly to the selected GameObject in the hierarchy view
                GameObjectUtility.SetParentAndAlign(terraForgeTerrainGrid, menuCommand.context as GameObject);

                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(terraForgeTerrainGrid, "Create TerraForge Terrain Grid");

                // Select the newly created GameObject
                Selection.activeObject = terraForgeTerrainGrid;
            }
        #endif
    }
}