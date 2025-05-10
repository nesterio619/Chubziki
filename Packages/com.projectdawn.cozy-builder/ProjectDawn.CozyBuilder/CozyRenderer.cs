using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace ProjectDawn.CozyBuilder
{
    public enum Scaling
    {
        None,
        Stretch,
    }

    public enum Placement
    {
        Center,
        Edge,
    }

    public interface ICozyHashable
    {
        uint CalculateHash();
    }

    public interface ICozyBuilder : ICozyHashable
    {
        void Build(ref CozyBuilderContext context);
    }

    public struct CozyRenderNode
    {
        public Material Material;
        public Mesh Mesh;
        public float4x4 Transform;
    }

    public struct CozyRenderNodeBatchComparer : IComparer<CozyRenderNode>
    {
        public int Compare(CozyRenderNode x, CozyRenderNode y)
        {
            int result = x.Material.GetHashCode().CompareTo(y.Material.GetHashCode());
            if (result != 0)
                return result;
            return x.Mesh.GetHashCode().CompareTo(y.Mesh.GetHashCode());
        }
    }


    public class CozyArtifact
    {
        public uint Hash;
        public ulong Version;
        public List<GameObject> GameObjects;
        public List<CozyRenderNode> Nodes;
    }

    public struct CozyBuilderContext
    {
        internal CozyRenderMode Mode;
        internal Transform Parent;
        internal List<GameObject> GameObjects;
        internal List<CozyRenderNode> Nodes;


        public void Instantiate(GameObject prefab, float3 position, quaternion rotation, float3 scale, Material overrideMaterial = null)
        {
            Assert.IsFalse(math.any(math.isnan(position)));
            Assert.IsFalse(math.any(math.isnan(rotation.value)));
            Assert.IsFalse(math.any(math.isnan(scale)));
            if (Mode == CozyRenderMode.GameObjects)
            {
                //if (GameObjects == null)
                //    GameObjects = new List<GameObject>();

                GameObject instance = GameObject.Instantiate(prefab);
                if (overrideMaterial != null)
                {
                    foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                        renderer.sharedMaterial = overrideMaterial;
                }
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.transform.localScale = scale;
                instance.transform.SetParent(Parent);
                instance.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
#if UNITY_EDITOR
                foreach (Transform transform in instance.GetComponentsInChildren<Transform>())
                {
                    UnityEditor.SceneVisibilityManager.instance.DisablePicking(transform.gameObject, false);
                }
#endif
                GameObjects.Add(instance);
            }
            else
            {
                //if (Nodes == null)
                //    Nodes = new List<CozyRenderNode>();

                foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>())
                {
                    if (!renderer.TryGetComponent(out MeshFilter filter))
                        continue;

                    Nodes.Add(new CozyRenderNode
                    {
                        Mesh = filter.sharedMesh,
                        Material = overrideMaterial ? overrideMaterial : renderer.sharedMaterial,
                        Transform = math.mul(float4x4.TRS(position, rotation, scale), renderer.transform.localToWorldMatrix),
                    });
                }
            }
        }

        public void Instantiate(Material material, Mesh mesh, float3 position, quaternion rotation)
        {
            Assert.IsFalse(math.any(math.isnan(position)));
            Assert.IsFalse(math.any(math.isnan(rotation.value)));
            if (Mode == CozyRenderMode.GameObjects)
            {
                //if (GameObjects == null)
                //    GameObjects = new List<GameObject>();

                GameObject instance = new GameObject("instance", typeof(MeshRenderer), typeof(MeshFilter));
                instance.GetComponent<MeshFilter>().sharedMesh = mesh;
                instance.GetComponent<MeshRenderer>().sharedMaterial = material;
                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.transform.SetParent(Parent);
                instance.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
#if UNITY_EDITOR
                foreach (Transform transform in instance.GetComponentsInChildren<Transform>())
                {
                    UnityEditor.SceneVisibilityManager.instance.DisablePicking(transform.gameObject, false);
                }
#endif
                GameObjects.Add(instance);
            }
            else
            {
                //if (Nodes == null)
                //    Nodes = new List<CozyRenderNode>();

                if (!material.enableInstancing)
                {
                    Debug.LogWarning($"Material {material} need instancing enabled!");
                    return;
                }

                Nodes.Add(new CozyRenderNode
                {
                    Mesh = mesh,
                    Material = material,
                    Transform = float4x4.TRS(position, rotation, 1),
                });
            }
        }
    }

    public enum CozyRenderMode
    {
        GameObjects,
        ProceduralDraw,
    }

    public enum CozyBuildMode
    {
        Always,
        Never,
    }

    [AddComponentMenu("Cozy Builder/Cozy Renderer")]
    [ExecuteAlways]
    public class CozyRenderer : MonoBehaviour
    {
        readonly static ProfilerMarker RenderMarker = new ProfilerMarker("CozyRenderer.Render");
        readonly static ProfilerMarker BuildMarker = new ProfilerMarker("CozyRenderer.Build");

        CozyRenderMode RenderMode = CozyRenderMode.ProceduralDraw;
        public CozyBuildMode BuildMode = CozyBuildMode.Always;

        ulong ArtifactVersion;
        Dictionary<object, CozyArtifact> Artifacts = new();
        Transform ArtifactsParent;

        ulong RenderVersion;
        List<CozyRenderNode> RenderNodes = new();
        NativeList<Matrix4x4> RenderTransforms;
        NativeList<RangeInt> RenderRanges;

        ulong CleanupVersion;

        bool IsNestedRenderer()
        {
            var parent = transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<CozyRenderer>() != null)
                    return true;
                parent = parent.parent;
            }
            return false;
        }

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera.cameraType == CameraType.Preview)
                return;

            if (RenderPipelineManager.currentPipeline == null)
                return;

            // In prefab preview skip all renderers that are not part of prefab
#if UNITY_EDITOR
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (stage && stage.mode == UnityEditor.SceneManagement.PrefabStage.Mode.InIsolation && stage.prefabContentsRoot != gameObject)
                return;
#endif

            Render(camera);
        }

        void Update()
        {
            if (BuildMode == CozyBuildMode.Always)
                Build();

            // For builtin pipeline
            if (RenderPipelineManager.currentPipeline == null)
                Render();
        }

        public void Build()
        {
            if (IsNestedRenderer())
            {
                Clear();
                return;
            }

            using var _ = BuildMarker.Auto();
            using (var builders = this.GetComponentsInChildrenScoped<ICozyBuilder>())
                foreach (var builder in builders.List)
                {
                    uint hash = builder.CalculateHash();
                    if (Artifacts.TryGetValue(builder, out CozyArtifact artifact))
                    {
                        if (artifact.Hash != hash)
                        {
                            artifact.Hash = hash;

                            ClearArtifact(artifact);

                            if (hash != 0)
                            {
                                if (ArtifactsParent == null)
                                {
                                    var artifactParent = new GameObject("Artifacts");
                                    artifactParent.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                                    ArtifactsParent = artifactParent.transform;
                                    ArtifactsParent.transform.SetParent(transform);
                                }
                                CozyBuilderContext context = new()
                                {
                                    Mode = RenderMode,
                                    Parent = ArtifactsParent,
                                    GameObjects = artifact.GameObjects,
                                    Nodes = artifact.Nodes,
                                };
                                builder.Build(ref context);
                                ArtifactVersion++;
                            }
                        }
                        artifact.Version = CleanupVersion;
                    }
                    else if (hash != 0)
                    {
                        artifact = new CozyArtifact();
                        artifact.Hash = hash;
                        artifact.Nodes = new();
                        artifact.GameObjects = new();
                        if (ArtifactsParent == null)
                        {
                            var artifactParent = new GameObject("Artifacts");
                            artifactParent.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                            ArtifactsParent = artifactParent.transform;
                            ArtifactsParent.transform.SetParent(transform);
                        }
                        CozyBuilderContext context = new()
                        {
                            Mode = RenderMode,
                            Parent = ArtifactsParent,
                            GameObjects = artifact.GameObjects,
                            Nodes = artifact.Nodes,
                        };
                        builder.Build(ref context);
                        Artifacts.Add(builder, artifact);
                        ArtifactVersion++;
                        artifact.Version = CleanupVersion;
                    }
                }

            foreach (var artifact in Artifacts.Values)
            {
                if (CleanupVersion != artifact.Version)
                {
                    ClearArtifact(artifact);
                    artifact.Hash = 0;
                    ArtifactVersion++;
                }
            }

            using (var points = this.GetComponentsInChildrenScoped<CozyPoint>(true))
                foreach (var point in points.List)
                {
                    point.transform.localScale = Vector3.one;
                }
            using (var splines = this.GetComponentsInChildrenScoped<CozySpline>(true))
                foreach (var spline in splines.List)
                {
                    if (spline.CalculateHash() == 0)
                        continue;
                    spline.Position = spline.SampleSpline(0.5f);
                    spline.transform.localScale = Vector3.one;
                }
            using (var planes = this.GetComponentsInChildrenScoped<CozyPlane>(true))
                foreach (var plane in planes.List)
                {
                    if (plane.CalculateHash() == 0)
                        continue;
                    plane.Position = plane.SamplePlane(0.5f);
                    plane.transform.localScale = Vector3.one;
                }

            CleanupVersion++;
        }

        public void Clear()
        {
            foreach (var artifact in Artifacts.Values)
            {
                ClearArtifact(artifact);
            }
            Artifacts.Clear();
            RenderNodes.Clear();
            RenderTransforms.Clear();
            RenderRanges.Clear();
        }

        void Render(Camera camera = null)
        {
            using var _ = RenderMarker.Auto();
            if (RenderMode != CozyRenderMode.ProceduralDraw)
                return;

            if (RenderVersion != ArtifactVersion)
            {
                // Collect render nodes and sort them for batching
                RenderNodes.Clear();
                foreach (var artifact in Artifacts.Values)
                {
                    foreach (var node in artifact.Nodes)
                    {
                        RenderNodes.Add(node);
                    }
                }
                RenderNodes.Sort(new CozyRenderNodeBatchComparer());

                // Find batch ranges
                Mesh mesh = null;
                Material material = null;
                int start = 0;
                int count = 0;
                RenderRanges.Clear();
                RenderTransforms.Resize(RenderNodes.Count, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < RenderNodes.Count; i++)
                {
                    CozyRenderNode node = RenderNodes[i];
                    if (node.Mesh != mesh || node.Material != material || count == 1023)
                    {
                        if (count > 0)
                            RenderRanges.Add(new RangeInt(start, count));

                        mesh = node.Mesh;
                        material = node.Material;
                        count = 0;
                        start = i;
                    }

                    RenderTransforms[i] = RenderNodes[i].Transform;
                    count++;
                }
                if (count > 0)
                    RenderRanges.Add(new RangeInt(start, count));
                RenderVersion = ArtifactVersion;
            }

            foreach (var range in RenderRanges)
            {
                CozyRenderNode node = RenderNodes[range.start];

                // TODO
                RenderParams renderParams = new RenderParams(node.Material);
                renderParams.receiveShadows = true;
                renderParams.shadowCastingMode = ShadowCastingMode.On;
                renderParams.camera = camera;

                Graphics.RenderMeshInstanced(renderParams, node.Mesh, 0, RenderTransforms.AsArray(), range.length, range.start);
            }
        }

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderTransforms = new NativeList<Matrix4x4>(1, Allocator.Persistent);
            RenderRanges = new NativeList<RangeInt>(1, Allocator.Persistent);
        }

        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;

            Clear();

            RenderTransforms.Dispose();
            RenderRanges.Dispose();
        }

        void ClearArtifact(CozyArtifact artifact)
        {
            if (artifact.GameObjects != null)
            {
                foreach (var gameObject in artifact.GameObjects)
                {
                    DestroyImmediate(gameObject);
                }
            }

            if (artifact.Nodes != null)
            {
                artifact.Nodes.Clear();
            }
        }
    }
}