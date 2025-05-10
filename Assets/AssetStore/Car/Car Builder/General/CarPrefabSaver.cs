using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralCarBuilder
{
    public class CarPrefabSaver : MonoBehaviour
    {
        [SerializeField] private bool _overrideExisting = false;
        [Space]
        [SerializeField] private TemporaryCarInitializer _car = default;
        [SerializeField] private string _defaultPath = "Assets/CarGenerator";
        [SerializeField] private string _defaultName = "";

        private static string _meshSubFolder = "Model";
        private static string _textureSubfolder = "Textures";
        private static string _materialSubfolder = "Materials";
        private static string _prefabSubPath = "Prefab";

        private static string _colorTextureName = "ColorTexture";

        [ContextMenu("Save Manually")]
        private void Save()
        {
            SaveCar(_car, _defaultPath, _defaultName);
        }

        public void SaveCar(TemporaryCarInitializer car)
        {
            SaveCar(car, _defaultPath);
        }

        public void SaveCar(TemporaryCarInitializer car, string generalFilePath, string carName = "")
        {
            if (Directory.Exists(generalFilePath) == false)
            {
                Directory.CreateDirectory(generalFilePath);
            }

            if (string.IsNullOrEmpty(carName))
            {
                carName = "Car_" + Directory.GetDirectories(generalFilePath).Length.ToString();
            }
        
            if(DoesDirectoryExist(generalFilePath, carName) == false || _overrideExisting)
            {
                var carFolderPath = CreateDirectory(generalFilePath, carName, true);

                SaveTextures(car, CreateDirectory(carFolderPath, _textureSubfolder, true), carName);
                SaveMaterials(car, CreateDirectory(carFolderPath, _materialSubfolder, true));
                SaveMeshes(car, CreateDirectory(carFolderPath, _meshSubFolder, true));
                SavePrefab(car.gameObject, CreateDirectory(carFolderPath, _prefabSubPath, true), carName);

                Debug.LogWarning("Car prefab saved at: " + Path.Combine(generalFilePath, carName));
            }
            else
            {
                Debug.LogError("Car prefab not saved, Car already exists at given path");
            }    
        }


        private void SaveTextures(TemporaryCarInitializer car, string filePath, string textureAppendName)
        {
#if UNITY_EDITOR
            string filePathFinal = Path.Combine(filePath, _colorTextureName + "_" + textureAppendName) + ".png";
           
            if (File.Exists(filePathFinal))
            {
                Debug.Log("Delete Asset");
                AssetDatabase.DeleteAsset(filePathFinal);
            }
            else
            {
                var texture = car.ColorTextureExternal;
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(filePathFinal, bytes);

                AssetDatabase.Refresh();

                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(filePathFinal);
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = true;
                importer.SaveAndReimport();

                texture = (Texture2D)AssetDatabase.LoadAssetAtPath(filePathFinal, typeof(Texture2D));
                

                var renderers = car.GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    for (int j = 0; j < renderers[i].sharedMaterials.Length; j++)
                    {
                        var mat = renderers[i].sharedMaterials[j];

                        if (mat.mainTexture == car.ColorTextureInternal)
                            mat.mainTexture = texture;
                    }
                }

                AssetDatabase.SaveAssets();
            }
#endif
        }

        private void SaveMaterials(TemporaryCarInitializer car, string filePath)
        {
#if UNITY_EDITOR
            Dictionary<Material, Material> materialsMap = new Dictionary<Material, Material>();
            var renderers = car.GetComponentsInChildren<MeshRenderer>();

            #region CreateNewMaterialAssets
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].sharedMaterials.Length; j++)
                {
                    var sharedMat = renderers[i].sharedMaterials[j];

                    if (materialsMap.ContainsKey(sharedMat) == false)
                    {
                        var matAsset = new Material(sharedMat);
                        matAsset.name = sharedMat.name;
                        materialsMap.Add(sharedMat, matAsset);
                    }
                }
            }
            #endregion

            #region AssignNewMaterialAssets
            for (int i = 0; i < renderers.Length; i++)
            {
                List<Material> rendererMaterials = new List<Material>();

                for (int j = 0; j < renderers[i].sharedMaterials.Length; j++)
                {
                    var matAsset = materialsMap[renderers[i].sharedMaterials[j]];
                    Debug.Log(matAsset.name);
                    rendererMaterials.Add(matAsset);
                }

                renderers[i].sharedMaterials = rendererMaterials.ToArray();
            }
            #endregion

            foreach (var pair in materialsMap)
            {
                SaveMaterial(pair.Value, filePath, pair.Value.name);
            }

            void SaveMaterial(Material material, string materialFilePath, string name)
            {
                string filePathFinal = Path.Combine(materialFilePath, name) + ".mat";

                if (File.Exists(filePathFinal))
                {
                    AssetDatabase.DeleteAsset(filePathFinal);
                }

                if (AssetDatabase.Contains(material) == false)
                {
                    AssetDatabase.CreateAsset(material, filePathFinal);
                    AssetDatabase.SaveAssets();
                }
            }
#endif
        }

        private void SaveMeshes(TemporaryCarInitializer car, string filePath)
        {
#if UNITY_EDITOR
            SaveMesh(car.Windows.mesh, filePath, car.Windows.gameObject.name);
            SaveMesh(car.BodySide.mesh, filePath, car.BodySide.gameObject.name);
            SaveMesh(car.BodyTop.mesh, filePath, car.BodyTop.gameObject.name);
            SaveMesh(car.TrunkOuterSide.mesh, filePath, car.TrunkOuterSide.gameObject.name);
            SaveMesh(car.TrunkInnerSide.mesh, filePath, car.TrunkInnerSide.gameObject.name);
            SaveMesh(car.HoodOuterside.mesh, filePath, car.HoodOuterside.gameObject.name);
            SaveMesh(car.HoodInnerSide.mesh, filePath, car.HoodInnerSide.gameObject.name);
            SaveMesh(car.HeadLight_Left.mesh, filePath, car.HeadLight_Left.gameObject.name);
            SaveMesh(car.HeadLight_Right.mesh, filePath, car.HeadLight_Right.gameObject.name);

            for (int i = 0; i < car.Wheels.Length; i++)
                SaveMesh(car.Wheels[i].mesh, filePath, car.Wheels[i].gameObject.name);

            void SaveMesh(Mesh mesh, string meshFilePath, string meshName)
            {
                string filePathFinal = Path.Combine(meshFilePath, meshName) + ".asset";

                if (File.Exists(filePathFinal))
                {
                    AssetDatabase.DeleteAsset(filePathFinal);
                }

                if (AssetDatabase.Contains(mesh) == false)
                {
                    AssetDatabase.CreateAsset(mesh, filePathFinal);
                    AssetDatabase.SaveAssets();
                }
            }
#endif
        }

        private void SavePrefab(GameObject prefab, string filePath, string prefabName)
        {
#if UNITY_EDITOR
            var initializerComponent = prefab.GetComponent<TemporaryCarInitializer>();
            if (initializerComponent != null)
            {
                DestroyImmediate(initializerComponent, false);
            }

            prefab.name = prefabName;

            string filePathFinal = Path.Combine(filePath, prefabName) + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(prefab, filePathFinal);

            Destroy(prefab);
#endif
        }


        private string CreateDirectory(string path, string folder, bool deleteAlreadyExisting = false)
        {
            var completePath = Path.Combine(path, folder);

            if (DoesDirectoryExist(path, folder))
            {
                if (deleteAlreadyExisting)
                {
                    Directory.Delete(completePath, true);
                    Directory.CreateDirectory(completePath);
                }
            }
            else
            {
                Directory.CreateDirectory(completePath);
            }

            return completePath;
        }

        private bool DoesDirectoryExist(string path, string folder)
        {
            var completePath = Path.Combine(path, folder);

            return Directory.Exists(completePath);
        }
    }
}