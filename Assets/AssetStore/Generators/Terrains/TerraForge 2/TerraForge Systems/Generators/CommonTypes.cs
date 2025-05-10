// CommonTypes.cs
// Essential for maintaining organized, readable, and consistent code across the Unity project by centralizing common data structures and types.
// It prevents duplicate definitions and facilitates easy reference and modification of these types throughout the project.
// TerraForge 2.0.0

using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_BURST
    using static Unity.Mathematics.math;
    using Unity.Mathematics;
#endif
using Unity.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_BURST
    using Unity.Burst;
    using Unity.Jobs;
#endif
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.Generators
{
    /// <summary>
    /// Enum for terrain heightmap resolution settings.
    /// </summary>
    public enum TerrainResolution
    {
        Resolution33 = 33,
        Resolution65 = 65,
        Resolution129 = 129,
        Resolution257 = 257,
        Resolution513 = 513,
        Resolution1025 = 1025,
        Resolution2049 = 2049,
        Resolution4097 = 4097
    }

    /// <summary>
    /// Enumeration of resolutions for hydraulic erosion.
    /// </summary>
    public enum HydraulicErosionResolution
    {

        Resolution129 = 129,
        Resolution257 = 257,
        Resolution513 = 513,
        Resolution1025 = 1025
    }

    /// <summary>
    /// General settings for terrain generation.
    /// </summary>
    [System.Serializable]
    public class GeneralTerrainSettings
    {
        /// <summary>
        /// Automatically update the terrain when changes are made.
        /// </summary>
        [Tooltip("Automatically update the terrain when changes are made.")]
        public bool autoUpdateTerrainGeneration;

        /// <summary>
        /// Indicates if there's a new generation waiting.
        /// </summary>
        [NonSerialized] public bool isNewGenerationWaiting;

        /// <summary>
        /// Height of the terrain. Changes the overall height of the terrain vertices in relation to the game terrain object
        /// </summary>
        [Tooltip("Height of the terrain.")]
        public float terrainHeight;

        /// <summary>
        /// The size of the terrain.
        /// </summary>
        [Min(10)]
        public float terrainSize = 1000f;

        /// <summary>
        /// Buffer copy of terrainSize to detect changes.
        /// </summary>
        [HideInInspector]
        public float bufferTerrainSize;

        /// <summary>
        /// Resolution of the terrain.
        /// </summary>
        [Tooltip("Resolution of the terrain.")]
        public TerrainResolution terrainResolution = TerrainResolution.Resolution513;

        /// <summary>
        /// Buffer copy of terrainResolution to detect changes.
        /// </summary>
        [HideInInspector]
        public TerrainResolution bufferTerrainResolution;

        /// <summary>
        /// Data for the current terrain.
        /// </summary>
        [Tooltip("Data for the current terrain.")]
        public TerrainData currentTerrainData;

        /// <summary>
        /// Flag to save terrain data.
        /// </summary>
        [Tooltip("Save the generation in the terrainData file so that you do not lose the generation data. It is recommended to leave it switched on")]
        public bool saveTerrainData;

        /// <summary>
        /// Path to Save current Terrain Data [in the Runtime].
        /// </summary>
        [HideInInspector]
        public string currentTerrainDataFileRuntimePath;

        /// <summary>
        /// Material of the terrain.
        /// </summary>
        [Tooltip("Material of the terrain.")]
        public Material terrainMaterial;

        /// <summary>
        /// Buffer copy of terrainMaterial to detect changes.
        /// </summary>
        [HideInInspector]
        public Material bufferTerrainMaterial;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralTerrainSettings"/> class by copying another instance.
        /// </summary>
        /// <param name="other">The instance to copy.</param>
        public GeneralTerrainSettings(GeneralTerrainSettings other)
        {
            this.autoUpdateTerrainGeneration = other.autoUpdateTerrainGeneration;
            this.terrainHeight = other.terrainHeight;
            this.terrainSize = other.terrainSize;
            this.bufferTerrainSize = other.bufferTerrainSize;
            this.terrainResolution = other.terrainResolution;
            this.bufferTerrainResolution = other.bufferTerrainResolution;
            this.currentTerrainData = other.currentTerrainData;
            this.saveTerrainData = other.saveTerrainData;
            this.currentTerrainDataFileRuntimePath = other.currentTerrainDataFileRuntimePath;
            this.terrainMaterial = other.terrainMaterial;
            this.bufferTerrainMaterial = other.bufferTerrainMaterial;
        }
    }

    /// <summary>
    /// Settings of the generated terrain noise layer.
    /// </summary>
    [System.Serializable]
    public class TerrainLayerSettings
    {
        /// <summary>
        /// Indicates if the terrain noise layer is enabled.
        /// </summary>
        [Tooltip("Indicates if the terrain noise layer is enabled.")]
        public bool isEnabled = true;

        /// <summary>
        /// The blur radius of the terrain noise layer.
        /// </summary>
        [Range(0, 30)]
        [Tooltip("The blur radius of the terrain noise layer.")]
        public int blurRadius = 0;

        /// <summary>
        /// The noise type used for generating the terrain noise layer.
        /// </summary>
        [Tooltip("The noise type used for generating the terrain noise layer.")]
        public FastNoiseLite.noiseType noiseType = FastNoiseLite.noiseType.OpenSimplex2;

        /// <summary>
        /// The fractal type used for generating the terrain noise layer.
        /// </summary>
        [Tooltip("The fractal type used for generating the terrain noise layer.")]
        public FastNoiseLite.fractalType fractalType = FastNoiseLite.fractalType.Ridged;

        /// <summary>
        /// The depth of the terrain noise layer.
        /// </summary>
        [Min(0f)]
        [Tooltip("The depth of the terrain noise layer.")]
        public float depth = 10;

        /// <summary>
        /// The scale of the terrain noise layer.
        /// </summary>
        [Min(0.1f)]
        [Tooltip("The scale of the terrain noise layer.")]
        public float scale = 50f;

        /// <summary>
        /// The number of octaves used for generating the terrain noise layer.
        /// </summary>
        [Range(3, 8)]
        [Tooltip("The number of octaves used for generating the terrain noise layer.")]
        public int octaves = 4;

        /// <summary>
        /// The lacunarity of the terrain noise layer.
        /// </summary>
        [Tooltip("The lacunarity of the terrain noise layer.")]
        [Range(0f, 10f)]
        public float lacunarity = 2f;

        /// <summary>
        /// The persistence of the terrain noise layer.
        /// </summary>
        [Range(0.3f, 1)]
        [Tooltip("The persistence of the terrain noise layer.")]
        public float persistence = 0.5f;

        /// <summary>
        /// The seed used for generating the terrain noise layer.
        /// </summary>
        [Min(0f)]
        [Tooltip("The seed used for generating the terrain noise layer.")]
    
        public float seed = 100f;

        /// <summary>
        /// Specifies whether the height curve is to be inverted.
        /// </summary>
        [Tooltip("Specifies whether the height curve is to be inverted.")]
        public bool inversion = false;

        /// <summary>
        /// The height curve used for generating the terrain noise layer.
        /// </summary>
        [Tooltip("The height curve used for generating the terrain noise layer.")]
        public AnimationCurve heightCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        /// <summary>
        /// Inverted heightCurve (mirrored horizontally).
        /// </summary>
        [HideInInspector]
        public AnimationCurve invertedHeightCurve;

        /// <summary>
        /// Indicates if a falloff map is used for the terrain noise layer.
        /// </summary>
        [Tooltip("Indicates if a falloff map is used for the terrain noise layer.")]
        public bool useFalloffMap;

        /// <summary>
        /// Affects the "angle of cut" on faces on the falloff map.
        /// </summary>
        [Range(0.01f, 6)]
        [Tooltip("Affects the angle of cut on faces on the falloff map.")]
        public float falloffAngleFactor = 3f;

        /// <summary>
        /// The range of falloff for the terrain noise layer.
        /// </summary>
        [Range(0.1f, 60)]
        [Tooltip("The range of falloff for the terrain noise layer.")]
        public float falloffRange = 3f;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainLayerSettings"/> class by copying another instance.
        /// </summary>
        /// <param name="other">The instance to copy.</param>
        public TerrainLayerSettings(TerrainLayerSettings other)
        {
            this.isEnabled = other.isEnabled;
            this.blurRadius = other.blurRadius;
            this.noiseType = other.noiseType;
            this.fractalType = other.fractalType;
            this.depth = other.depth;
            this.scale = other.scale;
            this.octaves = other.octaves;
            this.lacunarity = other.lacunarity;
            this.persistence = other.persistence;
            this.seed = other.seed;
            this.inversion = other.inversion;
            this.heightCurve = new AnimationCurve(other.heightCurve.keys);
            this.invertedHeightCurve = other.invertedHeightCurve;
            this.useFalloffMap = other.useFalloffMap;
            this.falloffAngleFactor = other.falloffAngleFactor;
            this.falloffRange = other.falloffRange;
        }
    }

    /// <summary>
    /// Settings for terrain edge smoothing.
    /// </summary>
    [System.Serializable]
    public class TerrainEdgeSmoothingSettings
    {
        /// <summary>
        /// Indicates whether the edge smoothing is enabled.
        /// </summary>
        [Tooltip("Indicates whether the edge smoothing is enabled.")]
        public bool isEnabled = true;

        /// <summary>
        /// Radius for blurring the edges of the terrain.
        /// </summary>
        [Range(0, 30)]
        [Tooltip("Radius for blurring the edges of the terrain.")]
        public int blurRadius = 1;

        /// <summary>
        /// Width of the falloff transition for smoothing edges.
        /// </summary>
        [Range(0.001f, 0.5f)]
        [Tooltip("Width of the falloff transition for smoothing edges.")]
        public float falloffTransitionWidth = 3f;

        /// <summary>
        /// Range of the falloff for smoothing edges.
        /// </summary>
        [Range(0.001f, 0.5f)]
        [Tooltip("Range of the falloff for smoothing edges.")]
        public float falloffRange = 3f;
    }

    #if UNITY_BURST
        /// <summary>
        /// Job for performing blur operation on a source map.
        /// </summary>
        [BurstCompile]
        public struct BlurJob : IJobParallelFor
        {
            /// <summary>
            /// The source map to blur.
            /// </summary>
            [ReadOnly] public NativeArray<float> sourceMap;

            /// <summary>
            /// The blurred map to write the results to.
            /// </summary>
            [WriteOnly] public NativeArray<float> blurredMap;

            /// <summary>
            /// The width of the source and blurred maps.
            /// </summary>
            public int width;

            /// <summary>
            /// The height of the source and blurred maps.
            /// </summary>
            public int height;

            /// <summary>
            /// The radius of the blur operation.
            /// </summary>
            public int blurRadius;

            public void Execute(int index)
            {
                int x = index % width;
                int y = index / width;

                float sum = 0f;
                int count = 0;

                for (int offsetY = -blurRadius; offsetY <= blurRadius; offsetY++)
                {
                    for (int offsetX = -blurRadius; offsetX <= blurRadius; offsetX++)
                    {
                        int neighborX = Mathf.Clamp(x + offsetX, 0, width - 1);
                        int neighborY = Mathf.Clamp(y + offsetY, 0, height - 1);
                        int neighborIndex = neighborY * width + neighborX;

                        sum += sourceMap[neighborIndex];
                        count++;
                    }
                }

                blurredMap[index] = sum / count;
            }
        }
    #endif

    /// <summary>
    /// Settings for terrain hydraulic erosion layer.
    /// </summary>
    [System.Serializable]
    public class HydraulicErosionLayerSettings
    {
        /// <summary>
        /// Indicates whether the hydraulic erosion layer is enabled.
        /// </summary>
        public bool isEnabled = true;

        /// <summary>
        /// The number of iterations for the hydraulic erosion process.
        /// </summary>
        [Range(50000, 150000)]
        [Tooltip("The number of iterations for the hydraulic erosion process.")]
        public int iterations = 100000;

        /// <summary>
        /// Resolution of the terrain for erosion.
        /// </summary>
        [Tooltip("Resolution of the terrain.")]
        public HydraulicErosionResolution erosionResolution = HydraulicErosionResolution.Resolution257;

        /// <summary>
        /// Number of erosion steps per iteration.
        /// </summary>
        [Range(1, 4)]
        [Tooltip("Number of erosion steps per iteration.")]
        public int erosionSteps = 3;

        /// <summary>
        /// Radius of the erosion effect.
        /// </summary>
        [Range(1, 10)]
        [Tooltip("Radius of the erosion effect.")]
        public int erosionRadius = 3;

        /// <summary>
        /// At zero, water will instantly change direction to flow downhill. At 1, water will never change direction.
        /// </summary>
        [Range(0.005f, 0.1f)]
        [Tooltip("At zero, water will instantly change direction to flow downhill. At 1, water will never change direction.")]
        public float inertia = 0.05f;

        /// <summary>
        /// Minimum sediment capacity, influencing how much sediment a droplet can carry.
        /// </summary>
        [Range(0.01f, 0.05f)]
        [Tooltip("Minimum sediment capacity, influencing how much sediment a droplet can carry.")]
        public float minSedimentCapacity = 0.01f;

        /// <summary>
        /// Maximum lifetime of a droplet.
        /// </summary>
        [Range(1, 100)]
        [Tooltip("Maximum lifetime of a droplet.")]
        public int maxDropletLifetime = 30;

        /// <summary>
        /// Initial volume of water in a droplet.
        /// </summary>
        [Range(0.1f, 5)]
        [Tooltip("Initial volume of water in a droplet.")]
        public float initialWaterVolume = 1.0f;

        /// <summary>
        /// Indicates whether the hydraulic erosion layer settings have been modified.
        /// </summary>
        [HideInInspector]
        public bool isHydraulicErosionLayerModified;

        /// <summary>
        /// Strength of the erosion effect.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Strength of the erosion effect.")]
        public float strength = 1f;

        /// <summary>
        /// Indicates whether to use a falloff map for erosion.
        /// </summary>
        [Tooltip("Indicates whether to use a falloff map for erosion.")]
        public bool useFalloffMap = true;

        /// <summary>
        /// Width of the falloff transition for erosion.
        /// </summary>
        [Range(0.001f, 0.5f)]
        [Tooltip("Width of the falloff transition for erosion.")]
        public float falloffTransitionWidth = 3f;

        /// <summary>
        /// Range of the falloff for erosion.
        /// </summary>
        [Range(0.001f, 0.5f)]
        [Tooltip("Range of the falloff for erosion.")]
        public float falloffRange = 3f;

        /// <summary>
        /// Initializes a new instance of the <see cref="HydraulicErosionLayerSettings"/> class by copying another instance.
        /// </summary>
        /// <param name="other">The instance to copy.</param>
        public HydraulicErosionLayerSettings(HydraulicErosionLayerSettings other)
        {
            this.isEnabled = other.isEnabled;
            this.iterations = other.iterations;
            this.erosionResolution = other.erosionResolution;
            this.erosionSteps = other.erosionSteps;
            this.erosionRadius = other.erosionRadius;
            this.inertia = other.inertia;
            this.minSedimentCapacity = other.minSedimentCapacity;
            this.maxDropletLifetime = other.maxDropletLifetime;
            this.initialWaterVolume = other.initialWaterVolume;
            this.strength = other.strength;
            this.useFalloffMap = other.useFalloffMap;
            this.falloffTransitionWidth = other.falloffTransitionWidth;
            this.falloffRange = other.falloffRange;
        }
    }

    /// <summary>
    /// Static class responsible for handling hydraulic erosion on terrain.
    /// </summary>
    public static class HydraulicsSystem
    {
        /// <summary>
        /// Indices of the erosion brush precomputed for every node.
        /// </summary>
        static int[][] erosionBrushIndices;

        /// <summary>
        /// Weights of the erosion brush precomputed for every node.
        /// </summary>
        static float[][] erosionBrushWeights;

        /// <summary>
        /// Pseudo-random number generator for erosion.
        /// </summary>
        static System.Random prng;

        /// <summary>
        /// Current radius of the erosion effect.
        /// </summary>
        static int currentErosionRadius;

        /// <summary>
        /// Current size of the map.
        /// </summary>
        static int currentMapSize;

        /// <summary>
        /// Performs hydraulic erosion on the given heightmap.
        /// </summary>
        /// <param name="erosion">The compute shader used for erosion.</param>
        /// <param name="settings">Settings for the hydraulic erosion process.</param>
        /// <param name="numIterations">Number of iterations for the erosion process.</param>
        /// <param name="heightmap">The heightmap to be eroded.</param>
        /// <param name="heightmapResolution">The resolution of the heightmap.</param>
        /// <param name="erosionResolution">The resolution to be used for erosion.</param>
        /// <returns>The eroded heightmap.</returns>
        public static float[,] GenerateHydraulicErosion(ComputeShader erosion, HydraulicErosionLayerSettings settings, int numIterations, float[,] heightmap, int heightmapResolution, int erosionResolution)
        {
            if (settings == null)
            {
                Debug.LogError("HydraulicErosionLayerSettings 'settings' is null.");
                return heightmap;
            }

            if (heightmap == null)
            {
                Debug.LogError("Heightmap is null.");
                return null;
            }

            // Erosion parameters
            float inertia = settings.inertia;
            float sedimentCapacityFactor = 4f;
            float minSedimentCapacity = settings.minSedimentCapacity;
            float erosionRadius = settings.erosionRadius;
            float erodeSpeed = 0.3f;
            float depositSpeed = 0.3f;
            float evaporateSpeed = 0.01f;
            float gravity = 4f;
            int maxDropletLifetime = settings.maxDropletLifetime;
            float initialWaterVolume = settings.initialWaterVolume;
            float initialDropletSpeed = 1f;

            int erosionMapSize = erosionResolution;
            int originalMapSize = heightmapResolution;
            float[,] outputHeightmap = heightmap;

            // Convert heightmap to 1D array for compute shader
            List<float> heightmap1DList = new List<float>();
            for (int x = 0; x < erosionMapSize; x++)
            {
                for (int y = 0; y < erosionMapSize; y++)
                {
                    int sourceX = Mathf.FloorToInt(((float)x / (erosionMapSize - 1)) * (originalMapSize - 1));
                    int sourceY = Mathf.FloorToInt(((float)y / (erosionMapSize - 1)) * (originalMapSize - 1));
                    heightmap1DList.Add(outputHeightmap[sourceX, sourceY]);
                }
            }
            float[] heightmap1DArray = heightmap1DList.ToArray();

            int numComputeThreads = numIterations / 1024;

            // Create brush for erosion effect
            List<int> brushIndices = new List<int>();
            List<float> brushWeights = new List<float>();
            float totalBrushWeight = 0;

            // Calculate brush offsets and weights
            for (int brushY = -settings.erosionRadius; brushY <= settings.erosionRadius; brushY++)
            {
                for (int brushX = -settings.erosionRadius; brushX <= settings.erosionRadius; brushX++)
                {
                    float distanceSquared = brushX * brushX + brushY * brushY;
                    if (distanceSquared < settings.erosionRadius * settings.erosionRadius)
                    {
                        brushIndices.Add(brushY * erosionMapSize + brushX);
                        float brushWeight = 1 - Mathf.Sqrt(distanceSquared) / settings.erosionRadius;
                        totalBrushWeight += brushWeight;
                        brushWeights.Add(brushWeight);
                    }
                }
            }
            // Normalize brush weights
            for (int i = 0; i < brushWeights.Count; i++)
            {
                brushWeights[i] /= totalBrushWeight;
            }

            // Send brush data to compute shader
            ComputeBuffer brushIndexBuffer = new ComputeBuffer(brushIndices.Count, sizeof(int));
            ComputeBuffer brushWeightBuffer = new ComputeBuffer(brushWeights.Count, sizeof(float));
            brushIndexBuffer.SetData(brushIndices);
            brushWeightBuffer.SetData(brushWeights);
            erosion.SetBuffer(0, "brushIndices", brushIndexBuffer);
            erosion.SetBuffer(0, "brushWeights", brushWeightBuffer);

            // Generate random indices for droplet placement
            int[] dropletIndices = new int[numIterations];
            for (int i = 0; i < numIterations; i++)
            {
                int randomX = UnityEngine.Random.Range(settings.erosionRadius, erosionMapSize + settings.erosionRadius);
                int randomY = UnityEngine.Random.Range(settings.erosionRadius, erosionMapSize + settings.erosionRadius);
                dropletIndices[i] = randomY * erosionMapSize + randomX;
            }

            // Send random indices to compute shader
            ComputeBuffer dropletIndexBuffer = new ComputeBuffer(dropletIndices.Length, sizeof(int));
            dropletIndexBuffer.SetData(dropletIndices);
            erosion.SetBuffer(0, "randomIndices", dropletIndexBuffer);

            // Heightmap buffer
            ComputeBuffer heightmapBuffer = new ComputeBuffer(heightmap1DArray.Length, sizeof(float));
            heightmapBuffer.SetData(heightmap1DArray);
            erosion.SetBuffer(0, "map", heightmapBuffer);

            // Set erosion settings in compute shader
            erosion.SetInt("borderSize", settings.erosionRadius);
            erosion.SetInt("mapSize", erosionMapSize);
            erosion.SetInt("brushLength", brushIndices.Count);
            erosion.SetInt("maxLifetime", maxDropletLifetime);
            erosion.SetFloat("inertia", inertia);
            erosion.SetFloat("sedimentCapacityFactor", sedimentCapacityFactor);
            erosion.SetFloat("minSedimentCapacity", minSedimentCapacity);
            erosion.SetFloat("depositSpeed", depositSpeed);
            erosion.SetFloat("erodeSpeed", erodeSpeed);
            erosion.SetFloat("evaporateSpeed", evaporateSpeed);
            erosion.SetFloat("gravity", gravity);
            erosion.SetFloat("startSpeed", initialDropletSpeed);
            erosion.SetFloat("startWater", initialWaterVolume);

            // Run compute shader
            try
            {
                erosion.Dispatch(0, numComputeThreads, 1, 1);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during erosion dispatch: {e.Message}");
                // Release buffers to avoid memory leaks
                heightmapBuffer.Release();
                dropletIndexBuffer.Release();
                brushIndexBuffer.Release();
                brushWeightBuffer.Release();
                return heightmap;
            }

            // Retrieve data from the compute shader
            heightmapBuffer.GetData(heightmap1DArray);

            // Release buffers
            heightmapBuffer.Release();
            dropletIndexBuffer.Release();
            brushIndexBuffer.Release();
            brushWeightBuffer.Release();

            // Normalize heightmap back to 2D array
            int currentX = 0;
            int currentY = 0;
            float[,] scaledHeightmap = new float[erosionMapSize, erosionMapSize];
            for (int i = 0; i < heightmap1DArray.Length; i++)
            {
                scaledHeightmap[currentY, currentX] = heightmap1DArray[i];
                if (currentX == erosionMapSize - 1)
                {
                    currentX = 0;
                    currentY += 1;
                }
                else
                {
                    currentX += 1;
                }
            }

            // Upscale heightmap to original resolution
            for (int x = 0; x < originalMapSize; x++)
            {
                for (int y = 0; y < originalMapSize; y++)
                {
                    float scaledX = (((float)x / (originalMapSize - 1)) * (erosionMapSize - 1));
                    float scaledY = (((float)y / (originalMapSize - 1)) * (erosionMapSize - 1));

                    int x1 = Mathf.FloorToInt(scaledX);
                    int y1 = Mathf.FloorToInt(scaledY);

                    int x2 = Mathf.FloorToInt(scaledX) + 1;
                    int y2 = Mathf.FloorToInt(scaledY) + 1;

                    float lerpValueX = scaledX - Mathf.FloorToInt(scaledX);
                    float lerpValueY = scaledY - Mathf.FloorToInt(scaledY);

                    if (x2 < erosionMapSize && y2 < erosionMapSize)
                    {
                        float interpolatedHeightX = Mathf.Lerp(scaledHeightmap[x1, y1], scaledHeightmap[x2, y1], lerpValueX);
                        float interpolatedHeightY = Mathf.Lerp(scaledHeightmap[x1, y2], scaledHeightmap[x2, y2], lerpValueX);
                        float finalHeight = Mathf.Lerp(interpolatedHeightX, interpolatedHeightY, lerpValueY);

                        outputHeightmap[x, y] = finalHeight;
                    }
                }
            }

            return outputHeightmap;
        }

    }

    /// <summary>
    /// Settings for the stitch heights generator (contacting on the same side).
    /// </summary>
    [System.Serializable]
    public class ContactHeightsGenerator
    {
        /// <summary>
        /// The heights generator used for generating contact heights.
        /// </summary>
        [Tooltip("The heights generator used for generating contact heights.")]
        public TerraForgeTerrainGenerator heightsGenerator;

        /// <summary>
        /// The width of the transition between contact heights.
        /// </summary>
        [Tooltip("The width of the transition between contact heights.")]
        [Min(0.1f)]
        public float transitionWidth;

        /// <summary>
        /// The strength of the transition between contact heights.
        /// </summary>
        [Tooltip("The strength of the transition between contact heights.")]
        [Range(1f, 1.5f)]
        public float transitionStrength = 1f;

        /// <summary>
        /// If true, enables diagonal blending.
        /// </summary>
        [Tooltip("If true, enables diagonal blending.")]
        public bool diagonalBlending;

        /// <summary>
        /// If true, applies blending to both halves.
        /// </summary>
        [Tooltip("If true, applies blending to both halves.")]
        public bool bothHalves;

        /// <summary>
        /// If true, applies blending to the first half.
        /// </summary>
        [Tooltip("If true, applies blending to the first half.")]
        public bool firstHalf;

        /// <summary>
        /// If true, applies blending to the second half.
        /// </summary>
        [Tooltip("If true, applies blending to the second half.")]
        public bool secondHalf;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactHeightsGenerator"/> class by copying the settings from another instance.
        /// </summary>
        /// <param name="other">The <see cref="ContactHeightsGenerator"/> instance to copy settings from.</param>
        public ContactHeightsGenerator(ContactHeightsGenerator other)
        {
            this.heightsGenerator = other.heightsGenerator;
            this.transitionWidth = other.transitionWidth;
            this.transitionStrength = other.transitionStrength;
            this.diagonalBlending = other.diagonalBlending;
        }
    }

    /// <summary>
    /// Settings for contact heights generators.
    /// </summary>
    [System.Serializable]
    public class ContactHeightsGeneratorsSettings
    {
        /// <summary>
        /// Settings for the right contact stitch heights generator.
        /// </summary>
        [Tooltip("Settings for the right contact heights generator.")]
        public ContactHeightsGenerator contactHeightsGenerator_Right;

        /// <summary>
        /// Settings for the left contact stitch heights generator.
        /// </summary>
        [Tooltip("Settings for the left contact heights generator.")]
        public ContactHeightsGenerator contactHeightsGenerator_Left;

        /// <summary>
        /// Settings for the top contact stitch heights generator.
        /// </summary>
        [Tooltip("Settings for the top contact heights generator.")]
        public ContactHeightsGenerator contactHeightsGenerator_Top;

        /// <summary>
        /// Settings for the bottom contact stitch heights generator.
        /// </summary>
        [Tooltip("Settings for the bottom contact heights generator.")]
        public ContactHeightsGenerator contactHeightsGenerator_Bottom;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactHeightsGeneratorsSettings"/> class by copying the settings from another instance.
        /// </summary>
        /// <param name="other">The <see cref="ContactHeightsGeneratorsSettings"/> instance to copy settings from.</param>
        public ContactHeightsGeneratorsSettings(ContactHeightsGeneratorsSettings other)
        {
            this.contactHeightsGenerator_Right = other.contactHeightsGenerator_Right;
            this.contactHeightsGenerator_Left = other.contactHeightsGenerator_Left;
            this.contactHeightsGenerator_Top = other.contactHeightsGenerator_Top;
            this.contactHeightsGenerator_Bottom = other.contactHeightsGenerator_Bottom;
        }
    }

    /// <summary>
    /// Represents an element in the terrain generation queue.
    /// </summary>
    [System.Serializable]
    public class TerrainElement
    {
        /// <summary>
        /// The action number associated with the terrain element.
        /// </summary>
        [Tooltip("The action number associated with the terrain element.")]
        public int actionNumber;

        /// <summary>
        /// Index in queue for stitch height generation.
        /// </summary>
        [Tooltip("Index in queue for stitch height generation.")]
        public int indexInQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainElement"/> class.
        /// </summary>
        /// <param name="actionNumber">The action number associated with the terrain element.</param>
        /// <param name="indexInQueue">Index in queue for stitch height generation.</param>
        public TerrainElement(int actionNumber, int indexInQueue)
        {
            this.actionNumber = actionNumber;
            this.indexInQueue = indexInQueue;
        }
    }

    /// <summary>
    /// A dictionary that maps grid coordinates to terrain elements.
    /// </summary>
    public class TerrainElementDictionary
    {
        /// <summary>
        /// The dictionary storing terrain elements keyed by their grid coordinates.
        /// </summary>
        private static readonly Dictionary<(int column, int line), TerrainElement> terrainElements = new Dictionary<(int column, int line), TerrainElement>();

        /// <summary>
        /// Initializes the <see cref="TerrainElementDictionary"/> class and populates it with predefined terrain elements.
        /// </summary>
        static TerrainElementDictionary()
        {
            AddTerrainElement(0, 0, 7, 3);
            AddTerrainElement(0, 1, 2, 2);
            AddTerrainElement(0, 2, 9, 5);
            AddTerrainElement(0, 3, 12, 18);
            AddTerrainElement(0, 4, 2, 15);
            AddTerrainElement(0, 5, 9, 20);
            AddTerrainElement(0, 6, 12, 64);
            AddTerrainElement(0, 7, 2, 61);
            AddTerrainElement(0, 8, 9, 66);
            AddTerrainElement(0, 9, 12, 96);
            AddTerrainElement(0, 10, 2, 93);
            AddTerrainElement(0, 11, 9, 98);

            AddTerrainElement(1, 0, 1, 124); // No blending required
            AddTerrainElement(1, 1, 5, 1);
            AddTerrainElement(1, 2, 1, 125); // No blending required
            AddTerrainElement(1, 3, 4, 17);
            AddTerrainElement(1, 4, 5, 14);
            AddTerrainElement(1, 5, 1, 126); // No blending required
            AddTerrainElement(1, 6, 4, 63);
            AddTerrainElement(1, 7, 5, 60);
            AddTerrainElement(1, 8, 1, 127); // No blending required
            AddTerrainElement(1, 9, 4, 95);
            AddTerrainElement(1, 10, 5, 92);
            AddTerrainElement(1, 11, 1, 128); // No blending required

            AddTerrainElement(2, 0, 8, 4);
            AddTerrainElement(2, 1, 3, 3);
            AddTerrainElement(2, 2, 10, 6);
            AddTerrainElement(2, 3, 13, 19);
            AddTerrainElement(2, 4, 3, 16);
            AddTerrainElement(2, 5, 10, 21);
            AddTerrainElement(2, 6, 13, 65);
            AddTerrainElement(2, 7, 3, 62);
            AddTerrainElement(2, 8, 10, 67);
            AddTerrainElement(2, 9, 13, 97);
            AddTerrainElement(2, 10, 3, 94);
            AddTerrainElement(2, 11, 10, 99);

            AddTerrainElement(3, 0, 14, 10);
            AddTerrainElement(3, 1, 6, 8);
            AddTerrainElement(3, 2, 15, 12);
            AddTerrainElement(3, 3, 11, 26);
            AddTerrainElement(3, 4, 6, 23);
            AddTerrainElement(3, 5, 15, 28);
            AddTerrainElement(3, 6, 11, 72);
            AddTerrainElement(3, 7, 6, 69);
            AddTerrainElement(3, 8, 15, 74);
            AddTerrainElement(3, 9, 11, 104);
            AddTerrainElement(3, 10, 6, 101);
            AddTerrainElement(3, 11, 15, 106);

            AddTerrainElement(4, 0, 1, 129); // No blending required
            AddTerrainElement(4, 1, 5, 7);
            AddTerrainElement(4, 2, 1, 130); // No blending required
            AddTerrainElement(4, 3, 4, 25);
            AddTerrainElement(4, 4, 5, 22);
            AddTerrainElement(4, 5, 1, 131); // No blending required
            AddTerrainElement(4, 6, 4, 71);
            AddTerrainElement(4, 7, 5, 68);
            AddTerrainElement(4, 8, 1, 132); // No blending required
            AddTerrainElement(4, 9, 4, 103);
            AddTerrainElement(4, 10, 5, 100);
            AddTerrainElement(4, 11, 1, 133); // No blending required

            AddTerrainElement(5, 0, 8, 11);
            AddTerrainElement(5, 1, 3, 9);
            AddTerrainElement(5, 2, 10, 13);
            AddTerrainElement(5, 3, 13, 27);
            AddTerrainElement(5, 4, 3, 24);
            AddTerrainElement(5, 5, 10, 29);
            AddTerrainElement(5, 6, 13, 73);
            AddTerrainElement(5, 7, 3, 70);
            AddTerrainElement(5, 8, 10, 75);
            AddTerrainElement(5, 9, 13, 105);
            AddTerrainElement(5, 10, 3, 102);
            AddTerrainElement(5, 11, 10, 107);

            AddTerrainElement(6, 0, 14, 33);
            AddTerrainElement(6, 1, 6, 31);
            AddTerrainElement(6, 2, 15, 34);
            AddTerrainElement(6, 3, 11, 48);
            AddTerrainElement(6, 4, 6, 45);
            AddTerrainElement(6, 5, 15, 50);
            AddTerrainElement(6, 6, 11, 80);
            AddTerrainElement(6, 7, 6, 77);
            AddTerrainElement(6, 8, 15, 82);
            AddTerrainElement(6, 9, 11, 112);
            AddTerrainElement(6, 10, 6, 109);
            AddTerrainElement(6, 11, 15, 114);

            AddTerrainElement(7, 0, 1, 134); // No blending required
            AddTerrainElement(7, 1, 5, 30);
            AddTerrainElement(7, 2, 1, 135); // No blending required
            AddTerrainElement(7, 3, 4, 47);
            AddTerrainElement(7, 4, 5, 44);
            AddTerrainElement(7, 5, 1, 136); // No blending required
            AddTerrainElement(7, 6, 4, 79);
            AddTerrainElement(7, 7, 5, 76);
            AddTerrainElement(7, 8, 1, 137); // No blending required
            AddTerrainElement(7, 9, 4, 111);
            AddTerrainElement(7, 10, 5, 108);
            AddTerrainElement(7, 11, 1, 138); // No blending required

            AddTerrainElement(8, 0, 8, 34);
            AddTerrainElement(8, 1, 3, 32);
            AddTerrainElement(8, 2, 10, 36);
            AddTerrainElement(8, 3, 13, 49);
            AddTerrainElement(8, 4, 3, 46);
            AddTerrainElement(8, 5, 10, 51);
            AddTerrainElement(8, 6, 13, 81);
            AddTerrainElement(8, 7, 3, 78);
            AddTerrainElement(8, 8, 10, 83);
            AddTerrainElement(8, 9, 13, 113);
            AddTerrainElement(8, 10, 3, 110);
            AddTerrainElement(8, 11, 10, 115);

            AddTerrainElement(9, 0, 14, 40);
            AddTerrainElement(9, 1, 6, 38);
            AddTerrainElement(9, 2, 15, 42);
            AddTerrainElement(9, 3, 11, 56);
            AddTerrainElement(9, 4, 6, 53);
            AddTerrainElement(9, 5, 15, 58);
            AddTerrainElement(9, 6, 11, 88);
            AddTerrainElement(9, 7, 6, 85);
            AddTerrainElement(9, 8, 15, 90);
            AddTerrainElement(9, 9, 11, 120);
            AddTerrainElement(9, 10, 6, 117);
            AddTerrainElement(9, 11, 15, 122);

            AddTerrainElement(10, 0, 1, 139); // No blending required
            AddTerrainElement(10, 1, 5, 37);
            AddTerrainElement(10, 2, 1, 140); // No blending required
            AddTerrainElement(10, 3, 4, 55);
            AddTerrainElement(10, 4, 5, 52);
            AddTerrainElement(10, 5, 1, 141); // No blending required
            AddTerrainElement(10, 6, 4, 87);
            AddTerrainElement(10, 7, 5, 84);
            AddTerrainElement(10, 8, 1, 142); // No blending required
            AddTerrainElement(10, 9, 4, 119);
            AddTerrainElement(10, 10, 5, 116);
            AddTerrainElement(10, 11, 1, 143); // No blending required

            AddTerrainElement(11, 0, 8, 41);
            AddTerrainElement(11, 1, 3, 39);
            AddTerrainElement(11, 2, 10, 45);
            AddTerrainElement(11, 3, 13, 57);
            AddTerrainElement(11, 4, 3, 54);
            AddTerrainElement(11, 5, 10, 59);
            AddTerrainElement(11, 6, 13, 89);
            AddTerrainElement(11, 7, 3, 86);
            AddTerrainElement(11, 8, 10, 91);
            AddTerrainElement(11, 9, 13, 121);
            AddTerrainElement(11, 10, 3, 118);
            AddTerrainElement(11, 11, 10, 123);
        }

        /// <summary>
        /// Adds a new terrain element to the dictionary.
        /// </summary>
        /// <param name="column">The column index of the terrain element.</param>
        /// <param name="line">The line index of the terrain element.</param>
        /// <param name="actionNumber">The action number associated with the terrain element.</param>
        /// <param name="indexInQueue">Index in queue for stitch height generation.</param>
        private static void AddTerrainElement(int column, int line, int actionNumber, int indexInQueue)
        {
            TerrainElement element = new TerrainElement(actionNumber, indexInQueue);
            terrainElements.Add((column, line), element);
        }

        /// <summary>
        /// Gets a terrain element from the dictionary based on its grid coordinates.
        /// </summary>
        /// <param name="column">The column index of the terrain element.</param>
        /// <param name="line">The line index of the terrain element.</param>
        /// <returns>The terrain element at the specified grid coordinates, or null if not found.</returns>
        public static TerrainElement GetTerrainElement(int column, int line)
        {
            terrainElements.TryGetValue((column, line), out TerrainElement element);
            return element;
        }
    }

    /// <summary>
    /// Represents a cell in the terrain grid.
    /// </summary>
    [System.Serializable]
    public class TerrainGridCell
    {
        /// <summary>
        /// The heights generator associated with this terrain grid cell.
        /// </summary>
        [Tooltip("The heights generator associated with this terrain grid cell.")]
        public TerraForgeTerrainGenerator heightsGenerator;

        /// <summary>
        /// The game object representing the terrain in this grid cell.
        /// </summary>
        [Tooltip("The game object representing the terrain in this grid cell.")]
        public GameObject terrainGameObject;

        /// <summary>
        /// The index of this cell in the array.
        /// </summary>
        [Tooltip("The index of this cell in the array.")]
        public int indexInArray;

        /// <summary>
        /// The index of this cell in the stitch height generation queue.
        /// </summary>
        [Tooltip("The index of this cell in the stitch height generation queue.")]
        public int indexInQueue;

        /// <summary>
        /// The column index of this grid cell.
        /// </summary>
        [Tooltip("The column index of this grid cell.")]
        public int column;

        /// <summary>
        /// The line index of this grid cell.
        /// </summary>
        [Tooltip("The line index of this grid cell.")]
        public int line;

        /// <summary>
        /// The action number associated with this grid cell.
        /// </summary>
        [Tooltip("The action number associated with this grid cell.")]
        public int actionNumber;
        
        /// <summary>
        /// Indicates whether the biome is empty.
        /// </summary>
        [Tooltip("Indicates whether the biome is empty.")]
        public bool isEmptyBiome;


        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainGridCell"/> class.
        /// </summary>
        /// <param name="terrainGameObject">The game object representing the terrain.</param>
        /// <param name="indexInArray">The index of this cell in the array.</param>
        /// <param name="column">The column index of this grid cell.</param>
        /// <param name="line">The line index of this grid cell.</param>
        /// <param name="heightsGenerator">The heights generator associated with this grid cell.</param>
        public TerrainGridCell(GameObject terrainGameObject, int indexInArray, int column, int line, TerraForgeTerrainGenerator heightsGenerator, bool isEmptyBiome)
        {
            this.heightsGenerator = heightsGenerator;
            this.terrainGameObject = terrainGameObject;
            this.indexInArray = indexInArray;
            this.column = column;
            this.line = line;
            this.isEmptyBiome = isEmptyBiome;
        }
    }
}