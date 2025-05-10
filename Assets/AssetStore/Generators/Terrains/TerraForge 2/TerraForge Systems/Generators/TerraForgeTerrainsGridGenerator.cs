// TerraForgeTerrainsGridGenerator.cs
// Responsible for generating a grid of terrain, which are then stitched together at the edges for smooth transitions (blended).
// TerraForge 2.0.0

using UnityEngine;
using System;
using Unity.Collections;
using System.Collections.Generic;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;
using TerraForge2.Scripts.TerraForgeEditor;
using TerraForge2.Scripts.TerrainPainter;

namespace TerraForge2.Scripts.Generators
{    
    /// <summary>
    /// Class responsible for generating a grid of terrains.
    /// </summary>
    [AddComponentMenu("TerraForge 2/Terrains Grid Generator")]
    public class TerraForgeTerrainsGridGenerator : MonoBehaviour
    {
        /// <summary>
        /// If true, deletes the terrain data from previous generations.
        /// </summary>
        [Tooltip("If true, deletes the terrain data from previous generations.")]
        public bool deleteTerrainDataPreviousGeneration = true;

        /// <summary>
        /// The material to apply to generated terrains.
        /// </summary>
        [Tooltip("The material to apply to generated terrains.")]
        public Material terrainsMaterial;

        /// <summary>
        /// Buffer copy of terrainsMaterial to detect changes.
        /// </summary>
        [HideInInspector] public Material bufferTerrainsMaterial;

        /// <summary>
        /// The resolution of the terrains to generate.
        /// </summary>
        [Tooltip("The resolution of the terrains to generate.")]
        public TerrainResolution terrainsResolution = TerrainResolution.Resolution513;

        /// <summary>
        /// Buffer copy of terrainsResolution to detect changes.
        /// </summary>
        [HideInInspector] public TerrainResolution bufferTerrainsResolution = TerrainResolution.Resolution513;

        /// <summary>
        /// The number of columns in the terrain grid.
        /// </summary>
        [Range(2, 12)]
        [Tooltip("The number of columns in the terrain grid.")]
        public int gridColumns = 3;

        /// <summary>
        /// The number of lines in the terrain grid.
        /// </summary>
        [Range(2, 12)]
        [Tooltip("The number of lines in the terrain grid.")]
        public int gridLines = 3;

        /// <summary>
        /// The width of the transition between terrains.
        /// </summary>
        [Tooltip("The width of the transition between terrains")]
        public float transitionWidth;

        /// <summary>
        /// The strength of the transition between terrains.
        /// </summary>
        [Range(1f, 1.5f)]
        [Tooltip("The strength of the transition between terrains")]
        public float transitionStrength = 1f;

        /// <summary>
        /// Array of biomes. When generating the grid, one of the biomes will be randomly selected for each of the terrains. 
        /// </summary>
        [Tooltip("Array of biomes. When generating the grid, one of the biomes will be randomly selected for each of the terrains.")]
        public List<BiomeSettings> biomes;

        /// <summary>
        /// Enables the creation of empty biomes.
        /// </summary>
        [Tooltip("Enables the creation of empty biomes.")]
        public bool enableEmptyBiomesCreation;

        /// <summary>
        /// Chance of creating an empty biome, ranging from 0 (no chance) to 1 (certain).
        /// </summary>
        [Tooltip("Chance of creating an empty biome, ranging from 0 (no chance) to 1 (certain).")]
        [Range(0f, 1f)]
        public float emptyBiomesCreationChance = 0.5f;

        /// <summary>
        /// Height of the empty biomes.
        /// </summary>
        [Tooltip("Height of the empty biomes.")]
        public float emptyBiomesHeight = 15f;

        //Settings for terrain edge smoothing.
        
        /// <summary>
        /// Indicates whether the edge smoothing is enabled.
        /// </summary>
        [Tooltip("Indicates whether the edge smoothing is enabled.")]
        public bool isEnabled = true;

        /// <summary>
        /// Radius for blurring the edges of the terrains.
        /// </summary>
        [Range(0, 30)]
        [Tooltip("Radius for blurring the edges of the terrains.")]
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

        /// <summary>
        /// The size of each terrain.
        /// </summary>
        [Min(10f)]
        [Tooltip("The size of each terrain.")]
        public float terrainSize = 1000f;

        /// <summary>
        /// Buffer copy of terrainSize to detect changes.
        /// </summary>
        [HideInInspector]
        public float bufferTerrainSize;

        /// <summary>
        /// List of generated terrain cells.
        /// </summary>
        [HideInInspector]
        public List<TerrainGridCell> terrains = new List<TerrainGridCell>();

        /// <summary>
        /// List of terrain cells queued for blending.
        /// </summary>
        [HideInInspector]
        public List<TerrainGridCell> terrainsBlendQueue = new List<TerrainGridCell>();

        /// <summary>
        /// Indicates whether a new terrain grid generation is waiting.
        /// </summary>
        [NonSerialized] public bool isNewGenerateTerrainGrid;

        
    #if UNITY_EDITOR
        // Variables for the Custom Editor:
        [HideInInspector] public bool showGridSettings = true;
        [HideInInspector] public bool showTerrainsSettings;
        [HideInInspector] public bool showBiomesSettings;

    #endif

        /// <summary>
        /// Deletes the terrain grid, including associated TerrainData assets.
        /// </summary>
        public void DeleteTerrainGrid()
        {
            // Destroy previous terrains and their data
            DestroyPreviousTerrainsData();
            DestroyPreviousTerrains();
            terrains.Clear();
            terrainsBlendQueue.Clear();
        }

        /// <summary>
        /// Retrieves a random biome from the list of available biomes.
        /// </summary>
        /// <returns>A randomly selected biome.</returns>
        public BiomeSettings GetRandomBiome()
        {
            // Check if the list of biomes is empty or not assigned
            if (biomes == null || biomes.Count == 0)
            {
                Debug.LogError("Biome list is empty or not assigned!");
                return null;
            }

            // Generate a random index to select a biome from the list
            int randomIndex = UnityEngine.Random.Range(0, biomes.Count);

            return biomes[randomIndex];
        }

        /// <summary>
        /// Initiates the generation of a new terrain grid asynchronously.
        /// </summary>
        public async void NewGenerateTerrainGrid()
        {
            isNewGenerateTerrainGrid = true;

            // Delay to allow for any pending operations to complete
            await System.Threading.Tasks.Task.Delay(100);

            // Check if the script is still attached to an object
            if (this == null)
            {
                isNewGenerateTerrainGrid = false;
                return;
            }

            // Check if the list of biomes is empty or not assigned
            if (biomes == null || biomes.Count == 0)
            {
                Debug.LogError("Biome list is empty or not assigned!");
                return;
            }

            // Generate the terrain grid
            GenerateTerrainGrid();

            isNewGenerateTerrainGrid = false;
        }

        /// <summary>
        /// Generates a grid of terrain based on the specified parameters.
        /// </summary>
        public void GenerateTerrainGrid()
        {    
            if (terrains.Count != 0)
            {
                if (deleteTerrainDataPreviousGeneration)
                {
                    // Destroy previous terrains and their data
                    DestroyPreviousTerrainsData();
                }

                DestroyPreviousTerrains();
            }

            // Store initial position for terrain grid generation
            Vector3 initialPosition = transform.position;

            // Iterate through grid lines and columns to generate terrain
            for (int x = 0; x < gridLines; x++)
            {
                for (int z = 0; z < gridColumns; z++)
                {
                    // Calculate position for current terrain cell
                    Vector3 position = initialPosition + new Vector3(x * terrainSize, 0, z * terrainSize);

                    // Instantiate terrain prefab at calculated position
                    GameObject terrainInstance = Instantiate(TerraForgeGlobalSettings.Instance.defaultTerraForgeTerrainForGrid, position, Quaternion.identity, transform);

                    // Apply terrain material and settings
                    ApplyTerrainMaterial(terrainInstance);

                    // Retrieve the generator component
                    TerraForgeTerrainGenerator generator = terrainInstance.GetComponent<TerraForgeTerrainGenerator>();
                    
                    if (generator == null)
                    {
                        Debug.LogError($"Terrain generator component not found on {terrainInstance.name}. Aborting terrain generation for this cell.");
                        continue; // Skip current cell if generator component is missing
                    }

                    // Set terrain resolution and apply changes
                    generator.generalSettings.terrainResolution = terrainsResolution;
                    generator.generalSettings.bufferTerrainResolution = terrainsResolution;
                    bufferTerrainsResolution = terrainsResolution;
                    generator.generalSettings.terrainSize = terrainSize;
                    generator.ChangeTerrainResolution(false);

                    // Calculate column and line indices for the terrain cell
                    Vector3 differenceOfStartPosition = initialPosition - position;
                    int column = (int)((Mathf.Abs(differenceOfStartPosition.z) + 0.1) / terrainSize);
                    int line = (int)((Mathf.Abs(differenceOfStartPosition.x) + 0.1) / terrainSize);

                    // Calculate index in the terrains array
                    int indexInArray = x * gridColumns + z;
                    
                    bool isEmptyBiome = false;
                    if (enableEmptyBiomesCreation)
                    {
                        float randomValue = UnityEngine.Random.Range(0f, 1f);

                        if (randomValue <= emptyBiomesCreationChance)
                        {
                            isEmptyBiome = true;
                        }
                    }

                    // Create TerrainGridCell object and add to terrains list
                    TerrainGridCell cell = new TerrainGridCell(terrainInstance, indexInArray, column, line, generator, isEmptyBiome);
                    terrains.Add(cell);

                    // Set terrain instance name for identification
                    terrainInstance.name = $"Terrain_{column}_{line}";
                }
            }

            // Clear terrains blend queue and terrain data array
            terrainsBlendQueue.Clear();
            
            foreach (TerrainGridCell cell in terrains)
            {
                int column = cell.column;
                int line = cell.line;

                // Assign neighbor generators
                TerraForgeTerrainGenerator generator = cell.heightsGenerator;
                generator.contactHeightsGenerators.contactHeightsGenerator_Right.heightsGenerator = GetNeighborGenerator(column + 1, line);
                generator.contactHeightsGenerators.contactHeightsGenerator_Left.heightsGenerator = GetNeighborGenerator(column - 1, line);
                generator.contactHeightsGenerators.contactHeightsGenerator_Top.heightsGenerator = GetNeighborGenerator(column, line - 1);
                generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.heightsGenerator = GetNeighborGenerator(column, line + 1);
                
                // Assign terrain material
                generator.generalSettings.terrainMaterial = terrainsMaterial;
                generator.generalSettings.bufferTerrainMaterial = generator.generalSettings.terrainMaterial;

                bufferTerrainsMaterial = terrainsMaterial;

                generator.generalSettings.autoUpdateTerrainGeneration = false;
                generator.generalSettings.isNewGenerationWaiting = false;

                // Create and set clone terrain data
                generator.CreateAndSetCloneTerrainData();

                float terrainHeight = terrainSize * 0.6f;
                cell.terrainGameObject.GetComponent<Terrain>().terrainData.size = new Vector3(terrainSize, terrainHeight, terrainSize);

                // Apply biome settings
                BiomeSettings chosenBiome = GetRandomBiome();
                                
                chosenBiome.ApplyingBiomeSettings(generator, cell.terrainGameObject, cell.isEmptyBiome, emptyBiomesHeight);

                // Setting the parameters for smoothing the contact edges of the terrains
                generator.terrainEdgeSmoothingSettings.isEnabled = isEnabled;
                generator.terrainEdgeSmoothingSettings.blurRadius = blurRadius;
                generator.terrainEdgeSmoothingSettings.falloffTransitionWidth = falloffTransitionWidth;
                generator.terrainEdgeSmoothingSettings.falloffRange = falloffRange;

                // Set transition width and strength
                generator.contactHeightsGenerators.contactHeightsGenerator_Right.transitionWidth = transitionWidth;
                generator.contactHeightsGenerators.contactHeightsGenerator_Right.transitionStrength = transitionStrength;
                generator.contactHeightsGenerators.contactHeightsGenerator_Left.transitionWidth = transitionWidth;
                generator.contactHeightsGenerators.contactHeightsGenerator_Left.transitionStrength = transitionStrength;
                generator.contactHeightsGenerators.contactHeightsGenerator_Top.transitionWidth = transitionWidth;
                generator.contactHeightsGenerators.contactHeightsGenerator_Top.transitionStrength = transitionStrength;
                generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.transitionWidth = transitionWidth;
                generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.transitionStrength = transitionStrength;

                if (enableEmptyBiomesCreation)
                {
                    for (int i = 0; i < generator.terrainLayers.Length; i++)
                    {
                        TerrainLayerSettings layer = generator.terrainLayers[i];
                        
                        layer.useFalloffMap = true;
                    }
                }

                generator.RefreshGenerationAllLayers();
                
                // Retrieve index and action number from dictionary
                TerrainElement terrainElement = TerrainElementDictionary.GetTerrainElement(column, line);
                if (terrainElement != null)
                {
                    cell.indexInQueue = terrainElement.indexInQueue;
                    cell.actionNumber = terrainElement.actionNumber;
                    terrainsBlendQueue.Add(cell);
                }
                else
                {
                    // Handle error when terrain element is not found in dictionary
                    Debug.LogError("Terrain element not found for column: " + column + ", line: " + line);
                }
            }

            // Sort terrains blend queue by index in queue
            terrainsBlendQueue.Sort((cell1, cell2) => cell1.indexInQueue.CompareTo(cell2.indexInQueue));

            // Blend terrains in the blend queue
            if (terrainsBlendQueue != null)
            {
                foreach (TerrainGridCell cell in terrainsBlendQueue)
                {
                    ApplicationBlendTerrains(cell.heightsGenerator, cell.actionNumber);
                }
            }
            else
            {
                // Handle error when terrains blend queue is null
                Debug.LogError("Terrains blend queue is null.");
            }
        }

        /// <summary>
        /// Refreshes the resolution of all terrains in the grid.
        /// </summary>
        public void RefreshTerrainsResolution()
        {
            if (bufferTerrainsResolution != terrainsResolution)
            {
                foreach (TerrainGridCell cell in terrains)
                {
                    if (cell.heightsGenerator != null)
                    {
                        // Set terrain resolution
                        cell.heightsGenerator.generalSettings.terrainResolution = terrainsResolution;
                        // Update buffer terrain resolution
                        cell.heightsGenerator.generalSettings.bufferTerrainResolution = terrainsResolution;

                        bufferTerrainsResolution = terrainsResolution;

                        // Change terrain resolution
                        cell.heightsGenerator.ChangeTerrainResolution(true);
                    }
                    else
                    {
                        Debug.LogWarning($"TerrainGridCell at column {cell.column}, line {cell.line} does not have a heightsGenerator.");
                    }
                }
                
                // Blend terrains in the blend queue
                if (terrainsBlendQueue != null)
                {
                    foreach (TerrainGridCell cell in terrainsBlendQueue)
                    {
                        if (cell.heightsGenerator.terrainLayers.Length != 0)
                            ApplicationBlendTerrains(cell.heightsGenerator, cell.actionNumber);
                    }
                }
                else
                {
                    // Handle error when terrains blend queue is null
                    Debug.LogError("Terrains blend queue is null.");
                }
            }
        }

        /// <summary>
        /// Refreshes the size of all terrains.
        /// </summary>
        public void RefreshTerrainsSize()
        {
            if (bufferTerrainSize != terrainSize)
            {
                foreach (TerrainGridCell cell in terrains)
                {
                    if (cell.heightsGenerator != null)
                    {
                        // Set terrain resolution
                        cell.heightsGenerator.generalSettings.terrainSize = terrainSize;
                        // Update buffer terrain resolution
                        cell.heightsGenerator.generalSettings.bufferTerrainSize = terrainSize;

                        bufferTerrainSize = terrainSize;

                        float terrainHeight = terrainSize * 0.6f;
                        cell.terrainGameObject.GetComponent<Terrain>().terrainData.size = new Vector3(terrainSize, terrainHeight, terrainSize);

                        // Calculate position for current terrain cell
                        cell.terrainGameObject.transform.position = this.transform.position + new Vector3(cell.line * terrainSize, 0, cell.column * terrainSize);
                    }
                    else
                    {
                        Debug.LogWarning($"TerrainGridCell at column {cell.column}, line {cell.line} does not have a heightsGenerator.");
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the material of all terrains in the grid.
        /// </summary>
        public void RefreshTerrainsMaterial()
        {
            if (bufferTerrainsMaterial != terrainsMaterial)
            {
                // Apply terrain material to all terrains
                foreach (TerrainGridCell cell in terrains)
                {
                    ApplyTerrainMaterial(cell.terrainGameObject);
                }
            }
        }

        /// <summary>
        /// Calculates the local position of a child transform relative to a parent transform.
        /// </summary>
        /// <param name="child">The child transform.</param>
        /// <param name="parent">The parent transform.</param>
        /// <returns>The local position of the child transform relative to the parent transform.</returns>
        public static Vector3 CalculateLocalPosition(Transform child, Transform parent)
        {
            // Check if child and parent transforms are valid
            if (child == null || parent == null)
            {
                Debug.LogError("Cannot calculate local position: Child or parent transform is null.");
                return Vector3.zero;
            }

            // Calculate local position relative to parent
            Vector3 localPosition = parent.InverseTransformPoint(child.position);
            
            return localPosition;
        }

        /// <summary>
        /// Deletes the terrain data assets from the previous generation.
        /// </summary>
        void DestroyPreviousTerrainsData()
        {
            foreach (TerrainGridCell cell in terrains)
            {
                try
                {
                    if (cell.heightsGenerator != null)
                    {
                        cell.heightsGenerator.DeleteCurrentTerrainData();
                    }
                    else
                    {
                        Debug.LogWarning($"TerrainGridCell at column {cell.column}, line {cell.line} does not have a heightsGenerator.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error deleting terrain data for cell at column {cell.column}, line {cell.line}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Destroys the previously generated terrain GameObjects.
        /// </summary>
        void DestroyPreviousTerrains()
        {
            // Check if the terrain list is not null
            if (terrains != null)
            {
                // Iterate through each terrain cell
                foreach (TerrainGridCell cell in terrains)
                {
                    // Check if the terrain GameObject exists
                    if (cell.terrainGameObject != null)
                    {
                        // Destroy the terrain GameObject immediately
                        DestroyImmediate(cell.terrainGameObject);
                    }
                }
                // Clear the list of terrains
                terrains.Clear();
            }
            else
            {
                // Log warning if the terrains list is null
                Debug.LogWarning("Terrains list is null. No terrain GameObjects to destroy.");
            }
        }

        /// <summary>
        /// Applies the specified terrain material to a given terrain GameObject.
        /// </summary>
        /// <param name="terrain">The terrain GameObject to which the material will be applied.</param>
        void ApplyTerrainMaterial(GameObject terrain)
        {
            // Check if a terrain material is assigned
            if (terrainsMaterial != null)
            {
                // Get the Terrain component from the GameObject
                Terrain terrainComponent = terrain.GetComponent<Terrain>();
                
                // Check if the Terrain component exists
                if (terrainComponent != null)
                {
                    // Assign the material to the Terrain component
                    terrainComponent.materialTemplate = terrainsMaterial;
                    bufferTerrainsMaterial = terrainsMaterial;
                    terrain.GetComponent<TerraForgeTerrainGenerator>().generalSettings.terrainMaterial = terrainsMaterial;
                }
                else
                {
                    // Log a warning if the Terrain component is not found
                    Debug.LogWarning($"Terrain component not found on {terrain.name}. Material not applied.");
                }
            }
            else
            {
                // Log a warning if the terrain material is not assigned
                Debug.LogWarning("Terrain material is not assigned. Material not applied.");
            }
        }

        /// <summary>
        /// Applies the appropriate terrain blending action based on the specified action number.
        /// Terrain blending is the process of stitching together the edges of neighboring terrains to ensure a smooth transition between them.
        /// </summary>
        /// <param name="generator">The TerraForgeTerrainGenerator responsible for generating the terrain.</param>
        /// <param name="actionNumber">The action number that determines which blending method to apply.</param>
        public void ApplicationBlendTerrains(TerraForgeTerrainGenerator generator, int actionNumber)
        {
            switch (actionNumber)
            {
                case 1:
                    BlendTerrainsAction1(generator);
                    break;
                case 2:
                    BlendTerrainsAction2(generator);
                    break;
                case 3:
                    BlendTerrainsAction3(generator);
                    break;
                case 4:
                    BlendTerrainsAction4(generator);
                    break;
                case 5:
                    BlendTerrainsAction5(generator);
                    break;
                case 6:
                    BlendTerrainsAction6(generator);
                    break;
                case 7:
                    BlendTerrainsAction7(generator);
                    break;
                case 8:
                    BlendTerrainsAction8(generator);
                    break;
                case 9:
                    BlendTerrainsAction9(generator);
                    break;
                case 10:
                    BlendTerrainsAction10(generator);
                    break;
                case 11:
                    BlendTerrainsAction11(generator);
                    break;
                case 12:
                    BlendTerrainsAction12(generator);
                    break;
                case 13:
                    BlendTerrainsAction13(generator);
                    break;
                case 14:
                    BlendTerrainsAction14(generator);
                    break;
                case 15:
                    BlendTerrainsAction15(generator);
                    break;
                default:
                    Debug.LogWarning($"Unknown action number: {actionNumber}. No blending action applied.");
                    break;
            }
        }

        /// <summary>
        /// Retrieves the terrain generator for the neighboring terrain cell based on the specified column and line indices.
        /// </summary>
        /// <param name="column">The column index of the neighboring cell.</param>
        /// <param name="line">The line index of the neighboring cell.</param>
        /// <returns>The terrain generator for the neighboring cell, or null if no such cell exists.</returns>
        public TerraForgeTerrainGenerator GetNeighborGenerator(int column, int line)
        {
            // Check if the specified column and line indices are within valid bounds.
            if (column >= 0 && line < gridLines && line >= 0 && column < gridColumns)
            {
                // Iterate through the list of terrain cells to find the matching cell.
                foreach (TerrainGridCell cell in terrains)
                {
                    if (column == cell.column && line == cell.line)
                    {
                        // Return the heights generator for the found cell.
                        return cell.heightsGenerator;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Action 1: Terrains do not need to be blended.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction1(TerraForgeTerrainGenerator generator)
        {
            // No blending needed for this action.
        }

        /// <summary>
        /// Action 2: Blends the right side of the terrain.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction2(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the right side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending = false;

            // Perform the blending on the right side of the terrain.
            generator.BlendTerrains_Right();
        }

        /// <summary>
        /// Action 3: Blends the left side of the terrain.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction3(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the left side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending = false;
            
            // Perform the blending on the left side of the terrain.
            generator.BlendTerrains_Left();
        }

        /// <summary>
        /// Action 4: Blends the top side of the terrain.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction4(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the top side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending = false;
            
            // Perform the blending on the top side of the terrain.
            generator.BlendTerrains_Top();
        }

        /// <summary>
        /// Action 5: Blends the top and bottom sides of the terrain.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction5(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for both top and bottom sides.
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.diagonalBlending = false;
            
            // Perform the blending on both top and bottom sides of the terrain.
            generator.BlendTerrains_Top();
            generator.BlendTerrains_Bottom();
        }

        /// <summary>
        /// Action 6: Blends the right and left sides of the terrain.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction6(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for both right and left sides.
            generator.contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending = false;
            
            // Perform the blending on both right and left sides of the terrain.
            generator.BlendTerrains_Right();
            generator.BlendTerrains_Left();
        }

        /// <summary>
        /// Action 7: Blends the right and bottom sides of the terrain with specific diagonal settings.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction7(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the right side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending = false;
            
            // Enable diagonal blending for the bottom side with specific settings.
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.bothHalves = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.firstHalf = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.secondHalf = true;
            
            // Perform the blending on the right and bottom sides of the terrain.
            generator.BlendTerrains_Right();
            generator.BlendTerrains_Bottom();
        }

        /// <summary>
        /// Action 8: Blends the left and bottom sides of the terrain with specific settings for diagonal blending.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction8(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the left side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending = false;

            // Enable and configure diagonal blending for the bottom side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.bothHalves = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.firstHalf = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.secondHalf = false;

            // Perform the blending on the left and bottom sides of the terrain.
            generator.BlendTerrains_Left();
            generator.BlendTerrains_Bottom();
        }

        /// <summary>
        /// Action 9: Blends the right and top sides of the terrain with specific settings for diagonal blending.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction9(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the right side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending = false;

            // Enable and configure diagonal blending for the top side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.bothHalves = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.firstHalf = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.secondHalf = true;

            // Perform the blending on the right and top sides of the terrain.
            generator.BlendTerrains_Right();
            generator.BlendTerrains_Top();
        }

        /// <summary>
        /// Action 10: Blends the left and top sides of the terrain with specific settings for diagonal blending.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction10(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the left side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending = false;

            // Enable and configure diagonal blending for the top side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.bothHalves = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.firstHalf = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.secondHalf = false;

            // Perform the blending on the left and top sides of the terrain.
            generator.BlendTerrains_Left();
            generator.BlendTerrains_Top();
        }

        /// <summary>
        /// Action 11: Blends all four sides (right, left, top, bottom) of the terrain with specific settings for diagonal blending.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction11(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the right and left sides.
            generator.contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending = false;

            // Enable and configure diagonal blending for the top and bottom sides.
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.bothHalves = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.firstHalf = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.secondHalf = true;

            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.bothHalves = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.firstHalf = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.secondHalf = true;

            // Perform the blending on all four sides of the terrain.
            generator.BlendTerrains_Right();
            generator.BlendTerrains_Left();
            generator.BlendTerrains_Top();
            generator.BlendTerrains_Bottom();
        }

        /// <summary>
        /// Action 12: Blends the right, top, and bottom sides of the terrain with specific settings for diagonal blending.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        public void BlendTerrainsAction12(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the right side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending = false;

            // Enable and configure diagonal blending for the top and bottom sides.
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.bothHalves = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.firstHalf = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.secondHalf = true;

            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.bothHalves = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.firstHalf = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.secondHalf = true;

            // Perform the blending on the right, top, and bottom sides of the terrain.
            generator.BlendTerrains_Right();
            generator.BlendTerrains_Top();
            generator.BlendTerrains_Bottom();
        }

        /// <summary>
        /// Action 13: Blends the left, top, and bottom sides of the terrain with specific settings for diagonal blending.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        private void BlendTerrainsAction13(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the left side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending = false;

            // Enable and configure diagonal blending for the top and bottom sides.
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.bothHalves = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.firstHalf = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.secondHalf = false;

            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.bothHalves = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.firstHalf = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.secondHalf = false;

            // Perform the blending on the left, top, and bottom sides of the terrain.
            generator.BlendTerrains_Left();
            generator.BlendTerrains_Top();
            generator.BlendTerrains_Bottom();
        }

        /// <summary>
        /// Action 14: Blends the right, left, and bottom sides of the terrain with specific settings for diagonal blending.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        private void BlendTerrainsAction14(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the right and left sides.
            generator.contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending = false;
            generator.contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending = false;

            // Enable and configure diagonal blending for the bottom side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.bothHalves = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.firstHalf = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Bottom.secondHalf = true;

            // Perform the blending on the right, left, and bottom sides of the terrain.
            generator.BlendTerrains_Right();
            generator.BlendTerrains_Left();
            generator.BlendTerrains_Bottom();
        }

        /// <summary>
        /// Action 15: Blends the right, left, and top sides of the terrain with specific diagonal settings.
        /// </summary>
        /// <param name="generator">The terrain generator to use for blending.</param>
        private void BlendTerrainsAction15(TerraForgeTerrainGenerator generator)
        {
            // Disable diagonal blending for the right side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Right.diagonalBlending = false;
            
            // Disable diagonal blending for the left side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Left.diagonalBlending = false;
            
            // Enable and configure diagonal blending for the top side.
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.diagonalBlending = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.bothHalves = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.firstHalf = true;
            generator.contactHeightsGenerators.contactHeightsGenerator_Top.secondHalf = true;
            
            // Perform the blending on the right, left, and top sides of the terrain.
            generator.BlendTerrains_Right();
            generator.BlendTerrains_Left();
            generator.BlendTerrains_Top();
        }
    }
}