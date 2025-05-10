using Core.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Backwards
/// 
/// UV0
/// X = 0 - 1: width
/// Y = 0 - 1: height
/// 
/// UV1
/// WorldSize
/// 
/// </summary>

namespace SimpleMeshGenerator
{
    public static class QuadGenerator_3D
    {
        private static Vector2 _uv = new Vector2(0, 0);

        private static Mesh _mesh = new Mesh();
        private static Mesh _meshCombined = new Mesh();

        private static List<Vector3> _verts = new List<Vector3>();
        private static List<Vector2> _uvs = new List<Vector2>();
        private static List<Vector3> _normals = new List<Vector3>();

        public static Mesh Generate(Vector3[] points, Vector2Int resolution, Vector3 normal, bool flipOrientation = false)
        {
            return Generate(points, resolution, Vector3.zero, normal, flipOrientation);
        }

        public static Mesh Generate(Vector3[] points, Vector2Int resolution, Vector3 posOffset, Vector3 normal, bool flipOrientation = false)
        {
            resolution.x += 1;
            resolution.y += 1;

            var capacity = resolution.x * resolution.y;

            _verts.Clear();
            _verts.Capacity = capacity;
            _uvs.Clear();
            _uvs.Capacity = capacity;
            _normals.Clear();
            _normals.Capacity = capacity;

            for (int x = 0; x < resolution.x; x++)
            {
                float progressX = x / (float)(resolution.x - 1);

                for (int y = 0; y < resolution.y; y++)
                {
                    float progressY = y / (float)(resolution.y - 1);

                    var posA = Vector3.Lerp(points[0], points[1], progressY);
                    var posB = Vector3.Lerp(points[3], points[2], progressY);
                    var pos = Vector3.Lerp(posA, posB, progressX);

                    pos += posOffset;

                    _uv.x = progressX;
                    _uv.y = progressY;

                    if (flipOrientation)
                        _uv.x = 1 - _uv.x;

                    _verts.Add(pos);
                    _uvs.Add(_uv);
                    _normals.Add(normal);
                }
            }

            if(_mesh==null) _mesh = new Mesh();

            if (_verts.Exists(vector => vector.IsNaN()))
                return _mesh;

            _mesh.Clear();
            _mesh.SetVertices(_verts);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetUVs(1, _uvs);
            _mesh.SetTriangles(MeshManipulation.TriangleGeneration(resolution.x, resolution.y, !flipOrientation), 0);
            _mesh.SetNormals(_normals);

            return _mesh;
        }

        public static Mesh Generate(List<Vector3> pointsTop, List<Vector3> pointsSide, Vector3 normal, GeneralMeshGenerator.NormalsCalculationType normalCalculationType = GeneralMeshGenerator.NormalsCalculationType.Off, bool flipNormal = false, bool flipOrientation = false)
        {
            return Generate(pointsTop.ToArray(), pointsSide.ToArray(), normal, normalCalculationType, flipNormal, flipOrientation);
        }

        public static Mesh Generate(Vector3[] pointsTop, Vector3[] pointsSide, Vector3 normal, GeneralMeshGenerator.NormalsCalculationType normalCalculationType = GeneralMeshGenerator.NormalsCalculationType.Off, bool flipNormal = false, bool flipOrientation = false)
        {
            if(pointsTop.Length <= 1 || pointsSide.Length <= 1)
            {
                Debug.LogError("Invalid input, point count to low");
                return null;
            }

            var setsStartOverlap = Utility.PointsAreIdentical(pointsTop[0], pointsSide[0]);
            if(setsStartOverlap == false)
            {
                Debug.LogError("Invalid input, starting points do not match");
                return null;
            }

            var deltaTopPointMax = pointsTop.Last() - pointsTop[0];
            var deltaSidePointMax = pointsSide.Last() - pointsSide[0];
            var distanceTopPointMax = deltaTopPointMax.magnitude;
            var distanceSidePointMax = deltaSidePointMax.magnitude;

            var capacity = 0;
            if (normalCalculationType == GeneralMeshGenerator.NormalsCalculationType.PerFace)
            {
                capacity = Utility.VertCountOfHardEdgedObject(pointsTop.Length, pointsSide.Length);
            }
            else
            {
                capacity = pointsTop.Length * pointsSide.Length;
            }


            normal = normalCalculationType == GeneralMeshGenerator.NormalsCalculationType.PerObject ? Utility.CalculateNormal(deltaTopPointMax, deltaSidePointMax) : normal;
            normal = flipNormal ? -normal : normal;
            normal = normal.normalized;

            _verts.Clear();
            _verts.Capacity = capacity;
            _uvs.Clear();
            _uvs.Capacity = capacity;
            _normals.Clear();
            _normals.Capacity = capacity;

            if (normalCalculationType == GeneralMeshGenerator.NormalsCalculationType.PerFace)
            {
                for (int y = 0; y < pointsTop.Length - 1; y++)
                {
                    var deltaTopPoint = pointsTop[y] - pointsTop[0];
                    var deltaTopPointNext = pointsTop[y + 1] - pointsTop[0];

                    for (int x = 0; x < pointsSide.Length - 1; x++)
                    {
                        var deltaSidePoint = pointsSide[x] - pointsSide[0];
                        var deltaSidePointNext = pointsSide[x + 1] - pointsSide[0];

                        var topLeftCorner = pointsTop[0] + deltaTopPoint + deltaSidePoint;

                        _verts.Add(topLeftCorner);
                        _verts.Add(topLeftCorner + deltaTopPointNext);
                        _verts.Add(topLeftCorner + deltaTopPointNext + deltaSidePointNext);
                        _verts.Add(topLeftCorner + deltaSidePointNext);


                        _uvs.Add(new Vector2(deltaTopPoint.magnitude / distanceTopPointMax, deltaSidePoint.magnitude / distanceSidePointMax));
                        _uvs.Add(new Vector2(deltaTopPoint.magnitude / distanceTopPointMax, deltaSidePoint.magnitude / distanceSidePointMax));
                        _uvs.Add(new Vector2(deltaTopPoint.magnitude / distanceTopPointMax, deltaSidePoint.magnitude / distanceSidePointMax));
                        _uvs.Add(new Vector2(deltaTopPoint.magnitude / distanceTopPointMax, deltaSidePoint.magnitude / distanceSidePointMax));


                        normal = Utility.CalculateNormal(pointsTop[y + 1] - pointsTop[y], pointsSide[x + 1] - pointsSide[x]);
                        for (int i = 0; i < 4; i++)
                        {
                            _normals.Add(normal);
                        }
                    }
                }
            }
            else
            {
                for (int y = 0; y < pointsTop.Length; y++)
                {
                    var deltaTopPoint = pointsTop[y] - pointsTop[0];

                    for (int x = 0; x < pointsSide.Length; x++)
                    {
                        var deltaSidePoint = pointsSide[x] - pointsSide[0];

                        _verts.Add(pointsTop[0] + deltaTopPoint + deltaSidePoint);
                        _uvs.Add(new Vector2(deltaTopPoint.magnitude / distanceTopPointMax, deltaSidePoint.magnitude / distanceSidePointMax));
                        _normals.Add(normal);
                    }
                }
            }


            if (_verts.Exists(vector => vector.IsNaN()))
                return _mesh;

            _mesh.Clear();
            _mesh.SetVertices(_verts);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetNormals(_normals);
            _mesh.SetTriangles(MeshManipulation.TriangleGeneration(pointsTop.Length, pointsSide.Length, flipOrientation), 0);

            return _mesh;
        }

        public static Mesh GenerateHollow(Vector3[] pointsOuter_4, Vector3[] pointsInner_4, Vector2Int resolution, Vector3 normal, bool flipOrientation = false)
        {
            _meshCombined.Clear();

            for (int i = 0; i < 4; i++)
            {
                var points = new Vector3[4];
                points[0] = pointsInner_4[i];
                points[1] = pointsOuter_4[i];
                points[2] = pointsOuter_4[(i + 1) % 4];
                points[3] = pointsInner_4[(i + 1) % 4];

                CombineMeshes.Combine(_meshCombined, Generate(points, resolution, Vector3.zero, normal, flipOrientation));
            }

            return _meshCombined;
        }
    }
}
