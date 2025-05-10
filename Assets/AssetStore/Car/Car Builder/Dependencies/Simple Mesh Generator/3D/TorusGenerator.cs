using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Outwards
/// 
/// UV0
/// X = 0 - 1: length
/// Y = 0 - 1: center to edge
/// 
/// UV1
/// X = 0 - 1: length
/// Y = height relative to circumference / X
/// 
/// UV2
/// X = circumference
/// Y = height relative to circumference
/// 
/// </summary>

namespace SimpleMeshGenerator
{
    public static class TorusGenerator
    {
        private static Vector2 _uv = new Vector2(0, 0);

        private static Mesh _mesh = new Mesh();

        private static List<Vector3> _verts = new List<Vector3>();
        private static List<Vector2> _uvs0 = new List<Vector2>();
        private static List<Vector2> _uvs1 = new List<Vector2>();
        private static List<Vector2> _uvs2 = new List<Vector2>();
        private static List<Vector3> _normals = new List<Vector3>();
        
        public static Mesh Generate(float radius, float thickness, int resolutionTorus = 20, int resolutionLoop = 12)
        {
            _verts.Clear();
            _uvs0.Clear();
            _uvs1.Clear();
            _uvs2.Clear();
            _normals.Clear();

            var circumferenceLoop = 2 * thickness * Mathf.PI;
            var circumferenceTorus = 2 * (radius + thickness * 0.5f) * Mathf.PI;

            var circumferenceRatio = circumferenceLoop / circumferenceTorus;

            Vector3 dir = new Vector3(0, 0, 0);

            for (int i = 0; i < resolutionTorus; i++)
            {
                var torusRingPercentage = (float)i / (resolutionTorus - 1);
                var rotationOffset_Radians = Mathf.PI * 0.5f;
                var radians = torusRingPercentage * 2 * Mathf.PI + rotationOffset_Radians;

                dir.x = Mathf.Cos(radians);
                dir.z = Mathf.Sin(radians);
                dir = dir.normalized;
                Vector3 pos = dir * radius;

                var loop = CircleGenerator.GetPoints(thickness, pos, Vector3.up, dir, resolutionLoop - 1);

                for (int j = 0; j < loop.Count; j++)
                {
                    var data = loop[j];
                    _verts.Add(data.Position);

                    _uv.x = torusRingPercentage;
                    _uv.y = data.UV.y;
                    _uvs0.Add(_uv);

                    _uv.y = data.UV.y * circumferenceRatio;
                    _uvs1.Add(_uv);

                    _uv.x = torusRingPercentage * circumferenceTorus;
                    _uv.y = data.UV.y * circumferenceRatio * circumferenceTorus;
                    _uvs2.Add(_uv);

                    _normals.Add(data.Normal);
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(_verts);
            _mesh.SetUVs(0, _uvs0);
            _mesh.SetUVs(1, _uvs1);
            _mesh.SetUVs(2, _uvs2);
            _mesh.SetTriangles(MeshManipulation.TriangleGeneration(resolutionTorus, resolutionLoop, true), 0);
            _mesh.SetNormals(_normals);

            return _mesh;
        }
    }
}
