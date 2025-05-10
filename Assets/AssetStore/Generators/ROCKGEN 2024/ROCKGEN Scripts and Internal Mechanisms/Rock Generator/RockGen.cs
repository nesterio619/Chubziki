using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RockGenerator
{
    // Namespace for Rock Generator functionalities
    [AddComponentMenu("ROCKGEN 2024/RockGen")] // Menu path for easier component access
    [ExecuteInEditMode] // Allow execution in Edit mode
    public class RockGen : MonoBehaviour
    {
        // Enumeration for different surface shapes
        public enum SurfaceShape
        {
            Box = 0,
            Noise = 1
        }

        // Global parameters
        [Header("Global")]
        [Tooltip("Main material")]
        public Material mainMaterial;

        [Tooltip("Number of faces depending on the shape")]
        [Range(0, 3)]
        public int detailLevel = 1;

        [Tooltip("Modifier for subsidence forms")]
        [Range(0, 100)]
        public float downForce = 3.0f;

        [Tooltip("Noise seed distribution")]
        [Min(0)]
        public int noiseSeed = 0;

        // Surface parameters
        [Header("Surface")]
        [Tooltip("Shape type or modifier specifying the form")]
        public SurfaceShape surfaceType = SurfaceShape.Box;

        [Tooltip("Number of modifications or segments of the form")]
        [Range(1, 15)]
        public int segmentCount = 2;

        [Tooltip("Influence radius")]
        [Range(0.48f, 1.4f)]
        public float segmentRadius = 1.14f;

        [Tooltip("Distribution function values for scale segments")]
        private AnimationCurve segmentScaleCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f, -1.936f, -1.936f), new Keyframe(0.997f, 0.0f, -0.289f, -0.289f));
        
        // Downforce customization
        [Header("DownForce Customization")]
        [Tooltip("Toggle to enable/disable the downForceCurve and downForceThreshold effects")]
        public bool useDownForceAdditionalEffects = true;

        [Tooltip("Direction of the downForce")]
        public Vector3 downForceDirection = new Vector3(0.0f, -1.0f, 0.0f);

        [Tooltip("Modify the influence of downForce based on vertex position")]
        [Range(0.0f, 1.0f)]
        public float vertexPositionInfluence = 0.5f;
        
        [Tooltip("Curve for customizing the downForce effect")]
        public AnimationCurve downForceCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("Threshold value for the downForce effect")]
        [Range(0, 1)]
        public float downForceThreshold = 0.5f;

        [Tooltip("Toggle to use a random downForce direction")]
        public bool useRandomDownForceDirection = false;

        [Tooltip("Customize the randomness seed for downForce direction")]
        public int downForceRandomSeed = 0;

        // Stone size settings
        [Header("Stone Size Settings")]
        [Tooltip("Offset along the X-axis")]
        public float xAxisOffset = 0.0f;

        [Tooltip("Offset along the Y-axis")]
        public float yAxisOffset = 0.0f;

        [Tooltip("Offset along the Z-axis")]
        public float zAxisOffset = 0.0f;

        [Tooltip("Rotation around the X-axis")]
        [Range(-360f, 360f)]
        public float xRotation = 0.0f;

        [Tooltip("Rotation around the Y-axis")]
        [Range(-360f, 360f)]
        public float yRotation = 0.0f;

        [Tooltip("Rotation around the Z-axis")]
        [Range(-360f, 360f)]
        public float zRotation = 0.0f;

        [Tooltip("Scale along the X-axis")]
        [Range(0.1f, 5)]
        public float xScale = 1.0f;

        [Tooltip("Scale along the Y-axis")]
        [Range(0.1f, 5)]
        public float yScale = 1.0f;

        [Tooltip("Scale along the Z-axis")]
        [Range(0.1f, 5)]
        public float zScale = 1.0f;

        // Advanced settings
        [Header("Advanced Settings")]
        [Tooltip("Toggle to use Perlin noise for surface variations")]
        public bool usePerlinNoise = true;

        [Tooltip("Scale of the Perlin noise")]
        [Range(0f, 15f)]
        public float noiseScale = 0.1f;

        [Tooltip("Strength of the Perlin noise effect")]
        public float noiseStrength = 0.1f;

        // Tilt settings
        [Header("Tilt Settings")]
        [Tooltip("Angle of tilt around the selected tilt axis")]
        [Range(0.0f, 180.0f)]
        public float tiltAngle = 0.0f;

        [Tooltip("Axis around which the stone tilts")]
        public Vector3 tiltAxis = Vector3.up;

        [Header("Tilt Pivot Point")]
        [Tooltip("Pivot point for the tilt operation")]
        public Vector3 tiltPivot = Vector3.zero;

        // Dummy comments for non-functional components
        [Space(10)]
        [TextArea]
        [Tooltip("Doesn't do anything. Just comments shown in inspector")]
        public string ConvertToFBXPrefabVariant = "Main Editor top bar -> GameObject -> Convert To FBX Prefab Variant (More details in the documentation)";

        [Space(10)]
        [TextArea]
        [Tooltip("Doesn't do anything. Just comments shown in inspector")]
        public string ExportToFBX = "Main Editor top bar -> GameObject -> Export To FBX (More details in the documentation)";

        // Reference to RockCluster component
        [HideInInspector] public RockCluster rockCluster;

        // Static variables for temporary mesh data
        private static MeshTmp _rock_tmp;
        private static MeshTmp _sub_tmp;
        private static int _seed_tmp;
        private bool _generate = false;

        void Start()
        {
            // Automatically generate mesh on Start
            GenerateMesh();
        }

        // Method called when any parameter changes in the inspector
        void OnValidate()
        {
            // Mark for mesh generation
            _generate = true;
        }

        // LateUpdate method to ensure mesh is generated after all updates
        private void LateUpdate()
        {
            if (_generate)
            {
                GenerateMesh();
                _generate = false;
            }
        }

        // Method to add RockCluster component to GameObject
        public void AddClusterComponent()
        {
            rockCluster = gameObject.GetComponent<RockCluster>();
            if (rockCluster == null)
            {
                rockCluster = gameObject.AddComponent<RockCluster>();
            }
            else
            {
                Debug.LogWarning("The RockCluster component has already been added!");
            }

            if (rockCluster == null)
            {
                Debug.LogError("RockCluster component not found!");
            }
        }

        // Method to generate the mesh
        public void GenerateMesh()
        {
            segmentCount = Mathf.Clamp(segmentCount, 0, 25);
            if (mainMaterial == null)
                mainMaterial = new Material(Shader.Find("Diffuse"));

            Mesh mesh = new Mesh();
            _rock_tmp.Clean();
            _sub_tmp.Clean();

            _seed_tmp = noiseSeed;
            GenerateRock(Vector3.one, Vector3.zero, Vector3.up, Vector3.right);

            if (surfaceType == SurfaceShape.Box || surfaceType == SurfaceShape.Noise)
                mesh.name = "Rock";

            mesh.triangles = null;
            MeshTmp.MixToSubMesh(new MeshTmp[2] { _rock_tmp, _sub_tmp }, ref mesh);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.uv = LowPolyGenerator.CalcUV(mesh.vertices, mesh.normals, mesh.bounds);

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            if (mesh.subMeshCount == 1)
                meshRenderer.sharedMaterials = new Material[1] { mainMaterial };

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            _rock_tmp.Clean();
            _sub_tmp.Clean();
        }

        // Method to apply sphere surface modification for scaling
        void SphereScaleSurfaceMod(ref Vector3[] vertices, Vector3 position, float offset, AnimationCurve force, float radius)
        {
            Vector3 normalizedPosition = position.normalized;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertexPos = vertices[i] - position;
                float length = vertexPos.magnitude;
                if (length < radius)
                {
                    vertices[i] = position + vertexPos - normalizedPosition * Vector3.Dot(normalizedPosition, vertexPos) * force.Evaluate(vertexPos.magnitude / radius);
                }
            }
        }

        // Method to apply downforce curve to vertices
        void ApplyDownForceCurve(ref Vector3[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                float downForceFactor = downForceCurve.Evaluate(vertices[i].y);
                vertices[i] = new Vector3(vertices[i].x, Mathf.Lerp(-1.0f, vertices[i].y, Mathf.Pow(Mathf.Clamp(vertices[i].y * 0.5f + 0.5f, 0.0f, 1.0f), downForceFactor)), vertices[i].z);
            }
        }

        // Method to apply downforce threshold to vertices
        void ApplyDownForceThreshold(ref Vector3[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                float force = Vector3.Dot(vertices[i], downForceDirection);

                if (force < downForceThreshold)
                {
                    float downForceFactor = Mathf.Lerp(0.0f, 1.0f, vertexPositionInfluence);
                    vertices[i] = new Vector3(vertices[i].x, Mathf.Lerp(-1.0f, vertices[i].y, Mathf.Pow(Mathf.Clamp(vertices[i].y * 0.5f + 0.5f, 0.0f, 1.0f), downForceFactor)), vertices[i].z);
                }
            }
        }

        // Method to apply downforce direction to vertices
        void ApplyDownForceDirection(ref Vector3[] vertices)
        {
            Vector3 direction = useRandomDownForceDirection ? GetRandomDownForceDirection() : downForceDirection;

            for (int i = 0; i < vertices.Length; i++)
            {
                float force = Vector3.Dot(vertices[i], direction.normalized);

                if (force < downForceThreshold)
                {
                    float downForceFactor = Mathf.Lerp(0.0f, 1.0f, vertexPositionInfluence);
                    vertices[i] = new Vector3(vertices[i].x, Mathf.Lerp(-1.0f, vertices[i].y, Mathf.Pow(Mathf.Clamp(vertices[i].y * 0.5f + 0.5f, 0.0f, 1.0f), downForceFactor)), vertices[i].z);
                }
            }
        }

        // Method to get a random downforce direction
        Vector3 GetRandomDownForceDirection()
        {
            Random.InitState(downForceRandomSeed);
            float randomX = Random.Range(-1.0f, 1.0f);
            float randomY = Random.Range(-1.0f, 1.0f);
            float randomZ = Random.Range(-1.0f, 1.0f);

            Vector3 randomDirection = new Vector3(randomX, randomY, randomZ);
            randomDirection.Normalize();

            return randomDirection;
        }

        // Method to generate rock mesh
        void GenerateRock(Vector3 scale, Vector3 from, Vector3 normal, Vector3 tangent)
        {
            if (surfaceType == SurfaceShape.Box || surfaceType == SurfaceShape.Noise)
                GenerateIcosahedron(scale, from, normal, tangent);

            Vector3[] vertices = _rock_tmp.vec;
            if (vertices != null && useDownForceAdditionalEffects)
            {
                ApplyDownForceCurve(ref vertices);
                ApplyDownForceThreshold(ref vertices);
                ApplyDownForceDirection(ref vertices);
                _rock_tmp.vec = vertices;
            }

            Matrix4x4 transformationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(xRotation, yRotation, zRotation), new Vector3(xScale, yScale, zScale));

            // Calculate tilt matrix
            Vector3 tiltAxis = Vector3.Cross(normal, Vector3.up).normalized;
            Quaternion tiltRotation = Quaternion.AngleAxis(tiltAngle, tiltAxis);
            Matrix4x4 tiltMatrix = Matrix4x4.TRS(Vector3.zero, tiltRotation, Vector3.one);

            for (int i = 0; i < vertices.Length; i++)
            {
                // Apply the tilt matrix to each vertex
                vertices[i] = tiltMatrix.MultiplyPoint3x4(vertices[i]);

                vertices[i] = transformationMatrix.MultiplyPoint3x4(vertices[i]);
                vertices[i] += new Vector3(xAxisOffset, yAxisOffset, zAxisOffset);
            }
        }


        // Method to generate icosahedron mesh
        void GenerateIcosahedron(Vector3 scale, Vector3 from, Vector3 normal, Vector3 tangent)
        {
            if (_rock_tmp.vec != null && _rock_tmp.vec.Length > 32768)
                return;

            detailLevel = Mathf.Clamp(detailLevel, 0, 3);
            int vertexCount = (20) * 3;
            Vector3[] vertices = new Vector3[vertexCount];

            Matrix4x4 u = LowPolyGenerator.Matrix((normal + LowPolyGenerator.VecrotNoize(noiseSeed).normalized * 0.5f).normalized, tangent);

            LowPolyGenerator.IcosahedronBegin(ref vertices);
            for (int i = 0; i < detailLevel; i++)
            {
                Vector3[] newVertices = new Vector3[vertices.Length * 4];
                LowPolyGenerator.IcosahedronDetailed(ref vertices, ref newVertices);
                vertices = newVertices;
            }
            vertexCount = vertices.Length;

            float tiltAngleRad = tiltAngle * Mathf.Deg2Rad;
            Quaternion tiltQuaternion = Quaternion.AngleAxis(tiltAngle, tiltAxis);

            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 vertex = vertices[i];

                // Apply pivot point for the tilt operation
                vertex -= tiltPivot;

                // Apply the combined tilt quaternion around the specified axis
                vertex = tiltQuaternion * vertex;

                // Apply pivot point back to the vertex
                vertex += tiltPivot;

                vertices[i] = vertex;
            }


            if (surfaceType == SurfaceShape.Noise)
            {
                for (int j = 0; j < 10; j++)
                {
                    for (int i = 0; i < segmentCount; i++)
                    {
                        SphereScaleSurfaceMod(ref vertices, LowPolyGenerator.VecrotNoize(_seed_tmp + i).normalized * 0.9f, 1.0f, segmentScaleCurve, segmentRadius);
                    }
                }
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i] = Quaternion.FromToRotation(LowPolyGenerator.VecrotNoize(_seed_tmp + segmentCount - 1).normalized, Vector3.down) * vertices[i];
                }
            }

            if (usePerlinNoise)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    float perlinValue = Mathf.PerlinNoise(vertices[i].x * noiseScale, vertices[i].y * noiseScale) * noiseStrength;
                    vertices[i] += normal * perlinValue;
                }
            }

            if (surfaceType == SurfaceShape.Box)
            {
                for (int j = 0; j < segmentCount; j++)
                {
                    SphereScaleSurfaceMod(ref vertices, Vector3.left * 0.9f, 1.0f, segmentScaleCurve, segmentRadius);
                    SphereScaleSurfaceMod(ref vertices, Vector3.right * 0.9f, 1.0f, segmentScaleCurve, segmentRadius);
                    SphereScaleSurfaceMod(ref vertices, Vector3.up * 0.9f, 1.0f, segmentScaleCurve, segmentRadius);
                    SphereScaleSurfaceMod(ref vertices, Vector3.down * 0.9f, 1.0f, segmentScaleCurve, segmentRadius);
                    SphereScaleSurfaceMod(ref vertices, Vector3.forward * 0.9f, 1.0f, segmentScaleCurve, segmentRadius);
                    SphereScaleSurfaceMod(ref vertices, Vector3.back * 0.9f, 1.0f, segmentScaleCurve, segmentRadius);
                }
            }
            _seed_tmp += segmentCount;

            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i] = new Vector3(vertices[i].x, Mathf.Lerp(-1.0f, vertices[i].y, Mathf.Pow(Mathf.Clamp(vertices[i].y * 0.5f + 0.5f, 0.0f, 1.0f), downForce)), vertices[i].z);
                vertices[i].Scale(scale);
                vertices[i] += from;
            }

            _rock_tmp.AddVertex(vertices, vertexCount, 3);
        }
    }
}
