// TerraForgeTerrainPainter.cs
// Component for painting terrains in the TerraForge 2 system.
// TerraForge 2.0.0

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.TerrainPainter
{
    /// <summary>
    /// Component for painting terrains in the TerraForge 2 system.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("TerraForge 2/Terrain Painter")]
    public partial class TerraForgeTerrainPainter : MonoBehaviour
    {
        /// <summary>
        /// The terrains to be painted.
        /// </summary>
        public Terrain[] terrains;

        /// <summary>
        /// The resolution of the splatmap for the terrains.
        /// </summary>
        [TerraForgeTerrainPainterAttributes.ResolutionDropdown(64, 1024)]
        [Tooltip("The resolution of the splatmap for the terrains.")]
        public int splatmapResolution = 256;

        /// <summary>
        /// The resolution of the color/base map for the terrains.
        /// </summary>
        [TerraForgeTerrainPainterAttributes.ResolutionDropdown(16, 2048)]
        [Tooltip("The color/base map is a pre-rendered texture for the terrain color. This is shown on the terrain in the distance. High resolutions usually have little benefit.")]
        public int colorMapResolution = 256;

        /// <summary>
        /// The settings for each layer to be applied to the terrains.
        /// </summary>
        [Tooltip("The settings for each layer to be applied to the terrains.")]
        public List<TerraForgeTerrainPainterLayerSettings> layerSettings = new List<TerraForgeTerrainPainterLayerSettings>();

        /// <summary>
        /// Automatically repaints the terrains if their heightmap is modified.
        /// </summary>
        [Tooltip("Automatically repaint the terrains if their heightmap is modified. Repaints when the left-mouse button is released.")]
        public bool autoRepaint;

        /// <summary>
        /// List of listeners for terrain changes.
        /// </summary>
        [Tooltip("List of listeners for terrain changes.")]
        public List<TerrainChangeListener> terrainListeners = new List<TerrainChangeListener>();

        /// <summary>
        /// The bounds of the terrains.
        /// </summary>
        [Tooltip("The bounds of the terrains.")]
        public Bounds bounds;

        /// <summary>
        /// The shader used for filters.
        /// </summary>
        [SerializeField]
        [Tooltip("The shader used for filters.")]
        private Shader filterShader;

        /// <summary>
        /// Event triggered when a terrain is repainted.
        /// </summary>
        public delegate void TerrainRepaintEvent(Terrain terrain);

        /// <summary>
        /// Triggers whenever a terrain is repainted. Passes the context terrain as a parameter.
        /// </summary>
        public static event TerrainRepaintEvent OnTerrainRepaint;

        /// <summary>
        /// Initializes the TerraForgeTerrainPainter.
        /// </summary>
        private void OnEnable()
        {
            if (!filterShader) filterShader = Shader.Find("Hidden/TerraForgeTerrainPainter/TerraForgeTerrainPainterModifier");
            AssignActiveTerrains();
            RepaintAll();
        }

        /// <summary>
        /// Applies the splatmapResolution value to all terrains.
        /// This must be called when changing the resolution before repainting a single terrain.
        /// Automatically done in the <see cref="RepaintAll"/> function.
        /// </summary>
        public void ResizeSplatmaps()
        {
            // Needs to happen before repainting, all terrains must have the same splatmap resolution. PaintContext throws warnings otherwise
            foreach (Terrain terrain in terrains)
            {
                if (terrain) terrain.terrainData.alphamapResolution = splatmapResolution;
            }
        }

        /// <summary>
        /// Recalculates the bounds of the terrains.
        /// </summary>
        public void RecalculateBounds()
        {
            bounds = TerraForgeTerrainPainterUtilities.RecalculateBounds(terrains);
        }

        /// <summary>
        /// Assigns the active terrains to be painted.
        /// </summary>
        [ContextMenu("Assign active terrains")]
        public void AssignActiveTerrains()
        {
            Terrain[] activeTerrains = Terrain.activeTerrains;
            List<Terrain> filteredTerrains = new List<Terrain>();

            foreach (Terrain terrain in activeTerrains)
            {
                if (terrain.transform.IsChildOf(transform) || terrain.gameObject == gameObject)
                {
                    filteredTerrains.Add(terrain);
                }
            }

            SetTargetTerrains(filteredTerrains.ToArray());
            RepaintAll();

            if (autoRepaint)
            {
                RemoveTerrainListeners();

                foreach (Terrain terrain in terrains)
                {
                    TerrainChangeListener listener = terrain.GetComponent<TerrainChangeListener>();
                    if (!listener) listener = terrain.gameObject.AddComponent<TerrainChangeListener>();

                    listener.terrain = terrain;
                    this.terrainListeners.Add(listener);
                }
            }
            else
            {
                RemoveTerrainListeners();
            }
        }

        /// <summary>
        /// Sets the target terrains to be painted.
        /// </summary>
        /// <param name="terrains">Array of terrains to be painted.</param>
        public void SetTargetTerrains(Terrain[] terrains)
        {
            this.terrains = terrains;
            RecalculateBounds();
        }

        /// <summary>
        /// Repaints all the assigned terrains using the current configuration.
        /// </summary>
        public void RepaintAll()
        {
            if (layerSettings.Count == 0) return;

            ResizeSplatmaps();

            foreach (Terrain terrain in terrains)
            {
                if (!terrain)
                {
                    Debug.LogError("Missing terrain assigned to TerraForgeTerrainPainter", this);
                    continue;
                }

                RepaintTerrain(terrain);
            }

            // ApplyAllStamps();
        }

        /// <summary>
        /// Repaints an individual terrain.
        /// </summary>
        /// <param name="terrain">The terrain to repaint.</param>
        public void RepaintTerrain(Terrain terrain)
        {
            if (layerSettings.Count == 0 || terrain == null) return;

            TerraForgeTerrainPainterModifierStack.Configure(terrain, bounds, splatmapResolution);

            TerraForgeTerrainPainterModifierStack.ProcessLayers(terrain, layerSettings);

            ApplyStampsToTerrain(terrain);

            // Regenerate basemap
            terrain.terrainData.baseMapResolution = colorMapResolution;
            terrain.terrainData.SetBaseMapDirty();
            terrain.Flush();

            OnTerrainRepaint?.Invoke(terrain);

#if UNITY_EDITOR
            EditorUtility.SetDirty(terrain.terrainData);
#endif
        }

        /// <summary>
        /// Creates settings for a new layer.
        /// </summary>
        /// <param name="layer">The terrain layer to create settings for.</param>
        public void CreateSettingsForLayer(TerrainLayer layer)
        {
            TerraForgeTerrainPainterLayerSettings s = new TerraForgeTerrainPainterLayerSettings();
            s.layer = layer;
            s.modifierStack = new List<TerraForgeTerrainPainterModifier>();

            layerSettings.Insert(0, s);

            SetTerrainLayers();
        }

        /// <summary>
        /// Adds or removes the TerrainChangeListener component from all assigned terrains.
        /// If enabled, terrains will be repainted when their height is modified.
        /// </summary>
        /// <param name="value">Whether to enable or disable auto repaint.</param>
        public void SetAutoRepaint(bool value)
        {
            autoRepaint = value;

            if (value)
            {
                RemoveTerrainListeners();

                foreach (Terrain terrain in terrains)
                {
                    TerrainChangeListener listener = terrain.GetComponent<TerrainChangeListener>();
                    if (!listener) listener = terrain.gameObject.AddComponent<TerrainChangeListener>();

                    listener.terrain = terrain;
                    this.terrainListeners.Add(listener);
                }
            }
            else
            {
                RemoveTerrainListeners();
            }
        }

        /// <summary>
        /// Removes all terrain listeners.
        /// </summary>
        private void RemoveTerrainListeners()
        {
            for (int i = 0; i < terrainListeners.Count; i++)
            {
                DestroyImmediate(terrainListeners[i]);
            }

            terrainListeners.Clear();
        }

        /// <summary>
        /// Ensures that all configured layers are assigned to the terrains.
        /// Also removes layers if they were.
        /// </summary>
        [ContextMenu("Set terrain layers")]
        public void SetTerrainLayers()
        {
            TerrainLayer[] layers = TerraForgeTerrainPainterUtilities.SettingsToLayers(layerSettings);

            foreach (Terrain terrain in terrains)
            {
                terrain.terrainData.terrainLayers = layers;
                terrain.terrainData.SetBaseMapDirty();

    #if UNITY_EDITOR
                EditorUtility.SetDirty(terrain.terrainData);
    #endif
            }
        }

        // Only should be called after the modifiers have been applied, otherwise it acts as a persistent brush
        private void ApplyStampsToTerrain(Terrain terrain)
        {
            ApplyStamps(terrain);
        }

        partial void ApplyAllStamps();

        partial void ApplyStamps(Terrain terrain);
    }
}