using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class Wheels
    {
        public static int WheelAcrossResolution = 2;

        public static void Generate(Body.RunTimeData bodyData)
        {
            var hoodLength = CarGenerator.ActiveDataSet.BodyData.HoodLength;
            var wheelData = CarGenerator.ActiveDataSet.WheelData;

            var bottom = bodyData.FrontRight[0].y;
            var frontTop = bodyData.FrontRight[bodyData.FrontRight.Length - 1];
            var backTop = bodyData.BackRight[bodyData.BackRight.Length - 1];

            var frontBottom = bodyData.FrontRight[0];
            var backBottom = bodyData.BackRight[0];

            var backWheelsDepth = backBottom.z;

            var frontDepthMin = frontBottom.z + wheelData.TotalWheelAreaHalf();
            var frontDepthMax = frontBottom.z + hoodLength - wheelData.TotalWheelAreaHalf();

            var frontWheelsDepth = Mathf.Lerp(frontDepthMin, frontDepthMax, Mathf.Lerp(0.02f, 0.98f, wheelData.FrontPositionPercentage));

            var wheelSide = Mathf.Abs(frontBottom.x) + wheelData.OutwardsDistance - (wheelData.Width * 0.5f);

            var wheels = CarGenerator.CarInitializerInstance.Wheels;
            for (int i = 0; i < wheels.Length; i++)
            {
                var rightSide = i % 2 == 0;
                GenerateIndividualWheel(wheels[i].GetComponent<MeshFilter>(), wheelData, wheelData.Radius, wheelData.Width, rightSide);
            }

            wheels[0].transform.parent.position = new Vector3(wheelSide, wheelData.Radius, frontWheelsDepth);
            wheels[1].transform.parent.position = new Vector3(-wheelSide, wheelData.Radius, frontWheelsDepth);
            wheels[2].transform.parent.position = new Vector3(wheelSide, wheelData.Radius, backWheelsDepth);
            wheels[3].transform.parent.position = new Vector3(-wheelSide, wheelData.Radius, backWheelsDepth);

            CarGenerator.CarInitializerInstance.CarPartReferences.WheelRadius = wheelData.Radius;
            CarGenerator.CarInitializerInstance.CarPartReferences.WheelWidth = wheelData.Width;
        }

        private static void GenerateIndividualWheel(MeshFilter filter, Data wheelData, float radius, float tireThickness, bool rightSide)
        {
            if (wheelData.CapStyle == null)
            {
                Debug.LogWarning("Car Wheel Deactivated as there is no wheel Shape defined");
                return;
            }

            var capStyle = wheelData.CapStyle;
            var meshes = capStyle.Generate(radius, tireThickness, rightSide, wheelData.Resolution);

            CombineMeshes.Combine(filter.sharedMesh, meshes.Tire);
            CombineMeshes.Combine(filter.sharedMesh, meshes.Cap);
        }





        [System.Serializable]
        public class ColorSettings
        {
            [SerializeField] [Range(0, 16)] private int _tireID = 0;
            [SerializeField] [Range(0, 16)] private int _capID = 0;
            [SerializeField] [Range(0, 16)] private int _capCenterID = 0;
            [SerializeField] [Range(0, 16)] private int _fenderID = 0;

            [System.NonSerialized] public Vector2 TireUV = Vector2.zero;
            [System.NonSerialized] public Vector2 CapUV = Vector2.zero;
            [System.NonSerialized] public Vector2 CapCenterUV = Vector2.zero;
            [System.NonSerialized] public Vector2 FenderUV = Vector2.zero;

            public void UpdateValues(Color[] carColors)
            {
                CarSettings.ColorSettings.GetColorUV(carColors, _tireID, ref TireUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _capID, ref CapUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _capCenterID, ref CapCenterUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _fenderID, ref FenderUV);

            }

            public static void Blend(ColorSettings a, ColorSettings b, float progress, ref ColorSettings target)
            {
                progress = Mathf.Clamp01(progress);

                target.TireUV = Utility.Vector2Lerp_HardSwitch(a.TireUV, b.TireUV, progress);
                target.CapUV = Utility.Vector2Lerp_HardSwitch(a.CapUV, b.CapUV, progress);
                target.CapCenterUV = Utility.Vector2Lerp_HardSwitch(a.CapCenterUV, b.CapCenterUV, progress);
                target.FenderUV = Utility.Vector2Lerp_HardSwitch(a.FenderUV, b.FenderUV, progress);
            }
        }


        [System.Serializable]
        public class Settings
        {
            [Header("Shape")]
            public WheelScriptable[] WheelCapStyles = default;
            [Range(6, 24)] public int Resolution = 16;

            [Space(8)]
            [Range(0.2f,0.8f)] public float WheelRadius = 0.35f;
            [Range(0.1f, 0.5f)] public float WheelWidth = 0.35f;

            [Header("Placement")]
            [Range(0,1)] public float WheelFrontPositionPercent = 0.95f;
            [Range(-0.2f,0.3f)] public float WheelOutwardsOffset = 0.08f;

            [Header("Spacing")]
            [Range(0,2)] public float WheelFreeSpace = default;

            [Header("Fender")]
            public bool IsFenderEnabled = true;
            public RidgeData[] OuterFenderShapes = default;
            [Range(6,24)] public int OuterFenderResolution = 10;

            [Space(8)]
            [Range(0.01f,0.1f)] public float OuterFenderWidth = 0.06f;
            [Range(0.01f, 0.3f)] public float OuterFenderLength = 0.06f;
        }



        public class Data
        {
            public int Resolution;

            public float Radius;
            public float Width;

            public float FrontPositionPercentage;
            public float OutwardsDistance;

            public float FreeSpace;

            public bool IsFenderEnabled;
            public int OuterFenderResolution;
            public float OuterFenderWidth;
            public float OuterFenderLength;
  
            public WheelScriptable CapStyle;
            public RidgeData OuterFenderData;

            public float TotalWheelArea() { return TotalWheelAreaHalf() * 2; }
            public float TotalWheelAreaHalf() { return (Radius + FreeSpace + OuterFenderWidth); }

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.Resolution = settings.Resolution;

                data.Radius = settings.WheelRadius;
                data.Width = settings.WheelWidth;

                data.FrontPositionPercentage = settings.WheelFrontPositionPercent;
                data.OutwardsDistance = settings.WheelOutwardsOffset;

                data.FreeSpace = settings.WheelFreeSpace;

                data.IsFenderEnabled = settings.IsFenderEnabled;
                data.OuterFenderResolution = settings.OuterFenderResolution;
                data.OuterFenderWidth = settings.OuterFenderWidth;
                data.OuterFenderLength = settings.OuterFenderLength;

                data.CapStyle = settings.WheelCapStyles.IsNullOrEmpty() ? null : settings.WheelCapStyles.GetRandomElement();

                if (settings.OuterFenderShapes.IsNullOrEmpty())
                {
                    data.IsFenderEnabled = false;
                    data.OuterFenderData = null;
                }
                else
                {
                    data.OuterFenderData = settings.OuterFenderShapes.GetRandomElement();
                }

                if (data.CapStyle != null ) data.CapStyle.Initialize();

                data.ColorSettings = colorSettings;


                return data;
            }




            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.Resolution = Utility.IntLerp(a.Resolution, b.Resolution, progress);

                dataBlend.Radius = Mathf.Lerp(a.Radius, b.Radius, progress);
                dataBlend.Width = Mathf.Lerp(a.Width, b.Width, progress);

                dataBlend.FrontPositionPercentage = Mathf.Lerp(a.FrontPositionPercentage, b.FrontPositionPercentage, progress);
                dataBlend.OutwardsDistance = Mathf.Lerp(a.OutwardsDistance, b.OutwardsDistance, progress);

                dataBlend.FreeSpace = Mathf.Lerp(a.FreeSpace, b.FreeSpace, progress);

                dataBlend.IsFenderEnabled = Utility.BoolLerp(a.IsFenderEnabled, b.IsFenderEnabled, progress);
                dataBlend.OuterFenderResolution = Utility.IntLerp(a.OuterFenderResolution, b.OuterFenderResolution, progress);
                dataBlend.OuterFenderWidth = Mathf.Lerp(a.OuterFenderWidth, b.OuterFenderWidth, progress);
                dataBlend.OuterFenderLength = Mathf.Lerp(a.OuterFenderLength, b.OuterFenderLength, progress);

                dataBlend.CapStyle = progress < 0.5f ? a.CapStyle : b.CapStyle;
                dataBlend.OuterFenderData = progress < 0.5f ? a.OuterFenderData : b.OuterFenderData;

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }




    }
}
