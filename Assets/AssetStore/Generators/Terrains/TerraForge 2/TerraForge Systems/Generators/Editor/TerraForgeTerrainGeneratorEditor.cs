// TerraForgeTerrainGeneratorEditor.cs
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
    [CustomEditor(typeof(TerraForgeTerrainGenerator))]
    class TerraForgeTerrainGeneratorEditor : UnityEditor.Editor
    {
        private TerraForgeTerrainGenerator script;

        private Texture2D banner;
        private Texture2D bannerBiomePreview;
        private Texture2D buttonNormalBackground;
        private Texture2D buttonActiveBackground;
        private Texture2D outlineTexture_0;
        private Texture2D outlineTexture_1;
        private Texture2D outlineTexture_2;
        private Texture2D outlineTexture_3;

        private bool previousAutoUpdateTerrainGeneration;
        private bool previousSaveTerrainData;
        private float previousTerrainHeight;

        private void OnEnable()
        {
            script = (TerraForgeTerrainGenerator)target;

            banner = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TerraForge 2/TerraForge Systems/Textures Editor GUI/Banners/TerraForgeTerrainGeneratorEditor_Banner.png");
            bannerBiomePreview = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TerraForge 2/TerraForge Systems/Textures Editor GUI/Banners/TerraForgeTerrainGeneratorEditor_BiomePreview_Banner.png");

            outlineTexture_0 = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f));
            outlineTexture_1 = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f));
            outlineTexture_2 = CreateOutlineTexture(10, 10, new Color(0.4f, 0.4f, 0.4f), new Color(0.2f, 0.2f, 0.2f));
            outlineTexture_3 = CreateOutlineTexture(10, 10, new Color(0.25f, 0.25f, 0.25f), new Color(0.18f, 0.18f, 0.18f));
            buttonNormalBackground = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f));
            buttonActiveBackground = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f));

            previousAutoUpdateTerrainGeneration = script.generalSettings.autoUpdateTerrainGeneration;
            previousSaveTerrainData = script.generalSettings.saveTerrainData;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            if (script.biomeSettings != null)
            {
                if (bannerBiomePreview != null)
                {
                    float aspectRatio = (float)bannerBiomePreview.width / bannerBiomePreview.height;
                    float bannerBiomePreviewWidth = EditorGUIUtility.currentViewWidth - 30;
                    float bannerBiomePreviewHeight = bannerBiomePreviewWidth / aspectRatio;

                    GUILayout.Label(bannerBiomePreview, GUILayout.Width(bannerBiomePreviewWidth), GUILayout.Height(bannerBiomePreviewHeight));
                }
                else
                {
                    EditorGUILayout.HelpBox("Banner image not found!", MessageType.Warning);
                }

                GUILayout.Space(10);

                GUILayout.BeginVertical(GetOutlineStyle_1());
                GUILayout.Space(4);
                script.biomeSettings = (BiomeSettings)EditorGUILayout.ObjectField(new GUIContent("Biome", "Biome"), script.biomeSettings, typeof(BiomeSettings), true);
                GUILayout.Space(5);
                if (GUILayout.Button("Apply the changes to Biome"))
                {
                    script.ApplyTheChangesToBiome();
                }
                GUILayout.EndVertical();
            }
            else
            {
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
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Terrain Settings", GetButtonStyle(script.showTerrainSettings)))
            {
                script.showTerrainSettings = true;
                script.showTerrainNoiseLayersSettings = false;
                script.showHydraulicErosionLayerSettings = false;
            }
            if (GUILayout.Button("Terrain Noise Layers", GetButtonStyle(script.showTerrainNoiseLayersSettings)))
            {
                script.showTerrainSettings = false;
                script.showTerrainNoiseLayersSettings = true;
                script.showHydraulicErosionLayerSettings = false;
            }
            if (GUILayout.Button("Hydraulic Erosion Layer", GetButtonStyle(script.showHydraulicErosionLayerSettings)))
            {
                script.showTerrainSettings = false;
                script.showTerrainNoiseLayersSettings = false;
                script.showHydraulicErosionLayerSettings = true;
            }
            GUILayout.EndHorizontal();

            if (script.showTerrainSettings)
            {
                DrawTerrainSettings();
            }
            else if (script.showTerrainNoiseLayersSettings)
            {
                DrawTerrainNoiseLayersSettings();
            }
            else if (script.showHydraulicErosionLayerSettings)
            {
                DrawHydraulicErosionLayerSettings();
            }
            else {script.showTerrainSettings = true;}

            GUILayout.Space(5);

            GUILayout.BeginVertical(GetOutlineStyle_D_O());

            // Auto Update Terrain Generation Toggle with action
            GUILayout.BeginHorizontal();

            Undo.RecordObject(script, "Toggle Auto Update Terrain Generation");
            bool newAutoUpdateTerrainGeneration = EditorGUILayout.Toggle(script.generalSettings.autoUpdateTerrainGeneration, GUILayout.Width(15));
            GUILayout.Label(new GUIContent("Auto Update Terrain Generation", "Automatically update the terrain when changes are made. Auto-update is available only when terrain resolution is less than or equal to 513"));
            GUILayout.EndHorizontal();

            if (newAutoUpdateTerrainGeneration != previousAutoUpdateTerrainGeneration)
            {
                // Perform the single action when toggled on
                if (newAutoUpdateTerrainGeneration)
                {
                    script.TerrainGenerate();   
                }
                previousAutoUpdateTerrainGeneration = newAutoUpdateTerrainGeneration;
            }
            script.generalSettings.autoUpdateTerrainGeneration = newAutoUpdateTerrainGeneration;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Generate Terrain", "Generates the terrain based on the configured terrain layers.")))
            {
                if (TerraForgeGlobalSettings.Instance.enableConfirmation && (int)script.generalSettings.terrainResolution > 513)
                {
                    if (TerraForgeEditorUtilities.ShowConfirmationDialog_2())
                    {
                        script.TerrainGenerate();
                    }
                }
                else
                {
                    script.TerrainGenerate();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTerrainSettings()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "Modify Terrain Settings");
            
            GUILayout.BeginVertical(GetOutlineStyle());

            EditorGUILayout.PropertyField(serializedObject.FindProperty("generalSettings.terrainResolution"));
            if (!script.generalSettings.autoUpdateTerrainGeneration)
            {
                if (GUILayout.Button("ChangeTerrainResolution"))
                {
                    if (script.generalSettings.terrainResolution != script.generalSettings.bufferTerrainResolution)
                    {
                        if (TerraForgeGlobalSettings.Instance.enableConfirmation && (int)script.generalSettings.terrainResolution < (int)script.generalSettings.bufferTerrainResolution && (int)script.generalSettings.bufferTerrainResolution > 513)
                        {
                            if (TerraForgeEditorUtilities.ShowConfirmationDialog_3())
                            {
                                script.ChangeTerrainResolution(true);
                            }
                            else
                            {
                                script.generalSettings.terrainResolution = script.generalSettings.bufferTerrainResolution;
                            }
                        }
                        else
                        {
                            if (TerraForgeGlobalSettings.Instance.enableConfirmation && (int)script.generalSettings.terrainResolution > 513)
                            {
                                if (TerraForgeEditorUtilities.ShowConfirmationDialog_14())
                                {
                                    script.ChangeTerrainResolution(true);
                                }
                                else
                                {
                                    script.generalSettings.terrainResolution = script.generalSettings.bufferTerrainResolution;
                                }
                            }
                            else
                            {
                                script.ChangeTerrainResolution(true);
                            }
                        }
                    }
                }

                GUILayout.Space(10);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("generalSettings.terrainMaterial"));
            if (!script.generalSettings.autoUpdateTerrainGeneration)
            {
                if (GUILayout.Button("ChangeTerrainMaterial"))
                {
                    script.ChangeTerrainMaterial();
                }
                GUILayout.Space(10);
            }
            //float newSaveTerrainData = EditorGUILayout.Toggle(script.generalSettings.saveTerrainData, GUILayout.Width(15));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("generalSettings.terrainHeight"));
            /*if (newTerrainHeight != previousTerrainHeight)
            {
                if (newTerrainHeight)
                {
                    //script.TerrainGenerate();   
                }
                previousTerrainHeight = newTerrainHeight;
            }*/
            
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            Undo.RecordObject(script, "Toggle Save Terrain Data");
            bool newSaveTerrainData = EditorGUILayout.Toggle(script.generalSettings.saveTerrainData, GUILayout.Width(15));
            GUILayout.Label(new GUIContent("Save Terrain Data File", "Save the generation in the terrainData file so that you do not lose the generation data. It is recommended to leave it switched on"));
            
            GUILayout.EndHorizontal();
            if (newSaveTerrainData != previousSaveTerrainData)
            {
                script.generalSettings.saveTerrainData = newSaveTerrainData;
                script.CreateAndSetCloneTerrainData();
                previousSaveTerrainData = newSaveTerrainData;
            }   

            EditorGUILayout.PropertyField(serializedObject.FindProperty("generalSettings.currentTerrainData"));

            GUILayout.Space(10);

            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Set Terrain Data", "Allows you to manually set the desired terrainData")))
            {
                if (TerraForgeEditorUtilities.ShowConfirmationDialog_6())
                {
                    script.ManualChangeTerrainData();
                }
                else
                {
                    script.generalSettings.currentTerrainData = script.terrain.terrainData;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Clone Terrain Data", "Cloning the current TerrainData and set as new TerrainData file")))
            {
                if (TerraForgeEditorUtilities.ShowConfirmationDialog_6())
                {
                    script.CreateAndSetCloneTerrainData();
                }
            }

            if (GUILayout.Button(new GUIContent("Empty Terrain Data", "Creating a new default TerrainData and set it up")))
            {
                if (TerraForgeEditorUtilities.ShowConfirmationDialog_6())
                {
                    script.CreateAndSaveEmptyTerrainData();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }
        }

        private void DrawTerrainNoiseLayersSettings()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "Modify Terrain Noise Layers Settings");

            GUILayout.BeginVertical(GetOutlineStyle_TerrainNoiseLayers());
            EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainLayers"), true);
            GUILayout.Space(10);
            if (GUILayout.Button(new GUIContent("Refresh Generation All Terrain Layers", "Refreshes the generation of all terrain layers. Forcibly updates generation even for layers that have not been modified. Usually takes longer than normal generation.")))
            {
                if (TerraForgeGlobalSettings.Instance.enableConfirmation && (int)script.generalSettings.terrainResolution > 513)
                {
                    if (TerraForgeEditorUtilities.ShowConfirmationDialog_2())
                    {
                        script.ChangeTerrainResolution(true);
                    }
                }
                else
                {
                    script.ChangeTerrainResolution(true);
                }
            }
            GUILayout.Space(10);
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
                if (!script.generalSettings.autoUpdateTerrainGeneration)
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("Hydraulic Erode Generate"))
                    {
                        script.HydraulicErodeGenerateEditor();
                    }
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

        private GUIStyle GetOutlineStyle_D_O()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = outlineTexture_2;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(10, 10, 5, 10);

            return style;
        }
    }
}