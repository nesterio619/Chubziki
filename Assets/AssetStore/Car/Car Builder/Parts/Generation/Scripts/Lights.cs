using System.Collections.Generic;
using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class Lights
    {
        private enum LightBorder
        {
            Border,
            NoBorder
        }

        private enum LightShape
        {
            Circle,
            Rectangle,
            RectangleSlanted
        }

        public static void Generate(Rect worldSpaceArea, float worldZPos, bool front, Bumper.Data bumperData)
        {
            var shape = LightShape.Rectangle;
            var border = LightBorder.NoBorder;

            var noseData = CarGenerator.ActiveDataSet.NoseData;
            var lightsData = CarGenerator.ActiveDataSet.LightsData;

            var outerShape = new List<Vector3>();

            worldSpaceArea.min = new Vector2(worldSpaceArea.min.x, Mathf.Min(worldSpaceArea.max.y, worldSpaceArea.min.y + lightsData.YOffset));

            Vector2 size = new Vector2(lightsData.Width, lightsData.Height);
            size.x = Mathf.Min(size.x, worldSpaceArea.width * 0.45f);
            size.y = Mathf.Min(size.y, worldSpaceArea.height);

            if (size.x < CarGenerator.MinimumDistance || size.y < CarGenerator.MinimumDistance) return;



            var sideOffset = Vector3.left * size.x * 0.5f;
            var heightOffset = Vector3.down * size.y * 0.5f;

            Mesh lampMesh = null;
            switch (shape)
            {
                case LightShape.Circle:
                    var radius = Mathf.Min(size.x, size.y * 0.9f) * 0.5f;

                    sideOffset = (Vector3.left * radius * 0.5f);
                    outerShape = CircleGenerator.GetPoints(Mathf.Min(size.x, size.y * 0.9f) * 0.5f, Vector3.zero, 12);

                    lampMesh = CylinderGenerator.Generate(radius, lightsData.Thickness, 20, 2, true, GeneralMeshGenerator.Axis.Z);

                    break;

                case LightShape.Rectangle:

                    lampMesh = RectangleGenerator.Generate(new Vector3(size.x, size.y, lightsData.Thickness));

                    break;


                case LightShape.RectangleSlanted:

                    break;
            }


            var topRight = new Vector3(worldSpaceArea.x, worldSpaceArea.center.y + size.y * 0.5f, worldZPos);
            var rightLightPos = topRight + sideOffset + heightOffset + Vector3.forward * lightsData.Thickness * 0.5f;


            if (border == LightBorder.NoBorder)
            {
                lampMesh.OverrideUVs(lightsData.ColorSettings.BodyUV, 0);
                lampMesh.AddPositionOffset(rightLightPos);
                CarGenerator.AddLight(lampMesh, front, true, rightLightPos);

                var leftLightPos = rightLightPos.FlipX();
                lampMesh.AddPositionOffset(leftLightPos);
                CarGenerator.AddLight(lampMesh, front, false, leftLightPos);
            }
            else
            {

            }


            if (noseData.IsGrillEnabled)
            {
                Vector2 grillArea = new Vector2((rightLightPos.x - size.x * 0.5f) * 2, size.y);
                grillArea.x *= noseData.GrillWidthPercentage;
                grillArea.y *= noseData.GrillHeightPercentage;
                Vector2 grillRightBottom = new Vector2(-grillArea.x * 0.5f, rightLightPos.y - grillArea.y * 0.5f);

                var grillExtends = new Vector3(grillArea.x, grillArea.y, noseData.GrillThickness);
                var grillMesh = RectangleGenerator.Generate(grillExtends);
                grillMesh.OverrideUVs(noseData.ColorSettings.GrillPlateUV, 0);
                grillMesh.AddPositionOffset(new Vector3(0, grillRightBottom.y + grillArea.y * 0.5f, worldZPos + grillExtends.z * 0.5f));
                CarGenerator.AddBodySidePart(grillMesh);

                var lineCount = noseData.GrillGapCount;
                var stepTotal = lineCount + lineCount + 1;
                var height = grillArea.y / stepTotal;
                for (int i = 0; i < lineCount; i++)
                {
                    var lineThickness = grillExtends.z * 0.5f;
                    var line = RectangleGenerator.Generate(new Vector3(grillArea.x * 0.975f, height, lineThickness));
                    line.AddPositionOffset(Vector3.forward * (grillExtends.z + lineThickness * 0.5f));
                    line.OverrideUVs(noseData.ColorSettings.GrillSlitsUV, 0);
                    grillMesh.AddPositionOffset(new Vector3(0, grillRightBottom.y + (1 + i * 2) * height + height * 0.5f, worldZPos));
                    CarGenerator.AddBodySidePart(grillMesh);
                }
            }
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
            [Range(0, 1)] public float Width = 1f;
            [Range(0, 1)] public float Height = 0.05f;
            [Range(0.01f, 0.1f)] public float Thickness = 0.05f;

            [Header("Spacing")]
            [Range(-0.5f, 0.5f)] public float YOffset = 0.1f;
        }


        public class Data
        {
            public float Width;
            public float Height;
            public float Thickness;

            public float YOffset;

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.Width = settings.Width;
                data.Height = settings.Height;
                data.Thickness = settings.Thickness;

                data.YOffset = settings.YOffset;

                data.ColorSettings = colorSettings;

                return data;
            }


            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.Width = Mathf.Lerp(a.Width, b.Width, progress);
                dataBlend.Height = Mathf.Lerp(a.Height, b.Height, progress);
                dataBlend.Thickness = Mathf.Lerp(a.Thickness, b.Thickness, progress);

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }
    }
}
