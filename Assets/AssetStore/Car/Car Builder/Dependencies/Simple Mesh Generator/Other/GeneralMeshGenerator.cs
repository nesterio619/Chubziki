using Core.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LinearAlgebra.Line_Line_Intersection;

/// <summary>
/// Created by : Glenn korver
/// </summary>

namespace SimpleMeshGenerator
{
    public static class GeneralMeshGenerator
    {
        public class PointData
        {
            public Vector3 Position;
            public Vector2 UV;
            public Vector3 Normal;
        }

        public enum Axis
        {
            X,
            Y,
            Z
        }

        public enum Axis2D
        {
            XY,
            XZ,
            ZY
        }

        public enum NormalsCalculationType
        {
            Off,
            PerObject,
            PerFace
        }


        // creation
        // triangle / uv helpers
        // extension methods
        // manipulation (override / rotate)

        private static List<Vector3> _vertsSide = new List<Vector3>();
        private static List<Vector3> _normalsSegment = new List<Vector3>();
        private static List<Vector2> _uvSide = new List<Vector2>();

        private static Mesh _mesh = new Mesh();
        private static Mesh _meshCombined = new Mesh();

        private static List<Vector3> _verts = new List<Vector3>();
        private static List<Vector2> _uvs0 = new List<Vector2>();
        private static List<Vector2> _uvs1 = new List<Vector2>();
        private static List<Vector3> _normals = new List<Vector3>();
        private static List<int> _triangles = new List<int>();

        private static Vector3[] _inputHolderVec3 = new Vector3[100];
        private static Vector2[] _inputHolderVec2 = new Vector2[100];

        /// <summary>
        /// Close off a ring of points
        /// </summary>
        public static Mesh CreateFan(IEnumerable<Vector3> surrounding, Vector3 snapToPoint, Vector2 mainUV, Vector3 normal, bool makeItLoop = false, bool flipOrientation = false)
        {
            _verts.Clear();
            _uvs0.Clear();
            _normals.Clear();
            _triangles.Clear();

            var count = surrounding.Count();
            for (int i = 0; i < count - (makeItLoop ? 0 : 1); i++)
            {
                var surroundingCurrent = surrounding.ElementAt(i);
                var surroundingNext = surrounding.ElementAt((i + 1) % count);

                _verts.Add(surroundingCurrent);
                _verts.Add(surroundingNext);
                _verts.Add(snapToPoint);

                _uvs0.Add(mainUV);
                _uvs0.Add(mainUV);
                _uvs0.Add(mainUV);

                _normals.Add(normal);
                _normals.Add(normal);
                _normals.Add(normal);
            }

            for (int i = 0; i < _verts.Count; i += 3)
            {
                if (flipOrientation)
                {
                    _triangles.Add(i);
                    _triangles.Add(i + 2);
                    _triangles.Add(i + 1);
                }
                else
                {
                    _triangles.Add(i);
                    _triangles.Add(i + 1);
                    _triangles.Add(i + 2);
                }
            }

            _mesh.Clear(false);
            _mesh.SetVertices(_verts);
            _mesh.SetNormals(_normals);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.SetUVs(0, _uvs0);

            return _mesh;
        }


        /// <summary>
        /// Extrude points inwards or outwards
        /// </summary>
        public static KeyValuePair<bool, Vector3[]> ExtrudePoints(Vector3[] input_WorldSpace, float width, Vector3 forwardDirection, Vector3 upDirection, Transform helper, bool makeItLoop, bool outwards)
        {
            KeyValuePair<bool, Vector3[]> returnResult;
     
            helper.position = input_WorldSpace[0];
            helper.LookAt(helper.position + forwardDirection, upDirection);

            bool extendsAreConnected = Utility.IsPointsSetLooping(input_WorldSpace);
            bool loopExtrusion = makeItLoop || extendsAreConnected;
            int inputLength = input_WorldSpace.Length + (extendsAreConnected ? -1 : 0);

            InputHoldersCheck(inputLength);

            for (int i = 0; i < inputLength; i++)
            {
                _inputHolderVec3[i] = input_WorldSpace[i];
            }

            var input_LocalSpace = _inputHolderVec2;
            var lineDirections = new List<Vector2>();
            for (int i = 0; i < inputLength; i++)
            {
                input_LocalSpace[i] = helper.InverseTransformPoint(_inputHolderVec3[i]).XY();
                var nextLocalPos = helper.InverseTransformPoint(_inputHolderVec3[(i + 1) % inputLength]).XY();

                var dir = nextLocalPos - input_LocalSpace[i];

                if (loopExtrusion == false)
                {
                    var last = i == inputLength - 1;
                    if (last)
                    {
                        var prevLocalPos = helper.InverseTransformPoint(_inputHolderVec3.GetValueAt(i - 1)).XY();
                        dir = input_LocalSpace[i] - prevLocalPos;
                    }
                }

                lineDirections.Add(dir.normalized);
            }


            var extrudedPoints = new Vector3[input_WorldSpace.Length];
            for (int i = 0; i < lineDirections.Count; i++)
            {
                var first = i == 0;
                var last = i == lineDirections.Count - 1;

                var lineA = GetLine(i);
                var lineB = GetLine(i - 1);

                if ((first || last) && loopExtrusion == false)
                {
                    var localPos = lineA.Center();
                    extrudedPoints[i] = helper.TransformPoint(localPos);
                }
                else
                {
                    bool parallel = Vector2.Dot(lineA.Dir, lineB.Dir) >= 0.999f;
                    if (parallel)
                    {
                        var localPos = Vector2.Lerp(lineA.Start, lineA.End, 0.5f);
                        extrudedPoints[i] = helper.TransformPoint(localPos);
                    }
                    else
                    {
                        var result = IsIntersecting2D(lineA, lineB);
                        if (result.Key == false)
                        {
                            //Debug.LogError("No Point Found at " + i);
                            returnResult = new KeyValuePair<bool, Vector3[]>(false, null);
                            return returnResult;
                        }
                        else
                        {
                            var localPos = result.Value;
                            extrudedPoints[i] = helper.TransformPoint(localPos);
                        } 
                    }
                }

                

                //////////////////////////////////////////////////////////
                Line GetLine(int index)
                {
                    var dir = lineDirections.GetValueAt(index).normalized;

                    var line = new Line();
                    line.Start = input_LocalSpace.GetValueAt(index) - (dir * 10);
                    line.End = input_LocalSpace.GetValueAt(index) + (dir * 10);

                    var offset = new Vector2(dir.y, -dir.x) * width * (outwards ? 1 : -1);
                    line.Start += offset;
                    line.End += offset;
                    line.Dir = dir;

                    return line;
                }
            }

            if (extendsAreConnected)
            {
                extrudedPoints[extrudedPoints.Length - 1] = extrudedPoints[0];
            }

            returnResult = new KeyValuePair<bool, Vector3[]>(true, extrudedPoints);
            return returnResult;
        }

        /// <summary>
        /// Extrude points into a direction, generating a mesh along the way
        /// </summary>
        public static Mesh Extrude(Vector3[] input_WorldSpace, Vector3 direction, float length, bool makeItLoop, out Vector3[] extrudedPoints, bool flip = false)
        {
            var inputLength = input_WorldSpace.Length;
            InputHoldersCheck(inputLength);

            var lengths = new float[input_WorldSpace.Length];
            var directions = new Vector3[input_WorldSpace.Length];
            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = length;
                directions[i] = direction;
            }

            return Extrude(input_WorldSpace, directions, lengths, makeItLoop, out extrudedPoints, flip);
        }

        public static Mesh Extrude(Vector3[] input_WorldSpace, Vector3[] direction, float length, bool makeItLoop, out Vector3[] extrudedPoints, bool flip = false)
        {
            float[] lengths = new float[input_WorldSpace.Length];
            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = length;
            }

            return Extrude(input_WorldSpace, direction, lengths, makeItLoop, out extrudedPoints, flip);
        }

        public static Mesh Extrude(Vector3[] input_WorldSpace, Vector3[] direction, float[] lengths, bool makeItLoop, out Vector3[] extrudedPoints, bool flip = false)
        {
            _mesh.Clear();

            extrudedPoints = new Vector3[input_WorldSpace.Length];
            bool extendsAreConnected = Utility.IsPointsSetLooping(input_WorldSpace);

            for (int i = 0; i < extrudedPoints.Length; i++)
            {
                var last = i == extrudedPoints.Length - 1;
                var lengthOffset = direction[i] * lengths.GetValueAt(i);
                var lengthOffsetNext = direction.GetValueAt(i + 1) * lengths.GetValueAt(i + 1);

                extrudedPoints[i] = input_WorldSpace.GetValueAt(i) + lengthOffset;

                if (last && makeItLoop == false && extendsAreConnected == false)
                {
                    // do nothing at the last slot if we dont want to loop it
                    // but if the input already loops then contiue as per normal
                }
                else
                {
                    var quad = QuadGenerator_3D.Generate(new Vector3[]
                    {
                        input_WorldSpace.GetValueAt(i), input_WorldSpace.GetValueAt(i + 1),
                        input_WorldSpace.GetValueAt(i + 1) + lengthOffsetNext, input_WorldSpace.GetValueAt(i) + lengthOffset
                    },
                    Vector2Int.one,
                    Vector3.zero,
                    Utility.CalculateNormal(input_WorldSpace.GetValueAt(i + 1) - input_WorldSpace.GetValueAt(i), lengthOffset) * (flip ? -1 : 1),
                    flip);

                    CombineMeshes.Combine(_mesh, quad);
                }
            }

            return _mesh;
        }



        /// <summary>
        /// Bridge between 2 sets of points
        /// </summary>
        //public static Mesh CreateBridgeSoftEdged(Vector3[] sideA, Vector3[] sideB, bool makeItLoop = false, bool flipOrientation = false, bool flipNormals = false)
        //{
        //    return CreateBridgeSoftEdged(new List<Vector3>(sideA), new List<Vector3>(sideB), makeItLoop, flipOrientation, flipNormals);
        //}

        public static Mesh CreateBridgeSoftEdged(IEnumerable<Vector3> sideA, IEnumerable<Vector3> sideB, bool makeItLoop = false, bool flipOrientation = false, bool flipNormals = false)
        {
            _verts.Clear();
            _uvs0.Clear();
            _normals.Clear();
            _triangles.Clear();
            _normalsSegment.Clear();
            _uvSide.Clear();

            var aCount = sideA.Count();
            var bCount = sideB.Count();

            for (int i = 0; i < aCount + (makeItLoop ? 1 : 0); i++)
            {
                _verts.Add(sideA.ElementAt(i % aCount));
            }


            var sideLength = _verts.Count;
            var inputIsLooping = Utility.PointsListLoops(_verts);
            var ridgeLoops = inputIsLooping || makeItLoop;


            for (int i = 0; i < bCount + (makeItLoop ? 1 : 0); i++)
            {
                _verts.Add(sideB.ElementAt(i % bCount));
            }


            var uv = new Vector2(0, 0);
            for (int i = 0; i < sideLength; i++)
            {
                var progress = (float)i / (aCount - 1);
                uv.y = progress;
                _uvSide.Add(uv);
            }
            _uvs0.AddRange(_uvSide);

            uv.x = 1;
            for (int i = 0; i < _uvSide.Count; i++)
            {
                uv.y = _uvSide[i].y;
                _uvSide[i] = uv;
            }
            _uvs0.AddRange(_uvSide);

   
            for (int i = 0; i < sideLength; i ++)
            {
                var index = i;

                bool first = i == 0;
                bool last = i == sideLength - 1;

                var nextVert = last ? _verts[(i + 2) % sideLength] : _verts[(i + 1) % sideLength];
                var prevVert = first ? _verts.GetValueAt(i - 2) : _verts.GetValueAt(i - 1);

                var dirA = (_verts[i + sideLength] - _verts[i]).normalized;
                var dirB = (nextVert - _verts[i]).normalized;
                var normalA = Vector3.Cross(dirA, dirB);
          
                dirB = (prevVert - _verts[i]).normalized;
                var normalB = Vector3.Cross(dirB, dirA);

                var normal = Vector3.Lerp(normalA, normalB, 0.5f).normalized;

                if (ridgeLoops == false)
                {
                    if(first)
                    {
                        normal = normalA;
                    }
                    else if (last)
                    {
                        normal = normalB;
                    }
                }

                if (flipNormals) normal = -normal;

                _normalsSegment.Add(normal);
            }
        
            _normals.AddRange(_normalsSegment);
            _normals.AddRange(_normalsSegment);


            for (int i = 0; i < sideLength - 1; i++)
            {
                if (flipOrientation)
                {
                    _triangles.Add(i);
                    _triangles.Add(i + sideLength + 1);
                    _triangles.Add(i + sideLength);

                    _triangles.Add(i);
                    _triangles.Add(i + 1);
                    _triangles.Add(i + sideLength + 1);
                }
                else
                {
                    _triangles.Add(i);
                    _triangles.Add(i + sideLength);
                    _triangles.Add(i + sideLength + 1);

                    _triangles.Add(i);
                    _triangles.Add(i + sideLength + 1);
                    _triangles.Add(i + 1);
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(_verts);
            _mesh.SetNormals(_normals);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.SetUVs(0, _uvs0);

            return _mesh;
        }

        public static Mesh CreateMultiBridgeHardEdged(Vector3[][] sides, bool makeItLoop = false, bool flipOrientation = false, bool flipNormals = false)
        {
            if(_meshCombined == null) _meshCombined = new Mesh();
            _meshCombined.Clear();
            for (int i = 0; i < sides.Length - 1; i++)
            {
                CombineMeshes.Combine(_meshCombined, CreateBridgeHardEdged(sides[i], sides[i+1], makeItLoop, flipOrientation, flipNormals));
            }

            return _meshCombined;
        }

        public static Mesh CreateBridgeHardEdged(IEnumerable<Vector3> sideA, IEnumerable<Vector3> sideB, bool makeItLoop = false, bool flipOrientation = false, bool flipNormals = false)
        {
            _verts.Clear();
            _uvs0.Clear();
            _normals.Clear();
            _triangles.Clear();
            _normalsSegment.Clear();
            _uvSide.Clear();

            var aCount = sideA.Count();
            var bCount = sideB.Count();

            for (int i = 0; i < aCount + (makeItLoop ? 0 : -1); i++)
            {
                _verts.Add(sideA.ElementAt(i));
                _verts.Add(sideA.ElementAt((i + 1) % aCount));
            }

            var sideLength = _verts.Count;

            for (int i = 0; i < bCount + (makeItLoop ? 0 : -1); i++)
            {
                _verts.Add(sideB.ElementAt(i));
                _verts.Add(sideB.ElementAt((i + 1) % bCount));
            }


            var uv = new Vector2();
            for (int i = 0; i < sideLength; i++)
            {
                var even = (i % 2) == 0;
                var index = 0;

                if (i == 0)
                {
                    index = 0;
                }
                else if (even)
                {
                    index = i / 2;
                }
                else
                {
                    index = (i + 1) / 2;
                }

                var progress = (float)index / aCount;
                uv.y = progress;
                _uvSide.Add(uv);
            }

            for (int i = 0; i < sideLength - 1; i += 2)
            {

                var index = i;

                var dirA = (_verts[i + sideLength] - _verts[i]).normalized;
                var dirB = (_verts[(i + 1) % sideLength] - _verts[i]).normalized;
                var normal = Vector3.Cross(dirA, dirB);

                if (flipNormals) normal = -normal;

                _normalsSegment.Add(normal);
                _normalsSegment.Add(normal);
            }

            _uvs0.AddRange(_uvSide);

            uv.x = 1;
            for (int i = 0; i < _uvSide.Count; i++)
            {
                uv.y = _uvSide[i].y;
                _uvSide[i] = uv;
            }
            _uvs0.AddRange(_uvSide);

            _normals.AddRange(_normalsSegment);
            _normals.AddRange(_normalsSegment);


            for (int i = 0; i < sideLength - 1; i++)
            {
                if (flipOrientation)
                {
                    _triangles.Add(i);
                    _triangles.Add(i + sideLength + 1);
                    _triangles.Add(i + sideLength);

                    _triangles.Add(i);
                    _triangles.Add(i + 1);
                    _triangles.Add(i + sideLength + 1);
                }
                else
                {
                    _triangles.Add(i);
                    _triangles.Add(i + sideLength);
                    _triangles.Add(i + sideLength + 1);

                    _triangles.Add(i);
                    _triangles.Add(i + sideLength + 1);
                    _triangles.Add(i + 1);
                }
            }

            if (_mesh == null) _mesh = new Mesh();

            if (_verts.Exists(vector => vector.IsNaN()))
                return _mesh;

            _mesh.Clear();
            _mesh.SetVertices(_verts);
            _mesh.SetNormals(_normals);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.SetUVs(0, _uvs0);

            return _mesh;
        }



        /// <summary>
        /// Create a ridge, push a set of points in a given direction and cap of inwards 
        /// </summary>
        public static Mesh TryCreateRidge(Vector3[] ridgePoints, Vector2[] ridgeCapShape, Vector3 meshDir, float width, float length, Vector3 extrudeDirForward, Vector3 extrudeDirUp, bool makeItLoop, bool flipInOutwardsDir, Transform helper, out Vector3[] innerRidge)
        {
            var meshDirs = new Vector3[ridgePoints.Length];
            var lengths = new float[ridgePoints.Length];
            for (int i = 0; i < meshDirs.Length; i++)
            {
                meshDirs[i] = meshDir;
                lengths[i] = length;
            }

            return TryCreateRidge(ridgePoints, ridgeCapShape, meshDirs, width, lengths, extrudeDirForward, extrudeDirUp, makeItLoop, flipInOutwardsDir, helper, out innerRidge);
        }

        public static Mesh TryCreateRidge(Vector3[] ridgePoints, Vector2[] ridgeCapShape, Vector3[] meshDir, float width, float[] length, Vector3 extrudeDirForward, Vector3 extrudeDirUp, bool makeItLoop, bool flipInOutwardsDir, Transform helper, out Vector3[] innerRidge)
        {
            _meshCombined.Clear();

            for(int i = 0; i < ridgeCapShape.Length; i++)
            {
                var pos = ridgeCapShape[i];
                pos.x = pos.x * 2 - 1;
                pos.y = pos.y * 2;
                ridgeCapShape[i] = pos;
            }

                
            var extrusion = ExtrudePoints(
                ridgePoints, width, extrudeDirForward, extrudeDirUp, helper,
                makeItLoop, flipInOutwardsDir);

            if (extrusion.Key == false)
            {
                innerRidge = null;
                return null;
            }
            else
            {
                innerRidge = extrusion.Value;
            }

            DebugUtility.DrawPoints(innerRidge, Vector3.forward, Color.green, 10);

            helper.position = ridgePoints[0];
            helper.LookAt(helper.position + extrudeDirForward, extrudeDirUp);

            #region MeshWalls
            var input_LocalSpaceZ = new float[ridgePoints.Length];
            for (int i = 0; i < ridgePoints.Length; i++)
            {
                input_LocalSpaceZ[i] = -helper.InverseTransformPoint(ridgePoints[i]).z + length[i];
            }
            var ridgePoints_Mesh = new Vector3[0];
            var quadFromExtruding = Extrude(ridgePoints, meshDir, input_LocalSpaceZ, makeItLoop, out ridgePoints_Mesh);
            CombineMeshes.Combine(_meshCombined, quadFromExtruding);

            DebugUtility.DrawPoints(ridgePoints_Mesh, Vector3.forward, Color.red, 10);

            for (int i = 0; i < innerRidge.Length; i++)
            {
                input_LocalSpaceZ[i] = -helper.InverseTransformPoint(innerRidge[i]).z + length[i];
            }
            var ridgePointsExtruded_Mesh = new Vector3[0];
            quadFromExtruding = Extrude(innerRidge, meshDir, input_LocalSpaceZ, makeItLoop, out ridgePointsExtruded_Mesh, true);
            CombineMeshes.Combine(_meshCombined, quadFromExtruding);
            #endregion


            #region CapWalls
            var inputIsLooping = Utility.IsPointsSetLooping(ridgePoints);
            var ridgeLoops = inputIsLooping || makeItLoop;

            if (ridgeLoops == false)
            {
                //cap start
                var quad = QuadGenerator_3D.Generate(new Vector3[] {
                    ridgePoints[0], ridgePoints_Mesh[0],
                    ridgePointsExtruded_Mesh[0], innerRidge[0]
                },
                Vector2Int.one,
                Utility.CalculateNormal(
                    ridgePoints_Mesh[0], ridgePointsExtruded_Mesh[0],
                    ridgePoints_Mesh[0], ridgePoints[0]),
                false);

                CombineMeshes.Combine(_meshCombined, quad);

                //cap end
                quad = QuadGenerator_3D.Generate(new Vector3[] {
                    ridgePoints.Last(), ridgePoints_Mesh.Last(),
                    ridgePointsExtruded_Mesh.Last(), innerRidge.Last()
                },
                Vector2Int.one,
                -Utility.CalculateNormal(
                    ridgePoints_Mesh.Last(), ridgePointsExtruded_Mesh.Last(),
                    ridgePoints_Mesh.Last(), ridgePoints.Last()),
                true);

                CombineMeshes.Combine(_meshCombined, quad);
            }
            #endregion


            #region MeshTop
            for (int i = 0; i < ridgePoints_Mesh.Length; i++)
            {
                var first = i == 0;
                var last = i == ridgePoints_Mesh.Length - 1;


                //Start
                var sectionWidth = Vector3.Distance(ridgePoints_Mesh[i], ridgePointsExtruded_Mesh[i]);
                helper.position = Vector3.Lerp(ridgePoints_Mesh[i], ridgePointsExtruded_Mesh[i], 0.5f);
                helper.LookAt(ridgePointsExtruded_Mesh[i], extrudeDirForward);

                var fenderShapeA = new Vector3[ridgeCapShape.Length];
                var posRef = Vector3.zero;
                for (int j = 0; j < fenderShapeA.Length; j++)
                {
                    posRef.x = 0;
                    posRef.y = ridgeCapShape[j].y;
                    posRef.z = ridgeCapShape[j].x;

                    var shapePos = posRef * sectionWidth * 0.5f;
                    shapePos.y *= width / sectionWidth;

                    fenderShapeA[j] = helper.TransformPoint(shapePos);
                }

                //End
                sectionWidth = Vector3.Distance(ridgePoints_Mesh.GetValueAt(i + 1), ridgePointsExtruded_Mesh.GetValueAt(i + 1));
                helper.position = Vector3.Lerp(ridgePoints_Mesh.GetValueAt(i + 1), ridgePointsExtruded_Mesh.GetValueAt(i + 1), 0.5f);
                helper.LookAt(ridgePointsExtruded_Mesh.GetValueAt(i + 1), extrudeDirForward);
                var fenderShapeB = new Vector3[ridgeCapShape.Length];
                for (int j = 0; j < fenderShapeB.Length; j++)
                {
                    posRef.x = 0;
                    posRef.y = ridgeCapShape[j].y;
                    posRef.z = ridgeCapShape[j].x;

                    var shapePos = posRef * sectionWidth * 0.5f;
                    shapePos.y *= width / sectionWidth;

                    fenderShapeB[j] = helper.TransformPoint(shapePos);
                }

                var indexTest = i + 1;

                if (inputIsLooping == false && makeItLoop == false && last)
                {
                    // do nothing / calulate the values so that the caps can be made
                }
                else
                {
                    CombineMeshes.Combine(_meshCombined, CreateBridgeHardEdged(fenderShapeA, fenderShapeB, false, false, false));
                }


                //caps
                if (ridgeLoops == false)
                {
                    if (first)
                    {
                        var normal = Utility.CalculateNormal(
                                ridgePoints_Mesh[0], ridgePointsExtruded_Mesh[0],
                                ridgePoints_Mesh[0], ridgePoints[0]);

                        var cap = CreateFan(
                            fenderShapeA, fenderShapeA[0], Vector2.zero,
                            normal,
                            false,
                            false);

                        CombineMeshes.Combine(_meshCombined, cap);
                    }
                    else if (last)
                    {
                        var normal = -Utility.CalculateNormal(
                                ridgePoints_Mesh.Last(), ridgePointsExtruded_Mesh.Last(),
                                ridgePoints_Mesh.Last(), ridgePoints.Last());

                        var cap = CreateFan(
                              fenderShapeA, fenderShapeA[0], Vector2.right,
                              normal,
                              false,
                              true);

                        CombineMeshes.Combine(_meshCombined, cap);
                    }
                }
            }
            #endregion

            return _meshCombined;
        }


        private static void InputHoldersCheck(int length)
        {
            if (length > _inputHolderVec3.Length)
            {
                _inputHolderVec3 = new Vector3[length];
                _inputHolderVec2 = new Vector2[length];
            }
        }
    }
}
