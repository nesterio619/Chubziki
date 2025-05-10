using UnityEngine;
using SimpleMeshGenerator;
using System.Collections.Generic;

namespace ProceduralCarBuilder
{
    public class Nose
    {
        public static void Generate(Body.RunTimeData bodyData, Front.RuntimeData frontData)
        {
            var noseData = CarGenerator.ActiveDataSet.NoseData;
            var panelData = frontData.WheelPanel_RightSide;
            var leftSideBottom = bodyData.FrontRight[0];
            var maxBumperPos = panelData.SidePointsRight[panelData.SidePointsRight.Length - 2];// GetFrontSidePos(  leftSidePointAboveWheel, leftSideBottom, scaler);
            var maxBumperHeight = maxBumperPos.y - leftSideBottom.y;

            var freeArea = new Rect();
            var freeAreaZ = 0f;
            var ridgeEnabled = noseData.IsRidgeEnabled && noseData.RidgeShape != null;


            Vector2[] ridgePath = new Vector2[0];
            if (ridgeEnabled)
            {
                noseData.RidgeShape.GetPath(ref ridgePath);
                ridgeEnabled  = ridgeEnabled && ridgePath.IsNullOrEmpty() == false;
            }

          
            if (noseData.IsRidgeEnabled && ridgeEnabled == false)
            {
                Debug.LogWarning("Car Nose Ridge Deactivated as there is no Ridge Shape defined");
            }

            #region Bumper
            var bottomPoints = new Vector3[]
                {
                    panelData.SidePointsRight[0],
                    panelData.SidePointsRight[0].FlipXClone()
                };

            if (ridgeEnabled)
            {
                bottomPoints = new Vector3[]
                {
                    panelData.SidePointsRight[0] + Vector3.forward * noseData.RidgeLength,
                    panelData.SidePointsRight[0].FlipXClone() + Vector3.forward * noseData.RidgeLength
                };
            }

            var bumperdata = Bumper.Generate(CarGenerator.ActiveDataSet.BumperDataFront, bottomPoints, true, maxBumperHeight);
            #endregion


            if (ridgeEnabled)
            {
                var ridgePoints = new Vector3[panelData.SidePointsRight.Length * 2];
                for (int i = 0; i < panelData.SidePointsRight.Length; i++)
                {
                    ridgePoints[i] = panelData.SidePointsRight[i];
                    ridgePoints[ridgePoints.Length - 1 - i] = panelData.SidePointsRight[i].FlipXClone();
                }


                var hoodDirection = (frontData.WheelPanel_RightSide.TopPoints.Last() - frontData.WheelPanel_RightSide.TopPoints[0]).normalized;
                var hoodDirectionUp = Vector3.Cross(hoodDirection, Vector3.right).normalized;

                var meshDirs = new Vector3[ridgePoints.Length];
                var lengths = new float[ridgePoints.Length];
                for (int i = 0; i < meshDirs.Length; i++)
                {
                    var first = i == 0;
                    var last = i == meshDirs.Length - 1;

                    if (first || last)
                    {
                        meshDirs[i] = Vector3.forward;
                        lengths[i] = noseData.RidgeLength;
                    }
                    else
                    {
                        meshDirs[i] = hoodDirection;
                        lengths[i] = noseData.RidgeLength * (1f / hoodDirection.z);
                    }
                }

                var ridgePointsInner = new Vector3[0];
                var ridge = GeneralMeshGenerator.TryCreateRidge
                     (
                         ridgePoints,
                         ridgePath,
                         meshDirs,
                         noseData.RidgeWidth,
                         lengths,
                         Vector3.forward, Vector3.up,
                         true,
                         false,
                         CarGenerator.MeshGenerationHelper,
                         out ridgePointsInner
                     );

                if (ridge == null || ridgePointsInner == null)
                {
                    ridgeEnabled = false;
                }
                else
                {
                    CarGenerator.AddBodySidePart(ridge.OverrideUVs(noseData.ColorSettings.RidgeUV, 0));

                    var innerPointsRight = ridgePointsInner.GetRange(ridgePointsInner.Length / 2);
                    // fill area
                    for (int i = 0; i < innerPointsRight.Length - 1; i++)
                    {
                        var mesh = QuadGenerator_3D.Generate(
                        new Vector3[]
                        {
                        innerPointsRight[i + 1], innerPointsRight[i + 1].FlipXClone(),
                        innerPointsRight[i].FlipXClone(), innerPointsRight[i]
                        },
                        Vector2Int.one, Vector3.forward);

                        CarGenerator.AddBodySidePart(mesh.OverrideUVs(noseData.ColorSettings.BodyUV, 0));
                    }


                    var topInnerY = innerPointsRight.Last().y;
                    var topRidgeY = (innerPointsRight.Last() + hoodDirection.normalized * noseData.RidgeLength * (1f / hoodDirection.z)).y;
                    var bottomInnerY = innerPointsRight[0].y;
                    var bumperTop = bumperdata.Height + bumperdata.YPosOffSet + leftSideBottom.y;

                    var top = Mathf.Max(Mathf.Min(topInnerY, topRidgeY), bumperTop);
                    var bottom = Mathf.Max(bottomInnerY, bumperTop);
                    var height = top - bottom;

                    freeArea = new Rect(innerPointsRight.Last().x, bottom, innerPointsRight.Last().x * 2, height);
                    freeAreaZ = innerPointsRight[0].z;
                }
            }

            if(ridgeEnabled == false)
            {
                var points = new List<Vector3>();
                points.AddRange(panelData.SidePointsRight);
                points.AddRange(panelData.SidePointsRight.FlipXClone().ReversedClone());

                var mesh = GeneralMeshGenerator.CreateFan(points, points[0], noseData.ColorSettings.BodyUV, Vector3.forward);
                CarGenerator.AddBodySidePart(mesh);

                var top = panelData.SidePointsRight.Last().y;
                var bottom = bumperdata.Height + bumperdata.YPosOffSet + panelData.SidePointsRight[0].y;
                var height = top - bottom;
                freeArea = new Rect(panelData.SidePointsRight.Last().x, bottom, panelData.SidePointsRight.Last().x * 2, height);
                freeAreaZ = panelData.SidePointsRight[0].z;
            }

            if (freeArea.height > CarGenerator.MinimumDistance)
            {
                Lights.Generate(freeArea, freeAreaZ, true, bumperdata);
            }
        }




        [System.Serializable]
        public class ColorSettings
        {
            [SerializeField] [Range(0, 16)] private int _ridgeID = 0;
            [SerializeField] [Range(0, 16)] private int _bodyID = 0;
            [Space]
            [SerializeField] [Range(0, 16)] private int _grillPlateID = 0;
            [SerializeField] [Range(0, 16)] private int _grillSlitsID = 0;

            [System.NonSerialized] public Vector2 RidgeUV = Vector2.zero;
            [System.NonSerialized] public Vector2 BodyUV = Vector2.zero;

            [System.NonSerialized] public Vector2 GrillPlateUV = Vector2.zero;
            [System.NonSerialized] public Vector2 GrillSlitsUV = Vector2.zero;

            public void UpdateValues(Color[] carColors)
            {
                CarSettings.ColorSettings.GetColorUV(carColors, _ridgeID, ref RidgeUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _bodyID, ref BodyUV);

                CarSettings.ColorSettings.GetColorUV(carColors, _grillPlateID, ref GrillPlateUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _grillSlitsID, ref GrillSlitsUV);
            }

            public static void Blend(ColorSettings a, ColorSettings b, float progress, ref ColorSettings target)
            {
                progress = Mathf.Clamp01(progress);

                target.RidgeUV = Utility.Vector2Lerp_HardSwitch(a.RidgeUV, b.RidgeUV, progress);
                target.BodyUV = Utility.Vector2Lerp_HardSwitch(a.BodyUV, b.BodyUV, progress);

                target.GrillPlateUV = Utility.Vector2Lerp_HardSwitch(a.GrillPlateUV, b.GrillPlateUV, progress);
                target.GrillSlitsUV = Utility.Vector2Lerp_HardSwitch(a.GrillSlitsUV, b.GrillSlitsUV, progress);
            }
        }


        [System.Serializable]
        public class Settings
        {
            [Header("Nose")]
            public bool IsRidgeEnabled = true;
            public RidgeData[] RidgeShapes = default;
            [Range(0.01f, 0.1f)] public float RidgeWidth = 0.06f;
            [Range(0,0.4f)] public float RidgeLength = 0.06f;

            [Header("Grill")]
            public bool IsGrillEnabled = true;
            [Range(0, 10)] public int GrillGapCount = 5;
            [Range(0, 1)] public float GrillWidthPercentage = 1f;
            [Range(0, 1)] public float GrillHeightPercentage = 1f;
            [Range(0.01f, 0.12f)] public float GrillThickness = 0.03f;
        }

        public class Data
        {
            public bool IsRidgeEnabled;
            public RidgeData RidgeShape;
            public float RidgeWidth;
            public float RidgeLength;

            public bool IsGrillEnabled;
            public int GrillGapCount;
            public float GrillWidthPercentage;
            public float GrillHeightPercentage;
            public float GrillThickness;

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.RidgeShape = settings.RidgeShapes.IsNullOrEmpty() ? null : settings.RidgeShapes.GetRandomElement();
                data.IsRidgeEnabled = settings.IsRidgeEnabled;
                data.RidgeWidth = settings.RidgeWidth;
                data.RidgeLength = settings.RidgeLength;

                data.IsGrillEnabled = settings.IsGrillEnabled;
                data.GrillGapCount = settings.GrillGapCount;
                data.GrillWidthPercentage = settings.GrillWidthPercentage;
                data.GrillHeightPercentage = settings.GrillHeightPercentage;
                data.GrillThickness = settings.GrillThickness;

                data.ColorSettings = colorSettings;

                return data;
            }


            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.IsRidgeEnabled = Utility.BoolLerp(a.IsRidgeEnabled, b.IsRidgeEnabled, progress);
                dataBlend.RidgeShape = progress < 0.5f ? a.RidgeShape : b.RidgeShape;
                dataBlend.RidgeWidth = Mathf.Lerp(a.RidgeWidth, b.RidgeWidth, progress);
                dataBlend.RidgeLength = Mathf.Lerp(a.RidgeLength, b.RidgeLength, progress);

                dataBlend.IsGrillEnabled = Utility.BoolLerp(a.IsGrillEnabled, b.IsGrillEnabled, progress);
                dataBlend.GrillGapCount = Utility.IntLerp(a.GrillGapCount, b.GrillGapCount, progress);
                dataBlend.GrillWidthPercentage = Mathf.Lerp(a.GrillWidthPercentage, b.GrillWidthPercentage, progress);
                dataBlend.GrillHeightPercentage = Mathf.Lerp(a.GrillHeightPercentage, b.GrillHeightPercentage, progress);
                dataBlend.GrillThickness = Mathf.Lerp(a.GrillThickness, b.GrillThickness, progress);

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }





    }
}
