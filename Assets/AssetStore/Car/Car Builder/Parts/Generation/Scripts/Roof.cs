using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class Roof
    {
        public static RunTimeData Generate(Body.RunTimeData bodyData)
        {
            var roofData = CarGenerator.ActiveDataSet.RoofData;
           
            var body_frontTop = bodyData.FrontRight.Last();
            var body_backTop = bodyData.BackRight.Last();

            var heightOffset = Vector3.up * roofData.Height;
            var roof_backTop = body_backTop + heightOffset + new Vector3(-roofData.DistanceFromSide, 0, roofData.DistanceFromBack);
            var roof_frontTop = body_frontTop + heightOffset + new Vector3(-roofData.DistanceFromSide, 0, -roofData.DistanceFromFront);

            var roof = QuadGenerator_3D.Generate(
            new Vector3[] {
                roof_backTop.FlipXClone(),
                roof_frontTop.FlipXClone(),
                roof_frontTop,
                roof_backTop
            }, 
            Vector2Int.one, Vector3.up);
            
            CarGenerator.AddBodyTopPart(roof.OverrideUVs(roofData.ColorSettings.BodyUV, 0));


            var runtimeData = new RunTimeData();
            runtimeData.Bounds = new BoundsWrapper
            (
                Utility.GetMiddle(roof_frontTop.ReplaceXClone(0), roof_backTop.ReplaceXClone(0)),
                new Vector3(roof_frontTop.x * 2, 0, roof_frontTop.z - roof_backTop.z)
            );

            return runtimeData;
        }
        public struct RunTimeData
        {
            public BoundsWrapper Bounds;
        }


        [System.Serializable]
        public class ColorSettings
        {
            [SerializeField] [Range(0, 16)] private int _bodyID = 0;

            [System.NonSerialized] public Vector2 BodyUV = Vector2.zero;

            public void UpdateValues(Color[] carColors)
            {
                CarSettings.ColorSettings.GetColorUV(carColors, _bodyID, ref BodyUV);
            }

            public static void Blend(ColorSettings a, ColorSettings b, float progress, ref ColorSettings target)
            {
                progress = Mathf.Clamp01(progress);

                target.BodyUV = Utility.Vector2Lerp_HardSwitch(a.BodyUV, b.BodyUV, progress);
            }
        }

        [System.Serializable]
        public class Settings
        {
            [Range(0.2f, 1)] public float Height = 1f;
            [Space(8)]
            [Range(0,1)] public float DistanceFromFront = 0f;
            [Range(0,0.5f)] public float DistanceFromSide = 0f;
            [Range(0,1)] public float DistanceFromBack = 0f;
        }

        public class Data
        {
            public float Height;
            public float DistanceFromFront;
            public float DistanceFromSide;
            public float DistanceFromBack;

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.Height = settings.Height;
                data.DistanceFromFront = settings.DistanceFromFront;
                data.DistanceFromSide = settings.DistanceFromSide;
                data.DistanceFromBack = settings.DistanceFromBack;
              
                data.ColorSettings = colorSettings;

                return data;
            }


            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.Height = Mathf.Lerp(a.Height, b.Height, progress);
                dataBlend.DistanceFromFront = Mathf.Lerp(a.DistanceFromFront, b.DistanceFromFront, progress);
                dataBlend.DistanceFromSide = Mathf.Lerp(a.DistanceFromSide, b.DistanceFromSide, progress);
                dataBlend.DistanceFromBack = Mathf.Lerp(a.DistanceFromBack, b.DistanceFromBack, progress);

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }
    }
}
