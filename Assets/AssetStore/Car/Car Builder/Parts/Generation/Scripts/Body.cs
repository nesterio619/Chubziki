using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class Body
    {
        public enum StyleTypes
        {
            Straight,
            Slanted
        }

        public static RunTimeData Generate(CarData cardata)
        {
            var bodyData = cardata.BodyData;

            var interiorBounds = new Bounds();
            interiorBounds.Encapsulate(Vector3.right * bodyData.TotalWidth * 0.5f);
            interiorBounds.Encapsulate(-Vector3.right * bodyData.TotalWidth * 0.5f);
            interiorBounds.Encapsulate(Vector3.forward * bodyData.BodyLength * (1 - cardata.PivotPercentage));
            interiorBounds.Encapsulate(Vector3.back * bodyData.BodyLength * cardata.PivotPercentage);
            interiorBounds.Encapsulate(Vector3.up * bodyData.BodyHeight);

            var offsetFromGround = Vector3.up * bodyData.HeightOfTheGround;
            interiorBounds.center += offsetFromGround;

            var frontRight = new Vector3[0];
            var backRight = new Vector3[0];

            switch (bodyData.Style)
            {
                case StyleTypes.Slanted:
                    var sidewaysOffset = bodyData.SlantedShapeSidewaysOffset;
                    var middleHeight = Mathf.Lerp(interiorBounds.min.y, interiorBounds.max.y, bodyData.SlantedShapeHeightPercentage);

                    frontRight = new Vector3[3];
                    frontRight[0] = new Vector3(interiorBounds.max.x + sidewaysOffset, interiorBounds.min.y, interiorBounds.max.z);
                    frontRight[1] = new Vector3(interiorBounds.max.x + sidewaysOffset, middleHeight, interiorBounds.max.z);
                    frontRight[2] = interiorBounds.max;

                    backRight = new Vector3[frontRight.Length];
                    backRight[0] = frontRight[0].ReplaceZClone(interiorBounds.min.z);
                    backRight[1] = frontRight[1].ReplaceZClone(interiorBounds.min.z);
                    backRight[2] = frontRight[2].ReplaceZClone(interiorBounds.min.z);
                    break;

                case StyleTypes.Straight:
                    frontRight = new Vector3[2];
                    frontRight[0] = new Vector3(interiorBounds.max.x, interiorBounds.min.y, interiorBounds.max.z);
                    frontRight[1] = interiorBounds.max;

                    backRight = new Vector3[frontRight.Length];
                    backRight[0] = frontRight[0].ReplaceZClone(interiorBounds.min.z);
                    backRight[1] = frontRight[1].ReplaceZClone(interiorBounds.min.z);
                    break;
            }

            var runtimeData = new RunTimeData();
            runtimeData.FrontRight = frontRight;
            runtimeData.FrontLeft = frontRight.FlipXClone();
            runtimeData.BackRight = backRight;
            runtimeData.BackLeft = backRight.FlipXClone();
            runtimeData.Bottom = runtimeData.FrontLeft[0].y;
            runtimeData.Height = runtimeData.FrontLeft.Last().y - runtimeData.Bottom;

            return runtimeData;
        }

        public struct RunTimeData
        {
            public Vector3[] FrontRight;
            public Vector3[] FrontLeft;

            public Vector3[] BackRight;
            public Vector3[] BackLeft;

            public float Bottom;
            public float Height;
        }



        [System.Serializable]
        public class ColorSettings
        {
            [SerializeField] [Range(0, 16)] private int _underSideID = 0;
            [SerializeField] [Range(0, 16)] private int _wheelCompartmentID = 0;

            [System.NonSerialized] public Vector2 UnderSideUV = Vector2.zero;
            [System.NonSerialized] public Vector2 WheelCompartmentUV = Vector2.zero;

            public void UpdateValues(Color[] carColors)
            {
                CarSettings.ColorSettings.GetColorUV(carColors, _underSideID, ref UnderSideUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _wheelCompartmentID, ref WheelCompartmentUV);
            }

            public static void Blend(ColorSettings a, ColorSettings b, float progress, ref ColorSettings target)
            {
                progress = Mathf.Clamp01(progress);

                target.UnderSideUV = Utility.Vector2Lerp_HardSwitch(a.UnderSideUV, b.UnderSideUV, progress);
                target.WheelCompartmentUV = Utility.Vector2Lerp_HardSwitch(a.WheelCompartmentUV, b.WheelCompartmentUV, progress);
            }
        }

        [System.Serializable]
        public class Settings
        {
            public StyleTypes Style = default;

            [Space(8)]
            [Range(0.5f,2.5f)] public float TotalWidth = 2f;
            [Range(0.15f,0.8f)] public float HeightOfTheGround = 0.15f;

            [Space(8)]
            [Range(0.5f,4)] public float BodyLength = 2f;
            [Range(0.25f,1.3f)] public float BodyHeight = 1f;
            [Range(0.05f,0.5f)] public float SlantedShapeSidewaysOffset = 0.25f;
            [Range(0,1f)]public float SlantedShapeHeightStartPercentage = 0.9f;

            [Space(8)]
            [Range(0.1f, 1.5f)] public float TrunkLength = 1.5f;

            [Space(8)]
            [Range(0.3f, 1.5f)] public float HoodLength = 2f;
            [Range(0.05f, 1)] public float HoodHeightPercentage = 1f;
        }


        public class Data
        {
            public StyleTypes Style = StyleTypes.Slanted;

            public float TotalWidth = 0;
            public float HeightOfTheGround = 0;
            public float BodyLength = 0;
            public float BodyHeight = 0;

            public float SlantedShapeSidewaysOffset = 0;
            public float SlantedShapeHeightPercentage = 0;
            public float TrunkLength = 0;
            public float HoodLength = 0;
            public float HoodHeightPercentage = 1;

            public ColorSettings ColorSettings = new ColorSettings();


            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.Style = settings.Style;

                data.TotalWidth = settings.TotalWidth;
                data.HeightOfTheGround = settings.HeightOfTheGround;
                data.BodyLength = settings.BodyLength;
                data.BodyHeight = settings.BodyHeight;

                data.SlantedShapeSidewaysOffset = settings.SlantedShapeSidewaysOffset;
                data.SlantedShapeHeightPercentage = settings.SlantedShapeHeightStartPercentage;
                data.TrunkLength = settings.TrunkLength;
                data.HoodLength = settings.HoodLength;
                data.HoodHeightPercentage = settings.HoodHeightPercentage;

                data.ColorSettings = colorSettings;

                return data;
            }

            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.Style = progress < 0.5f ? a.Style : b.Style;

                dataBlend.TotalWidth = Mathf.Lerp(a.TotalWidth, b.TotalWidth, progress);
                dataBlend.HeightOfTheGround = Mathf.Lerp(a.HeightOfTheGround, b.HeightOfTheGround, progress);
                dataBlend.BodyLength = Mathf.Lerp(a.BodyLength, b.BodyLength, progress);
                dataBlend.BodyHeight = Mathf.Lerp(a.BodyHeight, b.BodyHeight, progress);

                dataBlend.SlantedShapeSidewaysOffset = Mathf.Lerp(a.SlantedShapeSidewaysOffset, b.SlantedShapeSidewaysOffset, progress);
                dataBlend.SlantedShapeHeightPercentage = Mathf.Lerp(a.SlantedShapeHeightPercentage, b.SlantedShapeHeightPercentage, progress);
                dataBlend.TrunkLength = Mathf.Lerp(a.TrunkLength, b.TrunkLength, progress);
                dataBlend.HoodLength = Mathf.Lerp(a.HoodLength, b.HoodLength, progress);
                dataBlend.HoodHeightPercentage = Mathf.Lerp(a.HoodHeightPercentage, b.HoodHeightPercentage, progress);

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }
    }
}
