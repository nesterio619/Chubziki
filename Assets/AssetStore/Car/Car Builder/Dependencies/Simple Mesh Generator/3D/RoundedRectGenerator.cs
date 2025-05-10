using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Outwards
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
    public static class RoundedRectGenerator
    {
        private static Mesh _mesh = new Mesh();

        private static Mesh _meshFront = new Mesh();
        private static Mesh _meshBack = new Mesh();
        private static Mesh _meshHollowInside = new Mesh();

        private static List<Vector3> _frontVerts = new List<Vector3>();
        private static List<Vector3> _outerVerts = new List<Vector3>();
        private static List<Vector3> _outerVerts2 = new List<Vector3>();
        private static List<Vector3> _innerVerts = new List<Vector3>();   

        public static Mesh GenerateFlatSided(Vector2 size, float cornerSize, float thickness)
        {
            return GenerateFlatSided_Internal(size, cornerSize, thickness, Vector2.zero);
        }

        public static Mesh GenerateFlatSided_Hollow(Vector2 size, float cornerSize, float thickness, Vector2 hollowSize)
        {
            return GenerateFlatSided_Internal(size, cornerSize, thickness, hollowSize);
        }

        private static Mesh GenerateFlatSided_Internal(Vector2 size, float cornerSize, float thickness, Vector2 hollowSize)
        {
            _mesh.Clear();
            _meshFront.Clear();
            _meshBack.Clear();
            _outerVerts.Clear();
            _outerVerts2.Clear();

            int cornerCount = 4;
            int quadVertCount = 4;
            int hollowCenterVertCount = (4 * 4);
            int startIndex = 0;

            var hollow = (Mathf.Approximately(hollowSize.x, 0) && Mathf.Approximately(0, hollowSize.y)) == false;
            if (hollow)
            {
                CombineMeshes.Combine(_meshFront, RoundedQuadGenerator.Generate_Hollow(size, cornerSize, hollowSize).AddPositionOffset(-Vector3.forward * 0.5f * thickness));
                CombineMeshes.Combine(_meshBack, RoundedQuadGenerator.Generate_Hollow(size, cornerSize, hollowSize, true).AddPositionOffset(Vector3.forward * 0.5f * thickness));

                startIndex = quadVertCount * 4 + hollowCenterVertCount;
            }
            else
            {
                CombineMeshes.Combine(_meshFront, RoundedQuadGenerator.Generate(size, cornerSize).AddPositionOffset(-Vector3.forward * 0.5f * thickness));
                CombineMeshes.Combine(_meshBack, RoundedQuadGenerator.Generate(size, cornerSize, true).AddPositionOffset(Vector3.forward * 0.5f * thickness));

                startIndex = quadVertCount * 5;
            }

            _meshFront.GetVertices(_frontVerts);

            // stitch these sides together by going over only their outerEdges
            for (int i = 0; i < cornerCount; i++)
            {
                var totalVertsPerCorner = (RoundedQuadGenerator.CornerResolution) * 3;

                for (int j = 0; j < totalVertsPerCorner; j += 3)
                {
                    var index = startIndex + i * totalVertsPerCorner + j;
                    _outerVerts.Add(_frontVerts[index]);
                }

                _outerVerts.Add(_frontVerts[startIndex + (i + 1) * totalVertsPerCorner - 2]);
            }


            CombineMeshes.Combine(_mesh, _meshFront);


            _outerVerts2.AddRange(_outerVerts);
            _outerVerts2.Offset(Vector3.forward * thickness);
            var outerSide = GeneralMeshGenerator.CreateBridgeSoftEdged(_outerVerts, _outerVerts2, true, false, false);

            MeshManipulation.BoxUVChannel_XY(ref outerSide, size * -0.5f, size * 0.5f, 0);
            MeshManipulation.SetWorldUVs_XY(ref outerSide);

            CombineMeshes.Combine(_mesh, outerSide);

            if (hollow)
            {
                _innerVerts.Clear();
                for (int i = 0; i < hollowCenterVertCount; i += 4)
                {
                    _innerVerts.Add(_frontVerts[i]);
                }

                _meshHollowInside.Clear();
                var normals = new Vector3[] { Vector3.right, Vector3.down, Vector3.left, Vector3.up };
                var points = new Vector3[4];

                for (int i = 0; i < _innerVerts.Count; i++)
                {
                    points[0] = _innerVerts[i];
                    points[1] = _innerVerts[(i + 1) % _innerVerts.Count];
                    points[2] = _innerVerts[(i + 1) % _innerVerts.Count] + Vector3.forward * thickness;
                    points[3] = _innerVerts[i] + Vector3.forward * thickness;

                    CombineMeshes.Combine(_meshHollowInside, QuadGenerator_3D.Generate(points, Vector2Int.one, normals[i]));
                }

                MeshManipulation.BoxUVChannel_XY(ref _meshHollowInside, Vector2.one * -0.5f, Vector2.one * 0.5f, 0);
                MeshManipulation.SetWorldUVs_XY(ref _meshHollowInside);

                CombineMeshes.Combine(_mesh, _meshHollowInside);
            }

            CombineMeshes.Combine(_mesh, _meshBack);

            return _mesh;
        }
    }
}
