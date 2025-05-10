using System.Collections.Generic;
using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class WingMirror
    {
        private static Mesh _targetMesh;

        public static void Generate(Roof.RunTimeData roofData, Body.RunTimeData bodyData)
        {
            if (_targetMesh == null)
            {
                _targetMesh = new Mesh();
            }
            else
            {
                _targetMesh.Clear();
            }

            var mirrorData = CarGenerator.ActiveDataSet.WingMirrorData;
            var mirrorShapeData = mirrorData.ShapeData;

            if (mirrorData.IsEnabled == false || mirrorShapeData == null) return;


            var windowSpacing = CarGenerator.ActiveDataSet.WindowData.WindowSpacing;

            var directionBodyToRoof_Side = roofData.Bounds.Below_FrontRight - bodyData.FrontRight.Last();
            directionBodyToRoof_Side.z = 0;
            directionBodyToRoof_Side = directionBodyToRoof_Side.normalized;

            var directionBodyToRoof_Front = roofData.Bounds.Below_FrontRight - bodyData.FrontRight.Last();
            directionBodyToRoof_Front.x = 0;
            directionBodyToRoof_Front = directionBodyToRoof_Front.normalized;
       


			Vector2 size = new Vector2(mirrorData.SizeX, mirrorData.SizeY);
            var thickness = mirrorData.Thickness;
            var frontSideScale = mirrorData.FrontScaler;
            var armAngle = mirrorData.ArmAngle;
            var armThickness = mirrorData.ArmThickness;
            armThickness = Mathf.Min(thickness * 0.95f, windowSpacing * 0.95f, armThickness);

            



            //armThickness
            float armOffsetZ = directionBodyToRoof_Front.z * (windowSpacing / directionBodyToRoof_Front.y);
            armOffsetZ -= 0.01f + armThickness * 0.5f;

	        Vector2[] ridgePath = new Vector2[0];
	        mirrorShapeData.GetPath(ref ridgePath);

			var frontSide = new Vector3[ridgePath.Length];
            var backSide = new Vector3[ridgePath.Length];
            var scalerFront = new Vector3(mirrorData.FrontScaler * size.x, mirrorData.FrontScaler * size.y, 1);
            var scalerBack = new Vector3(size.x, size.y, 1);
            for (int i = 0; i < backSide.Length; i++)
            {
                backSide[i] = Vector3.Scale(ridgePath[i], scalerBack);
                backSide[i] += Vector3.back * thickness * 0.5f;

                frontSide[i] = Vector3.Scale(ridgePath[i], scalerFront);    
                frontSide[i] += Vector3.forward * thickness * 0.5f;
            }

            #region Housing
            CombineMeshes.Combine(_targetMesh, GeneralMeshGenerator.CreateBridgeHardEdged(frontSide, backSide, true, true, true));

            var a = new List<Vector3>(frontSide);
            CombineMeshes.Combine(_targetMesh, GeneralMeshGenerator.CreateFan(a, frontSide[0], mirrorData.ColorSettings.HousingUV, Vector3.forward, true, true));

    
            var bodyAttachmentPoint = bodyData.FrontRight.Last() + Vector3.forward * armOffsetZ;
            bodyAttachmentPoint += Vector3.back * mirrorData.AttachmentOffSet;
            bodyAttachmentPoint += directionBodyToRoof_Side * armThickness * 0.51f;

            var armLength = mirrorData.ArmLength;
            var offsetFromAttachment = Vector3.Scale(-mirrorShapeData.AttachmentPoint.ConvertToVec3(), size.ConvertToVec3());
            var armDirection = new Vector3(Mathf.Cos(armAngle * Mathf.Deg2Rad), Mathf.Sin(armAngle * Mathf.Deg2Rad), 0).normalized;
            var mirrorAttachmentPos = bodyAttachmentPoint + armDirection * armLength;
            _targetMesh.OverrideUVs(mirrorData.ColorSettings.HousingUV, 0).AddPositionOffset(mirrorAttachmentPos + offsetFromAttachment);

            CarGenerator.AddBodySidePart(_targetMesh);
            CarGenerator.AddBodySidePart(_targetMesh.FlipOverX());
            #endregion


            #region Arm
            _targetMesh.Clear();
            var armPositions = new Vector3[4];

            armPositions[0] = bodyAttachmentPoint + directionBodyToRoof_Side * armThickness * 0.5f * 0.75f;
            armPositions[1] = mirrorAttachmentPos + mirrorShapeData.AttachmentPointDirection.ConvertToVec3() * armThickness * 0.5f * 0.75f;

            armPositions[2] = mirrorAttachmentPos - mirrorShapeData.AttachmentPointDirection.ConvertToVec3() * armThickness * 0.5f * 0.75f;
            armPositions[3] = bodyAttachmentPoint - directionBodyToRoof_Side * armThickness * 0.5f * 0.75f;

            var armBackPositions = armPositions;
            var armFrontPositions = new Vector3[armPositions.Length];

            for (int i = 0; i < armBackPositions.Length; i++)
            {
                armBackPositions[i] += Vector3.forward * armThickness * 0.5f;
                armFrontPositions[i] = armBackPositions[i] + Vector3.back * armThickness;
            }

            CombineMeshes.Combine(_targetMesh, GeneralMeshGenerator.CreateBridgeHardEdged(armBackPositions, armFrontPositions, true, true, true));
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(armBackPositions, Vector2Int.one, Vector3.forward, true));
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(armFrontPositions, Vector2Int.one, Vector3.back));

            CarGenerator.AddBodySidePart(_targetMesh.OverrideUVs(mirrorData.ColorSettings.HousingUV, 0));
            CarGenerator.AddBodySidePart(_targetMesh.FlipOverX());
            #endregion

            for (int i = 0; i < backSide.Length; i++)
            {
                backSide[i] += mirrorAttachmentPos + offsetFromAttachment;
            }

            AddWindow(Window.GenerateWindow(GeneralMeshGenerator.Axis2D.XY, backSide, 0.005f, 0.004f, mirrorData.ColorSettings.HousingUV, false, true));
            AddWindow(Window.GenerateWindow(GeneralMeshGenerator.Axis2D.XY, backSide.FlipX().ReversedClone(), 0.005f, 0.004f, mirrorData.ColorSettings.HousingUV, false, true));

 
            void AddWindow(Window.WindowSet windowSet)
            {
                CarGenerator.AddBodySidePart(windowSet.Frame);
                CarGenerator.AddWindow(windowSet.Glass);
            }
        }

        [System.Serializable]
        public class ColorSettings
        {
            [SerializeField] [Range(0, 16)] private int _housingID = 0;

            [System.NonSerialized] public Vector2 HousingUV = Vector2.zero;

            public void UpdateValues(Color[] carColors)
            {
                CarSettings.ColorSettings.GetColorUV(carColors, _housingID, ref HousingUV);
            }

            public static void Blend(ColorSettings a, ColorSettings b, float progress, ref ColorSettings target)
            {
                progress = Mathf.Clamp01(progress);

                target.HousingUV = Utility.Vector2Lerp_HardSwitch(a.HousingUV, b.HousingUV, progress);
            }
        }

        [System.Serializable]
        public class Settings
        {
            public bool IsEnabled = true;
            public MirrorData[] Shapes = default;
            [Space(8)]
            [Range(0.05f,0.3f)] public float SizeX = 0.1f;
            [Range(0.05f,0.3f)] public float SizeY = 0.1f;
            [Range(0.01f, 0.2f)] public float Thickness = 0.15f;
            [Range(0,1)] public float FrontScaler = 1.25f;
            [Space(8)]
            [Range(0,0.5f)] public float ArmLength = 0.25f;
            [Range(0,0.1f)] public float ArmThickness = 0.15f;
            [Range(100,-100)] public float ArmAngle = 25f;
            [Range(-1.5f,3)] public float AttachmentOffSet = 0.15f;
        }

        public class Data
        {
			public bool IsEnabled;

            public float SizeX = 0;
            public float SizeY = 0;
            public float Thickness = 0;
            public float FrontScaler = 0;

            public float ArmLength = 0;
            public float ArmThickness = 0;
            public float ArmAngle = 0;
            public float AttachmentOffSet = 0;

            public MirrorData ShapeData;

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

				data.SizeX = settings.SizeX;
                data.SizeY =settings.SizeY;
                data.Thickness = settings.Thickness;
                data.FrontScaler = settings.FrontScaler;

                data.ArmLength = settings.ArmLength;
                data.ArmThickness = settings.ArmThickness;
                data.ArmAngle = settings.ArmAngle;
                data.AttachmentOffSet = settings.AttachmentOffSet;

                if (settings.Shapes.IsNullOrEmpty())
                {
                    data.ShapeData = null;
                    data.IsEnabled = false;
                }
                else
                {
                    data.ShapeData = settings.Shapes.GetRandomElement();
                    data.IsEnabled = settings.IsEnabled;
                }

                data.ColorSettings = colorSettings;

                return data;
            }


            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.SizeX = Mathf.Lerp(a.SizeX, b.SizeX, progress);
                dataBlend.SizeY = Mathf.Lerp(a.SizeY, b.SizeY, progress);
                dataBlend.Thickness = Mathf.Lerp(a.Thickness, b.Thickness, progress);
                dataBlend.FrontScaler = Mathf.Lerp(a.FrontScaler, b.FrontScaler, progress);

                dataBlend.ArmLength = Mathf.Lerp(a.ArmLength, b.ArmLength, progress);
                dataBlend.ArmThickness = Mathf.Lerp(a.ArmThickness, b.ArmThickness, progress);
                dataBlend.ArmAngle = Mathf.Lerp(a.ArmAngle, b.ArmAngle, progress);
                dataBlend.AttachmentOffSet = Mathf.Lerp(a.AttachmentOffSet, b.AttachmentOffSet, progress);

                dataBlend.ShapeData = progress < 0.5f ? a.ShapeData : b.ShapeData;

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }
    }
}
