// FalloffMap.cs
// It is needed for generating a falloff map, which creates a smooth gradient from the center to the edges of a terrain.
// TerraForge 2.0.0

using UnityEngine;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.Generators.Maps
{   
    /// <summary>
    /// Generates a falloff map, which creates a smooth gradient from the center to the edges.
    /// </summary>
    public class FalloffMap : IMap
    {
        /// <summary>
        /// Controls how steep the falloff curve is.
        /// </summary>
        [Tooltip("Controls how steep the falloff curve is.")]
        public float falloffAngleFactor; 

        /// <summary>
        /// Controls the range of the falloff effect.
        /// </summary>
        [Tooltip("Controls the range of the falloff effect.")]
        public float falloffRange;     

        /// <summary>
        /// The size of the falloff map (width and height).
        /// </summary>
        [Tooltip("The size of the falloff map (width and height).")]
        public int Size;               

        /// <summary>
        /// Sets the size of the falloff map. This method is part of the IMap interface but is not used here.
        /// </summary>
        /// <param name="width">The width of the map.</param>
        /// <param name="height">The height of the map.</param>
        public void SetSize(int width, int height)
        {
            Size = Mathf.Max(width, height);
        }

        /// <summary>
        /// Generates the falloff map as a 2D array of floats.
        /// </summary>
        /// <returns>A 2D array representing the falloff map.</returns>
        public float[,] Generate()
        {
            // Create a 2D array to store the falloff map
            float[,] map = new float[Size, Size];

            // Loop through each pixel of the map
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    // Convert the pixel coordinates to a range from -1 to 1
                    float x = i / (float)Size * 2 - 1;
                    float y = j / (float)Size * 2 - 1;

                    // Calculate the distance from the center of the map
                    float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                    // Evaluate the falloff function and store the result in the map
                    map[i, j] = Evaluate(value);
                }
            }

            // Return the generated falloff map
            return map;
        }

        /// <summary>
        /// Evaluates the falloff function for a given value.
        /// </summary>
        /// <param name="value">The input value to evaluate.</param>
        /// <returns>The result of the falloff function.</returns>
        float Evaluate(float value)
        {
            // Apply a falloff function to the input value
            // The result will be in the range from 0 to 1
            // The falloff function is designed to create a smooth gradient from the center to the edges
            // The falloffAngleFactor and falloffRange variables control the shape of the gradient
            return Mathf.Pow(value, falloffAngleFactor) / (Mathf.Pow(value, falloffAngleFactor) + Mathf.Pow(falloffRange - falloffRange * value, falloffAngleFactor));
        }
    }
}