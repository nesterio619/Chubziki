using SimpleMeshGenerator;
using UnityEngine;

namespace ProceduralCarBuilder
{
    public class CarData
    {
        [Header("Pivot")]
        public float PivotPercentage;

        [Header("Matererials")]
        public CarSettings.MaterialSettings MaterialsData;

        [Header("Colors")]
        public Color[] Colors;

        public Color HoodDecalColor;
        public Color BodyTopDecalColor;

        [Header("Body")]
        public Body.Data BodyData;

        [Header("Wheels")]
        public Wheels.Data WheelData;

        [Header("Trunk")]
        public Back.Data BackData;

        [Header("Hood")]
        public Front.Data FrontData;

        [Header("Nose")]
        public Nose.Data NoseData;

        [Header("Roof")]
        public Roof.Data RoofData;

        [Header("WindowSpacing")]
        public Window.Data WindowData;

        [Header("Door")]
        public Doors.Data DoorData;
    
        [Header("Lights")]
        public Lights.Data LightsData;

        [Header("Wing Mirror")]
        public WingMirror.Data WingMirrorData;

        [Header("Bumper")]
        public Bumper.Data BumperDataFront;
        public Bumper.Data BumperDataBack;

        [Header("LicensePlate")]
        public LicensePlate.Data LicensePlateData;

        public static CarData Blend(CarData a, CarData b, float progress, CarData targetData = null)
        {
            progress = Mathf.Clamp01(progress);

            var dataBlend = targetData;
            if (dataBlend == null) dataBlend = new CarData();

            dataBlend.BodyData = Body.Data.Blend(a.BodyData, b.BodyData, progress, dataBlend.BodyData);
            dataBlend.BackData = Back.Data.Blend(a.BackData, b.BackData, progress, dataBlend.BackData);
            dataBlend.FrontData = Front.Data.Blend(a.FrontData, b.FrontData, progress, dataBlend.FrontData);
            dataBlend.NoseData = Nose.Data.Blend(a.NoseData, b.NoseData, progress, dataBlend.NoseData);
            dataBlend.RoofData = Roof.Data.Blend(a.RoofData, b.RoofData, progress, dataBlend.RoofData);
            dataBlend.WindowData = Window.Data.Blend(a.WindowData, b.WindowData, progress, dataBlend.WindowData);
            dataBlend.DoorData = Doors.Data.Blend(a.DoorData, b.DoorData, progress, dataBlend.DoorData);
            dataBlend.LightsData = Lights.Data.Blend(a.LightsData, b.LightsData, progress, dataBlend.LightsData);
            dataBlend.WingMirrorData = WingMirror.Data.Blend(a.WingMirrorData, b.WingMirrorData, progress, dataBlend.WingMirrorData);
            dataBlend.BumperDataFront = Bumper.Data.Blend(a.BumperDataFront, b.BumperDataFront, progress, dataBlend.BumperDataFront);
            dataBlend.BumperDataBack = Bumper.Data.Blend(a.BumperDataBack, b.BumperDataBack, progress, dataBlend.BumperDataBack);
            dataBlend.LicensePlateData = LicensePlate.Data.Blend(a.LicensePlateData, b.LicensePlateData, progress, dataBlend.LicensePlateData);
            dataBlend.WheelData = Wheels.Data.Blend(a.WheelData, b.WheelData, progress, dataBlend.WheelData);

            dataBlend.PivotPercentage = Mathf.Lerp(a.PivotPercentage, b.PivotPercentage, progress);
            dataBlend.MaterialsData = progress < 0.5f ? a.MaterialsData : b.MaterialsData;
            dataBlend.Colors = Utility.ColorLerp(a.Colors, b.Colors, progress);
            return dataBlend;

        }

        // try to ensure the car appears "correct" within reasonable assumptions
        public static CarData SanityCheckData(CarData data)
        {
            var bodyMinimum = CarGenerator.MinimumDistance * 2;

            if (data.BodyData.BodyHeight < bodyMinimum) data.BodyData.BodyHeight = bodyMinimum;
            if (data.BodyData.BodyLength < bodyMinimum) data.BodyData.BodyLength = bodyMinimum;
            if (data.BodyData.HoodLength < bodyMinimum) data.BodyData.HoodLength = bodyMinimum;
            if (data.BodyData.TotalWidth < bodyMinimum) data.BodyData.TotalWidth = bodyMinimum;
            if (data.BodyData.TrunkLength < bodyMinimum) data.BodyData.TrunkLength = bodyMinimum;


            // down scale the wheel so that it fits betwheen the front / back ends of the car / its segment
            var maxWheelAreaRadius = data.BodyData.HoodLength * 0.95f;

            if (data.WheelData.TotalWheelArea() > maxWheelAreaRadius)
            {
                float downScale = maxWheelAreaRadius / data.WheelData.TotalWheelArea();

                data.WheelData.Radius *= downScale;
                data.WheelData.FreeSpace *= downScale;
                data.WheelData.OuterFenderWidth *= downScale;
            }


            // raise car frame to the point that the wheels cant stick out above the top of the car frame
            var bodyTopOffTheGround = data.BodyData.HeightOfTheGround + data.BodyData.BodyHeight;
            var wheelAreaTop = data.WheelData.Radius + data.WheelData.TotalWheelAreaHalf();
            if (wheelAreaTop - CarGenerator.MinimumDistance > bodyTopOffTheGround)
            {
                var heightDiff = wheelAreaTop - bodyTopOffTheGround;
                data.BodyData.HeightOfTheGround += heightDiff + CarGenerator.MinimumDistance;
            }


            data.BodyData.SlantedShapeHeightPercentage = Mathf.Clamp01(data.BodyData.SlantedShapeHeightPercentage);
            data.BodyData.HoodHeightPercentage = Mathf.Clamp01(data.BodyData.HoodHeightPercentage);


            var hoodTargetHeight = data.BodyData.HeightOfTheGround + data.BodyData.BodyHeight * data.BodyData.HoodHeightPercentage;
            if (hoodTargetHeight < wheelAreaTop + CarGenerator.MinimumDistance)
            {
                var targetHeight = wheelAreaTop - data.BodyData.HeightOfTheGround + CarGenerator.MinimumDistance * 2;
                data.BodyData.HoodHeightPercentage = Mathf.Clamp01(targetHeight / data.BodyData.BodyHeight);
            }

            // ensure the slanted pos is above the wheels
            var slantedTargetHeight = data.BodyData.HeightOfTheGround + data.BodyData.BodyHeight * data.BodyData.HoodHeightPercentage * data.BodyData.SlantedShapeHeightPercentage;
            if (slantedTargetHeight < wheelAreaTop + CarGenerator.MinimumDistance)
            {
                var targetHeight = wheelAreaTop - data.BodyData.HeightOfTheGround + CarGenerator.MinimumDistance * 2;
                data.BodyData.SlantedShapeHeightPercentage = Mathf.Clamp01(targetHeight / (data.BodyData.BodyHeight * data.BodyData.HoodHeightPercentage));
            }

            // cap window indentation
            data.WindowData.WindowIndentation = Mathf.Min(data.WindowData.WindowIndentation, data.WindowData.WindowSpacing * 0.5f);

            // Minimal height of roof to guarantee no Errors during window Generation
            data.RoofData.Height = Mathf.Max(data.RoofData.Height, data.WindowData.WindowSpacing + CarGenerator.MinimumDistance);

            data.BodyData.TrunkLength = Mathf.Max(data.BodyData.TrunkLength, data.WheelData.TotalWheelAreaHalf() + CarGenerator.MinimumDistance);

            return data;
        }

        public void EnsureBackTrunkEncapsulatesVolume(float width, float height, float thickness)
        {
            EnsureBackTrunkEncapsulatesVolume(new Vector3(width, height, thickness));
        }

        public void EnsureBackTrunkEncapsulatesVolume(Vector3 dimensions)
        {
            dimensions += Vector3.one * CarGenerator.MinimumDistance;

            BodyData.TotalWidth = Mathf.Max(BodyData.TotalWidth, dimensions.x + BackData.TrunkWallThickness * 2);
            BodyData.TrunkLength = Mathf.Max(BodyData.TrunkLength, dimensions.z + BackData.TrunkWallThickness * 2);

            BodyData.BodyHeight = Mathf.Max(BodyData.BodyHeight, dimensions.y);
            BackData.TruckDepth = Mathf.Max(BackData.TruckDepth, dimensions.y);
        }

        public void EnsureFrontTrunkEncapsulatesVolume(float width, float height, float thickness)
        {
            EnsureFrontTrunkEncapsulatesVolume(new Vector3(width, height, thickness));
        }

        public void EnsureFrontTrunkEncapsulatesVolume(Vector3 dimensions)
        {
            dimensions += Vector3.one * CarGenerator.MinimumDistance;

            BodyData.TotalWidth = Mathf.Max(BodyData.TotalWidth, dimensions.x + FrontData.HoodlidSpaceFromEdges * 2);
            BodyData.HoodLength = Mathf.Max(BodyData.HoodLength, dimensions.z + FrontData.HoodlidSpaceFromEdges * 2);        

            BodyData.BodyHeight = Mathf.Max(BodyData.BodyHeight, dimensions.y * (1f / BodyData.HoodHeightPercentage));
            FrontData.TrunkDepth = Mathf.Max(FrontData.TrunkDepth, dimensions.y);
        }


        public void EnsureRoofEncapsulatesVolume(float width, float height, float thickness)
        {
            EnsureRoofEncapsulatesVolume(new Vector3(width, height, thickness));
        }

        public void EnsureRoofEncapsulatesVolume(Vector3 dimensions)
        {
            BodyData.TotalWidth = Mathf.Max(BodyData.TotalWidth, dimensions.x + RoofData.DistanceFromSide * 2);
            BodyData.BodyLength = Mathf.Max(BodyData.BodyLength, dimensions.z + RoofData.DistanceFromBack + RoofData.DistanceFromFront);
        }

    }
}
