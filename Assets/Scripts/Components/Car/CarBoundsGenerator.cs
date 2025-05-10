using ProceduralCarBuilder;
using UnityEngine;

public static class CarBoundsGenerator
{
    public static Bounds GetHoodBounds(CarData carData)
    {
        var bodyData = carData.BodyData;
        var edge = carData.FrontData.HoodlidSpaceFromEdges;
        var offsetPercent = (1 - carData.BodyData.HoodHeightPercentage) / 2;

        return new Bounds(
            center: new Vector3(0, -bodyData.BodyHeight * offsetPercent, bodyData.HoodLength / 2 - edge),
            size: new Vector3( bodyData.TotalWidth - edge*2, 0, bodyData.HoodLength - edge*2)
        );
    }

    public static Bounds GetTrunkBounds(CarData carData)
    {
        return new Bounds(
            center: new Vector3(0, 0, -carData.BodyData.TrunkLength / 2),
            size: new Vector3(carData.BodyData.TotalWidth, 0, carData.BodyData.TrunkLength)
        );
    }

    public static Bounds GetSideBounds(CarData carData, bool leftSide)
    {
        var bodyData = carData.BodyData;
        var xOffset = bodyData.TotalWidth / 2 + bodyData.SlantedShapeSidewaysOffset;
        
        return new Bounds(
            center: new Vector3((leftSide ? -1 : 1) * xOffset, bodyData.BodyHeight / 2 + bodyData.HeightOfTheGround),
            size: new Vector3(0, bodyData.BodyHeight, bodyData.BodyLength)
        );
    }

    public static Bounds GetFrontBumperBounds(CarData carData)
    {
        var bumperData = carData.BumperDataFront;
        var bodyData = carData.BodyData;
        var width = bodyData.TotalWidth + bodyData.SlantedShapeSidewaysOffset * 2 + bumperData.Thickness * 2;

        return new Bounds(
            center: Vector3.zero,
            size: new Vector3(width, bumperData.Height)
        );
    }

    public static Bounds GetBackBumperBounds(CarData carData)
    {
        var bumperData = carData.BumperDataBack;
        var bodyData = carData.BodyData;
        var width = bodyData.TotalWidth + bodyData.SlantedShapeSidewaysOffset * 2 + bumperData.Thickness * 2;

        return new Bounds(
            center: new Vector3(0,-bodyData.BodyHeight+bumperData.Height/2, -bumperData.Thickness - bodyData.TrunkLength/2),
            size: new Vector3(width, bumperData.Height)
        );
    }

    public static Bounds GetRoofBounds(CarData carData)
    {
        var roofData = carData.RoofData;
        var xOffset = roofData.DistanceFromSide * 2;
        var zOffset = roofData.DistanceFromBack - roofData.DistanceFromFront;

        return new Bounds(
            center: Vector3.zero,
            size: new Vector3( carData.BodyData.TotalWidth - xOffset, 0, carData.BodyData.BodyLength - zOffset)
        );
    }
}