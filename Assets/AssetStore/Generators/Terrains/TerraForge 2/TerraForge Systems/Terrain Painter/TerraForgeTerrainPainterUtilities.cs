// TerraForgeTerrainPainterUtilities.cs
// Utility methods for terrain-related operations.
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
    /// Utility methods for terrain-related operations.
    /// </summary>
    public partial class TerraForgeTerrainPainterUtilities
    {
        /// <summary>
        /// Calculates the number of splatmaps needed based on the layer count.
        /// </summary>
        /// <param name="layerCount">Number of layers.</param>
        /// <returns>Number of splatmaps needed.</returns>
        public static int GetSplatmapCount(int layerCount)
        {
            if (layerCount > 12) return 4;
            if (layerCount > 8) return 3;
            if (layerCount > 4) return 2;

            return 1;
        }
        
        /// <summary>
        /// Gets the channel index from the layer index.
        /// </summary>
        /// <param name="layerIndex">Index of the layer.</param>
        /// <returns>Channel index.</returns>
        public static int GetChannelIndex(int layerIndex)
        {
            return (layerIndex % 4);
        }
        
        /// <summary>
        /// Creates an RGBA component mask based on the channel index.
        /// </summary>
        /// <param name="channelIndex">Index of the channel.</param>
        /// <returns>Vector mask.</returns>
        public static Vector4 GetVectorMask(int channelIndex)
        {
            switch (channelIndex)
            {
                case 0: return new Vector4(1, 0, 0, 0);
                case 1: return new Vector4(0, 1, 0, 0);
                case 2: return new Vector4(0, 0, 1, 0);
                case 3: return new Vector4(0, 0, 0, 1);
                default: return Vector4.zero;
            }
        }
        
        /// <summary>
        /// Gets the splatmap index from the layer index.
        /// </summary>
        /// <param name="layerIndex">Index of the layer.</param>
        /// <returns>Splatmap index.</returns>
        public static int GetSplatmapIndex(int layerIndex)
        {
            if (layerIndex > 11) return 3;
            if (layerIndex > 7) return 2;
            if (layerIndex > 3) return 1;
            
            return 0;
        }
        
        /// <summary>
        /// Recalculates the bounds encompassing all active terrains.
        /// </summary>
        /// <param name="terrains">Array of terrains to include in the bounds calculation.</param>
        /// <returns>Bounds encompassing all terrains.</returns>
        public static Bounds RecalculateBounds(Terrain[] terrains)
        {
            Vector3 minSum = Vector3.one * Mathf.Infinity;
            Vector3 maxSum = Vector3.one * Mathf.NegativeInfinity;

            foreach (Terrain terrain in terrains)
            {
                if(terrain == null) continue;
                if(!terrain.gameObject.activeInHierarchy) continue;;

                //Min/max bounds corners in world-space
                Vector3 min = terrain.GetPosition(); //Safe to assume terrain starts at origin
                Vector3 max = terrain.GetPosition() + terrain.terrainData.size; //Note, size is slightly more correct in height than bounds

                if (min.x < minSum.x || min.y < minSum.y || min.z < minSum.z) minSum = min;
                if (max.x > maxSum.x || max.y > maxSum.y || max.z > maxSum.z) maxSum = max;
            }

            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            bounds.SetMinMax(minSum, maxSum);

            //Increase bounds height for flat terrains
            if (bounds.size.y < 1f)
            {
                bounds.Encapsulate(new Vector3(bounds.center.x, bounds.center.y + 1f, bounds.center.z));
            }

            return bounds;
        }

        /// <summary>
        /// Converts a list of layer settings to an array of terrain layers.
        /// </summary>
        /// <param name="layerSettings">List of layer settings.</param>
        /// <returns>Array of terrain layers.</returns>
        public static TerrainLayer[] SettingsToLayers(List<TerraForgeTerrainPainterLayerSettings> layerSettings)
        {
            List<TerrainLayer> layerList = new List<TerrainLayer>();
            
            // Convert TerraForgeTerrainPainterLayerSettings to Layers
            for (int i = layerSettings.Count-1; i >= 0; i--)
            {
                layerList.Add(layerSettings[i].layer);
            }

            return layerList.ToArray();
        }

        /// <summary>
        /// Checks if there are any missing terrains in the array.
        /// </summary>
        /// <param name="terrains">Array of terrains to check.</param>
        /// <returns>True if any terrain is missing, otherwise false.</returns>
        public static bool HasMissingTerrain(Terrain[] terrains)
        {
            bool isMissing = false;

            for (int i = 0; i < terrains.Length; i++)
            {
                if (terrains[i] == null) isMissing = true;
            }

            return isMissing;
        }
    }
}
