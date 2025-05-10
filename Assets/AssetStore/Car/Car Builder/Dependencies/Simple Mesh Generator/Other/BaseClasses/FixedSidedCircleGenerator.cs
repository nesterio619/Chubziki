using UnityEngine;
using SimpleMeshGenerator;

/// <summary>
/// Created by : Glenn korver
/// </summary>
/// 
public static class FixedSidedCircleGenerator 
{
    public static Mesh Generate(int sideCount, float radius, Vector3 offset, bool flipOrientation = false)
    {
        var mesh = CircleGenerator.Generate(radius, offset, sideCount, 2, GeneralMeshGenerator.Axis2D.XY, flipOrientation);

        var minMax = Utility.GetMinMax_XY(mesh.vertices);
        var min = minMax.Key;
        var max = minMax.Value;

        //swap x keep y
        min.x = minMax.Value.x;
        max.x = minMax.Key.x;

        MeshManipulation.BoxUVChannel_XY(ref mesh, min, max, 2);

        return mesh;
    }

    public static Mesh Generate_Hollow(int sideCount, float radius, float thickness, Vector3 offset, bool flipOrientation = false)
    {
        var mesh = CircleGenerator.GenerateHollow(radius, thickness, offset, sideCount, 2, GeneralMeshGenerator.Axis2D.XY, flipOrientation);

        var minMax = Utility.GetMinMax_XY(mesh.vertices);
        var min = minMax.Key;
        var max = minMax.Value;

        //swap x keep y
        min.x = minMax.Value.x;
        max.x = minMax.Key.x;

        MeshManipulation.BoxUVChannel_XY(ref mesh, min, max, 2);

        return mesh;
    }

    public static Mesh Generate_Detailed(int sideCount, float radius, int detailResolution, Vector3 offset, bool flipOrientation = false)
    {
        return CircleGenerator.Generate_Detailed(radius, offset, sideCount, detailResolution, flipOrientation);
    }

    public static Mesh Generate_Hollow_Detailed(int sideCount, float radius, float thickness, int detailResolution, Vector3 offset, bool flipOrientation = false)
    {
        return CircleGenerator.Generate_Hollow_Detailed(radius, thickness, offset, sideCount, detailResolution, flipOrientation);
    }

    public static float Perimeter(int sideCount, float radius)
    {
        return radius * sideCount;
    }
}
