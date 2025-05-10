using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RockGenerator
{ 
    [AddComponentMenu("ROCKGEN 2024/RockCluster")]
    public class RockCluster : MonoBehaviour
    {
        // Reference to the RockGen component
        [HideInInspector] public RockGen rockGenerator;
        // Prefab for the rock fragment
        [HideInInspector] public GameObject rockFragmentPrefab;
        // List of materials for stone fragments
        public List<Material> fragmentMaterials = new List<Material>();
        // List to store child stones
        [HideInInspector] public List<GameObject> childStones = new List<GameObject>();
        // List to store RockGen components
        [HideInInspector] public List<RockGen> rockGenComponents = new List<RockGen>();
        
        // Number of fragments to generate
        public int numberOfFragments = 10;
        // Ground level
        public float groundLevel = 0f;
        // Minimum deviation from ground level
        public float minDeviation = -0.15f;
        // Maximum deviation from ground level
        public float maxDeviation = 0f;
        // Minimum placement radius
        public float minPlacementRadius = 3f;
        // Maximum placement radius
        public float maxPlacementRadius = 5f;

        // Curve for stone size 
        public AnimationCurve sizeCurve = AnimationCurve.Linear(0f, 1f, 0.2f, 0.5f);
        // Deviation from the curve
        public float sizeDeviation = 0.1f;

        // Random rotation settings
        public bool randomizeXRotation = true;
        public bool randomizeYRotation = true;
        public bool randomizeZRotation = true;
        public Vector2 xRotationRange = new Vector2(-10f, 10f);
        public Vector2 yRotationRange = new Vector2(-180f, 180f);
        public Vector2 zRotationRange = new Vector2(-10f, 10f);

        // Offset from center
        public Vector3 centerOffset;

        // Reference to RockMeshCombiner component
        private RockMeshCombiner rockMeshCombiner;

        private void Start()
        {
            // Find and assign RockGen component
            RockGenComponentFind();
            // Find and assign RockFragment prefab
            RockGenFragmentPrefabFind();
        }

#if UNITY_EDITOR
        // Editor-only validation
        void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // Validate and assign RockGen component
                RockGenComponentFind();
                // Validate and assign RockFragment prefab
                RockGenFragmentPrefabFind();
            }
        }
#endif

        // Find and assign RockGen component
        private void RockGenComponentFind()
        {
            if (rockGenerator == null)
            {
                rockGenerator = GetComponent<RockGen>();
            }
            if (rockGenerator == null)
            {
                Debug.LogError("RockGen component not found!");
            }
        }

        // Add RockMeshCombiner component to the game object
        public void AddRockMeshCombinerComponent()
        {
            rockMeshCombiner = gameObject.GetComponent<RockMeshCombiner>();
            if (rockMeshCombiner == null)
            {
                rockMeshCombiner = gameObject.AddComponent<RockMeshCombiner>();
            }
            else
            {
                Debug.LogWarning("The RockMeshCombiner component has already been added!");
            }

            if (rockMeshCombiner == null)
            {
                Debug.LogError("RockMeshCombiner component not found!");
            }
        }

        // Find and assign RockFragment prefab
        private void RockGenFragmentPrefabFind()
        {
            if (rockFragmentPrefab == null)
            {
                rockFragmentPrefab = FindRockFragmentPrefab();

                if (rockFragmentPrefab == null)
                {
                    Debug.LogError("RockFragment prefab not found!");
                }
            }
        }

        // Find RockFragment prefab in resources
        private GameObject FindRockFragmentPrefab()
        {
            GameObject prefab = Resources.Load<GameObject>("RockFragment");

            return prefab;
        }

        // Create child stones based on specified parameters
        public void CreateChildStones()
        {
            if (rockFragmentPrefab == null)
            {
                Debug.LogError("RockFragment prefab not assigned!");
                return;
            }

            DeleteChildStones();

            for (int i = 0; i < numberOfFragments; i++)
            {
                if (minPlacementRadius >= maxPlacementRadius)
                {
                    maxPlacementRadius = minPlacementRadius + 0.01f;
                }
                float randomRadius = Random.Range(minPlacementRadius, maxPlacementRadius);

                Vector3 randomPosition = Random.insideUnitSphere * randomRadius;
                randomPosition.y = groundLevel + Random.Range(minDeviation, maxDeviation); // Randomize height within deviation from ground level

                // Create a new vector with zero Y component
                Vector3 horizontalPosition = new Vector3(randomPosition.x, 0f, randomPosition.z);

                // Check condition only for horizontal position
                while (horizontalPosition.magnitude < minPlacementRadius)
                {
                    randomPosition = Random.insideUnitSphere * randomRadius;
                    horizontalPosition = new Vector3(randomPosition.x, 0f, randomPosition.z);
                    randomPosition.y = groundLevel + Random.Range(minDeviation, maxDeviation); // Randomize height within deviation from ground level
                }

                GameObject childStone = Instantiate(rockFragmentPrefab, new Vector3(transform.position.x + randomPosition.x, randomPosition.y, transform.position.z + randomPosition.z), Quaternion.identity);
                childStone.transform.SetParent(transform);

                // Sample the curve for stone size
                float curveT = Mathf.InverseLerp(minPlacementRadius, maxPlacementRadius, randomRadius);
                float curveValue = sizeCurve.Evaluate(curveT);
                float randomSize = curveValue + Random.Range(-sizeDeviation, sizeDeviation);

                childStone.transform.localScale = Vector3.one * randomSize;

                // Randomize rotation
                Vector3 randomRotation = Vector3.zero;
                if (randomizeXRotation)
                    randomRotation.x = Random.Range(xRotationRange.x, xRotationRange.y);
                if (randomizeYRotation)
                    randomRotation.y = Random.Range(yRotationRange.x, yRotationRange.y);
                if (randomizeZRotation)
                    randomRotation.z = Random.Range(zRotationRange.x, zRotationRange.y);

                childStone.transform.localRotation = Quaternion.Euler(randomRotation);

                RockGen rockGenComponent = childStone.GetComponent<RockGen>();

                if (rockGenerator.detailLevel >= 2)
                {
                    rockGenComponent.detailLevel = Random.Range(2, 0);
                }
                else
                {
                    rockGenComponent.detailLevel = rockGenerator.detailLevel;
                }

                if (randomSize < (curveValue + sizeDeviation) * 0.6f)
                {
                    rockGenComponent.detailLevel = 0;
                }

                if (fragmentMaterials.Count > 0)
                {
                    rockGenComponent.mainMaterial = fragmentMaterials[Random.Range(0, fragmentMaterials.Count)];
                }
                else
                {
                    rockGenComponent.mainMaterial = rockGenerator.mainMaterial;
                }
                rockGenComponent.downForce = rockGenerator.downForce;
                rockGenComponent.noiseSeed = Random.Range(rockGenerator.noiseSeed + 1, rockGenerator.noiseSeed + numberOfFragments);
                rockGenComponent.surfaceType = rockGenerator.surfaceType;
                rockGenComponent.segmentCount = rockGenerator.segmentCount;
                rockGenComponent.segmentRadius = rockGenerator.segmentRadius;
                rockGenComponent.useDownForceAdditionalEffects = false;
                rockGenComponent.downForceDirection = rockGenerator.downForceDirection;
                rockGenComponent.vertexPositionInfluence = rockGenerator.vertexPositionInfluence;
                rockGenComponent.downForceCurve = new AnimationCurve(rockGenerator.downForceCurve.keys);
                rockGenComponent.downForceThreshold = rockGenerator.downForceThreshold;
                rockGenComponent.useRandomDownForceDirection = rockGenerator.useRandomDownForceDirection;
                rockGenComponent.downForceRandomSeed = rockGenerator.downForceRandomSeed;
                rockGenComponent.xAxisOffset = 0f;
                rockGenComponent.yAxisOffset = 0f;
                rockGenComponent.zAxisOffset = 0f;
                rockGenComponent.xRotation = rockGenerator.xRotation;
                rockGenComponent.yRotation = rockGenerator.yRotation;
                rockGenComponent.zRotation = rockGenerator.zRotation;
                rockGenComponent.xScale = rockGenerator.xScale;
                rockGenComponent.yScale = rockGenerator.yScale;
                rockGenComponent.zScale = rockGenerator.zScale;
                rockGenComponent.usePerlinNoise = rockGenerator.usePerlinNoise;
                rockGenComponent.noiseScale = rockGenerator.noiseScale;
                rockGenComponent.noiseStrength = rockGenerator.noiseStrength;
                rockGenComponent.tiltAngle = 0f;

                rockGenComponents.Add(rockGenComponent);
                childStones.Add(childStone);
            }
        }


        // Deletes all child stones and clears related lists
        public void DeleteChildStones()
        {
            foreach (GameObject child in childStones)
            {
                DestroyImmediate(child);
            }
            childStones.Clear();
            rockGenComponents.Clear();
        }

        // Draw gizmos to visualize placement radius and ground level
        private void OnDrawGizmosSelected()
        {
            // Draw circles to represent the minimum and maximum placement radius
            DrawCircle(transform.position, minPlacementRadius, Color.red);
            DrawCircle(transform.position, maxPlacementRadius, Color.blue);

            // Draw a grid of lines to represent the ground level
            Gizmos.color = Color.yellow;
            float gridSize = maxPlacementRadius / 10f; // Adjust grid density as needed
            float halfGridSize = gridSize * 0.5f;
            for (float x = -maxPlacementRadius; x <= maxPlacementRadius; x += gridSize)
            {
                for (float z = -maxPlacementRadius; z <= maxPlacementRadius; z += gridSize)
                {
                    Vector3 start = new Vector3(transform.position.x + x - halfGridSize, groundLevel, transform.position.z + z);
                    Vector3 end = new Vector3(transform.position.x + x + halfGridSize, groundLevel, transform.position.z + z);
                    Gizmos.DrawLine(start, end);

                    start = new Vector3(transform.position.x + x, groundLevel, transform.position.z + z - halfGridSize);
                    end = new Vector3(transform.position.x + x, groundLevel, transform.position.z + z + halfGridSize);
                    Gizmos.DrawLine(start, end);
                }
            }
        }

        // Draw a circle in the scene view
        private void DrawCircle(Vector3 center, float radius, Color color)
        {
            Gizmos.color = color;
            Vector3 prevPos = Vector3.zero;
            float angleStep = 10f; // Angle step for circle segments
            for (float angle = 0; angle <= 360; angle += angleStep)
            {
                float x = center.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                float z = center.z + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                Vector3 newPos = new Vector3(x, groundLevel, z);
                if (prevPos != Vector3.zero)
                {
                    Gizmos.DrawLine(prevPos, newPos);
                }
                prevPos = newPos;
            }
        }
    }
}
