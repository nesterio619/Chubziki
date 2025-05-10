using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TerraForge2.Scripts;
using TerraForge2.Scripts.Generators;
using TerraForge2.Scripts.Generators.Maps;
using TerraForge2.Scripts.Generators.Abstract;

/// <summary>
/// Manages the sailboat demo, including terrain generation and UI interactions.
/// </summary>
public class SailBoatDemoManager : MonoBehaviour
{
    /// <summary>
    /// List of biome settings used for terrain generation.
    /// </summary>
    [Tooltip("List of biome settings used for terrain generation.")]
    public List<BiomeSettings> biomes;

    /// <summary>
    /// Reference to the TerraForge terrain generator.
    /// </summary>
    [Tooltip("Reference to the TerraForge terrain generator.")]
    public TerraForgeTerrainGenerator terrainGenerator;

    /// <summary>
    /// The delay in seconds between pre-generation setup and terrain generation.
    /// </summary>
    [Tooltip("The delay in seconds between pre-generation setup and terrain generation.")]
    public float generationDelay = 2.0f;

    /// <summary>
    /// Right cloud UI RectTransform.
    /// </summary>
    [Tooltip("Right cloud UI RectTransform.")]
    public RectTransform rightCloud;

    /// <summary>
    /// Left cloud UI RectTransform.
    /// </summary>
    [Tooltip("Left cloud UI RectTransform.")]
    public RectTransform leftCloud;

    /// <summary>
    /// Right cloud start position.
    /// </summary>
    [Tooltip("Right cloud start position.")]
    public Vector2 rightCloudStartPos;

    /// <summary>
    /// Right cloud end position.
    /// </summary>
    [Tooltip("Right cloud end position.")]
    public Vector2 rightCloudEndPos;

    /// <summary>
    /// Left cloud start position.
    /// </summary>
    [Tooltip("Left cloud start position.")]
    public Vector2 leftCloudStartPos;

    /// <summary>
    /// Left cloud end position.
    /// </summary>
    [Tooltip("Left cloud end position.")]
    public Vector2 leftCloudEndPos;

    /// <summary>
    /// Speed of the cloud movement.
    /// </summary>
    [Tooltip("Speed of the cloud movement.")]
    public float cloudMoveSpeed = 1.0f;

    /// <summary>
    /// Text UI element for displaying generation message.
    /// </summary>
    [Tooltip("Text UI element for displaying generation message.")]
    public Text generationText;

    /// <summary>
    /// Speed of the text transparency change.
    /// </summary>
    [Tooltip("Speed of the text transparency change.")]
    public float textFadeSpeed = 1.0f;

    /// <summary>
    /// Speed of the image fade.
    /// </summary>
    [Tooltip("Speed of the image fade.")]
    public float imadeFadeSpeed = 1.0f;

    /// <summary>
    /// List of cloud images for transparency adjustment.
    /// </summary>
    [Tooltip("List of cloud images for transparency adjustment.")]
    public List<Image> cloudImages;

    /// <summary>
    /// Indicates whether terrain generation is in progress.
    /// </summary>
    private bool isGenerating = false;

    /// <summary>
    /// Coroutine for returning the right cloud to its start position.
    /// </summary>
    private Coroutine rightCloudReturnCoroutine;

    /// <summary>
    /// Coroutine for returning the left cloud to its start position.
    /// </summary>
    private Coroutine leftCloudReturnCoroutine;

    /// <summary>
    /// Reference to the boat game object.
    /// </summary>
    [Tooltip("Reference to the boat game object.")]
    public GameObject boat;

    /// <summary>
    /// Initializes the manager by setting up event listeners and initial UI states.
    /// </summary>
    void Start()
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.LoadComponentData();
            terrainGenerator.OnGenerationComplete += OnTerrainGenerationComplete;
        }

        if (generationText != null)
        {
            Color textColor = generationText.color;
            textColor.a = 0f;
            generationText.color = textColor;
        }
    }

    /// <summary>
    /// Cleans up event listeners on destruction.
    /// </summary>
    void OnDestroy()
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.OnGenerationComplete -= OnTerrainGenerationComplete;
        }
    }

    /// <summary>
    /// Handles user input for triggering terrain generation and stopping cloud movement.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isGenerating)
        {
            StopCloudReturn();
            PreGenerationSetup();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StopCloudReturn();
        }
    }

    /// <summary>
    /// Sets up the pre-generation steps.
    /// </summary>
    void PreGenerationSetup()
    {
        StartCoroutine(MoveCloudsAndGenerateTerrain());
    }

    /// <summary>
    /// Coroutine for moving clouds and generating terrain.
    /// </summary>
    IEnumerator MoveCloudsAndGenerateTerrain()
    {
        Coroutine rightCloudMove = StartCoroutine(MoveCloud(rightCloud, rightCloud.anchoredPosition, rightCloudEndPos, true));
        Coroutine leftCloudMove = StartCoroutine(MoveCloud(leftCloud, leftCloud.anchoredPosition, leftCloudEndPos, true));

        yield return rightCloudMove;
        yield return leftCloudMove;

        if (generationText != null)
        {
            yield return StartCoroutine(FadeTextToFullAlpha(textFadeSpeed, generationText));
        }

        GenerateTerrain();
        MoveBoatToPosition(new Vector3(525.5f, 128.1394f, 0.8f));

        yield return new WaitForSeconds(generationDelay);

        rightCloudReturnCoroutine = StartCoroutine(MoveCloud(rightCloud, rightCloud.anchoredPosition, rightCloudStartPos, false));
        leftCloudReturnCoroutine = StartCoroutine(MoveCloud(leftCloud, leftCloud.anchoredPosition, leftCloudStartPos, false));

        if (generationText != null)
        {
            yield return StartCoroutine(FadeTextToZeroAlpha(textFadeSpeed / 3f, generationText));
        }
    }

    /// <summary>
    /// Coroutine for moving a cloud.
    /// </summary>
    IEnumerator MoveCloud(RectTransform cloud, Vector2 startPos, Vector2 endPos, bool fadingOut)
    {
        float elapsedTime = 0f;
        float journeyLength = Vector2.Distance(startPos, endPos);

        float recalculatedCloudMoveSpeed = cloudMoveSpeed;
        if (!fadingOut)
            recalculatedCloudMoveSpeed = cloudMoveSpeed / 2;

        while (elapsedTime < journeyLength / recalculatedCloudMoveSpeed)
        {
            cloud.anchoredPosition = Vector2.Lerp(startPos, endPos, (elapsedTime * recalculatedCloudMoveSpeed) / journeyLength);

            if (fadingOut)
            {
                foreach (var cloudImage in cloudImages)
                {
                    Color color = cloudImage.color;
                    float currentColorAlpha = color.a;
                    color.a = Mathf.Lerp(currentColorAlpha, 1f, (elapsedTime * imadeFadeSpeed) / journeyLength);
                    cloudImage.color = color;
                }
            }
            else
            {
                foreach (var cloudImage in cloudImages)
                {
                    Color color = cloudImage.color;
                    float currentColorAlpha = color.a;
                    color.a = Mathf.Lerp(currentColorAlpha, 0f, (elapsedTime * imadeFadeSpeed) / journeyLength);
                    cloudImage.color = color;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cloud.anchoredPosition = endPos;
    }

    /// <summary>
    /// Coroutine for fading text to full alpha.
    /// </summary>
    IEnumerator FadeTextToFullAlpha(float t, Text text)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        while (text.color.a < 1.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + (Time.deltaTime / t));
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine for fading text to zero alpha.
    /// </summary>
    IEnumerator FadeTextToZeroAlpha(float t, Text text)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        while (text.color.a > 0.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }

    /// <summary>
    /// Generates terrain based on the selected biome settings.
    /// </summary>
    void GenerateTerrain()
    {
        if (biomes.Count > 0 && terrainGenerator != null)
        {
            isGenerating = true;

            int randomBiomeIndex = Random.Range(0, biomes.Count);
            ChangeBiome(randomBiomeIndex);

            for (int i = 0; i < terrainGenerator.terrainLayers.Length; i++)
            {
                TerrainLayerSettings layer = terrainGenerator.terrainLayers[i];
                layer.useFalloffMap = true;
            }

            terrainGenerator.TerrainGenerate();
            terrainGenerator.SaveComponentData();
        }
        else
        {
            Debug.LogError("Please ensure biomes and terrain generator are assigned.");
        }
    }

    /// <summary>
    /// Changes the biome settings for the terrain generator.
    /// </summary>
    /// <param name="biomeIndex">Index of the biome to apply.</param>
    public void ChangeBiome(int biomeIndex)
    {
        if (biomeIndex < 0 || biomeIndex >= biomes.Count)
        {
            Debug.LogError("Invalid biome index.");
            return;
        }

        BiomeSettings selectedBiome = biomes[biomeIndex];
        selectedBiome.ApplyingBiomeSettings(terrainGenerator, null, false, 15);
    }

    /// <summary>
    /// Callback for when terrain generation is complete.
    /// </summary>
    void OnTerrainGenerationComplete()
    {
        isGenerating = false;
    }

    /// <summary>
    /// Stops the cloud return coroutines if they are running.
    /// </summary>
    void StopCloudReturn()
    {
        if (rightCloudReturnCoroutine != null)
        {
            StopCoroutine(rightCloudReturnCoroutine);
            rightCloudReturnCoroutine = null;
        }

        if (leftCloudReturnCoroutine != null)
        {
            StopCoroutine(leftCloudReturnCoroutine);
            leftCloudReturnCoroutine = null;
        }
    }

    /// <summary>
    /// Moves the boat to the specified world position.
    /// </summary>
    /// <param name="position">The target world position.</param>
    public void MoveBoatToPosition(Vector3 position)
    {
        if (boat != null)
        {
            boat.transform.position = position;
        }
        else
        {
            Debug.LogError("Boat reference is not assigned.");
        }
    }
}