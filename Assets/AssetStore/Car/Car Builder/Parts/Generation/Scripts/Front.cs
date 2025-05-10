using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleMeshGenerator;
using System.Linq;

namespace ProceduralCarBuilder
{
    public class Front
    {
        private static Mesh _hoodMesh_Outside;
        private static Mesh _hoodMesh_Inside;

        public static RuntimeData Generate(Body.RunTimeData bodyData, float hoodDropPercentage)
        {
            if (_hoodMesh_Outside == null)
            {
                _hoodMesh_Outside = new Mesh();
                _hoodMesh_Inside = new Mesh();
            }
            else
            {
                _hoodMesh_Outside.Clear();
                _hoodMesh_Inside.Clear();
            }

            var frontData = CarGenerator.ActiveDataSet.FrontData;
            var hoodLength = CarGenerator.ActiveDataSet.BodyData.HoodLength;
            var bodyStyle = CarGenerator.ActiveDataSet.BodyData.Style;
            var wheelData = CarGenerator.ActiveDataSet.WheelData;

            var lidSpacing = frontData.HoodlidSpaceFromEdges;
            var side_TopLeft = bodyData.FrontRight.Last();
            var side_BottomLeft = bodyData.FrontRight[0];
            var side_PointAboveWheelLeft = bodyData.FrontRight[1];

            var rightWheel = CarGenerator.CarInitializerInstance.Wheels[0];
            var leftWheel = CarGenerator.CarInitializerInstance.Wheels[1];


            // ensure front doesnt lower to much, needs to stay above heighest point of the wheel
            var height = side_PointAboveWheelLeft.y - side_BottomLeft.y;
            var side_PointAboveWheelRight = side_PointAboveWheelLeft + Vector3.forward * hoodLength;
            side_PointAboveWheelRight.y = side_BottomLeft.y;
            side_PointAboveWheelRight += Vector3.up * (height * hoodDropPercentage);


            var minHeight = rightWheel.transform.position.y + wheelData.TotalWheelAreaHalf() * 0.98f;
            side_PointAboveWheelRight.y = Mathf.Max(minHeight, side_PointAboveWheelRight.y);

            float scaler = (side_PointAboveWheelRight.y - side_BottomLeft.y) / height;
            scaler = 1 - scaler;

            // calculate end point
            var side_TopRight = GetFrontSidePos(side_TopLeft, side_BottomLeft, scaler);

            WheelPanel.PanelColorUVs wheelPanelColors = new WheelPanel.PanelColorUVs(frontData.ColorSettings.Body_TopLeftUV, frontData.ColorSettings.Body_TopRightUV, frontData.ColorSettings.Body_BottomLeftUV, frontData.ColorSettings.Body_BottomRightUV);
            var panelData = WheelPanel.Generate(bodyData.FrontRight, side_TopRight, rightWheel.transform.position, wheelPanelColors);

            // top lid
            var hoodSide = new List<Vector3>();
            hoodSide.AddRange(panelData.TopPoints);
            var hoodTop = new List<Vector3>() { side_TopLeft, side_TopLeft.FlipXClone() };

            var lidSide = new List<Vector3>();
            lidSide.AddRange(hoodSide);
            var lidTop = new List<Vector3>();
            lidTop.AddRange(hoodTop);


            var dirBodyToOutwards = (side_TopRight - side_TopLeft).normalized;

            // move points inwards
            for (int i = 0; i < lidSide.Count; i++)
            {
                var progress = (float)i / (lidSide.Count - 1);

                lidSide[i] += dirBodyToOutwards * lidSpacing * Mathf.Lerp(1, -1, progress);
                lidSide[i] += -Vector3.right * lidSpacing;
            }

            for (int i = 0; i < lidTop.Count; i++)
            {
                var progress = (float)i / (lidTop.Count - 1);

                lidTop[i] += Vector3.right * lidSpacing * Mathf.Lerp(-1, 1, progress);
                lidTop[i] += dirBodyToOutwards * lidSpacing;
            }


            DebugUtility.DrawLine(lidTop[lidTop.Count - 1], lidTop[lidTop.Count - 1] + Vector3.up, Color.green, 10);
            DebugUtility.DrawLine(lidTop[0], lidTop[0] + Vector3.up, Color.green, 10);


            var tangent = (lidSide[0] - lidSide[lidSide.Count - 1]).normalized;
            var normal = Vector3.Cross(Vector3.right, tangent).normalized;

            #region HoodMesh
            float aspectRatio = Vector3.Distance(lidTop[0], lidTop.Last()) / Vector3.Distance(lidSide[0], lidSide.Last());

            var hood = QuadGenerator_3D.Generate(lidTop, lidSide, normal);
            hood.OverrideUVs(frontData.ColorSettings.HoodUV, 0);
            hood.BoxUVChannel_XZ(
                new Vector2(lidSide.Last().x, lidSide.Last().z),
                new Vector2(lidTop.Last().x, lidTop.Last().z),
                aspectRatio,
                2);
            CombineMeshes.Combine(_hoodMesh_Outside, hood);

            hood = QuadGenerator_3D.Generate(lidTop, lidSide, normal, GeneralMeshGenerator.NormalsCalculationType.Off, true, true);
            hood.OverrideUVs(frontData.ColorSettings.TrunkFrameUV, 0);
            CombineMeshes.Combine(_hoodMesh_Inside, hood);

            CarGenerator.AddHood(_hoodMesh_Outside, Vector3.Lerp(lidTop[0], lidTop.Last(), 0.5f), true);
            CarGenerator.AddHood(_hoodMesh_Inside, Vector3.Lerp(lidTop[0], lidTop.Last(), 0.5f), false);
            #endregion


            #region HoodFrame
            var inner = new Vector3[]
            {
                lidTop.Last(),
                lidSide.Last().FlipXClone(),
                lidSide.Last(),
                lidTop[0]
            };

            var outer = new Vector3[]
            {
                panelData.TopPoints[0].FlipXClone(),
                panelData.SidePointsRight.Last().FlipXClone(),
                panelData.SidePointsRight.Last(),
                panelData.TopPoints[0]
            };

            var hoodFrame = GeneralMeshGenerator.CreateBridgeSoftEdged(inner, outer, true); 
            CarGenerator.AddBodyTopPart(hoodFrame.OverrideUVs(frontData.ColorSettings.HoodFrameUV, 0));
            #endregion


            #region Trunk
            var bottom = Mathf.Max(lidSide.Last().y - frontData.TrunkDepth, side_BottomLeft.y);

            var createWheelCovers = false;
            var wheelAreaHighestPoint_Y = 0f;
            var wheelAreaInwardsPoint_X = 0f;

            if (panelData.PointsAroundWheel_Inwards.Length > 0)
            {
                wheelAreaInwardsPoint_X = panelData.PointsAroundWheel_Inwards[0].x;
                var middlePointIndex = (panelData.PointsAroundWheel_Inwards.Length - 1) / 2;
                wheelAreaHighestPoint_Y = panelData.PointsAroundWheel_Inwards[middlePointIndex].y;

                if (wheelAreaHighestPoint_Y > bottom && wheelAreaInwardsPoint_X < lidSide[0].x)
                {
                    createWheelCovers = true;
                    wheelAreaHighestPoint_Y = Mathf.Min(wheelAreaHighestPoint_Y + CarGenerator.MinimumDistance / 2, lidSide.Last().y);
                    wheelAreaInwardsPoint_X = wheelAreaInwardsPoint_X - CarGenerator.MinimumDistance;
                }
            }


            if (createWheelCovers)
            {
                // side Wall
                var innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                            lidSide[0].ReplaceYClone(wheelAreaHighestPoint_Y),
                            lidSide[0],
                            lidSide.Last(),
                            lidSide.Last().ReplaceYClone(wheelAreaHighestPoint_Y)
                },
                Vector2Int.one,
                Vector3.left,
                true);

                innerRight.OverrideUVs(frontData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);
                CarGenerator.AddBodySidePart(innerRight.FlipOverX());


                // side Wall point up
                innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                            lidSide[0].ReplaceYClone(wheelAreaHighestPoint_Y).ReplaceXClone(wheelAreaInwardsPoint_X),
                            lidSide[0].ReplaceYClone(wheelAreaHighestPoint_Y),
                            lidSide.Last().ReplaceYClone(wheelAreaHighestPoint_Y),
                            lidSide.Last().ReplaceYClone(wheelAreaHighestPoint_Y).ReplaceXClone(wheelAreaInwardsPoint_X)
                },
                Vector2Int.one,
                Vector3.up,
                true);

                innerRight.OverrideUVs(frontData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);
                CarGenerator.AddBodySidePart(innerRight.FlipOverX());

                // side Wall 
                innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                            lidSide[0].ReplaceYClone(bottom).ReplaceXClone(wheelAreaInwardsPoint_X),
                            lidSide[0].ReplaceYClone(wheelAreaHighestPoint_Y).ReplaceXClone(wheelAreaInwardsPoint_X),
                            lidSide.Last().ReplaceYClone(wheelAreaHighestPoint_Y).ReplaceXClone(wheelAreaInwardsPoint_X),
                            lidSide.Last().ReplaceYClone(bottom).ReplaceXClone(wheelAreaInwardsPoint_X)
                },
                Vector2Int.one,
                Vector3.left,
                true);

                innerRight.OverrideUVs(frontData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);
                CarGenerator.AddBodySidePart(innerRight.FlipOverX());

                // bottom plate Wall 
                innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                            lidSide[0].ReplaceYClone(bottom).ReplaceXClone(wheelAreaInwardsPoint_X),
                            lidSide.Last().ReplaceYClone(bottom).ReplaceXClone(wheelAreaInwardsPoint_X),
                            lidSide.Last().ReplaceYClone(bottom).ReplaceXClone(-wheelAreaInwardsPoint_X),
                            lidSide[0].ReplaceYClone(bottom).ReplaceXClone(-wheelAreaInwardsPoint_X)
                },
                Vector2Int.one,
                Vector3.up,
                true);

                innerRight.OverrideUVs(frontData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);

            }
            else
            {
                // right Wall
                var innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                            lidSide[0].ReplaceYClone(bottom),
                            lidSide[0],
                            lidSide.Last(),
                            lidSide.Last().ReplaceYClone(bottom)
                },
                Vector2Int.one,
                Vector3.left,
                true);

                innerRight.OverrideUVs(frontData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);
                CarGenerator.AddBodySidePart(innerRight.FlipOverX());


                // bottom plate
                var innerBottom = QuadGenerator_3D.Generate(
                new Vector3[] 
                {
                    lidSide.Last().ReplaceYClone(bottom),
                    lidSide[0].ReplaceYClone(bottom),
                    lidSide[0].ReplaceYClone(bottom).FlipXClone(),
                    lidSide.Last().ReplaceYClone(bottom).FlipXClone()
                },
                Vector2Int.one,
                Vector3.up);

                CarGenerator.AddBodySidePart(innerBottom.OverrideUVs(frontData.ColorSettings.TrunkFrameUV, 0));
            }

            var innerFront = QuadGenerator_3D.Generate(
                new Vector3[] 
                {
                    lidSide.Last().FlipXClone().ReplaceYClone(bottom),
                    lidSide.Last().FlipXClone(),
                    lidSide.Last(),
                    lidSide.Last().ReplaceYClone(bottom)
                },
                Vector2Int.one,
                Vector3.back);

            CarGenerator.AddBodySidePart(innerFront.OverrideUVs(frontData.ColorSettings.TrunkFrameUV, 0));


            var innerBack = QuadGenerator_3D.Generate(
               new Vector3[] 
               {
                   lidSide[0].FlipXClone().ReplaceYClone(bottom),
                   lidSide[0].FlipXClone(),
                   lidSide[0],
                   lidSide[0].ReplaceYClone(bottom)
               },
               Vector2Int.one,
               Vector3.forward,
               true);

            CarGenerator.AddBodySidePart(innerBack.OverrideUVs(frontData.ColorSettings.TrunkFrameUV, 0));
            #endregion

            var runtimeData = new RuntimeData();
            runtimeData.WheelPanel_RightSide = panelData;
            runtimeData.zPosExtend = panelData.SidePointsRight[0].z;

            runtimeData.OrnamentBounds = new BoundsWrapper();
            runtimeData.OrnamentBounds.Size = new Vector3(panelData.SidePointsRight.Last().x * 2, 0, Vector3.Distance(lidSide.Last().ReplaceXClone(0), panelData.SidePointsRight.Last().ReplaceXClone(0)));
            runtimeData.OrnamentBounds.Center = Utility.GetMiddle(lidSide.Last().ReplaceXClone(0), panelData.SidePointsRight.Last().ReplaceXClone(0));

            var forward = Vector3.Normalize(panelData.SidePointsRight.Last().ReplaceXClone(0) - lidSide.Last().ReplaceXClone(0));
            runtimeData.OrnamentBounds.SetDirections(forward, Vector3.Cross(forward, Vector3.right));


            runtimeData.TrunkBounds = new BoundsWrapper();
            runtimeData.TrunkBounds.Size = new Vector3(lidSide[0].x * 2, lidSide.Last().y - bottom, lidSide.Last().z - lidSide[0].z);
            runtimeData.TrunkBounds.Center = new Vector3(0, Utility.GetMiddle(lidSide.Last().y,  bottom), Utility.GetMiddle(lidSide.Last().z, lidSide[0].z));

            Nose.Generate(bodyData, runtimeData);
            return runtimeData;
        }

        private static Vector3 GetFrontSidePos(Vector3 bodyPoint, Vector3 bodyPointBottom, float scaler)
        {
            var height = bodyPoint.y - bodyPointBottom.y;
            var endPos = bodyPoint + Vector3.forward * CarGenerator.ActiveDataSet.BodyData.HoodLength;
            endPos -= Vector3.up * (height * scaler);

            return endPos;
        }

        public struct RuntimeData
        {
            public WheelPanel.PanelData WheelPanel_RightSide;
            public float zPosExtend;
            public BoundsWrapper OrnamentBounds;
            public BoundsWrapper TrunkBounds;
        }



        [System.Serializable]
        public class ColorSettings
        {
            [SerializeField] [Range(0, 16)] private int _hoodID = 0;
            [SerializeField] [Range(0, 16)] private int _hoodFrameID = 0;
            [Header("Trunk")]
            [SerializeField] [Range(0, 16)] private int _trunkFrameID = 0;
            [Header("Bodyside")]
            [SerializeField] [Range(0, 16)] private int _body_TopLeftID = 0;
            [SerializeField] [Range(0, 16)] private int _body_TopRightID = 0;
            [Space]
            [SerializeField] [Range(0, 16)] private int _body_BottomLeftID = 0;
            [SerializeField] [Range(0, 16)] private int _body_BottomRightID = 0;

            [System.NonSerialized] public Vector2 HoodUV = Vector2.zero;
            [System.NonSerialized] public Vector2 HoodFrameUV = Vector2.zero;

            [System.NonSerialized] public Vector2 TrunkFrameUV = Vector2.zero;

            [System.NonSerialized] public Vector2 Body_TopLeftUV = Vector2.zero;
            [System.NonSerialized] public Vector2 Body_TopRightUV = Vector2.zero;

            [System.NonSerialized] public Vector2 Body_BottomLeftUV = Vector2.zero;
            [System.NonSerialized] public Vector2 Body_BottomRightUV = Vector2.zero;

            public void UpdateValues(Color[] carColors)
            {
                CarSettings.ColorSettings.GetColorUV(carColors, _hoodID, ref HoodUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _hoodFrameID, ref HoodFrameUV);

                CarSettings.ColorSettings.GetColorUV(carColors, _trunkFrameID, ref TrunkFrameUV);

                CarSettings.ColorSettings.GetColorUV(carColors, _body_TopLeftID, ref Body_TopLeftUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _body_TopRightID, ref Body_TopRightUV);

                CarSettings.ColorSettings.GetColorUV(carColors, _body_BottomLeftID, ref Body_BottomLeftUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _body_BottomRightID, ref Body_BottomRightUV);
            }

            public static void Blend(ColorSettings a, ColorSettings b, float progress, ref ColorSettings target)
            {
                progress = Mathf.Clamp01(progress);

                target.HoodUV = Utility.Vector2Lerp_HardSwitch(a.HoodUV, b.HoodUV, progress);
                target.HoodFrameUV = Utility.Vector2Lerp_HardSwitch(a.HoodFrameUV, b.HoodFrameUV, progress);

                target.TrunkFrameUV = Utility.Vector2Lerp_HardSwitch(a.TrunkFrameUV, b.TrunkFrameUV, progress);

                target.Body_TopLeftUV = Utility.Vector2Lerp_HardSwitch(a.Body_TopLeftUV, b.Body_TopLeftUV, progress);
                target.Body_TopRightUV = Utility.Vector2Lerp_HardSwitch(a.Body_TopRightUV, b.Body_TopRightUV, progress);

                target.Body_BottomLeftUV = Utility.Vector2Lerp_HardSwitch(a.Body_BottomLeftUV, b.Body_BottomLeftUV, progress);
                target.Body_BottomRightUV = Utility.Vector2Lerp_HardSwitch(a.Body_BottomRightUV, b.Body_BottomRightUV, progress);
            }
        }


        [System.Serializable]
        public class Settings
        {
            [Range(0.01f,0.5f)] public float HoodlidSpaceFromEdges = 0.05f;
            [Header("Trunk")]
            public bool KeepTrunkBottomAboveWheel = false;
            [Range(0.1f, 1f)] public float TrunkDepth = 0.2f;
        }

        public class Data
        {
            public float HoodlidSpaceFromEdges;
            public bool KeepTrunkBottomAboveWheel;
            public float TrunkDepth;

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.HoodlidSpaceFromEdges = settings.HoodlidSpaceFromEdges;
                data.KeepTrunkBottomAboveWheel = settings.KeepTrunkBottomAboveWheel;
                data.TrunkDepth = settings.TrunkDepth;

                data.ColorSettings = colorSettings;

                return data;
            }


            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.HoodlidSpaceFromEdges = Mathf.Lerp(a.HoodlidSpaceFromEdges, b.HoodlidSpaceFromEdges, progress);

                dataBlend.KeepTrunkBottomAboveWheel = Utility.BoolLerp(a.KeepTrunkBottomAboveWheel, b.KeepTrunkBottomAboveWheel, progress);
                dataBlend.TrunkDepth = Mathf.Lerp(a.TrunkDepth, b.TrunkDepth, progress);

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }




    }
}
