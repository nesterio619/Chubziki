using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// </summary>
namespace SimpleMeshGenerator
{
    public static class CombineMeshes
    {
        private static List<Vector3> _vertsA = new List<Vector3>();
        private static List<Vector3> _vertsB = new List<Vector3>();

        private static List<Vector3> _normalsA = new List<Vector3>();
        private static List<Vector3> _normalsB = new List<Vector3>();

        private static List<Color> _colorsA = new List<Color>();
        private static List<Color> _colorsB = new List<Color>();

        private static List<Vector4> _uvsA0 = new List<Vector4>();
        private static List<Vector4> _uvsB0 = new List<Vector4>();

        private static List<Vector4> _uvsA1 = new List<Vector4>();
        private static List<Vector4> _uvsB1 = new List<Vector4>();

        private static List<Vector4> _uvsA2 = new List<Vector4>();
        private static List<Vector4> _uvsB2 = new List<Vector4>();

        private static List<int> _trianglesA = new List<int>();
        private static List<int> _trianglesB = new List<int>();

        public static Mesh Combine(Mesh a, Mesh b, bool keepOriginal = true)
        {
            a.GetVertices(_vertsA);
            b.GetVertices(_vertsB);
            int vertsCount_A = _vertsA.Count;
            _vertsA.AddRange(_vertsB);

            a.GetNormals(_normalsA);
            b.GetNormals(_normalsB);
            _normalsA.AddRange(_normalsB);

     
            a.GetTriangles(_trianglesA, 0, false);
            b.GetTriangles(_trianglesB, 0, false);

            _trianglesB.Offset(vertsCount_A);
            _trianglesA.AddRange(_trianglesB);


            a.GetColors(_colorsA);
            b.GetColors(_colorsB);
            _colorsA.AddRange(_colorsB);

            a.GetUVs(0, _uvsA0);
            b.GetUVs(0, _uvsB0);
            _uvsA0.AddRange(_uvsB0);

            a.GetUVs(1, _uvsA1);
            b.GetUVs(1, _uvsB1);
            _uvsA1.AddRange(_uvsB1);

            a.GetUVs(2, _uvsA2);
            b.GetUVs(2, _uvsB2);
            _uvsA2.AddRange(_uvsB2);


            var mesh = keepOriginal ? a : new Mesh();
            mesh.Clear();
            mesh.SetVertices(_vertsA);
            
            if (_normalsA.Count == _vertsA.Count) mesh.SetNormals(_normalsA);
            if (_colorsA.Count == _vertsA.Count) mesh.SetColors(_colorsA);

            if (_uvsA0.Count == _vertsA.Count) mesh.SetUVs(0, _uvsA0);
            if (_uvsA1.Count == _vertsA.Count) mesh.SetUVs(1, _uvsA1);
            if (_uvsA2.Count == _vertsA.Count) mesh.SetUVs(2, _uvsA2);

            mesh.SetTriangles(_trianglesA, 0);

            return mesh;
        }

        public static Mesh Clone(this Mesh m)
        {
            m.GetVertices(_vertsA);
            m.GetNormals(_normalsA);
            m.GetTriangles(_trianglesA, 0, false);
            m.GetColors(_colorsA);
            m.GetUVs(0, _uvsA0);
            m.GetUVs(1, _uvsA1);
            m.GetUVs(2, _uvsA2);

            var mesh = new Mesh();

            mesh.SetVertices(_vertsA);
            mesh.SetNormals(_normalsA);
            if (_colorsA.Count == _vertsA.Count) mesh.SetColors(_colorsA);
            mesh.SetUVs(0, _uvsA0);
            if (_uvsA1.Count == _vertsA.Count) mesh.SetUVs(1, _uvsA1);
            if (_uvsA2.Count == _vertsA.Count) mesh.SetUVs(2, _uvsA2);

            mesh.SetTriangles(_trianglesA, 0);

            return mesh;
        }


        public static void CreateBlendShape(Mesh baseMesh, Mesh shapeMesh, out Vector3[] deltaVerts, out Vector3[] deltaNormals, out Vector3[] deltaTangents)
        {
            var vertsA = baseMesh.vertices;
            var vertsB = shapeMesh.vertices;

            var normalsA = baseMesh.normals;
            var normalsB = shapeMesh.normals;

            var tangentsA = baseMesh.tangents;
            var tangentsB = shapeMesh.tangents;

            if (vertsA.Length != vertsB.Length)
            {
                Debug.LogError("Meshes are not compatible");
                deltaVerts = null;
                deltaNormals = null;
                deltaTangents = null;
            }
            else
            {
                deltaVerts = new Vector3[vertsA.Length];
                for (int i = 0; i < deltaVerts.Length; i++)
                {
                    deltaVerts[i] = vertsB[i] - vertsA[i];
                }

                bool meshContainsNormals = (normalsA.Length == vertsA.Length) && (normalsB.Length == vertsB.Length);
                if(meshContainsNormals)
                {
                    deltaNormals = new Vector3[normalsA.Length];
                    for (int i = 0; i < deltaNormals.Length; i++)
                    {
                        deltaNormals[i] = normalsB[i] - normalsA[i];
                    }
                }
                else
                {
                    deltaNormals = null;
                }

                bool meshContainsTangents = (tangentsA.Length == vertsA.Length) && (tangentsB.Length == vertsB.Length);
                if (meshContainsTangents)
                {
                    deltaTangents = new Vector3[tangentsA.Length];
                    for (int i = 0; i < deltaTangents.Length; i++)
                    {
                        deltaTangents[i] = tangentsB[i] - tangentsA[i];
                    }
                }
                else
                {
                    deltaTangents = null;
                }
            }
        }

        public static void AddBlendShapeFrame(Mesh baseMesh, string name, float framePercentage_0_1, Vector3[] deltaVerts, Vector3[] deltaNormals, Vector3[] deltaTangents)
        {
            baseMesh.AddBlendShapeFrame(name, Mathf.Clamp01(framePercentage_0_1) * 100, deltaVerts, deltaNormals, deltaTangents);
            baseMesh.UploadMeshData(false);
        }

        public static Mesh FlipOverX(this Mesh m)
        {
            m.GetVertices(_vertsA);
            m.GetNormals(_normalsA);
            m.GetTriangles(_trianglesA, 0, false);
            m.GetColors(_colorsA);
            m.GetUVs(0, _uvsA0);

            m.Clear();

            var vertCount = _vertsA.Count;
            for (int i = 0; i < vertCount; i++)
            {
                _vertsA[i] = _vertsA[i].FlipX();
                _normalsA[i] = _normalsA[i].FlipX();
            }

            var triangleCount = _trianglesA.Count;
            for (int i = 0; i < triangleCount; i += 3)
            {
                var a = _trianglesA[i + 1];
                var b = _trianglesA[i + 2];

                _trianglesA[i + 1] = b;
                _trianglesA[i + 2] = a;
            }

            m.SetVertices(_vertsA);
            m.SetNormals(_normalsA);
            m.SetUVs(0, _uvsA0);
            if (_colorsA.Count == _vertsA.Count) m.SetColors(_colorsA);

            m.SetTriangles(_trianglesA, 0);

            return m;
        }

        public static Mesh FlipNormals(this Mesh m)
        {
            m.GetNormals(_normalsA);

            var count = _normalsA.Count;
            for (int i = 0; i < count; i++)
            {
                _normalsA[i] = -_normalsA[i];
            }

            m.SetNormals(_normalsA);

            return m;
        }


        public static Mesh FlipOrientation(this Mesh m, bool flipNormals)
        {
            m.GetNormals(_normalsA);
            m.GetTriangles(_trianglesA, 0, false);

            var count = _normalsA.Count;
            for (int i = 0; i < count; i++)
            {
                _normalsA[i] = -_normalsA[i];
            }

            var triangleCount = _trianglesA.Count;
            for (int i = 0; i < triangleCount; i += 3)
            {
                var a = _trianglesA[i + 1];
                var b = _trianglesA[i + 2];

                _trianglesA[i + 1] = b;
                _trianglesA[i + 2] = a;
            }

            m.SetNormals(_normalsA);
            m.SetTriangles(_trianglesA, 0);

            return m;
        }

    }
}
