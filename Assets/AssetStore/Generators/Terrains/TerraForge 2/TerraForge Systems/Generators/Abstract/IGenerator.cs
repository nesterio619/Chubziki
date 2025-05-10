// IGenerator.cs
// Interface for map generation.
// TerraForge 2.0.0

namespace TerraForge2.Scripts.Generators.Abstract
{
    /// <summary>
    /// Interface for map generation.
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Generates the map.
        /// </summary>
        void Generate();
    }
}