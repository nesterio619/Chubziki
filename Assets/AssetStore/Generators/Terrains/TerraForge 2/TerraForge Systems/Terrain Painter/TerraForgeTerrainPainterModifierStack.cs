// TerraForgeTerrainPainterModifierStack.cs
// Provides methods for configuring and processing terrain modifications using a stack of modifiers.
// TerraForge 2.0.0

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainTools;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.TerrainPainter
{
    /// <summary>
    /// Provides methods for configuring and processing terrain modifications using a stack of modifiers.
    /// </summary>
    public class TerraForgeTerrainPainterModifierStack
    {
        private static int m_resolution;
        private static float heightScale;
        public static Material filterMat;
        private static RenderTexture alphaMap;

        private const string UndoActionName = "Painted Terrain";
        private static readonly int HeightmapID = Shader.PropertyToID("_Heightmap");
        private static readonly int HeightmapScaleID = Shader.PropertyToID("_HeightmapScale");
        private static readonly int NormalMapID = Shader.PropertyToID("_NormalMap");
        private static readonly int TerrainPosScaleID = Shader.PropertyToID("_TerrainPosScale");
        private static readonly int TerrainBoundsID = Shader.PropertyToID("_TerrainBounds");

        /// <summary>
        /// Configures the terrain for painting. Sets up the material and generates a normal map.
        /// </summary>
        /// <param name="terrain">The terrain to configure.</param>
        /// <param name="bounds">The bounds of the terrain.</param>
        /// <param name="resolution">The resolution of the render texture.</param>
        public static void Configure(Terrain terrain, Bounds bounds, int resolution)
        {
            if (m_resolution != resolution || alphaMap == null)
            {
                alphaMap = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R8);
            }
            m_resolution = resolution;

            if (!filterMat) filterMat = new Material(Shader.Find("Hidden/TerraForgeTerrainPainter/TerraForgeTerrainPainterModifier"));

            filterMat.SetTexture(HeightmapID, terrain.terrainData.heightmapTexture);
            filterMat.SetTexture(NormalMapID, terrain.normalmapTexture);

            heightScale = bounds.max.y - bounds.min.y;
            filterMat.SetFloat(HeightmapScaleID, heightScale);

            float invWidth = 1.0f / bounds.size.x;
            float invHeight = 1.0f / bounds.size.z;

            var terrainPosScale = new Vector4(
                (terrain.GetPosition().x * invWidth) - (bounds.min.x * invWidth),
                (terrain.GetPosition().z * invHeight) - (bounds.min.z * invHeight),
                terrain.terrainData.size.x / bounds.size.x,
                terrain.terrainData.size.z / bounds.size.z
            );

            filterMat.SetVector(TerrainPosScaleID, terrainPosScale);
            filterMat.SetVector(TerrainBoundsID, new Vector4(bounds.min.x, bounds.max.z, bounds.size.x, bounds.size.z));
        }

        /// <summary>
        /// Processes all the layers for the specified terrain.
        /// </summary>
        /// <param name="terrain">The terrain to process.</param>
        /// <param name="layerSettings">The list of layer settings to apply.</param>
        public static void ProcessLayers(Terrain terrain, List<TerraForgeTerrainPainterLayerSettings> layerSettings)
        {
            for (int i = layerSettings.Count - 1; i >= 0; i--)
            {
                ProcessSingleLayer(terrain, layerSettings[i]);
            }
        }

        /// <summary>
        /// Processes a single layer of the terrain.
        /// </summary>
        /// <param name="terrain">The terrain to process.</param>
        /// <param name="settings">The settings for the layer to process.</param>
        public static void ProcessSingleLayer(Terrain terrain, TerraForgeTerrainPainterLayerSettings settings)
        {
            if (!settings.enabled || settings.layer == null) return;

            Graphics.SetRenderTarget(alphaMap);
            Graphics.Blit(Texture2D.whiteTexture, alphaMap);

            for (int i = settings.modifierStack.Count - 1; i >= 0; i--)
            {
                settings.modifierStack[i].Configure(filterMat, terrain);
                settings.modifierStack[i].Execute(alphaMap);
            }

            Vector2 scaledSplatmapSize = new Vector2(
                (terrain.terrainData.size.x / m_resolution) * m_resolution,
                (terrain.terrainData.size.z / m_resolution) * m_resolution
            );

            PaintContext c = TerrainPaintUtility.BeginPaintTexture(terrain, new Rect(0, 0, scaledSplatmapSize.x, scaledSplatmapSize.y), settings.layer);

            Graphics.Blit(alphaMap, c.destinationRenderTexture);

            TerrainPaintUtility.EndPaintTexture(c, UndoActionName);
        }
    }
}
