// TerraForgeTerrainPainterHeight.cs
// Represents a height modifier for terrain generation.
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
    /// Represents a height modifier for terrain generation.
    /// </summary>
    [System.Serializable]
    public class TerraForgeTerrainPainterHeight : TerraForgeTerrainPainterModifier
    {
        /// <summary>
        /// The minimum height value.
        /// </summary>
        public float min = 0;

        /// <summary>
        /// The falloff value at the minimum height.
        /// </summary>
        [Min(0.001f)]
        public float minFalloff = 1;

        /// <summary>
        /// The maximum height value.
        /// </summary>
        public float max = 2000;

        /// <summary>
        /// The falloff value at the maximum height.
        /// </summary>
        [Min(0.001f)]
        public float maxFalloff = 1;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Sets the filter pass index to TerraForgeTerrainPainterHeight.
        /// </summary>
        public void OnEnable()
        {
            passIndex = FilterPass.TerraForgeTerrainPainterHeight;
        }

        /// <summary>
        /// Configures the material properties for the height modifier.
        /// </summary>
        /// <param name="material">The material to configure.</param>
        public override void Configure(Material material, Terrain terrain)
        {
            base.Configure(material, terrain);

            float recalculated_min = min / (1000f / terrain.terrainData.size.x);
            float recalculated_max = max / (1000f / terrain.terrainData.size.x);
            float recalculated_minFalloff = minFalloff / (1000f / terrain.terrainData.size.x);
            float recalculated_maxFalloff = maxFalloff / (1000f / terrain.terrainData.size.x);

            material.SetVector("_MinMaxHeight", new Vector4(recalculated_min, recalculated_max, recalculated_minFalloff, recalculated_maxFalloff));
        }
    }
}
