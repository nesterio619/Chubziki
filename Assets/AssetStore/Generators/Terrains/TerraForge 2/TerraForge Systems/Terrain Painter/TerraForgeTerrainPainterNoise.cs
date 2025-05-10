// TerraForgeTerrainPainterNoise.cs
// Represents a noise modifier that can be applied to terrain.
// TerraForge 2.0.0

using System;
using UnityEngine;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.TerrainPainter
{
    /// <summary>
    /// Represents a noise modifier that can be applied to terrain.
    /// </summary>
    public class TerraForgeTerrainPainterNoise : TerraForgeTerrainPainterModifier
    {
        /// <summary>
        /// Enum for different types of noise generation.
        /// </summary>
        public enum NoiseType
        {
            Simplex,
            Gradient
        }
        /// <summary>
        /// The type of noise to generate.
        /// </summary>
        public NoiseType noiseType;

        /// <summary>
        /// The scale of the noise.
        /// </summary>
        public float noiseScale = 50f;

        /// <summary>
        /// The offset of the noise.
        /// </summary>
        public Vector2 noiseOffset;

        /// <summary>
        /// The levels of the noise represented as a range.
        /// </summary>
        [TerraForgeTerrainPainterAttributes.MinMaxSlider(0f, 1f)]
        public Vector2 levels = new Vector2(0.5f, 1f);

        /// <summary>
        /// Sets the pass index for the noise filter pass.
        /// </summary>
        public void OnEnable()
        {
            passIndex = FilterPass.TerraForgeTerrainPainterNoise;
        }
        
        /// <summary>
        /// Configures the noise properties on the given material.
        /// </summary>
        /// <param name="material">The material to configure.</param>
        public override void Configure(Material material, Terrain terrain)
        {
            base.Configure(material, terrain);

            material.SetVector("_NoiseScaleOffset", new Vector4(noiseScale * 0.001f, noiseScale * 0.001f, noiseOffset.x, noiseOffset.y));
            material.SetVector("_Levels", new Vector4(levels.x, levels.y, 0, 0));
            material.SetInt("_NoiseType", (int)noiseType);
        }
    }
}
