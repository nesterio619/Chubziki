using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// </summary>
namespace SimpleMeshGenerator
{
    public static class Utility
    {
        public static Vector3[][] CreateDoubbleArray_Vector3(int x, int y)
        {
            var array = new Vector3[x][];
            for (int i = 0; i < x; i++)
                array[i] = new Vector3[y];

            return array;
        }

        public static float GetPercentage(float number, float min, float max)
        {
            return (number - min) / (max - min);
        }

        public static KeyValuePair<Vector2, Vector2> GetMinMax_XY(Vector3[] points)
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

        public static Vector3 CalculateNormal(Vector3 topLeft, Vector3 topRight, Vector3 leftTop, Vector3 leftBottom)
        {
            var dirTopToBottom = (leftBottom - leftTop).normalized;
            var dirLeftRight = (topRight - topLeft).normalized;
            return CalculateNormal(dirLeftRight, dirTopToBottom);
        }

        public static Vector3 CalculateNormal(Vector3 dirLeftToRight, Vector3 dirTopToBottom)
        {
            var normal = Vector3.Cross(dirLeftToRight.normalized, dirTopToBottom.normalized).normalized;

            return normal;
        }

        public static Vector3 AxisNormal(GeneralMeshGenerator.Axis2D axis)
        {
            switch (axis)
            {
                case GeneralMeshGenerator.Axis2D.XY:
                    return Vector3.forward;
                case GeneralMeshGenerator.Axis2D.XZ:
                    return Vector3.up;
                case GeneralMeshGenerator.Axis2D.ZY:
                    return Vector3.right;
            }

            return Vector3.forward;
        }

        public static Vector3 GetMiddle(Vector3 a, Vector3 b)
        {
            return Vector3.Lerp(a, b, 0.5f);
        }

        public static float GetMiddle(float a, float b)
        {
            return Mathf.Lerp(a, b, 0.5f);
        }

        public static int BoolToFlipValue(bool value)
        {
            if (value)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        public static bool BoolLerp(bool a, bool b, float progress)
        {
            if (progress > 0.5f)
            {
                return b;
            }
            else
            {
                return a;
            }
        }

        public static int IntLerp(int a, int b, float progress)
        {
            if (progress > 0.5f)
            {
                return b;
            }
            else
            {
                return a;
            }
        }

        public static Vector2 Vector2Lerp_HardSwitch(Vector2 a, Vector2 b, float progress)
        {
            if (progress > 0.5f)
            {
                return b;
            }
            else
            {
                return a;
            }
        }

        public static Color[] ColorLerp(Color[] a, Color[] b, float progress)
        {
            var lerped = new Color[b.Length];

            for(int i = 0; i < lerped.Length; i++)
            {
                lerped[i] = Color.Lerp(a.GetValueAt(i), b[i], progress);
            }

            return lerped;
        }

        public static bool IsPointsSetLooping(Vector3[] points)
        {
            return PointsAreIdentical(points[0], points.Last());
        }

        public static bool PointsListLoops(List<Vector3> points)
        {
            return PointsAreIdentical(points[0], points.Last());
        }

        public static bool PointsAreIdentical(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b) < 0.0001f;
        }

        public static int VertCountOfHardEdgedObject(int topVertCount, int sideVertCount)
        {
            return 4 +
            (Mathf.Min(0, topVertCount - 2) * Mathf.Min(0, sideVertCount - 2)) * 4
            +
            (Mathf.Min(0, topVertCount - 2) * 2 + Mathf.Min(0, sideVertCount - 2) * 2) * 2;
        }

        [System.Serializable]
        public struct TransformData
        {
            public Vector3 Pos;
            public Vector3 Rotation;
        }
    }
}
