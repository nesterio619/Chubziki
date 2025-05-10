// TerraForgeTerrainPainterSlope.cs
// TerraForgeTerrainPainterModifier for adjusting terrain based on slope.
// TerraForge 2.0.0

using System;
using UnityEngine;
using UnityEngine.Serialization;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.TerrainPainter
{
    /// <summary>
    /// TerraForgeTerrainPainterModifier for adjusting terrain based on slope.
    /// </summary>
    [System.Serializable]
    public class TerraForgeTerrainPainterSlope : TerraForgeTerrainPainterModifier
    {
        /// <summary>
        /// The minimum and maximum slope angles to be affected by the modifier.
        /// </summary>
        [TerraForgeTerrainPainterAttributes.MinMaxSlider(0f, 90f)]
        [Tooltip("The minimum and maximum slope angles to be affected by the modifier.")]
        public Vector2 minMax = new Vector2(0, 90f);

        /// <summary>
        /// The falloff range for the minimum slope.
        /// </summary>
        [Range(0f, 90f)]
        [Tooltip("The falloff range for the minimum slope.")]
        public float minFalloff = 10;

        /// <summary>
        /// The falloff range for the maximum slope.
        /// </summary>
        [Range(0f, 90f)]
        [Tooltip("The falloff range for the maximum slope.")]
        public float maxFalloff = 10;

        /// <summary>
        /// Initializes the slope modifier and sets the filter pass index.
        /// </summary>
        public void OnEnable()
        {
            passIndex = FilterPass.TerraForgeTerrainPainterSlope;
        }

        /// <summary>
        /// Configures the material with the slope modifier settings.
        /// </summary>
        /// <param name="material">The material to configure.</param>
        /// <param name="terrain">The terrain to configure.</param>
        public override void Configure(Material material, Terrain terrain)
        {
            base.Configure(material, terrain);

            float resolution = (float)terrain.terrainData.heightmapResolution;
            
            float recalculated_minMax_x = minMax.x;
            float recalculated_minMax_y = minMax.y;
            float recalculated_minFalloff = minFalloff;
            float recalculated_maxFalloff = maxFalloff;

            switch (resolution)
            {
                case 33:
                    recalculated_minMax_x = recalculated_minMax_x * 9f;
                    recalculated_minMax_y = recalculated_minMax_y * 9f;
                    recalculated_minFalloff = recalculated_minFalloff * 2.85f;
                    recalculated_maxFalloff = recalculated_maxFalloff * 2.85f;
                    break;
                case 65:
                    recalculated_minMax_x = recalculated_minMax_x * 8f;
                    recalculated_minMax_y = recalculated_minMax_y * 8f;
                    recalculated_minFalloff = recalculated_minFalloff * 2.35f;
                    recalculated_maxFalloff = recalculated_maxFalloff * 2.35f;
                    break;
                case 129:
                    recalculated_minMax_x = recalculated_minMax_x * 4f;
                    recalculated_minMax_y = recalculated_minMax_y * 4f;
                    recalculated_minFalloff = recalculated_minFalloff * 1.875f;
                    recalculated_maxFalloff = recalculated_maxFalloff * 1.875f;
                    break;
                case 257:
                    recalculated_minMax_x = recalculated_minMax_x * 2f;
                    recalculated_minMax_y = recalculated_minMax_y * 2f;
                    recalculated_minFalloff = recalculated_minFalloff * 1.31f;
                    recalculated_maxFalloff = recalculated_maxFalloff * 1.31f;
                    break;
                case 513:
                    // This is the default resolution. No calculations are required
                    break;
                case 1025:
                    recalculated_minMax_x = recalculated_minMax_x * 0.5f;
                    recalculated_minMax_y = recalculated_minMax_y * 0.5f;
                    recalculated_minFalloff = recalculated_minFalloff * 0.4f;
                    recalculated_maxFalloff = recalculated_maxFalloff * 0.4f;
                    break;
                case 2049:
                    recalculated_minMax_x = recalculated_minMax_x * 0.25f;
                    recalculated_minMax_y = recalculated_minMax_y * 0.25f;
                    recalculated_minFalloff = recalculated_minFalloff * 0.2f;
                    recalculated_maxFalloff = recalculated_maxFalloff * 0.2f;
                    break;
                case 4097:
                    recalculated_minMax_x = recalculated_minMax_x * 0.125f;
                    recalculated_minMax_y = recalculated_minMax_y * 0.125f;
                    recalculated_minFalloff = recalculated_minFalloff * 0.1f;
                    recalculated_maxFalloff = recalculated_maxFalloff * 0.1f;
                    break;
                default:
                    Debug.LogWarning($"Unknown Heightmap Resolution: {resolution}x{resolution}.");
                    break;
            }

            material.SetVector("_MinMaxSlope", new Vector4(recalculated_minMax_x, recalculated_minMax_y, recalculated_minFalloff, recalculated_maxFalloff));
        }
    }
}