using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Created by : Glenn korver
/// </summary>
namespace SimpleMeshGenerator
{
    public static class ProjectUtility
    {
        private static string _splitter = ":::::";
        private static string[] _stringSeparators = new string[] { _splitter };

        public static void SaveMeshAsAsset(Mesh mesh, string filePath, string meshName)
        {
#if UNITY_EDITOR
            string filePathFinal = Path.Combine(filePath, meshName) + ".asset";

            if (File.Exists(filePathFinal))
            {
                AssetDatabase.DeleteAsset(filePathFinal);
            }
            else
            {
                CreateDirectory(filePath);
            }

            if (AssetDatabase.Contains(mesh) == false)
            {
                AssetDatabase.CreateAsset(mesh, filePathFinal);
                AssetDatabase.SaveAssets();
            }
#endif
        }

        public static void SaveMeshAsJSON(Mesh mesh, string filePath, string name, bool prettyPrint = false)
        {
            name = name.ToLowerInvariant();
            var data = MeshData.CreateData(mesh, name);

            string filePathFinal = Path.Combine(filePath, name) + ".json";

#if UNITY_EDITOR
            if (File.Exists(filePathFinal))
            {
                AssetDatabase.DeleteAsset(filePathFinal);
            }
            else
            {
                CreateDirectory(filePath);
            }
#else

            if(DoesDirectoryExist(filePath) == false)
            {
                CreateDirectory(filePath);
            }
#endif

            File.WriteAllText(filePathFinal, JsonUtility.ToJson(data, prettyPrint));
        }

        public static void SaveMeshToClipboard(Mesh mesh, string name, bool prettyPrint = false)
        {
            name = name.ToLowerInvariant();
            var data = MeshData.CreateData(mesh, name);

            GUIUtility.systemCopyBuffer = JsonUtility.ToJson(data, prettyPrint);
        }

        public static string ConvertMeshToString(Mesh mesh, string name, bool saveToClipboard)
        {
            string dataString = JsonUtility.ToJson(MeshData.CreateData(mesh, name));

            if (saveToClipboard)
            {
                GUIUtility.systemCopyBuffer = dataString;
            }

            return dataString;
        }

        public static string ConvertMeshesToString(Mesh[] meshes, bool saveToClipboard, bool prettyPrint = false)
        {
            string dataString = "";

            for(int i = 0; i < meshes.Length; i++)
            {
                dataString += _splitter;
                dataString += JsonUtility.ToJson(MeshData.CreateData(meshes[i], i.ToString()), prettyPrint);
            }

            if(saveToClipboard)
            {
                GUIUtility.systemCopyBuffer = dataString;
            }

            return dataString;
        }

        public static string LoadSavedMeshFileFromResources(string _textAssetName)
        {
            return ((TextAsset)Resources.Load(_textAssetName, typeof(TextAsset))).text;
        }


        public static void GenerateMeshesFromFile(string meshDataString, Transform parent = null)
        {
            var combinedJsonString = meshDataString;
            var splittedJsonStrings = combinedJsonString.Split(_stringSeparators, StringSplitOptions.None);

            for (int i = 0; i < splittedJsonStrings.Length; i++)
            {
                var jsonString = splittedJsonStrings[i];

                if (jsonString.Length > 0)
                {
                    MeshData data = JsonUtility.FromJson<MeshData>(jsonString);

                    var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    g.GetComponent<MeshFilter>().mesh = data.CreateMesh();

                    g.transform.parent = parent;
                    g.transform.position = Vector3.right * i * 2;

                    g.name = data.Name;
                }
                else
                {
                    Debug.LogError("No data found");
                }
            }
        }


        private static string CreateDirectory(string path, string folder, bool deleteAlreadyExisting = false)
        {
            var completePath = Path.Combine(path, folder);

            return CreateDirectory(completePath, deleteAlreadyExisting);
        }

        private static string CreateDirectory(string path, bool deleteAlreadyExisting = false)
        {
            if (DoesDirectoryExist(path))
            {
                if (deleteAlreadyExisting)
                {
                    Directory.Delete(path, true);
                    Directory.CreateDirectory(path);
                }
            }
            else
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }


        private static bool DoesDirectoryExist(string path, string folder)
        {
            var completePath = Path.Combine(path, folder);

            return DoesDirectoryExist(completePath);
        }

        private static bool DoesDirectoryExist(string path)
        {
            return Directory.Exists(path);
        }




        [Serializable]
        public class MeshData
        {
            public string Name;
            public Vector3[] Verts;
            public Vector4[] UVs0;
            public Vector4[] UVs1;
            public Vector4[] UVs2;
            public Vector4[] UVs3;
            public Vector3[] Normals;
            public Vector4[] Tangents;
            public int[] Triangles;

            public Mesh CreateMesh()
            {
                var mesh = new Mesh();
                mesh.SetVertices(Verts);
                mesh.SetUVs(0, UVs0);
                mesh.SetUVs(0, UVs1);
                mesh.SetUVs(0, UVs2);
                mesh.SetUVs(0, UVs3);
                mesh.SetNormals(Normals);
                mesh.SetTangents(Tangents);
                mesh.SetTriangles(Triangles, 0);

                return mesh;
            }

            private static float TrimmedFloat(float value)
            {
                return Mathf.RoundToInt(value * 1000f) / 1000f;
            }

            public static MeshData CreateData(Mesh mesh, string name, bool trim = false)
            {
                var data = new MeshData();

                data.Name = name;

                data.Verts = mesh.vertices;
                data.Normals = mesh.normals;
                data.Tangents = mesh.tangents;
                data.Triangles = mesh.triangles;

                List<Vector4> uvs = new List<Vector4>();
                mesh.GetUVs(0, uvs);
                data.UVs0 = uvs.ToArray();
                mesh.GetUVs(1, uvs);
                data.UVs1 = uvs.ToArray();
                mesh.GetUVs(2, uvs);
                data.UVs2 = uvs.ToArray();
                mesh.GetUVs(3, uvs);
                data.UVs3 = uvs.ToArray();

                if (trim)
                {
                    for (int i = 0; i < data.Verts.Length; i++)
                    {
                        data.Verts[i] = new Vector3(TrimmedFloat(data.Verts[i].x), TrimmedFloat(data.Verts[i].y), TrimmedFloat(data.Verts[i].z));
                        if (data.UVs0.Length == data.Verts.Length) data.UVs0[i] = new Vector4(TrimmedFloat(data.UVs0[i].x), TrimmedFloat(data.UVs0[i].y), TrimmedFloat(data.UVs0[i].z), TrimmedFloat(data.UVs0[i].w));
                        if (data.UVs1.Length == data.Verts.Length) data.UVs1[i] = new Vector4(TrimmedFloat(data.UVs1[i].x), TrimmedFloat(data.UVs1[i].y), TrimmedFloat(data.UVs1[i].z), TrimmedFloat(data.UVs1[i].w));
                        if (data.UVs2.Length == data.Verts.Length) data.UVs2[i] = new Vector4(TrimmedFloat(data.UVs2[i].x), TrimmedFloat(data.UVs2[i].y), TrimmedFloat(data.UVs2[i].z), TrimmedFloat(data.UVs2[i].w));
                        if (data.UVs3.Length == data.Verts.Length) data.UVs3[i] = new Vector4(TrimmedFloat(data.UVs3[i].x), TrimmedFloat(data.UVs3[i].y), TrimmedFloat(data.UVs3[i].z), TrimmedFloat(data.UVs3[i].w));
                    }
                }

                return data;
            }

        }
    }
}
