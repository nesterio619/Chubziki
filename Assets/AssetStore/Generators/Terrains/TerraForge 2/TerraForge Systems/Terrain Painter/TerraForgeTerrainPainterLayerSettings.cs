// TerraForgeTerrainPainterLayerSettings.cs
// Settings for a terrain layer including its enabled state, associated TerrainLayer, and a stack of modifiers.
// TerraForge 2.0.0

using System.Collections.Generic;
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
    /// Settings for a terrain layer including its enabled state, associated TerrainLayer, and a stack of modifiers.
    /// </summary>
    [System.Serializable]
    public class TerraForgeTerrainPainterLayerSettings
    {
        /// <summary>
        /// Indicates whether the terrain layer is enabled.
        /// </summary>
        public bool enabled = true;

        /// <summary>
        /// The TerrainLayer associated with this settings.
        /// </summary>
        public TerrainLayer layer;

        /// <summary>
        /// Stack of modifiers applied to this terrain layer.
        /// </summary>
        public List<TerraForgeTerrainPainterModifier> modifierStack = new List<TerraForgeTerrainPainterModifier>();
    }
}
