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
/// Note: caps are half spheres and keep their uvs
/// 
/// </summary>

namespace SimpleMeshGenerator
{
    public static class CapsuleGenerator
    {
        private static List<Vector3> _verts = new List<Vector3>();
        private static List<Vector2> _uvs0 = new List<Vector2>();
        private static List<Vector3> _normals = new List<Vector3>();

        private static Mesh _mesh = new Mesh();
        private static Mesh _meshOther = new Mesh();

        private static List<Vector3> _vertsCap = new List<Vector3>();
        private static List<Vector2> _uvs0Cap = new List<Vector2>();
        private static List<Vector3> _normalsCap = new List<Vector3>();

        public static Mesh Generate(float radius, float height, int resolutionLoops = 20)
        {
            _mesh.Clear();
            _verts.Clear();
            _uvs0.Clear();
            _normals.Clear();
        
            var middle = CylinderGenerator.Generate(radius, height, resolutionLoops, 2, false, GeneralMeshGenerator.Axis.Y, false);
            CombineMeshes.Combine(_mesh, middle);

            //caps
            var caps = SphereGenerator.Generate(radius, resolutionLoops, true);
            var ringMiddle = Mathf.FloorToInt((resolutionLoops / 2f) + 0.05f);
            var ringVertCount = resolutionLoops + 1;

            caps.GetVertices(_vertsCap);
            caps.GetUVs(0, _uvs0Cap);
            caps.GetNormals(_normalsCap);


            //top
            for (int i = ringMiddle * ringVertCount; i < _vertsCap.Count; i++)
            {
                _verts.Add(_vertsCap[i] + Vector3.up * height * 0.5f);
                _uvs0.Add(_uvs0Cap[i]);
                _normals.Add(_normalsCap[i]);
            }

            _meshOther.Clear();
            var top = _meshOther;
            top.SetVertices(_verts);
            top.SetUVs(0, _uvs0);
            top.SetNormals(_normals);

            var rings = _vertsCap.Count - (ringMiddle * ringVertCount);
            rings = rings / ringVertCount;
            top.SetTriangles(MeshManipulation.TriangleGeneration(rings, ringVertCount, false), 0);
            CombineMeshes.Combine(_mesh, top);


            //bottom
            _verts.Clear();
            _uvs0.Clear();
            _normals.Clear();

            for (int i = 0; i < (ringMiddle + 1) * ringVertCount; i++)
            {
                _verts.Add(_vertsCap[i] - Vector3.up * height * 0.5f);
                _uvs0.Add(_uvs0Cap[i]);
                _normals.Add(_normalsCap[i]);
            }

            _meshOther.Clear();
            var bottom = _meshOther;
            bottom.SetVertices(_verts);
            bottom.SetUVs(0, _uvs0);
            bottom.SetNormals(_normals);

            rings = ringMiddle + 1;
            bottom.SetTriangles(MeshManipulation.TriangleGeneration(rings, ringVertCount, false), 0);
            CombineMeshes.Combine(_mesh, bottom);

            return _mesh;
        }
    }
}
