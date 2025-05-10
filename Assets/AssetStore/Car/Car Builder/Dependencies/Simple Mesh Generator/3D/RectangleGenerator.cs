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
/// World Size per Face
/// 
/// </summary>

namespace SimpleMeshGenerator
{
    public static class RectangleGenerator
    {
        private static Mesh _mesh = new Mesh();
        private static Mesh _meshTemp = new Mesh();

        public static Mesh Generate(Vector3 extends, int resolution = 1, bool softNormals = false)
        {
            return GenerateMesh(extends, resolution, softNormals);
        }

        private static Mesh GenerateMesh(Vector3 extends, int resolution, bool softNormals)
        {
            if (_mesh == null) _mesh = new Mesh();
            if (_meshTemp == null) _meshTemp = new Mesh();
            _mesh.Clear();
            _meshTemp.Clear();

            //add bottom
            CombineMeshes.Combine(_mesh, QuadGenerator_2D.Generate(new Vector2(extends.x, extends.z), Vector2Int.one * resolution));
            MeshManipulation.Rotate(ref _mesh, new Vector3(0, extends.y * -0.5f, 0), new Vector3(-90, 0, 0));

            //add sides
            int corners = 4;
            for (int i = 0; i < corners; i++)
            {
                Vector2 size = new Vector2(i % 2 == 0 ? extends.x : extends.z, extends.y);
                _meshTemp = QuadGenerator_2D.Generate(size, Vector2Int.one * resolution);

                var offset = new Vector3(0, 0, 0);
                if (i == 0)
                    offset.z -= extends.z * 0.5f;
                else if (i == 1)
                    offset.x -= extends.x * 0.5f;
                else if (i == 2)
                    offset.z += extends.z * 0.5f;
                else if (i == 3)
                    offset.x += extends.x * 0.5f;


                MeshManipulation.Rotate(ref _meshTemp, offset, new Vector3(0, 90 * i, 0));
                CombineMeshes.Combine(_mesh, _meshTemp);
            }

            //add top
            //add bottom
            _meshTemp = QuadGenerator_2D.Generate(new Vector2(extends.x, extends.z), Vector2Int.one * resolution);
            MeshManipulation.Rotate(ref _meshTemp, new Vector3(0, extends.y * 0.5f, 0), new Vector3(90, 0, 0));
            CombineMeshes.Combine(_mesh, _meshTemp);

            if (softNormals)
                MeshManipulation.ConvertToSoftNormals(ref _mesh);

            return _mesh;
        }


        public static Mesh GenerateHollow(Vector3 extends, float borderWidth, int resolution = 1, bool softNormals = false)
        {
            return GenerateMeshHollow(extends, borderWidth, resolution, softNormals);
        }

        private static Mesh GenerateMeshHollow(Vector3 extends, float borderWidth, int resolution, bool softNormals)
        {
            if (_mesh == null) _mesh = new Mesh();
            if (_meshTemp == null) _meshTemp = new Mesh();
            _mesh.Clear();
            _meshTemp.Clear();

            //add bottom
            CombineMeshes.Combine(_mesh, QuadGenerator_2D.Generate_Hollow(new Vector2(extends.x, extends.z), Vector2Int.one * resolution, borderWidth));
            MeshManipulation.Rotate(ref _mesh, new Vector3(0, extends.y * -0.5f, 0), new Vector3(-90, 0, 0));

            //add sides
            int corners = 4;
            for (int i = 0; i < corners; i++)
            {
                Vector2 size = new Vector2(i % 2 == 0 ? extends.x : extends.z, extends.y);
                _meshTemp = QuadGenerator_2D.Generate_Hollow(size, Vector2Int.one * resolution, borderWidth);

                var offset = new Vector3(0, 0, 0);
                if (i == 0)
                    offset.z -= extends.z * 0.5f;
                else if (i == 1)
                    offset.x -= extends.x * 0.5f;
                else if (i == 2)
                    offset.z += extends.z * 0.5f;
                else if (i == 3)
                    offset.x += extends.x * 0.5f;


                MeshManipulation.Rotate(ref _meshTemp, offset, new Vector3(0, 90 * i, 0));
                CombineMeshes.Combine(_mesh, _meshTemp);
            }

            //add top
            //add bottom
            _meshTemp = QuadGenerator_2D.Generate_Hollow(new Vector2(extends.x, extends.z), Vector2Int.one * resolution, borderWidth);
            MeshManipulation.Rotate(ref _meshTemp, new Vector3(0, extends.y * 0.5f, 0), new Vector3(90, 0, 0));
            CombineMeshes.Combine(_mesh, _meshTemp);

            if (softNormals)
                MeshManipulation.ConvertToSoftNormals(ref _mesh);

            return _mesh;
        }
    }
}
