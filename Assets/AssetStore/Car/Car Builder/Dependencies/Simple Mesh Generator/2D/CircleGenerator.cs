using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Forward
/// 
/// UV0
/// X = 0 - 1: length
/// Y = 0 - 1: center to edge
/// 
/// UV1
/// X = 0 - 1: length
/// Y = height relative to (circumference ~ X)
/// 
/// UV2
/// World size
/// 
/// Notes:
/// When generating a "Hollow" variant. The "Radius" decides where the the ring lays, the thicknes is then applied half inwards and half outwards
/// </summary>

namespace SimpleMeshGenerator
{
    public static class CircleGenerator
    {
        private static Vector2 _uv = new Vector2(0, 0);

        private static Mesh _mesh = new Mesh();

        private static List<Vector3> _verts = new List<Vector3>();
        private static List<Vector3> _normals = new List<Vector3>();
        private static List<Vector2> _uvs0 = new List<Vector2>();
        private static List<Vector2> _uvs1 = new List<Vector2>();

        static List<Vector2> _uvsOriginal_Detailed = new List<Vector2>();
        static List<Vector2> _uvs0_Detailed = new List<Vector2>();
        static List<Vector2> _uvs1_Detailed = new List<Vector2>();

        private static List<GeneralMeshGenerator.PointData> _pointInfos = new List<GeneralMeshGenerator.PointData>();

        public static Mesh Generate(float radius, Vector3 offset, int resolutionLength = 30, int resolutionWidth = 2, GeneralMeshGenerator.Axis2D axis = GeneralMeshGenerator.Axis2D.XY, bool flipOrientation = false)
        {
            return GenerateMesh(radius, radius, false, offset, resolutionLength, Mathf.Max(resolutionWidth, 2), axis, flipOrientation);
        }

        public static Mesh GenerateHollow(float radius, float thickness, Vector3 offset, int resolutionLength = 30, int resolutionWidth = 2, GeneralMeshGenerator.Axis2D axis = GeneralMeshGenerator.Axis2D.XY, bool flipOrientation = false)
        {
            return GenerateMesh(radius, thickness, true, offset, resolutionLength, Mathf.Max(resolutionWidth, 2), axis, flipOrientation);
        }

        private static Mesh GenerateMesh(float radius, float thickness, bool hollow, Vector3 offset, int resolutionLength, int resolutionWidth, GeneralMeshGenerator.Axis2D axis, bool flipOrientation)
        {
            resolutionLength = resolutionLength + 1;

            var circumference_Outer = 2 * radius * Mathf.PI;
            var circumference_Inner = 2 * (radius - thickness * 2) * Mathf.PI;

            _verts.Clear();
            _uvs0.Clear();
            _uvs1.Clear();
            _normals.Clear();

            // offset rotation to make the "tip" side at the top
            var rotationOffset_Radians = Mathf.PI * 0.5f;

            // we are making a circle
            for (int i = 0; i < resolutionLength; i++)
            {
                float progressCircle = i / (float)(resolutionLength - 1);

                for (int j = 0; j < resolutionWidth; j++)
                {
                    float radians = progressCircle * 2 * Mathf.PI + rotationOffset_Radians;

                    float progressWidth = j / (float)(resolutionWidth - 1);
                    float dir = (progressWidth * 2) - 1;

                    var pos = Vector3.zero;

                    switch (axis)
                    {
                        case GeneralMeshGenerator.Axis2D.XY:
                            if (hollow)
                            {
                                pos.x = Mathf.Cos(radians) * radius + Mathf.Cos(radians) * thickness * dir;
                                pos.y = Mathf.Sin(radians) * radius + Mathf.Sin(radians) * thickness * dir;
                            }
                            else
                            {
                                pos.x = Mathf.Cos(radians) * radius * progressWidth;
                                pos.y = Mathf.Sin(radians) * radius * progressWidth;
                            }
                            break;
                        case GeneralMeshGenerator.Axis2D.XZ:
                            if (hollow)
                            {
                                pos.x = Mathf.Cos(radians) * radius + Mathf.Cos(radians) * thickness * dir;
                                pos.z = Mathf.Sin(radians) * radius + Mathf.Sin(radians) * thickness * dir;
                            }
                            else
                            {
                                pos.x = Mathf.Cos(radians) * radius * progressWidth;
                                pos.z = Mathf.Sin(radians) * radius * progressWidth;
                            }
                            break;
                        case GeneralMeshGenerator.Axis2D.ZY:
                            if (hollow)
                            {
                                pos.z = Mathf.Cos(radians) * radius + Mathf.Cos(radians) * thickness * dir;
                                pos.y = Mathf.Sin(radians) * radius + Mathf.Sin(radians) * thickness * dir;
                            }
                            else
                            {
                                pos.z = Mathf.Cos(radians) * radius * progressWidth;
                                pos.y = Mathf.Sin(radians) * radius * progressWidth;
                            }
                            break;
                    }


                    var uv0 = Vector2.zero;
                    uv0.x = progressCircle;
                    uv0.y = progressWidth;

                    var uv1 = Vector2.zero;
                    uv1.x = progressCircle;
                    uv1.y = (thickness / circumference_Outer) * progressWidth;

                    //add
                    _verts.Add(pos + offset);
                    _uvs0.Add(uv0);
                    _uvs1.Add(uv1);

                    switch (axis)
                    {
                        case GeneralMeshGenerator.Axis2D.XY:
                            _normals.Add(Vector3.forward);
                            break;
                        case GeneralMeshGenerator.Axis2D.XZ:
                            _normals.Add(Vector3.up);
                            break;
                        case GeneralMeshGenerator.Axis2D.ZY:
                            _normals.Add(Vector3.right);
                            break;
                    }
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(_verts);
            _mesh.SetUVs(0, _uvs0);
            _mesh.SetUVs(1, _uvs1);
            _mesh.SetTriangles(MeshManipulation.TriangleGeneration(resolutionLength, resolutionWidth, !flipOrientation), 0);
            _mesh.SetNormals(_normals);

            MeshManipulation.BoxUVChannel_XY(ref _mesh, Vector2.one * -radius, Vector2.one * radius, 2);

            return _mesh;
        }


        public static Mesh Generate_Detailed(float radius, Vector3 offset, int resolutionLength = 30, int detailResolution = 8, bool flipOrientation = false)
        {
            //generate as per usual
            var mesh = Generate(radius, offset, resolutionLength, 2, GeneralMeshGenerator.Axis2D.XY, flipOrientation);
            return GenerateMesh_Detailed(mesh, resolutionLength, detailResolution);
        }


        public static Mesh Generate_Hollow_Detailed(float radius, float thickness, Vector3 offset, int resolutionLength = 30, int detailResolution = 8, bool flipOrientation = false)
        {
            //generate as per usual
            var mesh = GenerateHollow(radius, thickness, offset, resolutionLength, 2, GeneralMeshGenerator.Axis2D.XY, flipOrientation);
            return GenerateMesh_Detailed(mesh, resolutionLength, detailResolution);
        }


        private static Mesh GenerateMesh_Detailed(Mesh mesh, int resolutionLength, int detailResolution)
        {
            var minMax = Utility.GetMinMax_XY(mesh.vertices);
            var min = minMax.Key;
            var max = minMax.Value;

            min.x = minMax.Value.x;
            max.x = minMax.Key.x;

            var detailedMesh = mesh;
            _uvsOriginal_Detailed.Clear();
            _uvs0_Detailed.Clear();
            _uvs1_Detailed.Clear();

            mesh.GetVertices(_verts);
            mesh.GetUVs(0, _uvs0);
            mesh.GetUVs(1, _uvs1);

            mesh.Clear();

            // recreate as detailed quads with better UVs
            for (int i = 0; i < resolutionLength; i++)
            {
                var startIndex = i * 2;
                var points = new Vector3[] { _verts[startIndex + 0], _verts[startIndex + 1], _verts[(startIndex + 3) % _verts.Count], _verts[(startIndex + 2) % _verts.Count] };

                var segment = QuadGenerator_3D.Generate(points, Vector2Int.one * detailResolution, Vector3.back);
                segment.GetUVs(0, _uvsOriginal_Detailed);

                // transfer uv0
                var uvXRange_0 = new Vector2(_uvs0[startIndex + 0].x, _uvs0[(startIndex + 2) % _verts.Count].x);
                var uvYRange_0 = new Vector2(_uvs0[startIndex + 0].y, _uvs0[startIndex + 1].y);
                _uvs0_Detailed.Clear();
                for (int j = 0; j < segment.vertexCount; j++)
                {
                    _uv.x = Mathf.Lerp(uvXRange_0.x, uvXRange_0.y, _uvsOriginal_Detailed[j].x);
                    _uv.y = Mathf.Lerp(uvYRange_0.x, uvYRange_0.y, _uvsOriginal_Detailed[j].y);
                    _uvs0_Detailed.Add(_uv);
                }
                segment.SetUVs(0, _uvs0_Detailed);

                // transfer uv1
                var uvXRange_1 = new Vector2(_uvs1[startIndex + 0].x, _uvs1[(startIndex + 2) % _verts.Count].x);
                var uvYRange_1 = new Vector2(_uvs1[startIndex + 0].y, _uvs1[startIndex + 1].y);
                _uvs1_Detailed.Clear();
                for (int j = 0; j < segment.vertexCount; j++)
                {
                    _uv.x = Mathf.Lerp(uvXRange_1.x, uvXRange_1.y, _uvsOriginal_Detailed[j].x);
                    _uv.y = Mathf.Lerp(uvYRange_1.x, uvYRange_1.y, _uvsOriginal_Detailed[j].y);
                    _uvs1_Detailed.Add(_uv);
                }
                segment.SetUVs(1, _uvs1_Detailed);

                CombineMeshes.Combine(detailedMesh, segment);
            }

            MeshManipulation.BoxUVChannel_XY(ref detailedMesh, min, max, 2);

            return detailedMesh;
        }


        public static List<Vector3> GetPoints(float radius, Vector3 offset, int resolutionLength, GeneralMeshGenerator.Axis2D axis = GeneralMeshGenerator.Axis2D.XY, bool fullLoop = true, bool startAtTop = true)
        {
            resolutionLength = resolutionLength + 1;
  
            _verts.Clear();

            // offset rotation to make the "tip" side at the top
            var rotationOffset_Radians = Mathf.PI * 0.5f;

            // we are making a circle
            for (int i = 0; i < resolutionLength; i++)
            {
                float progressCircle = i / (float)(resolutionLength - 1);
                float radians = progressCircle * 2 * Mathf.PI;
                if (startAtTop) radians += rotationOffset_Radians;

                var pos = Vector3.zero;

                switch (axis)
                {
                    case GeneralMeshGenerator.Axis2D.XY:
                        pos.x = Mathf.Cos(radians);
                        pos.y = Mathf.Sin(radians);
                        break;
                    case GeneralMeshGenerator.Axis2D.XZ:
                        pos.x = Mathf.Cos(radians);
                        pos.z = Mathf.Sin(radians);
                        break;
                    case GeneralMeshGenerator.Axis2D.ZY:
                        pos.z = Mathf.Cos(radians);
                        pos.y = Mathf.Sin(radians);
                        break;
                }

                pos *= radius;
                _verts.Add(pos + offset);

                if (fullLoop == false && i == resolutionLength - 2)
                    break;
            }

            return _verts;
        }


        public static List<GeneralMeshGenerator.PointData> GetPoints(float radius, Vector3 offset, Vector3 up, Vector3 side, int resolutionLength)
        {
            _pointInfos.Clear();
            var positions = GetPositions(radius, up, side, resolutionLength);

            _uv.x = 0;

            for (int i = 0; i < positions.Count; i++)
            {
                float progressCircle = i / (float)(positions.Count - 1);
                _uv.y = progressCircle;

                var info = new GeneralMeshGenerator.PointData();
                info.Position = positions[i] + offset;
                info.UV = _uv;
                info.Normal = positions[i].normalized;

                _pointInfos.Add(info);
            }

            return _pointInfos;
        }


        public static List<Vector3> GetPositions(float radius, Vector3 up, Vector3 side, int resolutionLength, float radiansOffset = 0)
        {
            resolutionLength = resolutionLength + 1;

            _verts.Clear();
            side = side.normalized;
            up = up.normalized;

            // we are making a circle
            for (int i = 0; i < resolutionLength; i++)
            {
                float progressCircle = i / (float)(resolutionLength - 1);
                float radians = progressCircle * 2 * Mathf.PI;

                var pos = Vector3.zero;
                pos += side * Mathf.Cos(radians + radiansOffset);
                pos += up * Mathf.Sin(radians + radiansOffset);
                pos = pos.normalized * radius;

                _verts.Add(pos);
            }

            return _verts;
        }
    }
}