// TerraForgeHelper.cs
// TerraForge 2.0.0

using UnityEngine;
using UnityEditor;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.TerraForgeEditor
{
    public class TerraForgeHelper : EditorWindow
    {
        private Texture2D banner;
        private Texture2D outlineTexture_2;
        private const string DeveloperName = "Wiskered";
        private const string ToolVersion = "v2.0.0";

        [MenuItem("Tools/TerraForge 2/Helper", false, 0)]
        public static void ShowWindow()
        {
            GetWindow<TerraForgeHelper>("TerraForge 2 Helper");
        }

        private void OnEnable()
        {
            banner = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TerraForge 2/TerraForge Systems/Textures Editor GUI/Banners/TerraForgeHelper_Banner.png");
            outlineTexture_2 = CreateOutlineTexture(10, 10, new Color(0.4f, 0.4f, 0.4f), new Color(0.2f, 0.2f, 0.2f));
        }

        private void OnGUI()
        {
            // Draw the banner at the top and adapt to window size
            if (banner != null)
            {
                float aspectRatio = (float)banner.width / banner.height;
                float bannerWidth = EditorGUIUtility.currentViewWidth - 8; // Subtract some padding
                float bannerHeight = bannerWidth / aspectRatio;

                GUILayout.Label(banner, GUILayout.Width(bannerWidth), GUILayout.Height(bannerHeight));
            }
            else
            {
                EditorGUILayout.HelpBox("Banner image not found!", MessageType.Warning);
            }

            // Add some spacing
            GUILayout.Space(10);

            // Useful links section

            GUILayout.BeginVertical(GetOutlineStyle_Generate());

            GUILayout.Space(5);
            
            if (GUILayout.Button("Official Documentation"))
            {
                Application.OpenURL("https://wiskered.gitbook.io/terraforge-documentation/");
            }

            if (GUILayout.Button("Discord Community"))
            {
                Application.OpenURL("https://discord.gg/KFWpRGMMbZ");
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GetOutlineStyle_Generate());

            GUILayout.Space(5);

            if (GUILayout.Button("Wiskered Unity Asset Store"))
            {
                Application.OpenURL("https://assetstore.unity.com/publishers/46701");
            }

            if (GUILayout.Button("Contact Support"))
            {
                Application.OpenURL("mailto:philunitypublisher@gmail.com");
            }

            if (GUILayout.Button("Wiskered.com"))
            {
                Application.OpenURL(" https://wiskered.com/ ");
            }

            GUILayout.EndVertical();

            // Add some spacing before the footer
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("Thank you for using TerraForge 2!", EditorStyles.miniLabel);
            // Draw developer's name and version at the bottom
            EditorGUILayout.LabelField("Developer: " + DeveloperName, EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Version: " + ToolVersion, EditorStyles.miniLabel);
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