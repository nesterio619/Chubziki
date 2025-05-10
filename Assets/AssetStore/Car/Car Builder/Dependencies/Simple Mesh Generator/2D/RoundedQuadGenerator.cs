using System.Collections.Generic;
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
/// World size
/// 
/// </summary>

namespace SimpleMeshGenerator
{
    public static class RoundedQuadGenerator
    {
        public static int CornerResolution = 12;

        private static Mesh _mesh = new Mesh();

        public static Mesh Generate(Vector2 size, float cornerSize, bool flipOrientation = false)
        {
            return GenerateMesh(size, cornerSize, Vector2.zero, flipOrientation);
        }

        public static Mesh Generate_Hollow(Vector2 size, float cornerSize, Vector2 hollowSize, bool flipOrientation = false)
        {
            return GenerateMesh(size, cornerSize, hollowSize, flipOrientation);
        }

        private static Mesh GenerateMesh(Vector2 size, float cornerSize, Vector2 hollowSize, bool flipOrientation = false)
        {
            _mesh.Clear();

            var hollow = (Mathf.Approximately(hollowSize.x, 0) && Mathf.Approximately(0, hollowSize.y)) == false;
            if (hollow)
            {
                var center = QuadGenerator_2D.Generate_Hollow(new Vector2(size.x - cornerSize * 2, size.y - cornerSize * 2), Vector2Int.one, hollowSize, flipOrientation);
                CombineMeshes.Combine(_mesh, center);
            }
            else
            {
                var center = QuadGenerator_2D.Generate(new Vector2(size.x - cornerSize * 2, size.y - cornerSize * 2), Vector2Int.one, Vector3.zero, flipOrientation);
                CombineMeshes.Combine(_mesh, center);
            }

            //top
            CombineMeshes.Combine(_mesh, QuadGenerator_2D.Generate(new Vector2(size.x - cornerSize * 2, cornerSize), Vector2Int.one, Vector3.up * (size.y * 0.5f - cornerSize * 0.5f), flipOrientation));

            //bottom
            CombineMeshes.Combine(_mesh, QuadGenerator_2D.Generate(new Vector2(size.x - cornerSize * 2, cornerSize), Vector2Int.one, Vector3.up * (-size.y * 0.5f + cornerSize * 0.5f), flipOrientation));

            //right
            CombineMeshes.Combine(_mesh, QuadGenerator_2D.Generate(new Vector2(cornerSize, size.y - cornerSize * 2), Vector2Int.one, Vector3.right * (-size.x * 0.5f + cornerSize * 0.5f), flipOrientation));

            //left
            CombineMeshes.Combine(_mesh, QuadGenerator_2D.Generate(new Vector2(cornerSize, size.y - cornerSize * 2), Vector2Int.one, Vector3.right * (size.x * 0.5f - cornerSize * 0.5f), flipOrientation));


            // needs to be a multiple of 4;
            var circleResolution = (CornerResolution * 4);// + 1;

            var circlePoints = CircleGenerator.GetPositions(cornerSize, Vector3.up, Vector3.right, circleResolution);
            circlePoints.Reverse();

            var bottomLeftCorner = circlePoints.GetRange(0, CornerResolution + 1);
            var bottomRightCorner = circlePoints.GetRange(CornerResolution, CornerResolution + 1);
            var topRightCorner = circlePoints.GetRange(CornerResolution * 2, CornerResolution + 1);
            var topLeftLeftCorner = circlePoints.GetRange(CornerResolution * 3, CornerResolution + 1);


            //corner
            var bottomLeftCenterPoint = new Vector3(size.x * 0.5f - cornerSize, -size.y * 0.5f + cornerSize, 0);
            for (int i = 0; i < bottomLeftCorner.Count; i++)
                bottomLeftCorner[i] += bottomLeftCenterPoint;

            var cornerMesh = GeneralMeshGenerator.CreateFan(bottomLeftCorner, bottomLeftCenterPoint, Vector2.zero, Vector3.forward, false, flipOrientation);
            CombineMeshes.Combine(_mesh, cornerMesh);

            //corner
            var bottomRightCenterPoint = new Vector3(-size.x * 0.5f + cornerSize, -size.y * 0.5f + cornerSize, 0);
            for (int i = 0; i < bottomRightCorner.Count; i++)
                bottomRightCorner[i] += bottomRightCenterPoint;

            cornerMesh = GeneralMeshGenerator.CreateFan(bottomRightCorner, bottomRightCenterPoint, Vector2.zero, Vector3.forward, false, flipOrientation);
            CombineMeshes.Combine(_mesh, cornerMesh);


            //corner
            var topRightCenterPoint = new Vector3(-size.x * 0.5f + cornerSize, size.y * 0.5f - cornerSize, 0);
            for (int i = 0; i < topRightCorner.Count; i++)
                topRightCorner[i] += topRightCenterPoint;

            cornerMesh = GeneralMeshGenerator.CreateFan(topRightCorner, topRightCenterPoint, Vector2.zero, Vector3.forward, false, flipOrientation);
            CombineMeshes.Combine(_mesh, cornerMesh);

            //corner
            var topLeftCenterPoint = new Vector3(+size.x * 0.5f - cornerSize, size.y * 0.5f - cornerSize, 0);
            for (int i = 0; i < topLeftLeftCorner.Count; i++)
                topLeftLeftCorner[i] += topLeftCenterPoint;

            cornerMesh = GeneralMeshGenerator.CreateFan(topLeftLeftCorner, topLeftCenterPoint, Vector2.zero, Vector3.forward, false, flipOrientation);
            CombineMeshes.Combine(_mesh, cornerMesh);

            _mesh.OverrideNormals(flipOrientation ? Vector3.forward : -Vector3.forward);


            Vector2 min = new Vector2(-size.x * 0.5f, -size.y * 0.5f);
            Vector2 max = new Vector2(size.x * 0.5f, size.y * 0.5f);
            MeshManipulation.BoxUVChannel_XY(ref _mesh, min, max, 0);
            MeshManipulation.SetWorldUVs_XY(ref _mesh);

            return _mesh;
        }


        public static KeyValuePair<Vector2, Vector2> GetMinMax(Vector2[] points)
        {
            Vector2 min = points[0];
            Vector2 max = points[0];
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].x < min.x) min.x = points[i].x;
                if (points[i].x > max.x) max.x = points[i].x;

                if (points[i].y < min.y) min.y = points[i].y;
                if (points[i].y > max.y) max.y = points[i].y;
            }

            return new KeyValuePair<Vector2, Vector2>(min, max);
        }
    }
}
