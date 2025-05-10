using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Backwards
/// 
/// UV0
/// X = 0 - 1: width
/// Y = 0 - 1: height)
/// 
/// UV1
/// World size
/// 
/// </summary>

namespace SimpleMeshGenerator
{
    public static class QuadGenerator_2D
    {
        private static Mesh _mesh = new Mesh();

        //Basic Quad
        public static Mesh Generate(Vector2 size, Vector2Int resolution)
        {
            return Generate(size, resolution, Vector3.zero, false);
        }
        public static Mesh Generate(Vector2 size, Vector2Int resolution, Vector3 offset, bool flipOrientation = false)
        {
            return GenerateMesh_2DRestricted(size, resolution, offset, flipOrientation);
        }

        //Hollowed out
        public static Mesh Generate_Hollow(Vector2 size, Vector2Int resolution, float widthInwards, bool flipOrientation = false)
        {
            var pointsOuter_4 = new Vector2[4];
            pointsOuter_4[0] = new Vector2(-size.x * 0.5f, -size.y * 0.5f);
            pointsOuter_4[1] = new Vector2(-size.x * 0.5f, size.y * 0.5f);
            pointsOuter_4[2] = new Vector2(size.x * 0.5f, size.y * 0.5f);
            pointsOuter_4[3] = new Vector2(size.x * 0.5f, -size.y * 0.5f);

            var pointsInner_4 = new Vector2[4];
            pointsInner_4[0] = pointsOuter_4[0] + new Vector2(1, 1).normalized * widthInwards;
            pointsInner_4[1] = pointsOuter_4[1] + new Vector2(1, -1).normalized * widthInwards;
            pointsInner_4[2] = pointsOuter_4[2] + new Vector2(-1, -1).normalized * widthInwards;
            pointsInner_4[3] = pointsOuter_4[3] + new Vector2(-1, 1).normalized * widthInwards;


            var mesh = GenerateMesh_Hollow(pointsOuter_4.ConvertToVec3(), pointsInner_4.ConvertToVec3(), resolution, flipOrientation);

            MeshManipulation.BoxUVChannel_XY(ref mesh, pointsOuter_4[0], pointsOuter_4[2], 0);
            MeshManipulation.SetWorldUVs_XY(ref mesh);

            return mesh;
        }

        public static Mesh Generate_Hollow(Vector2 size, Vector2Int resolution, Vector2 hollowSize, bool flipOrientation = false)
        {
            var pointsOuter_4 = new Vector2[4];
            pointsOuter_4[0] = new Vector2(-size.x * 0.5f, -size.y * 0.5f);
            pointsOuter_4[1] = new Vector2(-size.x * 0.5f, size.y * 0.5f);
            pointsOuter_4[2] = new Vector2(size.x * 0.5f, size.y * 0.5f);
            pointsOuter_4[3] = new Vector2(size.x * 0.5f, -size.y * 0.5f);

            var pointsInner_4 = new Vector2[4];
            pointsInner_4[0] = new Vector2(-hollowSize.x * 0.5f, -hollowSize.y * 0.5f);
            pointsInner_4[1] = new Vector2(-hollowSize.x * 0.5f, hollowSize.y * 0.5f);
            pointsInner_4[2] = new Vector2(hollowSize.x * 0.5f, hollowSize.y * 0.5f);
            pointsInner_4[3] = new Vector2(hollowSize.x * 0.5f, -hollowSize.y * 0.5f);


            var mesh = GenerateMesh_Hollow(pointsOuter_4.ConvertToVec3(), pointsInner_4.ConvertToVec3(), resolution, flipOrientation);

            MeshManipulation.BoxUVChannel_XY(ref mesh, pointsOuter_4[0], pointsOuter_4[2], 0);
            MeshManipulation.SetWorldUVs_XY(ref mesh);

            return mesh;
        }


        private static Mesh GenerateMesh_2DRestricted(Vector2 size, Vector2Int resolution, Vector3 posOffset, bool flipOrientation)
        {
            var points = new Vector3[4];
            points[0] = new Vector3(-size.x * 0.5f, -size.y * 0.5f, 0);
            points[1] = new Vector3(-size.x * 0.5f, size.y * 0.5f, 0);
            points[2] = new Vector3(size.x * 0.5f, size.y * 0.5f, 0);
            points[3] = new Vector3(size.x * 0.5f, -size.y * 0.5f, 0);

            var mesh = QuadGenerator_3D.Generate(points, resolution, posOffset, Vector3.back, flipOrientation);
            MeshManipulation.SetWorldUVs_XY(ref mesh);

            return mesh;
        }


        public static Mesh GenerateMesh_Hollow(Vector3[] pointsOuter_4, Vector3[] pointsInner_4, Vector2Int resolution, bool flipOrientation)
        {
            _mesh.Clear();

            for (int i = 0; i < 4; i++)
            {
                var points = new Vector3[4];
                points[0] = pointsInner_4[i];
                points[1] = pointsOuter_4[i];
                points[2] = pointsOuter_4[(i + 1) % 4];
                points[3] = pointsInner_4[(i + 1) % 4];

                _mesh = CombineMeshes.Combine(_mesh, QuadGenerator_3D.Generate(points, resolution, Vector3.zero, Vector3.back, flipOrientation));
            }

            return _mesh;
        }
    }
}
