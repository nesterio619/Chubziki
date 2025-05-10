using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Inwards
/// 
/// UV0
/// X = 0 - 1: length
/// Y = 0 - 1: edge to edge
/// 
/// UV1
/// X = 0 - 1 length per loop
/// Y = 0 - 1 edge to edge
/// 
/// </summary>

namespace SimpleMeshGenerator
{
    public static class DoubleHelixGenerator
    {
        private static Vector3 _pos = new Vector3(0, 0, 0);
        private static Vector3 _normal = new Vector3(0, 0, 0);
        private static Vector2 _uv = new Vector2(0, 0);

        private static Mesh _mesh = new Mesh();

        private static List<Vector3> _verts = new List<Vector3>();
        private static List<Vector2> _uvs0 = new List<Vector2>();
        private static List<Vector2> _uvs1 = new List<Vector2>();
        private static List<Vector3> _normals = new List<Vector3>();

        public static Mesh Generate(int loops = 5, int resolutionLoop = 30, int resolutionWidth = 30, float radius = 1)
        {
            return GenerateMesh(loops, resolutionLoop, resolutionWidth, radius);
        }

        private static Mesh GenerateMesh(int loops, int resolutionLoop, int resolutionWidth, float radius = 1)
        {
            _verts.Clear();
            _uvs0.Clear();
            _uvs1.Clear();
            _normals.Clear();

            var helixA = new Vector3[resolutionLoop * loops];

            //Helix A
            for (int i = 0; i < loops; i++)
            {
                for (int j = 0; j < resolutionLoop; j++)
                {
                    float progressCircle = j / (float)(resolutionLoop - 1);
                    progressCircle += i;

                    float radians = progressCircle * 2 * Mathf.PI;

                    _pos.x = Mathf.Cos(radians) * radius;
                    _pos.y = Mathf.Sin(radians) * radius;
                    _pos.z = radians * radius;

                    helixA[i * resolutionLoop + j] = _pos;

                    _normal.x = Mathf.Cos(radians - Mathf.PI * 0.5f) * radius;
                    _normal.y = Mathf.Sin(radians - Mathf.PI * 0.5f) * radius;

                    for (int x = 0; x < resolutionWidth; x++)
                    {
                        _normals.Add(_normal.normalized);
                    }
                }
            }

            var helixB = new Vector3[resolutionLoop * loops];

            //Helix B
            for (int i = 0; i < loops; i++)
            {
                for (int j = 0; j < resolutionLoop; j++)
                {
                    float progressCircle = j / (float)(resolutionLoop - 1);
                    progressCircle += i;

                    float radians = progressCircle * 2 * Mathf.PI;

                    _pos.z = radians * radius;

                    radians += Mathf.PI;
                    _pos.x = Mathf.Cos(radians) * radius;
                    _pos.y = Mathf.Sin(radians) * radius;

                    helixB[i * resolutionLoop + j] = _pos;
                }
            }

            resolutionWidth = Mathf.Max(resolutionWidth, 2);

            for (int i = 0; i < helixA.Length; i++)
            {
                for (int x = 0; x < resolutionWidth; x++)
                {
                    float percentage = x / (float)(resolutionWidth - 1);

                    _verts.Add(Vector3.Lerp(helixA[i], helixB[i], percentage));

                    _uv.x = (float)i / (helixA.Length - 1);
                    _uv.y = percentage;
                    _uvs0.Add(_uv);

                    _uv.x = (float)i / (helixA.Length - 1) * loops * Mathf.PI;
                    _uv.y = percentage;
                    _uvs1.Add(_uv);
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(_verts);
            _mesh.SetUVs(0, _uvs0);
            _mesh.SetUVs(1, _uvs1);
            _mesh.SetTriangles(MeshManipulation.TriangleGeneration(helixA.Length, resolutionWidth, false), 0);
            _mesh.SetNormals(_normals);

            return _mesh;
        }
    }
}
