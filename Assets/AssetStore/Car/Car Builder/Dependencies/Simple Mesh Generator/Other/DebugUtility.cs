using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// </summary>
namespace SimpleMeshGenerator
{
    public static class DebugUtility
    {
        public static bool DebugMode = false;

        public static void GizmosDrawPoints(List<Vector3> points, float size, Vector3 dir, Color color)
        {
            GizmosDrawPoints(points.ToArray(), size, dir, color);
        }

        public static void GizmosDrawPoints(Vector3[] points, float size, Vector3 dir, Color color)
        {
            if (DebugMode == false) return;

            for (int i = 0; i < points.Length; i++)
            {
                if (i == 0 || i == points.Length - 1)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = color;
                }

                Gizmos.DrawWireSphere(points[i], size);
                Gizmos.DrawRay(points[i], dir);
            }
        }
    
        public static void DrawPoints(Transform[] transforms, Vector3 dir, Color color, float duration = -1)
        {
            DrawPoints(transforms.Positions(), dir, color, duration);
        }

        public static void DrawPoints(List<Vector3> points, Vector3 dir, Color color, float duration = -1)
        {
            DrawPoints(points.ToArray(), dir, color, duration);
        }

        public static void DrawPoints(Vector3[] points, Vector3 dir, Color color, float duration = -1)
        {
            if (DebugMode == false) return;

            for (int i = 0; i < points.Length; i++)
            {
                if(duration > 0)
                {
                    Debug.DrawRay(points[i], dir * 0.05f, color, duration);
                }
                else
                {
                    Debug.DrawRay(points[i], dir * 0.05f, color);
                }
            }
        }

        public static void DrawPointsPath(Transform transformHolder, Color color, bool close)
        {
            if (DebugMode == false) return;

            var points = new Vector3[transformHolder.childCount];
            for(int i = 0; i < transformHolder.childCount; i++)
            {
                points[i] = transformHolder.GetChild(i).position;
            }

            DrawPointsPath(points, color, close);
        }

        public static void DrawPointsPath(Transform[] transforms, Color color, bool close)
        {
            DrawPointsPath(transforms.Positions(), color, close);
        }

        public static void DrawPointsPath(List<Vector3> points, Color color, bool close)
        {
            DrawPointsPath(points.ToArray(), color, close);
        }

        public static void DrawPointsPath(Vector3[] points, Color color, bool close)
        {
            if (DebugMode == false) return;

            for (int i = 0; i < points.Length - (close ? 0 : 1); i++)
            {
                Debug.DrawLine(points[i], points[(i + 1) % points.Length], color);
            }
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = -1)
        {
            if (DebugMode == false) return;

            if (duration > 0)
            {
                Debug.DrawLine(start, end, color, duration);
            }
            else
            {
                Debug.DrawLine(start, end, color);
            }
        }
    }
}
