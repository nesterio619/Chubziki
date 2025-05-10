using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Outwards
/// 
/// UV0
/// X = 0 - 1: length
/// Y = 0 - 1: (circumference ~ loop)
/// 
/// UV1
/// X = 0 - 1: length / (circumference ~ loop)
/// Y = 0 - 1: (circumference ~ loop)
/// 
/// Notes:
/// the path provided does more then just providing positions, it also provides orientations.
/// this is epecially important at the start / end, these 2 end points should be oriented by the user in way so that it reflects the surface you wish to connect the pipe to
/// </summary>

namespace SimpleMeshGenerator
{
    public class PipeGenerator
    {
        private static Vector2 _uv = new Vector2(0, 0);

        private static Mesh _mesh = new Mesh();

        private static List<Vector3> _verts = new List<Vector3>();
        private static List<Vector2> _uvs0 = new List<Vector2>();
        private static List<Vector2> _uvs1 = new List<Vector2>();
        private static List<Vector3> _normals = new List<Vector3>();   

        // debug
        private static bool _debugMode = false;
        private static float _debugTime = 10;
        private static bool _drawPlanes = true;
        private static bool _drawEndShapes = true;
        private static float _lineScaler = 1;

        // values
        private static int _angleDiffCheckIndex = 0;

        private static List<GeneralMeshGenerator.PointData> _allPoints = new List<GeneralMeshGenerator.PointData>();
        private static Plane _plane = new Plane(Vector3.forward, Vector3.zero);
        private static Ray _ray = new Ray(Vector3.forward, Vector3.zero);

        private static Vector3 _forwardDir;
        private static Vector3 _upDir;
        private static Vector3 _rightDir;

        private static Vector3 _forwardDir_Prev;
        private static Vector3 _upDir_Prev;
        private static Vector3 _rightDir_Prev;

        private static Vector3 _hitPoint = Vector3.zero;

        private static bool _useWorldPos;


        // Start is called before the first frame update
        public static Mesh Generate(Transform[] path, bool useWorldPos, float radius, int resolution, bool updatePathOrientations = true, bool hardNormals = true)
        {
            var angleBuildUp = 0f;
            resolution = resolution + 1;
            _useWorldPos = useWorldPos;
            _allPoints.Clear();

            if (updatePathOrientations)
            {
                for (int i = 1; i < path.Length - 1; i++)
                {
                    path[i].LookAt(path[i + 1].position, Vector3.up);
                }
            }

            if (_debugMode)
            {
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Debug.DrawLine(path[i].position, path[i + 1].position, Color.white, _debugTime);
                }
            }

            // go through path
            for (int i = 0; i < path.Length; i++)
            {
                _forwardDir_Prev = _forwardDir;
                _rightDir_Prev = _rightDir;
                _upDir_Prev = _upDir;

                if (i != path.Length - 1)
                {
                    _forwardDir = (path[i + 1].GetPosition(_useWorldPos) - path[i].GetPosition(_useWorldPos)).normalized;
                }
                else
                {
                    _forwardDir = (path[i].GetPosition(_useWorldPos) - path[i - 1].GetPosition(_useWorldPos)).normalized;
                }

                // prevent GimbalLock
                float gimbalThreshold = 0.95f;
                if (Vector3.Dot(Vector3.up, _forwardDir) < -gimbalThreshold || Vector3.Dot(Vector3.up, _forwardDir) > gimbalThreshold)
                {
                    _upDir = Vector3.Cross(Vector3.right, _forwardDir).normalized;
                    _rightDir = Vector3.Cross(_upDir, _forwardDir).normalized;
                }
                else
                {
                    _rightDir = Vector3.Cross(Vector3.up, _forwardDir).normalized;
                    _upDir = Vector3.Cross(_forwardDir, _rightDir).normalized;
                }

                if (i == 0) // first path point, add circle
                {
                    if (_debugMode)
                    {
                        Debug.DrawLine(path[i].position, path[i].position + _rightDir * 2 * _lineScaler, new Color(1, 0.5f, 0, 1), _debugTime);
                        Debug.DrawLine(path[i].position, path[i].position + _upDir * 2 * _lineScaler, new Color(1, 1, 0, 1), _debugTime);
                    }

                    AddPoints_StartEnd(path[i], Vector3.Lerp(path[i].GetPosition(_useWorldPos), path[i + 1].GetPosition(_useWorldPos), 0.5f), _upDir, _rightDir, radius, resolution);
                }
                else if (i != path.Length - 1) // middle path points, add 2 circles per path point which are exactly the same way alligned
                {
                    var dirToNext = (path[i + 1].GetPosition(_useWorldPos) - path[i].GetPosition(_useWorldPos)).normalized;
                    var dirToPrev = (path[i - 1].GetPosition(_useWorldPos) - path[i].GetPosition(_useWorldPos)).normalized;
                    var planeUp = dirToNext + dirToPrev;
                    planeUp = (planeUp * -1).normalized;

                    var planeForward = (dirToNext - dirToPrev).normalized;
                    var planeRight = Vector3.Cross(planeUp, planeForward);

                    // planes
                    if (_debugMode && _drawPlanes)
                    {
                        Debug.DrawLine(path[i].position - planeRight + planeUp, path[i].position + planeRight + planeUp, Color.cyan, _debugTime); // above
                        Debug.DrawLine(path[i].position - planeRight - planeUp, path[i].position + planeRight - planeUp, Color.white, _debugTime); // below

                        Debug.DrawLine(path[i].position + planeRight + planeUp, path[i].position + planeRight - planeUp, Color.white, _debugTime); // right
                        Debug.DrawLine(path[i].position - planeRight + planeUp, path[i].position - planeRight - planeUp, Color.white, _debugTime); // left
                    }

                    var _rotatedUp = (Quaternion.AngleAxis(angleBuildUp, _forwardDir_Prev) * _upDir_Prev).normalized;
                    var _rotatedSide = (Quaternion.AngleAxis(angleBuildUp, _forwardDir_Prev) * _rightDir_Prev).normalized;
                    var newlyAddedPoints = AddPoints(path[i], path[i - 1], planeForward, _rotatedUp, _rotatedSide, _forwardDir_Prev, radius, resolution);


                    // little summary of whats going to happen next
                    // - take a point out of the newly created set
                    // - move the point in the local space of the next / prev point in the path
                    // - get the angle relative to the local up vector of that point
                    // - get the angle difference and use it on the next batch of points to get a proper up /right dir
                    // - this angle difference is caried over to the next path point, ensuring the is no "twisting of the vertex points / UVs"


                    // get the angle of a point X as it has hit the prev plane
                    var angle1 = 0f;
       
                    _plane.SetNormalAndPosition(_forwardDir_Prev, path[i - 1].GetPosition(_useWorldPos));
                    var prevLinePlane = _plane;

                    _ray.origin = newlyAddedPoints[_angleDiffCheckIndex].Position;
                    _ray.direction = -_forwardDir_Prev;

                    var enter2 = 0f;
                    if (prevLinePlane.Raycast(_ray, out enter2))
                    {
                        //Get the point that is clicked
                        _hitPoint = _ray.GetPoint(enter2);

                        var dir = (_hitPoint - path[i - 1].GetPosition(_useWorldPos)).normalized;
                        angle1 = Vector3.SignedAngle(dir, _upDir_Prev, _forwardDir_Prev);
                    }
                    else
                    {
                        Debug.LogError("No HIT Search 1");
                        Debug.LogError("i " + i);
                    }


                    // get the angle of a point X as it has hit the next plane
                    var angle2 = 0f;

                    _plane.SetNormalAndPosition(-_forwardDir, path[i + 1].GetPosition(_useWorldPos));
                    var nextLinePlane = _plane;

                    _ray.origin = newlyAddedPoints[_angleDiffCheckIndex].Position;
                    _ray.direction = _forwardDir;

                    enter2 = 0f;
                    if (nextLinePlane.Raycast(_ray, out enter2))
                    {
                        //Get the point that is clicked
                        _hitPoint = _ray.GetPoint(enter2);

                        var dir = (_hitPoint - path[i + 1].GetPosition(_useWorldPos)).normalized;
                        angle2 = Vector3.SignedAngle(dir, _upDir, _forwardDir);
                    }
                    else
                    {
                        Debug.LogError("No HIT Search 2");
                        Debug.LogError("i " + i);
                    }

                    // get angle differnce
                    var angleDiff = angle1 - angle2;

                    // adjust the next Points position to take this rotation into account, so that in the end points of the same index meet at the same coordinate
                    _rotatedUp = (Quaternion.AngleAxis(angleDiff + angleBuildUp, _forwardDir) * _upDir).normalized;
                    _rotatedSide = (Quaternion.AngleAxis(angleDiff + angleBuildUp, _forwardDir) * _rightDir).normalized;


                    AddPoints(path[i], path[i + 1], planeForward, _rotatedUp, _rotatedSide, -_forwardDir, radius, resolution);
                    if (_debugMode)
                    {
                        Debug.DrawLine(path[i].position, path[i].position + _rotatedSide * 2 * _lineScaler, new Color(1, 0.5f, 0, 1), _debugTime);
                        Debug.DrawLine(path[i].position, path[i].position + _rotatedUp * 2 * _lineScaler, new Color(1, 1, 0, 1), _debugTime);
                    }

                    angleBuildUp += angleDiff;
                }
                else // last point
                {
                    _plane.SetNormalAndPosition(path[i].forward, path[i].position);

                    var _rotatedUp = (Quaternion.AngleAxis(angleBuildUp, _forwardDir) * _upDir).normalized;
                    var _rotatedSide = (Quaternion.AngleAxis(angleBuildUp, _forwardDir) * _rightDir).normalized;

                    if (_debugMode)
                    {
                        Debug.DrawLine(path[i].position, path[i].position + _rotatedSide * 2 * _lineScaler, new Color(1, 0.5f, 0, 1), _debugTime);
                        Debug.DrawLine(path[i].position, path[i].position + _rotatedUp * 2 * _lineScaler, new Color(1, 1, 0, 1), _debugTime);
                    }

                    AddPoints_StartEnd(path[i], Vector3.Lerp(path[i].GetPosition(_useWorldPos), path[i - 1].GetPosition(_useWorldPos), 0.5f), _rotatedUp, _rotatedSide, radius, resolution);

                }

                // draw directions of a path point
                if (_debugMode)
                {
                    Debug.DrawLine(path[i].position, path[i].position + _forwardDir * _lineScaler, Color.blue, _debugTime);
                    Debug.DrawLine(path[i].position, path[i].position + _rightDir * _lineScaler, Color.red, _debugTime);
                    Debug.DrawLine(path[i].position, path[i].position + _upDir * _lineScaler, Color.green, _debugTime);
                }
            }


            return GenerateMesh(path, radius, resolution);
        }

        private static void AddPoints_StartEnd(
            Transform pathPoint, Vector3 circlePointsPos,
            Vector3 upDir, Vector3 rightDir,
            float radius, int resolution)
        {
            _plane.SetNormalAndPosition(pathPoint.forward, pathPoint.GetPosition(_useWorldPos));
            var currentPointsSet = CircleGenerator.GetPoints(radius, circlePointsPos, upDir, rightDir, resolution -1);

            if (_debugMode && _drawEndShapes)
            {
                for(int i = 0; i < currentPointsSet.Count - 1; i++)
                {
                    Debug.DrawLine(currentPointsSet[i].Position, currentPointsSet[i + 1].Position, Color.yellow, _debugTime);
                }
            }

            for (int j = 0; j < currentPointsSet.Count; j++)
            {
                _ray.origin = currentPointsSet[j].Position;
                _ray .direction = -_forwardDir;

                var enter = 0f;
                if (_plane.Raycast(_ray, out enter))
                {
                    //Get the point that is clicked
                    _hitPoint = _ray.GetPoint(enter);
                    currentPointsSet[j].Position = _hitPoint;
                }
                else
                {
                    _ray.origin = currentPointsSet[j].Position;
                    _ray.direction = _forwardDir;

                    if (_plane.Raycast(_ray, out enter))
                    {
                        //Get the point that is clicked
                        _hitPoint = _ray.GetPoint(enter);
                        currentPointsSet[j].Position = _hitPoint;
                    }
                }
            }

            _allPoints.AddRange(currentPointsSet);
        }

        private static List<GeneralMeshGenerator.PointData> AddPoints(
            Transform pathPoint, Transform pathPoint_CirclePoints,
            Vector3 planeForward, Vector3 upDir, Vector3 rightDir, Vector3 rayDir,
            float radius, int resolution)
        {
            _plane.SetNormalAndPosition(planeForward, pathPoint.GetPosition(_useWorldPos));

            var currentPointsSet = CircleGenerator.GetPoints(radius, pathPoint_CirclePoints.GetPosition(_useWorldPos), upDir, rightDir, resolution -1);

            for (int j = 0; j < currentPointsSet.Count; j++)
            {
                _ray.origin = currentPointsSet[j].Position;
                _ray.direction = rayDir;

                var enter = 0f;
                if (_plane.Raycast(_ray, out enter))
                {
                    //Get the point that is clicked
                    _hitPoint = _ray.GetPoint(enter);
                    currentPointsSet[j].Position = _hitPoint;
                }
                else
                {
                    Debug.LogError("No HIT");
                }
            }
            _allPoints.AddRange(currentPointsSet);

            return currentPointsSet;
        }


        private static Mesh GenerateMesh(Transform[] path, float radius, int resolution)
        {
            _verts.Clear();
            _uvs0.Clear();
            _uvs1.Clear();
            _normals.Clear();

            var totalPathLength = 0f;
            var currentPathLength = 0f;
            for (int i = 0; i < path.Length - 1; i++)
            {
                totalPathLength += Vector3.Distance(path[i].GetPosition(_useWorldPos), path[i + 1].GetPosition(_useWorldPos));
            }

            int uvsCoverd = 0;
            int uvsTarget = 0;
            var circumference = 2 * radius * Mathf.PI;

            for (int i = 0; i < path.Length; i++)
            {
                var atStartOfPath = i == 0;
                var atEndOfPath = i == path.Length - 1;

                if (atStartOfPath == false)
                {
                    currentPathLength += Vector3.Distance(path[i - 1].GetPosition(_useWorldPos), path[i].GetPosition(_useWorldPos));
                }

                uvsTarget = (atStartOfPath || atEndOfPath) ? resolution : resolution * 2;

                // get the uv data from the circles added + adjust X to match Pipe shape 
                for (int j = uvsCoverd; j < uvsCoverd + uvsTarget; j++)
                {
                    _uv.x = currentPathLength / totalPathLength;
                    _uv.y = _allPoints[j].UV.y;
                    _uvs0.Add(_uv);

                    _uv.x = currentPathLength / circumference;
                    _uv.y = _allPoints[j].UV.y;
                    _uvs1.Add(_uv);
                }
            }

            for (int i = 0; i < _allPoints.Count; i++)
            {
                _verts.Add(_allPoints[i].Position);
                _normals.Add(_allPoints[i].Normal);
            }

            _mesh.Clear();
            _mesh.SetVertices(_verts);
            _mesh.SetUVs(0, _uvs0);
            _mesh.SetUVs(1, _uvs1);
            _mesh.SetNormals(_normals);

            var ringCount = 2 + ((path.Length - 2) * 2);
            _mesh.SetTriangles(MeshManipulation.TriangleGeneration(ringCount, resolution, true), 0);


            // caps
            var surrounding = _verts.GetRange(0, resolution);
            surrounding.Reverse();
            var startCap = GeneralMeshGenerator.CreateFan(surrounding, path[0].GetPosition(_useWorldPos), Vector2.zero, (path[0].GetPosition(_useWorldPos) - path[1].GetPosition(_useWorldPos)).normalized, false);
            startCap.OverrideUVs(Vector2.zero, 1);
            CombineMeshes.Combine(_mesh, startCap);

            surrounding = _verts.GetRange(_verts.Count - resolution, resolution);
            var endCap = GeneralMeshGenerator.CreateFan(surrounding, path[path.Length - 1].GetPosition(_useWorldPos), Vector2.one, (path[path.Length - 1].GetPosition(_useWorldPos) - path[path.Length - 2].GetPosition(_useWorldPos)).normalized, false);
            endCap.OverrideUVs(new Vector2(currentPathLength / circumference, 0), 1);
            CombineMeshes.Combine(_mesh, endCap);

            return _mesh;
        }
    }
}
