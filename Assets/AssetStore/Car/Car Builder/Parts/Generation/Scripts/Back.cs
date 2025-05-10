using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class Back
    {
        private static Mesh _trunkMesh_Outside;
        private static Mesh _trunkMesh_Inside;

        public enum StyleTypes
        {
            Regular,
            Truck,
            //StationWagon
        }


        public static RunTimeData Generate(Body.RunTimeData bodyData, Doors.RunTimeData doorsData)
        {
            if (_trunkMesh_Outside == null)
            {
                _trunkMesh_Outside = new Mesh();
                _trunkMesh_Inside = new Mesh();
            }
            else
            {
                _trunkMesh_Outside.Clear();
                _trunkMesh_Inside.Clear();
            }

            var length = CarGenerator.ActiveDataSet.BodyData.TrunkLength;
            var bodyStyle = CarGenerator.ActiveDataSet.BodyData.Style;
            var wheelData = CarGenerator.ActiveDataSet.WheelData;
            var backData = CarGenerator.ActiveDataSet.BackData;

            var rightWheel = CarGenerator.CarInitializerInstance.Wheels[2];
            var leftWheel = CarGenerator.CarInitializerInstance.Wheels[3];
            var wheelAreaTop = rightWheel.transform.position.y + wheelData.TotalWheelAreaHalf();
   

            var rightSide = new Vector3[bodyData.BackRight.Length];
            for (int i = 0; i < rightSide.Length; i++)
                rightSide[i] = bodyData.BackRight[i] - Vector3.forward * length;

            WheelPanel.PanelColorUVs wheelPanelColors = new WheelPanel.PanelColorUVs(backData.ColorSettings.Body_TopUV, backData.ColorSettings.Body_TopUV, backData.ColorSettings.Body_BottomUV, backData.ColorSettings.Body_BottomUV);
            var panelData = WheelPanel.Generate(rightSide, doorsData.BodyMiddlePoint, rightWheel.transform.position, wheelPanelColors);
            var isWheelPointValid = panelData.WheelTopPoint.y >= 0;


            var outerBounds = new BoundsWrapper(bodyData.BackLeft.Last(), Vector3.zero);
            outerBounds.Encapsulate(bodyData.BackLeft.Last());
            outerBounds.Encapsulate(bodyData.BackRight.Last());
            outerBounds.Encapsulate(bodyData.BackLeft.Last().ReplaceYClone(bodyData.BackLeft[0].y));
            outerBounds.Encapsulate(bodyData.BackLeft.Last() - Vector3.forward * length);

            var innerBoundsBottom = bodyData.Bottom + 0.001f;
            innerBoundsBottom = Mathf.Max(innerBoundsBottom, outerBounds.Ceiling - backData.TruckDepth);
            if (backData.KeepTruckBottomAboveWheel)
            {
                innerBoundsBottom = Mathf.Max(innerBoundsBottom, wheelAreaTop);
            }

            var prevYSize = outerBounds.Size.y;
            outerBounds.Size = new Vector3(outerBounds.Size.x, outerBounds.Ceiling - innerBoundsBottom, outerBounds.Size.z);
            outerBounds.Center += Vector3.up * (prevYSize * 0.5f - outerBounds.Size.y * 0.5f);

            var innerBounds = new BoundsWrapper();
            var bootOverhangBottom = 0f;

            switch (backData.Style)
            {
                default:
                case StyleTypes.Regular:

                    innerBounds = outerBounds;

                    var trunkDoorTop = QuadGenerator_3D.Generate(
                    new Vector3[] {
                            outerBounds.Above_BackLeft,
                            outerBounds.Above_FrontLeft,
                            outerBounds.Above_FrontRight,
                            outerBounds.Above_BackRight
                    },
                    Vector2Int.one, Vector3.up);
                    CombineMeshes.Combine(_trunkMesh_Outside, trunkDoorTop.OverrideUVs(backData.ColorSettings.DoorUV, 0));


                    trunkDoorTop = QuadGenerator_3D.Generate(
                    new Vector3[] {
                            outerBounds.Above_BackLeft,
                            outerBounds.Above_FrontLeft,
                            outerBounds.Above_FrontRight,
                            outerBounds.Above_BackRight
                    },
                    Vector2Int.one, Vector3.down, true);
                    CombineMeshes.Combine(_trunkMesh_Inside, trunkDoorTop.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0));


                    #region Trunk Boot
                    var bodyHeight = outerBounds.Ceiling - bodyData.Bottom;
                    var overhang = Mathf.Lerp(0, bodyHeight, backData.TrunkDoorHeightPercentage);
                    bootOverhangBottom = outerBounds.Ceiling - overhang;
                    bootOverhangBottom = Mathf.Max(bootOverhangBottom, innerBounds.Floor);

                    var trunkDoorOverhang = QuadGenerator_3D.Generate(
                    new Vector3[] {
                            outerBounds.Above_BackLeft.ReplaceYClone(bootOverhangBottom),
                            outerBounds.Above_BackLeft,
                            outerBounds.Above_BackRight,
                            outerBounds.Above_BackRight.ReplaceYClone(bootOverhangBottom)
                    },
                    Vector2Int.one, Vector3.back);
                    CombineMeshes.Combine(_trunkMesh_Outside, trunkDoorOverhang.OverrideUVs(backData.ColorSettings.DoorUV, 0));


                    trunkDoorOverhang = QuadGenerator_3D.Generate(
                    new Vector3[] {
                            outerBounds.Above_BackLeft.ReplaceYClone(bootOverhangBottom),
                            outerBounds.Above_BackLeft,
                            outerBounds.Above_BackRight,
                            outerBounds.Above_BackRight.ReplaceYClone(bootOverhangBottom)
                    },
                    Vector2Int.one, Vector3.forward, true);
                    CombineMeshes.Combine(_trunkMesh_Inside, trunkDoorOverhang.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0));


                    CarGenerator.AddTrunk(_trunkMesh_Outside, outerBounds.Above_FrontRight.ReplaceXClone(0), true);
                    CarGenerator.AddTrunk(_trunkMesh_Inside, outerBounds.Above_FrontRight.ReplaceXClone(0), false);
                    #endregion

                    #region Trunk Boot Remaining Panel
                    if (isWheelPointValid)
                    {
                        var backSide = QuadGenerator_3D.Generate(
                        new Vector3[] {
                            outerBounds.Below_BackLeft.ReplaceYClone(bodyData.Bottom),
                            outerBounds.Below_BackLeft.ReplaceYClone(Mathf.Min(panelData.WheelTopPoint.y, bootOverhangBottom)),
                            outerBounds.Below_BackRight.ReplaceYClone(Mathf.Min(panelData.WheelTopPoint.y, bootOverhangBottom)),
                            outerBounds.Below_BackRight.ReplaceYClone(bodyData.Bottom)
                        },
                        Vector2Int.one, Vector3.back);
                        CarGenerator.AddBodySidePart(backSide.OverrideUVs(backData.ColorSettings.Body_BottomUV, 0));
                        CarGenerator.AddBodySidePart(backSide.FlipOrientation(true).OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0));

                        if (bootOverhangBottom > panelData.WheelTopPoint.y)
                        {
                            backSide = QuadGenerator_3D.Generate(
                            new Vector3[] {
                                outerBounds.Below_BackLeft.ReplaceYClone(panelData.WheelTopPoint.y),
                                outerBounds.Below_BackLeft.ReplaceYClone(bootOverhangBottom),
                                outerBounds.Below_BackRight.ReplaceYClone(bootOverhangBottom),
                                outerBounds.Below_BackRight.ReplaceYClone(panelData.WheelTopPoint.y)
                            },
                            Vector2Int.one, Vector3.back);

                            CarGenerator.AddBodySidePart(backSide.OverrideUVs(backData.ColorSettings.Body_TopUV, 0));
                            CarGenerator.AddBodySidePart(backSide.FlipOrientation(true).OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0));
                        }
                    }
                    else
                    {
                        var backSide = QuadGenerator_3D.Generate(
                        new Vector3[] {
                            outerBounds.Below_BackLeft.ReplaceYClone(bodyData.Bottom),
                            outerBounds.Below_BackLeft.ReplaceYClone(bootOverhangBottom),
                            outerBounds.Below_BackRight.ReplaceYClone(bootOverhangBottom),
                            outerBounds.Below_BackRight.ReplaceYClone(bodyData.Bottom)
                        },
                        Vector2Int.one, Vector3.back);
                        CarGenerator.AddBodySidePart(backSide.OverrideUVs(backData.ColorSettings.Body_BottomUV, 0));
                    }
                    #endregion

                    break;
                case StyleTypes.Truck:

                    var wallThickness = backData.TrunkWallThickness;
                    wallThickness = Mathf.Min(wallThickness, CarGenerator.ActiveDataSet.BodyData.TotalWidth * 0.49f, CarGenerator.ActiveDataSet.BodyData.TrunkLength * 0.49f);
                    wallThickness = Mathf.Sqrt((wallThickness * wallThickness) + (wallThickness * wallThickness));

                    innerBounds.Center = outerBounds.Center;
                    innerBounds.Encapsulate(outerBounds.Above_FrontRight + new Vector3(-1, 0, -1).normalized * wallThickness);
                    innerBounds.Encapsulate(outerBounds.Below_BackLeft + new Vector3(1, 0, 1).normalized * wallThickness);

                    var outerPoints = new Vector3[] { outerBounds.Above_FrontLeft, outerBounds.Above_FrontRight, outerBounds.Above_BackRight, outerBounds.Above_BackLeft };
                    var upperWalls = QuadGenerator_3D.GenerateHollow(
                    outerPoints, 
                    new Vector3[] {
                        innerBounds.Above_FrontLeft,
                        innerBounds.Above_FrontRight,
                        innerBounds.Above_BackRight,
                        innerBounds.Above_BackLeft
                    },
                    Vector2Int.one, Vector3.up);
                    CarGenerator.AddBodySidePart(upperWalls.OverrideUVs(backData.ColorSettings.Body_BottomUV, 0));

                    //back side outer Body
                    var backSideTruck = QuadGenerator_3D.Generate(
                    new Vector3[] {
                        outerBounds.Below_BackLeft.ReplaceYClone(bodyData.Bottom),
                        outerBounds.Above_BackLeft,
                        outerBounds.Above_BackRight,
                        outerBounds.Below_BackRight.ReplaceYClone(bodyData.Bottom)
                    },
                    Vector2Int.one, Vector3.back);
                    CarGenerator.AddBodySidePart(backSideTruck.OverrideUVs(backData.ColorSettings.Body_BottomUV, 0));
  
                    // back plate inner
                    var frontPlate = QuadGenerator_3D.Generate(
                    new Vector3[] {
                        innerBounds.Below_BackLeft,
                        innerBounds.Above_BackLeft,
                        innerBounds.Above_BackRight,
                        innerBounds.Below_BackRight
                    },
                    Vector2Int.one,
                    Vector3.forward,
                    true);

                    CarGenerator.AddBodySidePart(frontPlate.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0));

                    break;
            }




            #region InnerWalls

            var createWheelCovers = false;
            var wheelAreaInwardsPoint_X = 0f;
            var wheelAreaHighestPoint_Y = 0f;
            var wheelAreaInwardsPoint_Z = 0f;

            if (panelData.PointsAroundWheel_Inwards.Length > 0)
            {
                wheelAreaInwardsPoint_X = panelData.PointsAroundWheel_Inwards[0].x;
                var middlePointIndex = (panelData.PointsAroundWheel_Inwards.Length - 1) / 2;
                wheelAreaHighestPoint_Y = wheelAreaTop;

                if (wheelAreaTop > innerBoundsBottom && wheelAreaInwardsPoint_X < innerBounds.Above_FrontRight.x && backData.KeepTruckBottomAboveWheel == false)
                {
                    createWheelCovers = true;
                    wheelAreaInwardsPoint_X = wheelAreaInwardsPoint_X - 0.01f;
                    wheelAreaHighestPoint_Y = Mathf.Min(wheelAreaHighestPoint_Y + 0.01f, innerBounds.Ceiling);
                    wheelAreaInwardsPoint_Z = Mathf.Max(panelData.PointsAroundWheel_Inwards.Last().z - 0.01f, innerBounds.Below_BackRight.z);
                }
            }

            if (createWheelCovers)
            {
                // side Wall
                var innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Above_FrontRight.ReplaceYClone(wheelAreaTop),
                        innerBounds.Above_FrontRight,
                        innerBounds.Above_BackRight.ReplaceZClone(wheelAreaInwardsPoint_Z),
                        innerBounds.Above_BackRight.ReplaceYClone(wheelAreaTop).ReplaceZClone(wheelAreaInwardsPoint_Z)
                },
                Vector2Int.one,
                Vector3.left,
                false);

                innerRight.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);
                CarGenerator.AddBodySidePart(innerRight.FlipOverX());

                // bottom plate Wall Sides
                innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Below_FrontRight.ReplaceZClone(wheelAreaInwardsPoint_Z),
                        innerBounds.Above_FrontRight.ReplaceZClone(wheelAreaInwardsPoint_Z),
                        innerBounds.Above_BackRight,
                        innerBounds.Below_BackRight
                },
                Vector2Int.one,
                Vector3.left,
                false);

                innerRight.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);
                CarGenerator.AddBodySidePart(innerRight.FlipOverX());

                // Wheel Wall point up
                innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Above_FrontRight.ReplaceYClone(wheelAreaHighestPoint_Y).ReplaceXClone(wheelAreaInwardsPoint_X),
                        innerBounds.Above_FrontRight.ReplaceYClone(wheelAreaHighestPoint_Y),
                        innerBounds.Above_BackRight.ReplaceYClone(wheelAreaHighestPoint_Y).ReplaceZClone(wheelAreaInwardsPoint_Z),
                        new Vector3(wheelAreaInwardsPoint_X, wheelAreaHighestPoint_Y, wheelAreaInwardsPoint_Z)
                },
                Vector2Int.one,
                Vector3.up,
                false);

                innerRight.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);
                CarGenerator.AddBodySidePart(innerRight.FlipOverX());

                // Wheel Wall side 
                innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Below_FrontRight.ReplaceXClone(wheelAreaInwardsPoint_X),
                        innerBounds.Below_FrontRight.ReplaceYClone(wheelAreaHighestPoint_Y).ReplaceXClone(wheelAreaInwardsPoint_X),
                        new Vector3(wheelAreaInwardsPoint_X, wheelAreaHighestPoint_Y, wheelAreaInwardsPoint_Z),
                        new Vector3(wheelAreaInwardsPoint_X, innerBounds.Floor, wheelAreaInwardsPoint_Z)
                },
                Vector2Int.one,
                Vector3.left,
                false);

                innerRight.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);
                CarGenerator.AddBodySidePart(innerRight.FlipOverX());


                // Wheel wall front side
                innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Below_BackRight.ReplaceZClone(wheelAreaInwardsPoint_Z),
                        innerBounds.Below_BackRight.ReplaceYClone(wheelAreaHighestPoint_Y).ReplaceZClone(wheelAreaInwardsPoint_Z),
                        new Vector3(wheelAreaInwardsPoint_X, wheelAreaHighestPoint_Y, wheelAreaInwardsPoint_Z),
                        new Vector3(wheelAreaInwardsPoint_X, innerBounds.Floor, wheelAreaInwardsPoint_Z)
                },
                Vector2Int.one,
                Vector3.back,
                true);

                innerRight.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);
                CarGenerator.AddBodySidePart(innerRight.FlipOverX());


                // bottom plate Wall 
                var bottomPlate = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Below_FrontRight.ReplaceXClone(wheelAreaInwardsPoint_X),
                        innerBounds.Below_BackRight.ReplaceXClone(wheelAreaInwardsPoint_X),
                        innerBounds.Below_BackRight.ReplaceXClone(-wheelAreaInwardsPoint_X),
                        innerBounds.Below_FrontRight.ReplaceXClone(-wheelAreaInwardsPoint_X)
                },
                Vector2Int.one,
                Vector3.up,
                false);

                bottomPlate.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(bottomPlate);

                // bottom plate Wall Sides
                bottomPlate = QuadGenerator_3D.Generate(
                new Vector3[] {
                        new Vector3(wheelAreaInwardsPoint_X, innerBounds.Floor, wheelAreaInwardsPoint_Z),
                        innerBounds.Below_FrontRight.ReplaceZClone(wheelAreaInwardsPoint_Z),
                        innerBounds.Below_BackRight,
                        innerBounds.Below_BackRight.ReplaceXClone(wheelAreaInwardsPoint_X)
                },
                Vector2Int.one,
                Vector3.up,
                false);

                bottomPlate.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(bottomPlate);
                CarGenerator.AddBodySidePart(bottomPlate.FlipOverX());

                // back plate top
                var backPlate = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Above_FrontLeft.ReplaceYClone(wheelAreaHighestPoint_Y),
                        innerBounds.Above_FrontLeft,
                        innerBounds.Above_FrontRight,
                        innerBounds.Above_FrontRight.ReplaceYClone(wheelAreaHighestPoint_Y)
                },
                Vector2Int.one,
                Vector3.back,
                false);

                CarGenerator.AddBodySidePart(backPlate.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0));


                // back plate bottom
                backPlate = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Below_FrontLeft.ReplaceXClone(-wheelAreaInwardsPoint_X),
                        innerBounds.Below_FrontLeft.ReplaceYClone(wheelAreaHighestPoint_Y).ReplaceXClone(-wheelAreaInwardsPoint_X),
                        innerBounds.Below_FrontRight.ReplaceYClone(wheelAreaHighestPoint_Y).ReplaceXClone(wheelAreaInwardsPoint_X),
                        innerBounds.Below_FrontRight.ReplaceXClone(wheelAreaInwardsPoint_X)
                },
                Vector2Int.one,
                Vector3.back,
                false);

                CarGenerator.AddBodySidePart(backPlate.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0));



                if (backData.Style == StyleTypes.Truck)
                {
                    // front plate top
                    var frontPlate = QuadGenerator_3D.Generate(
                    new Vector3[] {
                            innerBounds.Below_BackLeft,
                            innerBounds.Above_BackLeft,
                            innerBounds.Above_BackRight,
                            innerBounds.Below_BackRight
                    },
                    Vector2Int.one,
                    Vector3.forward,
                    true);

                    CarGenerator.AddBodySidePart(frontPlate.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0));
                }
            }
            else
            {
                // right Wall
                var innerRight = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Below_FrontRight,
                        innerBounds.Above_FrontRight,
                        innerBounds.Above_BackRight,
                        innerBounds.Below_BackRight
                },
                Vector2Int.one,
                Vector3.left,
                false);

                innerRight.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0);
                CarGenerator.AddBodySidePart(innerRight);
                CarGenerator.AddBodySidePart(innerRight.FlipOverX());


                // bottom plate
                var innerBottom = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Below_BackRight,
                        innerBounds.Below_FrontRight,
                        innerBounds.Below_FrontLeft,
                        innerBounds.Below_BackLeft
                },
                Vector2Int.one,
                Vector3.up,
                true);

                CarGenerator.AddBodySidePart(innerBottom.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0));

                // front plate
                var backPlate = QuadGenerator_3D.Generate(
                new Vector3[] {
                        innerBounds.Below_FrontLeft,
                        innerBounds.Above_FrontLeft,
                        innerBounds.Above_FrontRight,
                        innerBounds.Below_FrontRight
                },
                Vector2Int.one,
                Vector3.back,
                false);

                CarGenerator.AddBodySidePart(backPlate.OverrideUVs(backData.ColorSettings.TrunkFrameUV, 0));
            }
            #endregion


            #region BacksideFill
            if (bodyStyle == Body.StyleTypes.Slanted)
            {
                if (isWheelPointValid)
                {
                    //right
                    var surroundingPoints = new List<Vector3>();
                    surroundingPoints.AddRange(bodyData.BackRight.ReplaceZClone(outerBounds.Below_BackRight.z));
                    CarGenerator.AddBodySidePart(GeneralMeshGenerator.CreateFan(new List<Vector3>()
                    {
                        surroundingPoints[0],
                        surroundingPoints[0].ReplaceYClone(panelData.WheelTopPoint.y),
                        surroundingPoints[0].ReplaceYClone(panelData.WheelTopPoint.y).ReplaceXClone(surroundingPoints.Last().x),
                        surroundingPoints[0].ReplaceXClone(surroundingPoints.Last().x)

                    }
                    , surroundingPoints[0], backData.ColorSettings.Body_BottomUV, Vector3.back, false, true));


                    CarGenerator.AddBodySidePart(GeneralMeshGenerator.CreateFan(new List<Vector3>()
                    {
                        surroundingPoints[0].ReplaceYClone(panelData.WheelTopPoint.y),
                        surroundingPoints[1],
                        surroundingPoints[2],
                        surroundingPoints[2].ReplaceYClone(panelData.WheelTopPoint.y)
                    }
                    , surroundingPoints[1], backData.ColorSettings.Body_TopUV, Vector3.back, true, true));


                        surroundingPoints.Clear();
                        surroundingPoints.AddRange(bodyData.BackLeft.ReplaceZClone(outerBounds.Below_BackLeft.z));

                        CarGenerator.AddBodySidePart(GeneralMeshGenerator.CreateFan(new List<Vector3>()
                    {
                        surroundingPoints[0],
                        surroundingPoints[0].ReplaceYClone(panelData.WheelTopPoint.y),
                        surroundingPoints[0].ReplaceYClone(panelData.WheelTopPoint.y).ReplaceXClone(surroundingPoints.Last().x),
                        surroundingPoints[0].ReplaceXClone(surroundingPoints.Last().x)

                    }
                    , surroundingPoints[0], backData.ColorSettings.Body_BottomUV, Vector3.back, false));


                    CarGenerator.AddBodySidePart(GeneralMeshGenerator.CreateFan(new List<Vector3>()
                    {
                        surroundingPoints[0].ReplaceYClone(panelData.WheelTopPoint.y),
                        surroundingPoints[1],
                        surroundingPoints[2],
                        surroundingPoints[2].ReplaceYClone(panelData.WheelTopPoint.y)
                    }
                    , surroundingPoints[1], backData.ColorSettings.Body_TopUV, Vector3.back, true));
                }
                else
                {
                    var surroundingPoints = new List<Vector3>();
                    surroundingPoints.AddRange(bodyData.BackRight.ReplaceZClone(outerBounds.Below_BackRight.z));

                    var backPanel = GeneralMeshGenerator.CreateFan(surroundingPoints, surroundingPoints[0].ReplaceXClone(surroundingPoints.Last().x), backData.ColorSettings.Body_BottomUV, Vector3.back, false, true);

                    CarGenerator.AddBodySidePart(backPanel);
                    CarGenerator.AddBodySidePart(backPanel.FlipOverX());
                }
            }
            #endregion

            #region Bumper
            var maxBumperHeight = bodyData.BackLeft[1].y - bodyData.Bottom;
            if(backData.Style == StyleTypes.Regular) maxBumperHeight = Mathf.Clamp(maxBumperHeight, 0, bootOverhangBottom - bodyData.Bottom);

            var bottomPositions = new Vector3[] { bodyData.BackLeft[0] - Vector3.forward * length, bodyData.BackRight[0] - Vector3.forward * length };
            Bumper.Generate(CarGenerator.ActiveDataSet.BumperDataBack, bottomPositions, false, maxBumperHeight);
            #endregion

            var runtimeData = new RunTimeData();
            runtimeData.WheelPanel_RightSide = panelData;
            runtimeData.zPosExtend = bottomPositions[0].z;

            runtimeData.TrunkBounds = innerBounds;

            return runtimeData;
        }

        public struct RunTimeData
        {
            public WheelPanel.PanelData WheelPanel_RightSide;
            public float zPosExtend;
            public BoundsWrapper TrunkBounds;
        }



        [System.Serializable]
        public class ColorSettings
        {
            [SerializeField] [Range(0, 16)] private int _doorID = 0;
            [Header("Trunk")]
            [SerializeField] [Range(0, 16)] private int _trunkFrameID = 0;
            [Header("Body")]
            [SerializeField] [Range(0, 16)] private int _body_TopID = 0;
            [SerializeField] [Range(0, 16)] private int _body_BottomID = 0;

            [System.NonSerialized] public Vector2 DoorUV = Vector2.zero;

            [System.NonSerialized] public Vector2 TrunkFrameUV = Vector2.zero;

            [System.NonSerialized] public Vector2 Body_TopUV = Vector2.zero;
            [System.NonSerialized] public Vector2 Body_BottomUV = Vector2.zero;


            public void UpdateValues(Color[] carColors)
            {
                 CarSettings.ColorSettings.GetColorUV(carColors, _doorID, ref DoorUV);

                 CarSettings.ColorSettings.GetColorUV(carColors, _trunkFrameID, ref TrunkFrameUV);

                 CarSettings.ColorSettings.GetColorUV(carColors, _body_TopID, ref Body_TopUV);
                 CarSettings.ColorSettings.GetColorUV(carColors, _body_BottomID, ref Body_BottomUV);
            }

            public static void Blend(ColorSettings a, ColorSettings b, float progress, ref ColorSettings target)
            {
                progress = Mathf.Clamp01(progress);

                target.DoorUV = Utility.Vector2Lerp_HardSwitch(a.DoorUV, b.DoorUV, progress);

                target.TrunkFrameUV = Utility.Vector2Lerp_HardSwitch(a.TrunkFrameUV, b.TrunkFrameUV, progress);

                target.Body_TopUV = Utility.Vector2Lerp_HardSwitch(a.Body_TopUV, b.Body_TopUV, progress);
                target.Body_BottomUV = Utility.Vector2Lerp_HardSwitch(a.Body_BottomUV, b.Body_BottomUV, progress);
            }
        }



        [System.Serializable]
        public class Settings
        {
            public StyleTypes Style = StyleTypes.Regular;
            [Space(8)]
            public bool KeepTrunkBottomAboveWheel = false;
            [Space(8)]
            [Range(0,1)] public float TrunkDoorHeightPercentage = 0.75f;
            [Range(0.1f,2)] public float TrunkWallThickness = 0.2f;
            [Range(0, 1)] public float TrunkDepth = 0.2f;
        }


        public class Data
        {
            public StyleTypes Style;

            public float TrunkDoorHeightPercentage = 0;
            public float TrunkWallThickness = 0;
            public float TruckDepth = 0;
            public bool KeepTruckBottomAboveWheel = false;

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.Style = settings.Style;
                data.TrunkDoorHeightPercentage = settings.TrunkDoorHeightPercentage;
                data.TrunkWallThickness = settings.TrunkWallThickness;
                data.TruckDepth = settings.TrunkDepth;
                data.KeepTruckBottomAboveWheel = settings.KeepTrunkBottomAboveWheel;

                data.ColorSettings = colorSettings;

                return data;
            }


            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if(dataBlend == null) dataBlend = new Data();

                dataBlend.Style = progress < 0.5f ? a.Style : b.Style;

                dataBlend.TrunkDoorHeightPercentage = Mathf.Lerp(a.TrunkDoorHeightPercentage, b.TrunkDoorHeightPercentage, progress);
                dataBlend.TrunkWallThickness = Mathf.Lerp(a.TrunkWallThickness, b.TrunkWallThickness, progress);
                dataBlend.TruckDepth = Mathf.Lerp(a.TruckDepth, b.TruckDepth, progress);
                dataBlend.KeepTruckBottomAboveWheel = Utility.BoolLerp(a.KeepTruckBottomAboveWheel, b.KeepTruckBottomAboveWheel, progress);

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }
    }
}
