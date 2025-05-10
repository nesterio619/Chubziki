// PerlinMap.cs
// It is designed to create two-dimensional Perlin noise maps,
// which are used to generate realistic and diverse terrain features.
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
    /// Generates a Perlin noise map.
    /// </summary>
    public class PerlinMap : IMap
    {
        /// <summary>
        /// Gets or sets the size of the noise map.
        /// </summary>
        [Tooltip("Size of the noise map.")]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the number of octaves for the noise generation.
        /// </summary>
        [Tooltip("Number of octaves for the noise generation.")]
        public int octaves { get; set; }

        /// <summary>
        /// Gets or sets the scale of the noise.
        /// </summary>
        [Tooltip("Scale of the noise.")]
        public float scale { get; set; }

        /// <summary>
        /// Gets or sets the seed for the noise generation.
        /// </summary>
        [Tooltip("Seed for the noise generation.")]
        public float seed { get; set; }

        /// <summary>
        /// Gets or sets the persistence value for the noise generation.
        /// </summary>
        [Tooltip("Persistence value for the noise generation.")]
        public float persistence { get; set; }

        /// <summary>
        /// Gets or sets the lacunarity value for the noise generation.
        /// </summary>
        [Tooltip("Lacunarity value for the noise generation.")]
        public float lacunarity { get; set; }

        /// <summary>
        /// The type of fractal noise to use.
        /// </summary>
        [Tooltip("The type of fractal noise to use.")]
        public FastNoiseLite.fractalType fractalType;

        /// <summary>
        /// The type of noise to use.
        /// </summary>
        [Tooltip("The type of noise to use.")]
        public FastNoiseLite.noiseType noiseType;

        /// <summary>
        /// Sets the size of the noise map.
        /// </summary>
        /// <param name="width">The width of the map.</param>
        /// <param name="height">The height of the map.</param>
        public void SetSize(int width, int height)
        {
            // Set the size of the noise map based on the larger of width and height.
            Size = Mathf.Max(width, height);
        }

        /// <summary>
        /// Generates the noise map.
        /// </summary>
        /// <returns>A 2D array representing the noise map.</returns>
        public float[,] Generate()
        {
            // Generate the noise map using the default noise settings.
            return GenerateNoiseMap(out _, out _);
        }

        /// <summary>
        /// Generates the noise map and returns the maximum and minimum noise heights.
        /// </summary>
        /// <param name="maxLocalNoiseHeight">The maximum noise height.</param>
        /// <param name="minLocalNoiseHeight">The minimum noise height.</param>
        /// <returns>A 2D array representing the noise map.</returns>
        public float[,] Generate(out float maxLocalNoiseHeight, out float minLocalNoiseHeight)
        {
            // Generate the noise map and get the maximum and minimum noise heights.
            return GenerateNoiseMap(out maxLocalNoiseHeight, out minLocalNoiseHeight);
        }

        /// <summary>
        /// Generates the noise map and calculates the maximum and minimum noise heights.
        /// </summary>
        /// <param name="maxLocalNoiseHeight">The maximum noise height.</param>
        /// <param name="minLocalNoiseHeight">The minimum noise height.</param>
        /// <returns>A 2D array representing the noise map.</returns>
        private float[,] GenerateNoiseMap(out float maxLocalNoiseHeight, out float minLocalNoiseHeight)
        {
            // Create a 2D array to store the noise map.
            float[,] noiseMap = new float[Size, Size];

            // Create a new FastNoiseLite object to generate the noise.
            FastNoiseLite noise = new FastNoiseLite();

            // Configure the FastNoiseLite object with the provided noise settings.
            noise.SetNoiseType(noiseType);
            noise.SetFractalType(fractalType);
            noise.SetFrequency(0.05f); // Frequency determines the coarseness of the noise.
            noise.SetSeed(Mathf.FloorToInt(seed));
            noise.SetFractalOctaves(octaves);
            noise.SetFractalLacunarity(lacunarity);
            noise.SetFractalGain(persistence);

            // Initialize the max and min noise heights to track the range of noise values.
            maxLocalNoiseHeight = float.MinValue;
            minLocalNoiseHeight = float.MaxValue;

            // Calculate the halfSize of the noise map.
            float halfSize = Size / 2f;

            // Generate the noise map for each point (x, y).
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    // Calculate the sample position in world space.
                    float sampleX = (x - halfSize) / (float)Size * scale + seed;
                    float sampleY = (y - halfSize) / (float)Size * scale + seed;

                    // Get the noise value at the sample position.
                    float noiseHeight = noise.GetNoise(sampleX, sampleY);

                    // Store the noise value in the noise map.
                    noiseMap[x, y] = noiseHeight;

                    // Update the maximum and minimum noise heights.
                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }
                }
            }

            // Return the generated noise map.
            return noiseMap;
        }
    }
}