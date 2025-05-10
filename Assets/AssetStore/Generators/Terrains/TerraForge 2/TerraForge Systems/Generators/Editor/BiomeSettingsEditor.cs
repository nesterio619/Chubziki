// BiomeSettingsEditor.cs
// Custom editor for BiomeSettings
// TerraForge 2.0.0

using UnityEngine;
using UnityEditor;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.Generators
{
    [CustomEditor(typeof(BiomeSettings))]
    class BiomeSettingsEditor : UnityEditor.Editor
    {
        private BiomeSettings script;
        
        private Texture2D banner;
        private Texture2D buttonNormalBackground;
        private Texture2D buttonActiveBackground;
        private Texture2D outlineTexture_0;
        private Texture2D outlineTexture_1;
        private Texture2D outlineTexture_2;
        private Texture2D outlineTexture_3;

        private SerializedProperty showTerrainLayersSettings;
        private SerializedProperty showHydraulicErosionLayerSettings;

        private void OnEnable()
        {
            script = (BiomeSettings)target;
            banner = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TerraForge 2/TerraForge Systems/Textures Editor GUI/Banners/TerraForgeBiomeSettingsEditor_Banner.png");

            outlineTexture_0 = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f));
            outlineTexture_1 = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f));
            outlineTexture_2 = CreateOutlineTexture(10, 10, new Color(0.4f, 0.4f, 0.4f), new Color(0.2f, 0.2f, 0.2f));
            outlineTexture_3 = CreateOutlineTexture(10, 10, new Color(0.25f, 0.25f, 0.25f), new Color(0.18f, 0.18f, 0.18f));
            buttonNormalBackground = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f));
            buttonActiveBackground = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f));


            showTerrainLayersSettings = serializedObject.FindProperty("showTerrainLayersSettings");
            showHydraulicErosionLayerSettings = serializedObject.FindProperty("showHydraulicErosionLayerSettings");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            if (banner != null)
            {
                float aspectRatio = (float)banner.width / banner.height;
                float bannerWidth = EditorGUIUtility.currentViewWidth - 20;
                float bannerHeight = bannerWidth / aspectRatio;

                GUILayout.Label(banner, GUILayout.Width(bannerWidth), GUILayout.Height(bannerHeight));
            }
            else
            {
                EditorGUILayout.HelpBox("Banner image not found!", MessageType.Warning);
            }

            GUILayout.Space(5);

            GUILayout.BeginVertical(GetOutlineStyle_1());
            GUILayout.Space(4);
            script.biomeName = EditorGUILayout.TextField(new GUIContent("Biome Name", "Name of the biome"), script.biomeName);
            script.terrainHeight = EditorGUILayout.FloatField(new GUIContent("Biome Height", "Height of the terrain"), script.terrainHeight);
            GUILayout.Space(5);
            if (GUILayout.Button("Show Preview Biome"))
            {
                script.ShowPreviewBiome();
            }
            GUILayout.EndVertical();
            
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Terrain Noise Layers Settings", GetButtonStyle(showTerrainLayersSettings.boolValue)))
            {
                showTerrainLayersSettings.boolValue = true;
                showHydraulicErosionLayerSettings.boolValue = false;
            }
            if (GUILayout.Button("Hydraulic Erosion Layer Settings", GetButtonStyle(showHydraulicErosionLayerSettings.boolValue)))
            {
                showTerrainLayersSettings.boolValue = false;
                showHydraulicErosionLayerSettings.boolValue = true;
            }
            GUILayout.EndHorizontal();

            if (showTerrainLayersSettings.boolValue)
            {
                DrawTerrainLayersSettings();
            }
            else if (showHydraulicErosionLayerSettings.boolValue)
            {
                DrawHydraulicErosionLayerSettings();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTerrainLayersSettings()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "Modify Terrain Layers Settings");

            GUILayout.BeginVertical(GetOutlineStyle_TerrainNoiseLayers());
            EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainLayers"), true);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }
        }

        private void DrawHydraulicErosionLayerSettings()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "Modify Hydraulic Erosion Layer Settings");

            GUILayout.BeginVertical(GetOutlineStyle());

            // Toggle for enabling/disabling the hydraulic erosion layer
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.isEnabled"), true);

            if (script.hydraulicErosionLayerSettings.isEnabled)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.strength"));
                GUILayout.Space(20);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.iterations"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.erosionResolution"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.erosionSteps"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.erosionRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.inertia"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.minSedimentCapacity"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.maxDropletLifetime"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.initialWaterVolume"));

                GUILayout.Space(20);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.useFalloffMap"), true);

                if (script.hydraulicErosionLayerSettings.useFalloffMap)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.falloffTransitionWidth"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("hydraulicErosionLayerSettings.falloffRange"));
                }
            }
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }
        }

        private GUIStyle GetButtonStyle(bool isActive)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.margin = new RectOffset(0, 2, 0, 0);
            style.padding = new RectOffset(10, 2, 4, 4);
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;

            if (isActive)
            {
                style.fontStyle = FontStyle.Bold;
                style.normal.background = buttonActiveBackground;
                style.normal.textColor = new Color(1f, 1f, 1f);
            }
            else
            {
                style.fontStyle = FontStyle.Normal;
                style.normal.background = buttonNormalBackground;
                style.normal.textColor = new Color(0.75f, 0.75f, 0.75f);
            }

            style.border = new RectOffset(12, 12, 12, 12);

            return style;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
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

        private GUIStyle GetOutlineStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = outlineTexture_0;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(10, 10, 15, 10);

            return style;
        }

        private GUIStyle GetOutlineStyle_TerrainNoiseLayers()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = outlineTexture_0;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(20, 10, 10, 5);

            return style;
        }

        private GUIStyle GetOutlineStyle_1()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = outlineTexture_2;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(10, 10, 5, 10);

            return style;
        }
    }
}
