// TerrainChangeListener.cs
// Listens for changes in the terrain and triggers repaints if necessary.
// TerraForge 2.0.0

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
    /// Listens for changes in the terrain and triggers repaints if necessary.
    /// </summary>
    [ExecuteInEditMode]
    public class TerrainChangeListener : MonoBehaviour
    {
        /// <summary>
        /// The terrain being monitored for changes.
        /// </summary>
        [HideInInspector]
        public Terrain terrain;

        /// <summary>
        /// Called when the terrain changes.
        /// </summary>
        /// <param name="flags">Flags indicating what part of the terrain has changed.</param>
        void OnTerrainChanged(TerrainChangedFlags flags)
        {
            terrain = GetComponent<Terrain>();
            TerraForgeTerrainPainter terraForgeTerrainPainter = GetComponent<TerraForgeTerrainPainter>();
            if (!terrain || !terraForgeTerrainPainter) return;

            if ((flags & TerrainChangedFlags.Heightmap) != 0)
            {
                if (terraForgeTerrainPainter.autoRepaint)
                {
                    terraForgeTerrainPainter.RepaintTerrain(terrain);
                }
            }
        }
    }
}