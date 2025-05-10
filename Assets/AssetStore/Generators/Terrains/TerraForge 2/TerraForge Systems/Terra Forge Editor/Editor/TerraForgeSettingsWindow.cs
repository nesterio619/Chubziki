// TerraForgeSettingsWindow.cs
// Represents a window for managing global settings for TerraForge.
// TerraForge 2.0.0

using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
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
    /// Represents a window for managing global settings for TerraForge.
    /// </summary>
    public class TerraForgeSettingsWindow : EditorWindow
    {
        private TerraForgeGlobalSettings settings;

        private Texture2D banner;
        private Texture2D outlineTexture_2;

        // Menu item to show the TerraForge Global Settings window
        [MenuItem("Tools/TerraForge 2/Global Settings", false, 1)]
        public static void ShowWindow()
        {
            GetWindow<TerraForgeSettingsWindow>("TerraForge 2 Global Settings");
        }

        // Called when the window is enabled
        private void OnEnable() 
        {
            // Force reload settings each time the window is enabled
            banner = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TerraForge 2/TerraForge Systems/Textures Editor GUI/Banners/TerraForgeGlobalSettingsEditor_Banner.png");
            outlineTexture_2 = CreateOutlineTexture(10, 10, new Color(0.4f, 0.4f, 0.4f), new Color(0.2f, 0.2f, 0.2f));
            TerraForgeGlobalSettings.Reload();
            settings = TerraForgeGlobalSettings.Instance;
        }

        // GUI layout for the window
        private void OnGUI()
        {
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Settings asset not found. Please create one in the Resources folder.", MessageType.Error);
                return;
            }

            if (banner != null)
            {
                float aspectRatio = (float)banner.width / banner.height;
                float bannerWidth = EditorGUIUtility.currentViewWidth - 3;
                float bannerHeight = bannerWidth / aspectRatio;

                GUILayout.Label(banner, GUILayout.Width(bannerWidth), GUILayout.Height(bannerHeight));
            }
            else
            {
                EditorGUILayout.HelpBox("Banner image not found!", MessageType.Warning);
            }

            GUILayout.Space(10);

            GUILayout.BeginVertical(GetOutlineStyle_Generate());
            GUILayout.Space(3);
            EditorGUILayout.LabelField("Editor GUI settings", EditorStyles.boldLabel);
            GUILayout.Space(5);
            // Toggle for enabling confirmation
            GUILayout.BeginHorizontal();
            Undo.RecordObject(settings, "Toggle Caution and Confirmation Windows");
            settings.enableConfirmation = EditorGUILayout.Toggle(settings.enableConfirmation, GUILayout.Width(15));
            GUILayout.Label(new GUIContent("Enable Caution and Confirmation Windows", "Show warning and dialogue boxes. It is recommended to enable"));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(3);
            GUILayout.BeginVertical(GetOutlineStyle_Generate());
            GUILayout.Space(3);
            EditorGUILayout.LabelField("Generation settings", EditorStyles.boldLabel);
            GUILayout.Space(5);
            Undo.RecordObject(settings, "Delay Between Automatic Generating Operations");
            GUILayout.Label(new GUIContent("Delay Between Automatic Generating Operations", "Affects the delay between automatic generations (System.Threading.Tasks.Task.Delay(X))"));
            settings.delayBetweenAutomaticGeneratingOperations = EditorGUILayout.IntSlider(settings.delayBetweenAutomaticGeneratingOperations, 100, 500);
            GUILayout.Space(5);
            // Display and allow modification of the path string field
            settings.editorPathToSaveTerrainData = EditorGUILayout.TextField("Path to Save Terrain Data in the Editor", settings.editorPathToSaveTerrainData);
            GUILayout.EndVertical();
            GUILayout.Space(3);
            GUILayout.BeginVertical(GetOutlineStyle_Generate());
            GUILayout.Space(3);
            EditorGUILayout.LabelField("Object references", EditorStyles.boldLabel);
            GUILayout.Space(5);
            // Display and allow modification of the GameObject fields
            settings.defaultTerraForgeTerrain = (GameObject)EditorGUILayout.ObjectField("Default Terrain", settings.defaultTerraForgeTerrain, typeof(GameObject), false);
            settings.defaultTerraForgeTerrainForGrid = (GameObject)EditorGUILayout.ObjectField("Default Terrain For Grid", settings.defaultTerraForgeTerrainForGrid, typeof(GameObject), false);
            settings.biomePreviewTerraForgeTerrain = (GameObject)EditorGUILayout.ObjectField("Biome Preview Terrain", settings.biomePreviewTerraForgeTerrain, typeof(GameObject), false);
            settings.defaultTerraForgeTerrainGrid = (GameObject)EditorGUILayout.ObjectField("Default Terrain Grid", settings.defaultTerraForgeTerrainGrid, typeof(GameObject), false);

            // Display and allow modification of the TerrainData fields
            settings.defaultTerraForgeTerrainData = (TerrainData)EditorGUILayout.ObjectField("Default TerrainData", settings.defaultTerraForgeTerrainData, typeof(TerrainData), false);
            settings.biomePreviewTerraForgeTerrainData = (TerrainData)EditorGUILayout.ObjectField("Biome Preview TerrainData", settings.biomePreviewTerraForgeTerrainData, typeof(TerrainData), false);

            // Display and allow modification of the ComputeShader field
            settings.hydraulicErosionComputeShader = (ComputeShader)EditorGUILayout.ObjectField("Hydraulic Erosion ComputeShader", settings.hydraulicErosionComputeShader, typeof(ComputeShader), false);
            
            GUILayout.Space(10);

            // Add a button to automatically find and assign objects
            if (GUILayout.Button("Auto Assign Objects"))
            {
                if (TerraForgeGlobalSettings.Instance.enableConfirmation)
                {
                    if (TerraForgeEditorUtilities.ShowConfirmationDialog_5())
                    {
                        AutoAssignObjects();
                    }
                }
                else
                {
                    AutoAssignObjects();
                }
            }
            GUILayout.EndVertical();
            // Add some spacing before the footer
            GUILayout.FlexibleSpace();

            
            EditorGUILayout.LabelField("Represents a window for managing global settings for TerraForge.", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("The settings are saved automatically.", EditorStyles.miniLabel);

            // Save changes to settings if modified
            if (GUI.changed)
            {
                EditorUtility.SetDirty(settings);
            }
        }

        // Method to automatically find and assign objects
        private void AutoAssignObjects()
        {
            // Find and assign the default TerraForge terrain
            settings.defaultTerraForgeTerrain = Resources.Load<GameObject>("Default TerraForge Terrain");
            if (settings.defaultTerraForgeTerrain == null)
            {
                Debug.LogError("Default TerraForge Terrain not found in Resources.");
            }

            // Find and assign the default TerraForge terrain for Grid
            settings.defaultTerraForgeTerrainForGrid = Resources.Load<GameObject>("Default TerraForge Terrain For Grid");
            if (settings.defaultTerraForgeTerrainForGrid == null)
            {
                Debug.LogError("Default TerraForge Terrain For Grid not found in Resources.");
            }

            // Find and assign the biome preview TerraForge terrain
            settings.biomePreviewTerraForgeTerrain = Resources.Load<GameObject>("Biome Preview TerraForge Terrain");
            if (settings.biomePreviewTerraForgeTerrain == null)
            {
                Debug.LogError("Biome Preview TerraForge Terrain not found in Resources.");
            }

            // Find and assign the default TerraForge terrain grid
            settings.defaultTerraForgeTerrainGrid = Resources.Load<GameObject>("Default TerraForge Terrain Grid");
            if (settings.defaultTerraForgeTerrainGrid == null)
            {
                Debug.LogError("Default TerraForge Terrain Grid not found in Resources.");
            }

            // Find and assign the default TerraForge TerrainData
            settings.defaultTerraForgeTerrainData = Resources.Load<TerrainData>("Default TerraForge TerrainData");
            if (settings.defaultTerraForgeTerrainData == null)
            {
                Debug.LogError("Default TerraForge TerrainData not found in Resources.");
            }

            // Find and assign the biome preview TerraForge TerrainData
            settings.biomePreviewTerraForgeTerrainData = Resources.Load<TerrainData>("Biome Preview TerraForge TerrainData");
            if (settings.biomePreviewTerraForgeTerrainData == null)
            {
                Debug.LogError("Biome Preview TerraForge TerrainData not found in Resources.");
            }

            // Find and assign the Hydraulic Erosion ComputeShader
            settings.hydraulicErosionComputeShader = Resources.Load<ComputeShader>("HydraulicsSystemComputeShader");
            if (settings.hydraulicErosionComputeShader == null)
            {
                Debug.LogError("Hydraulics System ComputeShader not found in Resources.");
            }

            // Assign the path to save terrain data
            settings.editorPathToSaveTerrainData = "Assets/TerraForge 2/TerrainsData";
        }

        private Texture2D CreateOutlineTexture(int width, int height, Color borderColor, Color fillColor)
        {
            Color[] pix = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        pix[y * width + x] = borderColor;
                    }
                    else
                    {
                        pix[y * width + x] = fillColor;
                    }
                }
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private GUIStyle GetOutlineStyle_Generate()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = outlineTexture_2;
            style.margin = new RectOffset(5, 5, 5, 5);
            style.padding = new RectOffset(10, 10, 5, 10);

            return style;
        }
    }
}