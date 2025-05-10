using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Outwards
/// 
/// UV0
/// X = 0 - 1: length
/// Y = 0 - 1: center to edge
/// 
/// UV1
/// X = 0 - 1: length
/// Y = height relative to circumference / X
/// 
/// UV2
/// X = circumference
/// Y = height relative to circumference
/// 
/// </summary>

namespace SimpleMeshGenerator
{
    public static class CylinderGenerator
    {
        private static Vector2 _uv = new Vector2(0, 0);
        private static Vector3 _normal = new Vector3(0, 0, 0);

        private static Mesh _mesh = new Mesh();
        private static Mesh _meshOutside = new Mesh();
        private static Mesh _meshInside = new Mesh();

        private static Mesh _activeMeshRef = _mesh;

        private static List<Vector3> _vertsOutside = new List<Vector3>();
        private static List<Vector3> _vertsInside = new List<Vector3>();
        private static List<Vector2> _uvs0 = new List<Vector2>();
        private static List<Vector2> _uvs1 = new List<Vector2>();
        private static List<Vector2> _uvs2 = new List<Vector2>();
        private static List<Vector3> _normals = new List<Vector3>();

        public static Mesh Generate(float radius, float height, int resolutionLoops = 20, int resolutionHeight = 5, bool withCaps = true, GeneralMeshGenerator.Axis axis = GeneralMeshGenerator.Axis.Y, bool flip = false)
        {
            _vertsOutside.Clear();
            _uvs0.Clear();
            _uvs1.Clear();
            _uvs2.Clear();
            _normals.Clear();

            var circumference = 2 * radius * Mathf.PI;
            var heightToCircumferenceRatio = height / circumference;

            for (int i = 0; i < resolutionHeight; i++)
            {
                var heightPercentage = (float)i / (resolutionHeight - 1);
                var offset = Vector3.zero;

                List<Vector3> circleVerts = null;

                switch (axis)
                {
                    case GeneralMeshGenerator.Axis.X:
                        offset = -Vector3.right * height * 0.5f;
                        offset += Vector3.right * height * heightPercentage;
                        circleVerts = CircleGenerator.GetPoints(radius, offset, resolutionLoops, GeneralMeshGenerator.Axis2D.ZY);
                        break;
                    case GeneralMeshGenerator.Axis.Y:
                        offset = -Vector3.up * height * 0.5f;
                        offset += Vector3.up * height * heightPercentage;
                        circleVerts = CircleGenerator.GetPoints(radius, offset, resolutionLoops, GeneralMeshGenerator.Axis2D.XZ);
                        break;
                    case GeneralMeshGenerator.Axis.Z:
                        offset = Vector3.forward * height * 0.5f;
                        offset -= Vector3.forward * height * heightPercentage;
                        circleVerts = CircleGenerator.GetPoints(radius, offset, resolutionLoops, GeneralMeshGenerator.Axis2D.XY);
                        break;
                }

                _vertsOutside.AddRange(circleVerts);

                for (int v = 0; v < circleVerts.Count; v++)
                {
                    var vert = circleVerts[v];

                    var progressCircumference = (float)v / (circleVerts.Count - 1);

                    _uv.x = progressCircumference;
                    _uv.y = heightPercentage;
                    _uvs0.Add(_uv);

                    _uv.y = heightPercentage * heightToCircumferenceRatio;
                    _uvs1.Add(_uv);

                    _uv.x = progressCircumference * circumference;
                    _uv.y = heightPercentage * heightToCircumferenceRatio * circumference;
                    _uvs2.Add(_uv);

                    switch (axis)
                    {
                        case GeneralMeshGenerator.Axis.X:
                            _normal.x = 0;
                            _normal.y = vert.y;
                            _normal.z = vert.z;
                            _normal = (_normal * (flip ? -1 : 1));

                            _normals.Add(_normal.normalized);
                            break;
                        case GeneralMeshGenerator.Axis.Y:
                            _normal.x = vert.x;
                            _normal.y = 0;
                            _normal.z = vert.z;
                            _normal = (_normal * (flip ? -1 : 1));

                            _normals.Add(_normal.normalized);
                            break;
                        case GeneralMeshGenerator.Axis.Z:
                            _normal.x = vert.x;
                            _normal.y = vert.y;
                            _normal.z = 0;
                            _normal = (_normal * (flip ? -1 : 1));

                            _normals.Add(_normal.normalized);
                            break;
                    }
                }
            }

            _activeMeshRef.Clear();
            _activeMeshRef.SetVertices(_vertsOutside);
            _activeMeshRef.SetUVs(0, _uvs0);
            _activeMeshRef.SetUVs(1, _uvs1);
            _activeMeshRef.SetUVs(2, _uvs2);
            _activeMeshRef.SetNormals(_normals);

            var loop = resolutionLoops + 1; // actual amount of points in a loop
            _activeMeshRef.SetTriangles(MeshManipulation.TriangleGeneration(resolutionHeight, loop, flip), 0);

            if (withCaps)
            {
                Mesh meshTop = null;
                Mesh meshBottom = null;
                switch (axis)
                {
                    case GeneralMeshGenerator.Axis.X:
                        meshTop = CircleGenerator.Generate(radius, Vector3.zero, resolutionLoops, 2, GeneralMeshGenerator.Axis2D.ZY);
                        MeshManipulation.Rotate(ref meshTop, Vector3.right * height * 0.5f, new Vector3(0, 0, flip ? 0 : 180));
                        CombineMeshes.Combine(_activeMeshRef, meshTop);

                        meshBottom = CircleGenerator.Generate(radius, Vector3.zero, resolutionLoops, 2, GeneralMeshGenerator.Axis2D.ZY);
                        MeshManipulation.Rotate(ref meshBottom, -Vector3.right * height * 0.5f, new Vector3(0, 0, flip ? 180 : 0));
                        CombineMeshes.Combine(_activeMeshRef, meshBottom);
                        break;
                    case GeneralMeshGenerator.Axis.Y:
                        meshTop = CircleGenerator.Generate(radius, Vector3.zero, resolutionLoops, 2, GeneralMeshGenerator.Axis2D.XZ);
                        MeshManipulation.Rotate(ref meshTop, Vector3.up * height * 0.5f, new Vector3(flip ? 0 : 180, 0, 0));
                        CombineMeshes.Combine(_activeMeshRef, meshTop);

                        meshBottom = CircleGenerator.Generate(radius, Vector3.zero, resolutionLoops, 2, GeneralMeshGenerator.Axis2D.XZ);
                        MeshManipulation.Rotate(ref meshBottom, -Vector3.up * height * 0.5f, new Vector3(flip ? 180 : 0, 0, 0));
                        CombineMeshes.Combine(_activeMeshRef, meshBottom);
                        break;
                    case GeneralMeshGenerator.Axis.Z:
                        meshTop = CircleGenerator.Generate(radius, Vector3.zero, resolutionLoops, 2, GeneralMeshGenerator.Axis2D.XY);
                        MeshManipulation.Rotate(ref meshTop, Vector3.forward * height * 0.5f, new Vector3(0, flip ? 180 : 0, 0));
                        CombineMeshes.Combine(_activeMeshRef, meshTop);

                        meshBottom = CircleGenerator.Generate(radius, Vector3.zero, resolutionLoops, 2, GeneralMeshGenerator.Axis2D.XY);
                        MeshManipulation.Rotate(ref meshBottom, -Vector3.forward * height * 0.5f, new Vector3(0, flip ? 0 : 180, 0));
                        CombineMeshes.Combine(_activeMeshRef, meshBottom);
                        break;
                }
            }

            return _activeMeshRef;
        }

        public static Mesh Generate_Hollow(float radius, float height, float thickness, int resolutionLoops = 20, int resolutionHeight = 5, GeneralMeshGenerator.Axis axis = GeneralMeshGenerator.Axis.Y)
        {
            if (_meshOutside == null) _meshOutside = new Mesh();
            _activeMeshRef = _meshOutside;
            Generate(radius, height, resolutionLoops, resolutionHeight, false, axis, false);

            if (_meshInside == null) _meshInside = new Mesh();
            _activeMeshRef = _meshInside;
            Generate(radius - thickness, height, resolutionLoops, resolutionHeight, false, axis, true);

            if(_mesh == null) _mesh = new Mesh();
            _activeMeshRef = _mesh; // restore activeMesh reference
            _mesh.Clear();

            _meshOutside.GetVertices(_vertsOutside);
            _meshInside.GetVertices(_vertsInside);

            CombineMeshes.Combine(_mesh, _meshOutside);
            CombineMeshes.Combine(_mesh, _meshInside);

            var topNormal = Vector3.up;
            switch (axis)
            {
                case GeneralMeshGenerator.Axis.X:
                    topNormal = Vector3.right;
                    break;
                case GeneralMeshGenerator.Axis.Y:
                    topNormal = Vector3.up;
                    break;
                case GeneralMeshGenerator.Axis.Z:
                    topNormal = Vector3.back;
                    break;
            }

            // QuadGenerator Method
            var startIndex = _vertsOutside.Count - resolutionLoops;
 
            //top
            var points = new Vector3[4];

            var top = _meshOutside;
            top.Clear();
            for (int i = 0; i < resolutionLoops; i++)
            {
                var nextIndex = startIndex + i + 1;
                if (nextIndex >= _vertsOutside.Count)
                {
                    nextIndex = startIndex;
                }

                points[0] = _vertsOutside[startIndex + i];
                points[1] = _vertsOutside[nextIndex];
                points[2] = _vertsInside[nextIndex];
                points[3] = _vertsInside[startIndex + i];

                CombineMeshes.Combine(top, QuadGenerator_3D.Generate(points, Vector2Int.one, Vector3.back, true));
            }

            top.OverrideNormals(topNormal);
            top.OverrideUVs(Vector2.up, 0);

            _meshOutside.GetUVs(1, _uvs1);
            top.OverrideUVs(_uvs1.Last(), 1);

            _meshOutside.GetUVs(2, _uvs1);
            top.OverrideUVs(_uvs1.Last(), 2);

            CombineMeshes.Combine(_mesh, top);

            //bottom
            var bottom = _meshOutside;
            bottom.Clear();
            for (int i = 0; i < resolutionLoops; i++)
            {
                points[0] = _vertsOutside[i];
                points[1] = _vertsOutside[(i + 1) % resolutionLoops];
                points[2] = _vertsInside[(i + 1) % resolutionLoops];
                points[3] = _vertsInside[i];

                CombineMeshes.Combine(bottom, QuadGenerator_3D.Generate(points, Vector2Int.one, Vector3.back, false));// );
            }

            bottom.OverrideNormals(-topNormal);
            bottom.OverrideUVs(Vector2.zero, 0);
            bottom.OverrideUVs(Vector2.zero, 1);
            bottom.OverrideUVs(Vector2.zero, 2);
            CombineMeshes.Combine(_mesh, bottom);

            return _mesh;
        }
    }
}
