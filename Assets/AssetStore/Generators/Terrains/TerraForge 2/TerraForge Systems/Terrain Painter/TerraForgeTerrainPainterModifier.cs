using System;
using UnityEngine;
using UnityEngine.Rendering;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.TerrainPainter
{
    /// <summary>
    /// Represents a base class for all terrain modifiers.
    /// </summary>
    [System.Serializable]
    public class TerraForgeTerrainPainterModifier : ScriptableObject
    {
        /// <summary>
        /// Determines if the modifier is enabled.
        /// </summary>
        [HideInInspector]
        public bool enabled = true;

        /// <summary>
        /// The label for the modifier.
        /// </summary>
        [HideInInspector]
        public string label;

        /// <summary>
        /// The blend mode used by the modifier.
        /// </summary>
        [HideInInspector]
        public BlendMode blendMode;

        /// <summary>
        /// The opacity of the modifier.
        /// </summary>
        [HideInInspector]
        [Range(0f, 100f)]
        public float opacity = 100;

        /// <summary>
        /// Enum for filter passes.
        /// </summary>
        public enum FilterPass
        {
            TerraForgeTerrainPainterHeight,
            TerraForgeTerrainPainterSlope,
            Curvature,
            TextureMask,
            TerraForgeTerrainPainterNoise
        }

        /// <summary>
        /// The filter pass index used by the modifier.
        /// </summary>
        [HideInInspector]
        public FilterPass passIndex;

        /// <summary>
        /// Enum for blend modes.
        /// </summary>
        public enum BlendMode
        {
            Multiply,
            Add,
            Subtract,
        }

        [SerializeField]
        public TerrainData terrainData;

        private static int SrcFactorID = Shader.PropertyToID("_SrcFactor");
        private static int DstFactorID = Shader.PropertyToID("_DstFactor");
        private static int BlendOpID = Shader.PropertyToID("_BlendOp");
        private static int OpacityID = Shader.PropertyToID("_Opacity");

        /// <summary>
        /// Sets the blend mode on the specified material.
        /// </summary>
        /// <param name="mat">The material to set the blend mode on.</param>
        /// <param name="mode">The blend mode to set.</param>
        public static void SetBlendMode(Material mat, BlendMode mode)
        {
            switch (mode)
            {
                case BlendMode.Multiply:
                    mat.SetInt(SrcFactorID, (int)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetInt(DstFactorID, (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    mat.SetInt(BlendOpID, (int)BlendOp.Multiply);
                    break;
                case BlendMode.Add:
                    mat.SetInt(SrcFactorID, (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    mat.SetInt(DstFactorID, (int)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetInt(BlendOpID, (int)BlendOp.Add);
                    break;
                case BlendMode.Subtract:
                    mat.SetInt(SrcFactorID, (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    mat.SetInt(DstFactorID, (int)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetInt(BlendOpID, (int)BlendOp.ReverseSubtract);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        /// <summary>
        /// Configures the material properties. The base implementation must be called, it sets the blend mode and opacity.
        /// </summary>
        /// <param name="material">The material to configure.</param>
        public virtual void Configure(Material material, Terrain terrain)
        {
            SetBlendMode(material, blendMode);
            material.SetFloat(OpacityID, opacity * 0.01f);
        }

        /// <summary>
        /// Executes the modifier on the specified render texture.
        /// </summary>
        /// <param name="target">The target render texture.</param>
        public virtual void Execute(RenderTexture target)
        {
            if (!enabled || opacity == 0) return;

            Graphics.Blit(null, target, TerraForgeTerrainPainterModifierStack.filterMat, (int)passIndex);
        }
    }
}
