// IMap.cs
// Interface for map objects.
// TerraForge 2.0.0

namespace TerraForge2.Scripts.Generators.Abstract
{
    /// <summary> 
    /// Interface for map objects.
    /// </summary>
    public interface IMap
    {
        /// <summary>
        /// Sets the size of the map.
        /// </summary>
        /// <param name="width">The width of the map.</param>
        /// <param name="height">The height of the map.</param>
        void SetSize(int width, int height);
    }
}