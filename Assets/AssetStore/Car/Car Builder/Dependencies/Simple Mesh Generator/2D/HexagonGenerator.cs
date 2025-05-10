using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// 
/// Default facing direction: Forward
/// 
/// UV0
/// X = 0 - 1: length
/// Y = 0 - 1: center to edge)
/// 
/// UV1
/// X = 0 - 1: length
/// Y = height relative on circumference
/// 
/// UV2
/// World size
/// 
/// Notes:
/// When generating a "Hollow" variant. The "Radius" decides where the the ring lays, the thicknes is then applied half inwards and half outwards
/// </summary>

namespace SimpleMeshGenerator
{
    public static class HexagonGenerator
    {
        private static int _sideCount = 6;

        public static Mesh Generate(float radius, Vector3 offset, bool flipOrientation = false)
        {
            return FixedSidedCircleGenerator.Generate(_sideCount, radius, offset, flipOrientation);
        }

        public static Mesh Generate_Hollow(float radius, float thickness, Vector3 offset, bool flipOrientation = false)
        {
            return FixedSidedCircleGenerator.Generate_Hollow(_sideCount, radius, thickness, offset, flipOrientation);
        }

        public static Mesh Generate_Detailed(float radius, int detailResolution, Vector3 offset, bool flipOrientation = false)
        {
            return FixedSidedCircleGenerator.Generate_Detailed(_sideCount, radius, detailResolution, offset, flipOrientation);
        }

        public static Mesh Generate_Hollow_Detailed(float radius, float thickness, int detailResolution, Vector3 offset, bool flipOrientation = false)
        {
            return FixedSidedCircleGenerator.Generate_Hollow_Detailed(_sideCount, radius, thickness, detailResolution, offset, flipOrientation);
        }

        public static float Perimeter(float radius)
        {
            return FixedSidedCircleGenerator.Perimeter(_sideCount, radius);
        }
    }
}
