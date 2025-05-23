﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityMeshSimplifier
{
    public static class LODGenerator
    {
        public const string LODParentGameObjectName = "_Rocks_LODs_";
        public const string LODAssetParentPath = "Assets/ROCKGEN 2024/ROCKGEN Rocks LODs/";

        public struct StaticRenderer
        {
            public string name;
            public bool isNewMesh;
            public Transform transform;
            public Mesh mesh;
            public Material[] materials;
            public MeshRenderer originalRenderer;
        }

        public struct SkinnedRenderer
        {
            public bool hasBlendShapes;
            public string name;
            public bool isNewMesh;
            public Transform transform;
            public Mesh mesh;
            public Material[] materials;
            public Transform rootBone;
            public Transform[] bones;
            public SkinnedMeshRenderer originalRenderer;
        }

        public static LODGroup GenerateLODs(LODGeneratorHelper generatorHelper)
        {
            if (generatorHelper == null)
                throw new System.ArgumentNullException(nameof(generatorHelper));

            var gameObject = generatorHelper.gameObject;
            var levels = generatorHelper.Levels;
            bool autoCollectRenderers = generatorHelper.AutoCollectRenderers;
            var simplificationOptions = generatorHelper.SimplificationOptions;
            string saveAssetsPath = generatorHelper.SaveAssetsPath;

            var lodGroup = GenerateLODs(gameObject, levels, autoCollectRenderers, simplificationOptions, saveAssetsPath);
            if (lodGroup == null)
                return null;

            lodGroup.animateCrossFading = generatorHelper.AnimateCrossFading;
            lodGroup.fadeMode = generatorHelper.FadeMode;
            return lodGroup;
        }

        public static LODGroup GenerateLODs(GameObject gameObject, LODLevel[] levels, bool autoCollectRenderers, SimplificationOptions simplificationOptions)
        {
            return GenerateLODs(gameObject, levels, autoCollectRenderers, simplificationOptions, null);
        }

        public static LODGroup GenerateLODs(GameObject gameObject, LODLevel[] levels, bool autoCollectRenderers, SimplificationOptions simplificationOptions, string saveAssetsPath)
        {
            if (gameObject == null)
                throw new System.ArgumentNullException(nameof(gameObject));
            else if (levels == null)
                throw new System.ArgumentNullException(nameof(levels));

            var transform = gameObject.transform;
            var existingLodParent = transform.Find(LODParentGameObjectName);
            if (existingLodParent != null)
                throw new System.InvalidOperationException("The game object already appears to have LODs. Please remove them first.");

            var existingLodGroup = gameObject.GetComponent<LODGroup>();
            if (existingLodGroup != null)
                throw new System.InvalidOperationException("The game object already appears to have an LOD Group. Please remove it first.");

            saveAssetsPath = ValidateSaveAssetsPath(saveAssetsPath);

            var lodParentGameObject = new GameObject(LODParentGameObjectName);
            var lodParent = lodParentGameObject.transform;
            ParentAndResetTransform(lodParent, transform);

            var lodGroup = gameObject.AddComponent<LODGroup>();

            Renderer[] allRenderers = null;
            if (autoCollectRenderers)
            {
                // Collect all enabled renderers under the game object
                allRenderers = GetChildRenderersForLOD(gameObject);
            }

            var renderersToDisable = new List<Renderer>((allRenderers != null ? allRenderers.Length : 10));
            var lods = new LOD[levels.Length];

            for (int levelIndex = 0; levelIndex < levels.Length; levelIndex++)
            {
                var level = levels[levelIndex];
                var levelGameObject = new GameObject(string.Format("Level{0:00}", levelIndex));
                var levelTransform = levelGameObject.transform;
                ParentAndResetTransform(levelTransform, lodParent);

                Renderer[] originalLevelRenderers = allRenderers ?? level.Renderers;
                var levelRenderers = new List<Renderer>((originalLevelRenderers != null ? originalLevelRenderers.Length : 0));

                if (originalLevelRenderers != null && originalLevelRenderers.Length > 0)
                {
                    var meshRenderers = (from renderer in originalLevelRenderers
                                         where renderer.enabled && renderer as MeshRenderer != null
                                        && renderer.transform.GetComponent<MeshFilter>() != null
                                        && renderer.transform.GetComponent<MeshFilter>().sharedMesh != null
                                         select renderer as MeshRenderer).ToArray();
                    var skinnedMeshRenderers = (from renderer in originalLevelRenderers
                                                where renderer.enabled && renderer as SkinnedMeshRenderer != null
                                                && renderer.transform.GetComponent<SkinnedMeshRenderer>().sharedMesh != null
                                                select renderer as SkinnedMeshRenderer).ToArray();

                    StaticRenderer[] staticRenderers;
                    SkinnedRenderer[] skinnedRenderers;

                    if (level.CombineMeshes)
                    {
                        SkinnedMeshRenderer[] skinnedRenderersActuallyCombined = null;

                        staticRenderers = CombineStaticMeshes(transform, levelIndex, meshRenderers);
                        skinnedRenderers = CombineSkinnedMeshes(transform, levelIndex, skinnedMeshRenderers, ref skinnedRenderersActuallyCombined);             
                    }
                    else
                    {
                        staticRenderers = GetStaticRenderers(meshRenderers);
                        skinnedRenderers = GetSkinnedRenderers(skinnedMeshRenderers);
                    }

                    if (staticRenderers != null)
                    {
                        for (int rendererIndex = 0; rendererIndex < staticRenderers.Length; rendererIndex++)
                        {
                            var renderer = staticRenderers[rendererIndex];
                            var mesh = renderer.mesh;
                           
                            // Simplify the mesh if necessary
                            if (level.Quality < 1f)
                            {
                                mesh = SimplifyMesh(mesh, level.Quality, simplificationOptions);
                                SaveLODMeshAsset(mesh, gameObject.name, renderer.name, levelIndex, renderer.mesh.name, saveAssetsPath);

                                if (renderer.isNewMesh)
                                {
                                    DestroyObject(renderer.mesh);
                                    renderer.mesh = null;
                                }
                            }

                            string rendererName = string.Format("{0:000}_static_{1}", rendererIndex, renderer.name);
                            var levelRenderer = CreateLevelRenderer(rendererName, levelTransform, renderer.transform, mesh, renderer.materials, ref level);
                            levelRenderers.Add(levelRenderer);
                        }
                    }

                    if (skinnedRenderers != null)
                    {
                        for (int rendererIndex = 0; rendererIndex < skinnedRenderers.Length; rendererIndex++)
                        {
                            var renderer = skinnedRenderers[rendererIndex];
                            var mesh = renderer.mesh;

                            // Simplify the mesh if necessary
                            if (level.Quality < 1f)
                            {
                                mesh = SimplifyMesh(mesh, level.Quality, simplificationOptions);
                                SaveLODMeshAsset(mesh, gameObject.name, renderer.name, levelIndex, renderer.mesh.name, saveAssetsPath);

                                if (renderer.isNewMesh)
                                {
                                    DestroyObject(renderer.mesh);
                                    renderer.mesh = null;
                                }
                            }

                            string rendererName = string.Format("{0:000}_skinned_{1}", rendererIndex, renderer.name);
                            var levelRenderer = CreateSkinnedLevelRenderer(rendererName, levelTransform, renderer.transform, mesh, renderer.materials, renderer.rootBone, renderer.bones, ref level);
                            levelRenderers.Add(levelRenderer);
                        }
                    }
                }

                foreach (var renderer in originalLevelRenderers)
                {
                    if (!renderersToDisable.Contains(renderer))
                    {
                        renderersToDisable.Add(renderer);
                    }
                }

                lods[levelIndex] = new LOD(level.ScreenRelativeTransitionHeight, levelRenderers.ToArray());
            }

            foreach (var renderer in renderersToDisable)
            {
                renderer.enabled = false;
            }

            lodGroup.animateCrossFading = false;
            lodGroup.SetLODs(lods);
            return lodGroup;
        }

        public static StaticRenderer[] GetStaticRenderers(MeshRenderer[] renderers)
        {
            if(renderers == null || renderers.Length == 0) { return null; }

            var newRenderers = new List<StaticRenderer>(renderers.Length);

            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    Debug.LogWarning("A renderer was missing a mesh filter and was ignored.", renderer);
                    continue;
                }

                var mesh = meshFilter.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogWarning("A renderer was missing a mesh and was ignored.", renderer);
                    continue;
                }

                newRenderers.Add(new StaticRenderer()
                {
                    name = renderer.name,
                    isNewMesh = false,
                    transform = renderer.transform,
                    mesh = mesh,
                    materials = renderer.sharedMaterials,
                    originalRenderer = renderer
                });
            }
            return newRenderers.ToArray();
        }

        public static SkinnedRenderer[] GetSkinnedRenderers(SkinnedMeshRenderer[] renderers)
        {

            if (renderers == null || renderers.Length == 0) { return null; }

            var newRenderers = new List<SkinnedRenderer>(renderers.Length);

            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];

                var mesh = renderer.sharedMesh;
                if (mesh == null)
                {
                    Debug.LogWarning("A renderer was missing a mesh and was ignored.", renderer);
                    continue;
                }

                newRenderers.Add(new SkinnedRenderer()
                {
                    name = renderer.name,
                    isNewMesh = false,
                    transform = renderer.transform,
                    mesh = mesh,
                    materials = renderer.sharedMaterials,
                    rootBone = renderer.rootBone,
                    bones = renderer.bones,
                    originalRenderer = renderer
                });
            }
            return newRenderers.ToArray();
        }


        public static StaticRenderer[] CombineStaticMeshes(Transform transform, int levelIndex, MeshRenderer[] renderers, bool autoName = true, string combinedBaseName = "")
        {
            if (renderers.Length == 0)
                return null;

            var newRenderers = new List<StaticRenderer>(renderers.Length);
            Material[] combinedMaterials;
            var combinedMesh = MeshCombiner.CombineMeshes(transform, renderers, out combinedMaterials);
            string baseName = string.IsNullOrWhiteSpace(combinedBaseName) ? transform.name : combinedBaseName;

            string rendererName = string.Format("{0}_combined_static", baseName);

            if(autoName)
            {
                if(transform != null)
                {
                    combinedMesh.name = string.Format("{0}_static{1:00}", transform.name, levelIndex);
                }
            }

            newRenderers.Add(new StaticRenderer()
            {
                name = rendererName,
                isNewMesh = true,
                transform = null,
                mesh = combinedMesh,
                materials = combinedMaterials
            });

            return newRenderers.ToArray();
        }


        public static SkinnedRenderer[] CombineSkinnedMeshes(Transform transform, int levelIndex, SkinnedMeshRenderer[] renderers, ref SkinnedMeshRenderer[] renderersActuallyCombined, bool autoName = true, string combinedBaseName = "")
        {
            if (renderers.Length == 0)
                return null;

            // TODO: Support to merge sub-meshes and atlas textures

            var newRenderers = new List<SkinnedRenderer>(renderers.Length);
            //var blendShapeRenderers = (from renderer in renderers
            //                           where renderer.sharedMesh != null && renderer.sharedMesh.blendShapeCount > 0
            //                           select renderer); //baw did

            var renderersWithoutMesh = (from renderer in renderers
                                        where renderer.sharedMesh == null
                                        select renderer);
            var combineRenderers = (from renderer in renderers
                                    where renderer.sharedMesh != null // && renderer.sharedMesh.blendShapeCount == 0 baw did
                                    select renderer).ToArray(); 


            renderersActuallyCombined = combineRenderers;

            // Warn about renderers without a mesh
            foreach (var renderer in renderersWithoutMesh)
            {
                Debug.LogWarning("A renderer was missing a mesh and was ignored.", renderer);
            }

            if (combineRenderers.Length > 0)
            {
                Material[] combinedMaterials;
                Transform[] combinedBones;
                var combinedMesh = MeshCombiner.CombineMeshes(transform, combineRenderers, out combinedMaterials, out combinedBones);
                string baseName = string.IsNullOrWhiteSpace(combinedBaseName) ? transform.name : combinedBaseName;
                string rendererName = string.Format("{0}_combined_skinned", baseName);

                if (autoName)
                {
                    combinedMesh.name = string.Format("{0}_skinned{1:00}", transform.name, levelIndex);
                }

                var rootBone = FindBestRootBone(transform, combineRenderers);

                newRenderers.Add(new SkinnedRenderer()
                {
                    name = rendererName,
                    isNewMesh = false,
                    transform = null,
                    mesh = combinedMesh,
                    materials = combinedMaterials,
                    rootBone = rootBone,
                    bones = combinedBones
                });

            }

            return newRenderers.ToArray();
        }



        private static void ParentAndResetTransform(Transform transform, Transform parentTransform)
        {
            transform.SetParent(parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        private static void ParentAndOffsetTransform(Transform transform, Transform parentTransform, Transform originalTransform)
        {
            transform.position = originalTransform.position;
            transform.rotation = originalTransform.rotation;
            transform.localScale = originalTransform.lossyScale;
            transform.SetParent(parentTransform, true);
        }

        private static MeshRenderer CreateLevelRenderer(string name, Transform parentTransform, Transform originalTransform, Mesh mesh, Material[] materials, ref LODLevel level)
        {
            var levelGameObject = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            var levelTransform = levelGameObject.transform;
            if (originalTransform != null)
            {
                ParentAndOffsetTransform(levelTransform, parentTransform, originalTransform);
            }
            else
            {
                ParentAndResetTransform(levelTransform, parentTransform);
            }

            var meshFilter = levelGameObject.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = levelGameObject.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;
            SetupLevelRenderer(meshRenderer, ref level);
            return meshRenderer;
        }

        private static SkinnedMeshRenderer CreateSkinnedLevelRenderer(string name, Transform parentTransform, Transform originalTransform, Mesh mesh, Material[] materials, Transform rootBone, Transform[] bones, ref LODLevel level)
        {
            var levelGameObject = new GameObject(name, typeof(SkinnedMeshRenderer));
            var levelTransform = levelGameObject.transform;
            if (originalTransform != null)
            {
                ParentAndOffsetTransform(levelTransform, parentTransform, originalTransform);
            }
            else
            {
                ParentAndResetTransform(levelTransform, parentTransform);
            }

            var skinnedMeshRenderer = levelGameObject.GetComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.sharedMesh = mesh;
            skinnedMeshRenderer.sharedMaterials = materials;
            skinnedMeshRenderer.rootBone = rootBone;
            skinnedMeshRenderer.bones = bones;
            SetupLevelRenderer(skinnedMeshRenderer, ref level);
            return skinnedMeshRenderer;
        }

        private static Transform FindBestRootBone(Transform transform, SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
                return null;

            Transform bestBone = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                if (skinnedMeshRenderers[i] == null || skinnedMeshRenderers[i].rootBone == null)
                    continue;

                var rootBone = skinnedMeshRenderers[i].rootBone;
                var distance = (rootBone.position - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestBone = rootBone;
                    bestDistance = distance;
                }
            }

            return bestBone;
        }


        private static Transform FindBestRootBone(Dictionary<Transform, Transform> topLevelParents, SkinnedMeshRenderer[] skinnedMeshRenderers)
        {
            if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
                return null;

            Transform bestBone = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                if (skinnedMeshRenderers[i] == null || skinnedMeshRenderers[i].rootBone == null)
                    continue;

                Transform topParent = topLevelParents[skinnedMeshRenderers[i].transform];
                var rootBone = skinnedMeshRenderers[i].rootBone;
                var distance = (rootBone.position - topParent.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestBone = rootBone;
                    bestDistance = distance;
                }
            }

            return bestBone;
        }

        private static void SetupLevelRenderer(Renderer renderer, ref LODLevel level)
        {
            renderer.shadowCastingMode = level.ShadowCastingMode;
            renderer.receiveShadows = level.ReceiveShadows;
            renderer.motionVectorGenerationMode = level.MotionVectorGenerationMode;
            renderer.lightProbeUsage = level.LightProbeUsage;
            renderer.reflectionProbeUsage = level.ReflectionProbeUsage;

            var skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.quality = level.SkinQuality;
                skinnedMeshRenderer.skinnedMotionVectors = level.SkinnedMotionVectors;
            }
        }
        
        private static Renderer[] GetChildRenderersForLOD(GameObject gameObject)
        {
            var resultRenderers = new List<Renderer>();
            CollectChildRenderersForLOD(gameObject.transform, resultRenderers);
            return resultRenderers.ToArray();
        }

        private static void CollectChildRenderersForLOD(Transform transform, List<Renderer> resultRenderers)
        {
            // Collect the rendererers of this transform
            var childRenderers = transform.GetComponents<Renderer>();
            resultRenderers.AddRange(childRenderers);

            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                // Skip children that are not active
                var childTransform = transform.GetChild(i);
                if (!childTransform.gameObject.activeSelf)
                    continue;

                // If the transform have the identical name as to our LOD Parent GO name, then we also skip it
                if (string.Equals(childTransform.name, LODParentGameObjectName))
                    continue;

                // Skip children that has a LOD Group or a LOD Generator Helper component
                if (childTransform.GetComponent<LODGroup>() != null)
                    continue;
                else if (childTransform.GetComponent<LODGeneratorHelper>() != null)
                    continue;

                // Continue recursively through the children of this transform
                CollectChildRenderersForLOD(childTransform, resultRenderers);
            }
        }

        private static Mesh SimplifyMesh(Mesh mesh, float quality, SimplificationOptions options)
        {
            var meshSimplifier = new MeshSimplifier();
            meshSimplifier.PreserveBorderEdges = options.PreserveBorderEdges;
            meshSimplifier.PreserveUVSeamEdges = options.PreserveUVSeamEdges;
            meshSimplifier.PreserveUVFoldoverEdges = options.PreserveUVFoldoverEdges;
            meshSimplifier.EnableSmartLink = options.EnableSmartLink;
            meshSimplifier.VertexLinkDistance = options.VertexLinkDistance;
            meshSimplifier.MaxIterationCount = options.MaxIterationCount;
            meshSimplifier.Aggressiveness = options.Agressiveness;
           
            meshSimplifier.Initialize(mesh);
            meshSimplifier.SimplifyMesh(quality);

            var simplifiedMesh = meshSimplifier.ToMesh();
            simplifiedMesh.bindposes = mesh.bindposes;
            return simplifiedMesh;
        }

        private static void DestroyObject(Object obj)
        {
            if (obj == null)
                throw new System.ArgumentNullException(nameof(obj));

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj, false);
            }
#else
            Object.Destroy(obj);
#endif
        }

        private static void DestroyLODAssets(Transform transform)
        {
#if UNITY_EDITOR
            var renderers = transform.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                var meshRenderer = renderer as MeshRenderer;
                var skinnedMeshRenderer = renderer as SkinnedMeshRenderer;

                if (meshRenderer != null)
                {
                    var meshFilter = meshRenderer.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        DestroyLODAsset(meshFilter.sharedMesh);
                    }
                }
                else if (skinnedMeshRenderer != null)
                {
                    DestroyLODAsset(skinnedMeshRenderer.sharedMesh);
                }

                foreach (var material in renderer.sharedMaterials)
                {
                    DestroyLODMaterialAsset(material);
                }
            }

            // Delete any empty LOD asset directories
            DeleteEmptyDirectory(LODAssetParentPath.TrimEnd('/'));
#endif
        }

        private static void DestroyLODMaterialAsset(Material material)
        {
            if (material == null)
                return;

#if UNITY_EDITOR
            var shader = material.shader;
            if (shader == null)
                return;

            // We find all texture properties of materials and delete those assets also
            int propertyCount = UnityEditor.ShaderUtil.GetPropertyCount(shader);
            for (int propertyIndex = 0; propertyIndex < propertyCount; propertyIndex++)
            {
                var propertyType = UnityEditor.ShaderUtil.GetPropertyType(shader, propertyIndex);
                if (propertyType == UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propertyName = UnityEditor.ShaderUtil.GetPropertyName(shader, propertyIndex);
                    var texture = material.GetTexture(propertyName);
                    DestroyLODAsset(texture);
                }
            }

            DestroyLODAsset(material);
#endif
        }

        private static void DestroyLODAsset(Object asset)
        {
            if (asset == null)
                return;

#if UNITY_EDITOR
            // We only delete assets that we have automatically generated
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(asset);
            if (assetPath.StartsWith(LODAssetParentPath))
            {
                UnityEditor.AssetDatabase.DeleteAsset(assetPath);
            }
#endif
        }

        private static void SaveLODMeshAsset(Object asset, string gameObjectName, string rendererName, int levelIndex, string meshName, string saveAssetsPath)
        {
            gameObjectName = MakePathSafe(gameObjectName);
            rendererName = MakePathSafe(rendererName);
            meshName = MakePathSafe(meshName);
            meshName = string.Format("{0:00}_{1}", levelIndex, meshName);

            string path;
            if (!string.IsNullOrEmpty(saveAssetsPath))
            {
                path = string.Format("{0}{1}/{2}.mesh", LODAssetParentPath, saveAssetsPath, meshName);
            }
            else
            {
                path = string.Format("{0}{1}/{2}/{3}.mesh", LODAssetParentPath, gameObjectName, rendererName, meshName);
            }

            SaveAsset(asset, path);
        }

        private static void SaveAsset(Object asset, string path)
        {
#if UNITY_EDITOR
            CreateParentDirectory(path);

            // Make sure that there is no asset with the same path already
            path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path);

            UnityEditor.AssetDatabase.CreateAsset(asset, path);
#endif
        }

        private static void CreateParentDirectory(string path)
        {
#if UNITY_EDITOR
            int lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex != -1)
            {
                string parentPath = path.Substring(0, lastSlashIndex);
                if (!UnityEditor.AssetDatabase.IsValidFolder(parentPath))
                {
                    lastSlashIndex = parentPath.LastIndexOf('/');
                    if (lastSlashIndex != -1)
                    {
                        string folderName = parentPath.Substring(lastSlashIndex + 1);
                        string folderParentPath = parentPath.Substring(0, lastSlashIndex);
                        CreateParentDirectory(parentPath);
                        UnityEditor.AssetDatabase.CreateFolder(folderParentPath, folderName);
                    }
                    else
                    {
                        UnityEditor.AssetDatabase.CreateFolder(string.Empty, parentPath);
                    }
                }
            }
#endif
        }

        private static string MakePathSafe(string name)
        {
            var sb = new System.Text.StringBuilder(name.Length);
            bool lastWasSeparator = false;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    lastWasSeparator = false;
                    sb.Append(c);
                }
                else if (c == '_' || c == '-')
                {
                    if (!lastWasSeparator)
                    {
                        lastWasSeparator = true;
                        sb.Append(c);
                    }
                }
                else
                {
                    if (!lastWasSeparator)
                    {
                        lastWasSeparator = true;
                        sb.Append('_');
                    }
                }
            }
            return sb.ToString();
        }

        private static string ValidateSaveAssetsPath(string saveAssetsPath)
        {
            if (string.IsNullOrEmpty(saveAssetsPath))
                return null;

            saveAssetsPath = saveAssetsPath.Replace('\\', '/');
            saveAssetsPath = saveAssetsPath.Trim('/');

            if (System.IO.Path.IsPathRooted(saveAssetsPath))
                throw new System.InvalidOperationException("The save assets path cannot be rooted.");
            else if (saveAssetsPath.Length > 100)
                throw new System.InvalidOperationException("The save assets path cannot be more than 100 characters long to avoid I/O issues.");

            // Make the path safe
            var pathParts = saveAssetsPath.Split('/');
            for (int i = 0; i < pathParts.Length; i++)
            {
                pathParts[i] = MakePathSafe(pathParts[i]);
            }
            saveAssetsPath = string.Join("/", pathParts);

            return saveAssetsPath;
        }

        private static bool DeleteEmptyDirectory(string path)
        {
#if UNITY_EDITOR
            bool deletedAllSubFolders = true;
            var subFolders = UnityEditor.AssetDatabase.GetSubFolders(path);
            for (int i = 0; i < subFolders.Length; i++)
            {
                if (!DeleteEmptyDirectory(subFolders[i]))
                {
                    deletedAllSubFolders = false;
                }
            }

            if (!deletedAllSubFolders)
                return false;

            string[] assetGuids = UnityEditor.AssetDatabase.FindAssets(string.Empty, new string[] { path });
            if (assetGuids.Length > 0)
                return false;

            return UnityEditor.AssetDatabase.DeleteAsset(path);
#else
            return false;
#endif
        }
    }
}
