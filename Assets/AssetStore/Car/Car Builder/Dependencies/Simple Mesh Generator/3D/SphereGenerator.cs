using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Outwards
/// 
/// UV0
/// X = 0 - 1: right to left
/// Y = 0 - 1: bottom to top
/// 
/// </summary>

namespace SimpleMeshGenerator
{
    public static class SphereGenerator
    {
        private static Vector2 _uv = new Vector2(0, 0);

        private static Mesh _mesh = new Mesh();

        private static List<Vector3> _verts = new List<Vector3>();
        private static List<Vector2> _uvs0 = new List<Vector2>();
        private static List<Vector3> _normals = new List<Vector3>();

        public static Mesh Generate(float radius, int resolutionLoops = 24, bool ensureRingInMiddle = true)
        {
            _verts.Clear();
            _uvs0.Clear();
            _normals.Clear();

            var ringCount = resolutionLoops;
            //ensure an uneven number, to have a ring in the middle
            if (ensureRingInMiddle && ringCount % 2 == 0)
                ringCount += 1;

            for (int y = 0; y < ringCount; y++)
            {
                var heightProgress = (float)y / (ringCount - 1);
                var height = heightProgress * 2 - 1; // -1 to 1

                var radiusScaler = Mathf.Cos(Mathf.PI * 0.5f * height);
                height = Mathf.Sin(Mathf.PI * 0.5f * height);
                height *= radius;

                // create ring
                var ring = CircleGenerator.GetPoints(radius * radiusScaler, Vector3.up * height, resolutionLoops, GeneralMeshGenerator.Axis2D.XZ);
                for (int i = 0; i < ring.Count; i++)
                {
                    _verts.Add(ring[i]);
                    _normals.Add(ring[i].normalized);

                    var ringProgress = (float)i / (ring.Count - 1);
                    _uv.x = ringProgress;
                    _uv.y = heightProgress;
                    _uvs0.Add(_uv);
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(_verts);
            _mesh.SetUVs(0, _uvs0);
            _mesh.SetNormals(_normals);

            var ringLoop = resolutionLoops + 1;
            _mesh.SetTriangles(MeshManipulation.TriangleGeneration(ringCount, ringLoop, false), 0);

            return _mesh;
        }
    }
}
