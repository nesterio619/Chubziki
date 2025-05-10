// TerraForgeTerrainPainterAttributes.cs
// Contains custom property attributes for Unity Inspector.
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
    /// Contains custom property attributes for Unity Inspector.
    /// </summary>
    public class TerraForgeTerrainPainterAttributes
    {
        /// <summary>
        /// A property attribute for a dropdown with resolution values.
        /// </summary>
        public class ResolutionDropdown : PropertyAttribute
        {
            /// <summary>
            /// Minimum value of the resolution dropdown.
            /// </summary>
            public int min;

            /// <summary>
            /// Maximum value of the resolution dropdown.
            /// </summary>
            public int max;
            
            /// <summary>
            /// Constructs a ResolutionDropdown attribute with specified minimum and maximum values.
            /// </summary>
            /// <param name="min">Minimum value of the dropdown.</param>
            /// <param name="max">Maximum value of the dropdown.</param>
            public ResolutionDropdown(int min, int max)
            {
                this.min = min;
                this.max = max;
            }
        }
        
        /// <summary>
        /// A property attribute for a slider with minimum and maximum values.
        /// </summary>
        public class MinMaxSlider : PropertyAttribute
        {
            /// <summary>
            /// Minimum value of the slider.
            /// </summary>
            public float min;

            /// <summary>
            /// Maximum value of the slider.
            /// </summary>
            public float max;

            /// <summary>
            /// Constructs a MinMaxSlider attribute with specified minimum and maximum values.
            /// </summary>
            /// <param name="min">Minimum value of the slider.</param>
            /// <param name="max">Maximum value of the slider.</param>
            public MinMaxSlider(float min, float max)
            {
                this.min = min;
                this.max = max;
            }
        }
        
        // Placeholder class for future development
        public class ChannelPicker : PropertyAttribute
        {

        }
    }
}
