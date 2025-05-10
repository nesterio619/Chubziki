// TerraForgeTerrainsGridGeneratorEditor.cs
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
    [CustomEditor(typeof(TerraForgeTerrainsGridGenerator))]
    public class TerraForgeTerrainsGridGeneratorEditor : UnityEditor.Editor
    {
        private Texture2D banner;
        private TerraForgeTerrainsGridGenerator script;
        private Texture2D buttonNormalBackground;
        private Texture2D buttonActiveBackground;
        private Texture2D outlineTexture_0;
        private Texture2D outlineTexture_1;
        private Texture2D outlineTexture_2;
        private Texture2D outlineTexture_3;

        private bool isConfirmDeleteButtonPressed = false;
        private bool isConfirmButtonPressed = false;
        private float confirmButtonPressTime = 0f;
        private const float holdDuration = 1.5f;

        private void OnEnable()
        {
            banner = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TerraForge 2/TerraForge Systems/Textures Editor GUI/Banners/TerraForgeTerrainsGridGeneratorEditor_Banner.png");
            script = (TerraForgeTerrainsGridGenerator)target;

            outlineTexture_0 = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f));
            outlineTexture_1 = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f));
            outlineTexture_2 = CreateOutlineTexture(10, 10, new Color(0.4f, 0.4f, 0.4f), new Color(0.2f, 0.2f, 0.2f));
            outlineTexture_3 = CreateOutlineTexture(10, 10, new Color(0.25f, 0.25f, 0.25f), new Color(0.18f, 0.18f, 0.18f));
            buttonNormalBackground = MakeTex(2, 2, new Color(0.25f, 0.25f, 0.25f));
            buttonActiveBackground = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.3f));
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
            
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Grid Settings", GetButtonStyle(script.showGridSettings)))
            {
                script.showGridSettings = true;
                script.showTerrainsSettings = false;
                script.showBiomesSettings = false;
            }
            if (GUILayout.Button("Terrains Settings", GetButtonStyle(script.showTerrainsSettings)))
            {
                script.showGridSettings = false;
                script.showTerrainsSettings = true;
                script.showBiomesSettings = false;
            }
            if (GUILayout.Button("Biomes Settings", GetButtonStyle(script.showBiomesSettings)))
            {
                script.showGridSettings = false;
                script.showTerrainsSettings = false;
                script.showBiomesSettings = true;
            }
            GUILayout.EndHorizontal();

            if (script.showGridSettings)
            {
                DrawGridSettings();
            }
            else if (script.showTerrainsSettings)
            {
                DrawTerrainsSettings();
            }
            else if (script.showBiomesSettings)
            {
                DrawBiomesSettings();
            }
            else {script.showGridSettings = true;}

            GUILayout.Space(5);

            GUILayout.BeginVertical(GetOutlineStyle_Generate());

            GUILayout.BeginHorizontal();
            Undo.RecordObject(script, "Toggle Delete Terrain Data");
            script.deleteTerrainDataPreviousGeneration = EditorGUILayout.Toggle(script.deleteTerrainDataPreviousGeneration, GUILayout.Width(15));
            GUILayout.Label(new GUIContent("Delete Previous TerrainData Files (before a new generation)", "Whether to delete TerrainData files from a previous grid generation of this component before a new generation"));
            GUILayout.EndHorizontal();
            if (script.biomes.Count == 0)
            {
                EditorGUILayout.HelpBox("You didn't assign the Biomes! Go to the Biomes Settings tab", MessageType.Warning);
            }
            EditorGUI.BeginDisabledGroup(script.biomes.Count == 0);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Generate New Grid", "Start generation of the terrain grid by following the configured parameters")))
            {
                if (TerraForgeGlobalSettings.Instance.enableConfirmation && (int)script.terrainsResolution < (int)script.bufferTerrainsResolution && script.deleteTerrainDataPreviousGeneration)
                {
                    if (TerraForgeEditorUtilities.ShowConfirmationDialog_13())
                    {
                        if ((script.gridColumns * script.gridLines) > 9)
                        {
                            if (TerraForgeEditorUtilities.ShowConfirmationDialog_10())
                            {
                                if ((int)script.terrainsResolution > 513)
                                {
                                    if (TerraForgeEditorUtilities.ShowConfirmationDialog_8())
                                    {
                                        script.NewGenerateTerrainGrid();
                                        isConfirmDeleteButtonPressed = false;
                                    }
                                }
                                else
                                {
                                    script.NewGenerateTerrainGrid();
                                    isConfirmDeleteButtonPressed = false;
                                }
                            }
                        }
                        else
                        {
                            if ((int)script.terrainsResolution > 513)
                            {
                                if (TerraForgeEditorUtilities.ShowConfirmationDialog_8())
                                {
                                    script.NewGenerateTerrainGrid();
                                    isConfirmDeleteButtonPressed = false;
                                }
                            }
                            else
                            {
                                script.NewGenerateTerrainGrid();
                                isConfirmDeleteButtonPressed = false;
                            }
                        }
                    }
                }
                else
                {
                    if (TerraForgeGlobalSettings.Instance.enableConfirmation && (script.gridColumns * script.gridLines) > 9)
                    {
                        if (TerraForgeEditorUtilities.ShowConfirmationDialog_10())
                        {
                            if (TerraForgeGlobalSettings.Instance.enableConfirmation && (int)script.terrainsResolution > 513)
                            {
                                if (TerraForgeEditorUtilities.ShowConfirmationDialog_8())
                                {
                                    script.NewGenerateTerrainGrid();
                                    isConfirmDeleteButtonPressed = false;
                                }
                            }
                            else
                            {
                                script.NewGenerateTerrainGrid();
                                isConfirmDeleteButtonPressed = false;
                            }
                        }
                    }
                    else
                    {
                        if (TerraForgeGlobalSettings.Instance.enableConfirmation && (int)script.terrainsResolution > 513)
                        {
                            if (TerraForgeEditorUtilities.ShowConfirmationDialog_8())
                            {
                                script.NewGenerateTerrainGrid();
                                isConfirmDeleteButtonPressed = false;
                            }
                        }
                        else
                        {
                            script.NewGenerateTerrainGrid();
                            isConfirmDeleteButtonPressed = false;
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            

            GUILayout.Space(5);

            if (script.terrains != null && script.terrains.Count != 0)
            {
                GUILayout.BeginVertical(GetOutlineStyle_Delete());

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();

                if (!isConfirmDeleteButtonPressed)
                {
                    Rect confirmButtonRect = GUILayoutUtility.GetRect(new GUIContent("Confirm Delete"), GUI.skin.button);
                    if (Event.current.type == EventType.MouseDown && confirmButtonRect.Contains(Event.current.mousePosition))
                    {
                        isConfirmButtonPressed = true;
                        confirmButtonPressTime = (float)EditorApplication.timeSinceStartup;
                    }
            
                    if (isConfirmButtonPressed && Event.current.type == EventType.Repaint)
                    {
                        float elapsedTime = (float)EditorApplication.timeSinceStartup - confirmButtonPressTime;
                        if (elapsedTime >= holdDuration)
                        {
                            isConfirmDeleteButtonPressed = true;
                            isConfirmButtonPressed = false;
                            confirmButtonPressTime = 0f;
                            Repaint();
                        } 
                    }

                    if (Event.current.type == EventType.MouseUp)
                    {
                        isConfirmButtonPressed = false;
                    }

                    if (isConfirmButtonPressed)
                    {
                        float elapsedTime = (float)EditorApplication.timeSinceStartup - confirmButtonPressTime;
                        float progress = Mathf.Clamp01(elapsedTime / holdDuration);

                        EditorGUI.ProgressBar(confirmButtonRect, progress, "Hold to confirm...");
                    }
                    else
                    {
                        if (GUI.Button(confirmButtonRect, new GUIContent("Confirm Delete", "Hold to confirm the deletion of the terrains grid. After confirmation, the delete button will be available"))){}
                    }
                }

                EditorGUI.BeginDisabledGroup(!isConfirmDeleteButtonPressed);
                if (GUILayout.Button(new GUIContent("Delete Grid", "Completely delete the terrains grid (the terrains in the scene and their TerrainData files)")))
                {
                    if (TerraForgeGlobalSettings.Instance.enableConfirmation && TerraForgeEditorUtilities.ShowConfirmationDialog_7())
                    {
                        script.DeleteTerrainGrid();
                        isConfirmDeleteButtonPressed = false;
                    }
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.Space(5);
            }

            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGridSettings()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "Modify Grid Settings");
            
            GUILayout.BeginVertical(GetOutlineStyle());
            
            script.gridColumns = EditorGUILayout.IntSlider(new GUIContent("Grid Columns", "Number of columns in the terrain grid"), script.gridColumns, 2, 12);
            script.gridLines = EditorGUILayout.IntSlider(new GUIContent("Grid Lines", "The number of lines in the terrain grid"), script.gridLines, 2, 12);
            GUILayout.Space(20);
            script.transitionWidth = EditorGUILayout.Slider(new GUIContent("Transition Width", "The width of the transition between terrains"), script.transitionWidth, 100f, 500f);
            script.transitionStrength = EditorGUILayout.Slider(new GUIContent("Transition Strength", "The strength of the transition between terrains"), script.transitionStrength, 1f, 1.5f);
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            script.isEnabled = EditorGUILayout.Toggle(script.isEnabled, GUILayout.Width(15));
            GUILayout.Label(new GUIContent("Edges smoothing", "Indicates whether the edge smoothing is enabled"));
            GUILayout.EndHorizontal();
            if (script.isEnabled)
            {
                script.blurRadius = EditorGUILayout.IntSlider(new GUIContent("Blur Radius", "Radius for blurring the edges of the terrains"), script.blurRadius, 1, 30);
                script.falloffTransitionWidth = EditorGUILayout.Slider(new GUIContent("Falloff Transition Width", "Width of the falloff transition for smoothing edges"), script.falloffTransitionWidth, 0.001f, 0.5f);
                script.falloffRange = EditorGUILayout.Slider(new GUIContent("Falloff Range", "Range of the falloff for smoothing edges"), script.falloffRange, 0.001f, 0.5f);
            }
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }
        }

        private void DrawTerrainsSettings()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "Modify Terrains Settings");

            GUILayout.BeginVertical(GetOutlineStyle());

            EditorGUILayout.PropertyField(serializedObject.FindProperty("terrainSize"));
            if (GUILayout.Button(new GUIContent("Refresh Terrains Size", "Update the size of each terrain without changing other characteristics. The grid will also update the size")))
            {
                script.RefreshTerrainsSize();
            }
            GUILayout.Space(10);

            script.terrainsMaterial = (Material)EditorGUILayout.ObjectField(new GUIContent("Terrains Material", "The material to apply to generated terrains"), script.terrainsMaterial, typeof(Material), false);
            if (GUILayout.Button(new GUIContent("Refresh Terrains Material", "Apply the assigned material to all terrains in the grid")))
            {
                script.RefreshTerrainsMaterial();
            }

            GUILayout.Space(10);

            script.terrainsResolution = (TerrainResolution)EditorGUILayout.EnumPopup(new GUIContent("Terrains Resolution", "The resolution of the terrains to generate"), script.terrainsResolution);
            if (GUILayout.Button(new GUIContent("Refresh Terrains Resolution", "Create a grid of terrains with a specified resolution. Their positions and seeds do not change.")))
            {
                if (TerraForgeGlobalSettings.Instance.enableConfirmation && (int)script.terrainsResolution < (int)script.bufferTerrainsResolution)
                {
                    if (TerraForgeEditorUtilities.ShowConfirmationDialog_3())
                    {
                        if ((script.gridColumns * script.gridLines) > 9)
                        {
                            if (TerraForgeEditorUtilities.ShowConfirmationDialog_12())
                            {
                                if ((int)script.terrainsResolution > 513)
                                {
                                    if (TerraForgeEditorUtilities.ShowConfirmationDialog_11())
                                    {
                                        script.RefreshTerrainsResolution();
                                    }
                                }
                                else
                                {
                                    script.RefreshTerrainsResolution();
                                }
                            }
                        }
                        else
                        {
                            if ((int)script.terrainsResolution > 513)
                            {
                                if (TerraForgeEditorUtilities.ShowConfirmationDialog_11())
                                {
                                    script.RefreshTerrainsResolution();
                                }
                            }
                            else
                            {
                                script.RefreshTerrainsResolution();
                            }
                        }
                    }
                }
                else
                {
                    if (TerraForgeGlobalSettings.Instance.enableConfirmation && (script.gridColumns * script.gridLines) > 9)
                    {
                        if (TerraForgeEditorUtilities.ShowConfirmationDialog_12())
                        {
                            if (TerraForgeGlobalSettings.Instance.enableConfirmation && (int)script.terrainsResolution > 513)
                            {
                                if (TerraForgeEditorUtilities.ShowConfirmationDialog_11())
                                {
                                    script.RefreshTerrainsResolution();
                                }
                            }
                            else
                            {
                                script.RefreshTerrainsResolution();
                            }
                        }
                    }
                    else
                    {
                        if (TerraForgeGlobalSettings.Instance.enableConfirmation && (int)script.terrainsResolution > 513)
                        {
                            if (TerraForgeEditorUtilities.ShowConfirmationDialog_11())
                            {
                                script.RefreshTerrainsResolution();
                            }
                        }
                        else
                        {
                            script.RefreshTerrainsResolution();
                        }
                    }
                }
            }

            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(script);
            }
        }

        private void DrawBiomesSettings()
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(script, "Modify Biomes Settings");

            GUILayout.BeginVertical(GetOutlineStyle_Biomes());
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("biomes"), true);

            GUILayout.BeginHorizontal();
            Undo.RecordObject(script, "Toggle Enable Empty Biomes Creation");
            script.enableEmptyBiomesCreation = EditorGUILayout.Toggle(script.enableEmptyBiomesCreation, GUILayout.Width(20));
            GUILayout.Label(new GUIContent("Creating Empty Biomes", "Enables the creation of empty biomes. It takes much less computation to generate empty biomes. Also, their generation speed is not affected by terrain resolution."));
            GUILayout.EndHorizontal();

            if (script.enableEmptyBiomesCreation)
            {
                script.emptyBiomesCreationChance = EditorGUILayout.Slider(new GUIContent("Empty Biomes Creation Chance", "Chance of creating an empty biome, ranging from 0 (no chance) to 1 (certain)"), script.emptyBiomesCreationChance, 0f, 1f);
                script.emptyBiomesHeight = EditorGUILayout.FloatField(new GUIContent("Empty Biomes Height", "Height of the empty biomes"), script.emptyBiomesHeight);
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

        private GUIStyle GetOutlineStyle_Biomes()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = outlineTexture_0;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(20, 10, 15, 5);

            return style;
        }

        private GUIStyle GetOutlineStyle_Generate()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = outlineTexture_2;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(10, 10, 5, 10);

            return style;
        }

        private GUIStyle GetOutlineStyle_Delete()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = outlineTexture_3;
            style.margin = new RectOffset(0, 0, 0, 0);
            style.padding = new RectOffset(10, 10, 5, 10);

            return style;
        }
    }
}