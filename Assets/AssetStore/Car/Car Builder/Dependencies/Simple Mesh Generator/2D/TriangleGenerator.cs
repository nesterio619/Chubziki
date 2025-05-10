using Core.Utilities;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: NA
/// 
/// UV0
/// None
/// 
/// UV1
/// None
/// 
/// UV2
/// None
/// 
/// </summary>

namespace SimpleMeshGenerator
{
    public static class TriangleGenerator
    {
        private static Vector2 _uv = new Vector2(0, 0);

        private static Mesh _mesh = new Mesh();

        private static List<Vector3> _verts = new List<Vector3>();
        private static List<Vector2> _uvs = new List<Vector2>();
        private static List<Vector3> _normals = new List<Vector3>();
        private static List<int> _triangles = new List<int>();

        public static Mesh Generate(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector2 uv, Vector3 normal, bool flipOrientation = false)
        {
            return GenerateMesh(new Vector3[] {pointA, pointB, pointC }, uv, normal, flipOrientation);
        }

        public static Mesh Generate(Vector3[] points, Vector2 uv, Vector3 normal, bool flipOrientation = false)
        {
            return GenerateMesh(points, uv, normal, flipOrientation);
        }

        public static Mesh Generate_Hollow(Vector3[] pointsOuter_3, Vector3 normal, float widthInwards, bool flipOrientation = false)
        {
            var innerPoints = new Vector3[3];
            for (int i = 0; i < 3; i++)
            {
                var next = pointsOuter_3[(i + 1) % 3];

                var prevID = i - 1;
                if (prevID < 0) prevID = 2;
                var prev = pointsOuter_3[prevID];

                var dir = (next - pointsOuter_3[i]).normalized + (prev - pointsOuter_3[i]).normalized;
                dir = dir.normalized;
                innerPoints[i] = pointsOuter_3[i] + dir * widthInwards;
            }

            return GenerateMesh_Hollow(pointsOuter_3, innerPoints, normal, flipOrientation);
        }

        private static Mesh GenerateMesh(Vector3[] points, Vector2 uv, Vector3 normal, bool flipOrientation)
        {
            _verts.Clear();
            _uvs.Clear();
            _normals.Clear();
            _triangles.Clear();
  
            // we are making a circle
            for (int i = 0; i < points.Length; i++)
            {
                _verts.Add(points[i]);
                _uvs.Add(uv);
                _normals.Add(normal);
            }

            if (flipOrientation)
            {
                _triangles.Add(2);
                _triangles.Add(1);
                _triangles.Add(0);
            }
            else
            {
                _triangles.Add(0);
                _triangles.Add(1);
                _triangles.Add(2);
            }

            if (_mesh == null) _mesh = new Mesh();

            if (_verts.Exists(vector => vector.IsNaN()))
                return _mesh;

            _mesh.Clear();
            _mesh.SetVertices(_verts);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.SetNormals(_normals);

            return _mesh;
        }

        private static Mesh GenerateMesh_Hollow(Vector3[] pointsOuter_3, Vector3[] pointsInner_3, Vector3 normal, bool flipOrientation)
        {
            _mesh.Clear();

            for (int i = 0; i < 3; i++)
            {
                var points = new Vector3[4];
                points[0] = pointsInner_3[i];
                points[1] = pointsOuter_3[i];
                points[2] = pointsOuter_3[(i + 1) % 3];
                points[3] = pointsInner_3[(i + 1) % 3];

                CombineMeshes.Combine(_mesh, QuadGenerator_3D.Generate(points, Vector2Int.one, normal, flipOrientation));
            }

            return _mesh;
        }
    }
}
