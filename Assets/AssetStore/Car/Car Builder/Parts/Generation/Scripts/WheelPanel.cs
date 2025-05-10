using LinearAlgebra;
using System.Collections.Generic;
using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class WheelPanel
    {
        private static Mesh _targetMesh;

        public static PanelData Generate(Vector3[] leftSide, Vector3 endPointTopRight, Vector3 rightWheelPos, PanelColorUVs colorData)
        {
            if (_targetMesh == null)
            {
                _targetMesh = new Mesh();
            }
            else
            {
                _targetMesh.Clear();
            }
            var runtimeData = new PanelData();
            runtimeData.WheelTopPoint = Vector3.down;


            var mesh_RightPanel = _targetMesh;
            var bodyData = CarGenerator.ActiveDataSet.BodyData;
            var wheelData = CarGenerator.ActiveDataSet.WheelData;

            var fenderResolution = Mathf.Max(wheelData.OuterFenderResolution, 4);
            fenderResolution = fenderResolution.IsEven() ? fenderResolution : fenderResolution + 1;

            var leftSideTop = leftSide[leftSide.Length - 1];
            var leftSideBottom = leftSide[0];
            var rightSideBottom = new Vector3(endPointTopRight.x, leftSideBottom.y, endPointTopRight.z);
            var panelHeightRight = leftSideTop.y - leftSideBottom.y;
            var panelHeightLeft = endPointTopRight.y - leftSideBottom.y;


            var topPoints = new List<Vector3>();
            var sidePointsLeft = new List<Vector3>();
            var sidePointsRight = new List<Vector3>();
            var wheelFenderPositionsForward = new Vector3[0];
            var wheelFenderPositionsBackward = new Vector3[0];
            var panelPointsAroundWheel_Inwards = new List<Vector3>();


            bool frameIsAboveWheelArea = leftSideBottom.y > rightWheelPos.y + wheelData.Radius * 0.98f;
            if (frameIsAboveWheelArea == false)
            {
                var panelPointsAroundWheel = new List<Vector3>();
                var flatWheelTop = false;

                #region DefinePanelAroundWheel
                // wheelholder
                var circleCenter = new Vector3(leftSideBottom.x, rightWheelPos.y, rightWheelPos.z);
                var circlePoints = CircleGenerator.GetPoints(wheelData.TotalWheelAreaHalf(), circleCenter, fenderResolution, GeneralMeshGenerator.Axis2D.ZY, false, false);
                // these points will merge at the top / start

                // add top half
                for (int i = 0; i < (fenderResolution / 2) + 1; i++)
                {
                    panelPointsAroundWheel.Add(circlePoints[i]);
                }

                // add top middle point if needed for flat top
                flatWheelTop = panelPointsAroundWheel.Count % 2 == 0;
                if (flatWheelTop) // add point to be top middle
                {
                    var index = Mathf.CeilToInt(panelPointsAroundWheel.Count / 2f - 0.05f);
                    var pos = Vector3.Lerp(panelPointsAroundWheel[panelPointsAroundWheel.Count / 2], panelPointsAroundWheel[panelPointsAroundWheel.Count / 2 - 1], 0.5f);

                    panelPointsAroundWheel.Insert(index, pos);
                }


                DebugUtility.DrawPoints(panelPointsAroundWheel, Vector3.right, Color.yellow);

                var removedOutOfBodyPoints = false;
                var lastRemovedPoints = new List<Vector3>();
                for (int i = Mathf.FloorToInt(panelPointsAroundWheel.Count / 2); i > -1; i--)
                {
                    var pointIsUnderCarBody = panelPointsAroundWheel[i].y <= leftSideBottom.y;

                    if (pointIsUnderCarBody)
                    {
                        var oppositeSideindex = panelPointsAroundWheel.Count - 1 - i;

                        lastRemovedPoints.Clear();

                        lastRemovedPoints.Add(panelPointsAroundWheel[i]);
                        lastRemovedPoints.Add(panelPointsAroundWheel[oppositeSideindex]);

                        panelPointsAroundWheel.RemoveAt(oppositeSideindex); // remove higher value first to prevent incorrect index
                        panelPointsAroundWheel.RemoveAt(i);
                    }
                }

                removedOutOfBodyPoints = lastRemovedPoints.Count > 0;

                if (removedOutOfBodyPoints)
                {
                    // find replacement wheel positions
                    AssignReplacementWheelPosition(lastRemovedPoints[0], true);
                    AssignReplacementWheelPosition(lastRemovedPoints[1], false);

                    void AssignReplacementWheelPosition(Vector3 removedPos, bool forwards)
                    {
                        var connected = forwards ? panelPointsAroundWheel[0] : panelPointsAroundWheel[panelPointsAroundWheel.Count - 1];
                        var endCorner = forwards ? rightSideBottom : leftSideBottom;

                        var dirDownwards = (removedPos - connected);
                        dirDownwards.x = 0;
                        dirDownwards = dirDownwards.normalized;

                        var targetZ = 0f;
                        if (forwards)
                            targetZ = (rightWheelPos + Vector3.forward * wheelData.TotalWheelAreaHalf()).z;
                        else
                            targetZ = (rightWheelPos - Vector3.forward * wheelData.TotalWheelAreaHalf()).z;

                        var distanceZ = connected.z - targetZ;
                        var distanceY = connected.y - leftSideBottom.y;

                        var targetPos = connected + dirDownwards * Mathf.Abs(distanceZ / dirDownwards.z);
                        if (targetPos.y < leftSideBottom.y) // fix targetpos
                        {
                            targetPos = connected + dirDownwards * Mathf.Abs(distanceY / dirDownwards.y);
                        }


                        if ((forwards && targetPos.z > endCorner.z) || (forwards == false && targetPos.z < endCorner.z))
                        {
                            targetPos = endCorner;
                        }

                        if (forwards)
                            panelPointsAroundWheel.Insert(0, targetPos);
                        else
                            panelPointsAroundWheel.Add(targetPos);
                    }
                }

                if (Mathf.Approximately(panelPointsAroundWheel[0].y, leftSideBottom.y))
                {
                    // existing point are already at the bottom of the frame
                }
                else
                {
                    // ADD connection point at the bottom
                    panelPointsAroundWheel.Add(new Vector3(leftSideBottom.x, leftSideBottom.y, panelPointsAroundWheel[panelPointsAroundWheel.Count - 1].z)); // connection closest to body
                    panelPointsAroundWheel.Insert(0, new Vector3(leftSideBottom.x, leftSideBottom.y, panelPointsAroundWheel[0].z)); // connection closest to front
                }

                DebugUtility.DrawPoints(panelPointsAroundWheel, Vector3.right, Color.green);


                panelPointsAroundWheel_Inwards = new List<Vector3>(panelPointsAroundWheel);
                #endregion


                #region Generate WheelOuterFender
                if (wheelData.IsFenderEnabled)
                {
	                Vector2[] ridgePath = new Vector2[0];
	                wheelData.OuterFenderData.GetPath(ref ridgePath);

					var ridgeInnerPoints = new Vector3[0];
                    var ridge = GeneralMeshGenerator.TryCreateRidge
                    (
                        panelPointsAroundWheel.ToArray().ReversedClone(),
                        ridgePath,
                        Vector3.right,
                        wheelData.OuterFenderWidth,
                        wheelData.OuterFenderLength,
                        Vector3.right, Vector3.up,
                        false,
                        false,
                        CarGenerator.MeshGenerationHelper,
                        out ridgeInnerPoints
                    );

                    if(ridge != null) CombineMeshes.Combine(mesh_RightPanel, ridge.OverrideUVs(wheelData.ColorSettings.FenderUV, 0));
                }
                #endregion


                // create Panel
                sidePointsLeft = new List<Vector3>();
                sidePointsLeft.Add(leftSideBottom);

                var side = panelPointsAroundWheel.Count - Mathf.FloorToInt(panelPointsAroundWheel.Count / 2f);
                if (flatWheelTop) side -= 1;
                for (int i = 1; i < side; i++) // start from 1 as the first one already matches the bottom
                {
                    sidePointsLeft.Add(new Vector3(leftSideBottom.x, panelPointsAroundWheel[i].y, leftSideBottom.z));
                }

                // add all other side points
                for (int i = 1; i < leftSide.Length; i++)
                    sidePointsLeft.Add(leftSide[i]);


                sidePointsRight = new List<Vector3>();
                sidePointsRight.AddRange(sidePointsLeft);

                float rightSideScaler = panelHeightLeft / panelHeightRight;
                int forceConsistentYOffsetCount = 0;// bodyStyle == Body.BodyStyleTypes.Slanted ? 1 : 0;

                int counter = 0;
                for (int i = sidePointsRight.Count - 1; i > -1; i--)
                {
                    var pos = sidePointsRight[i];
                    pos.z = endPointTopRight.z;

                    if (i > sidePointsRight.Count - leftSide.Length) // only affect the upper body shape segments above the Wheel
                    {
                        if (counter > 0 && counter <= forceConsistentYOffsetCount)
                        {
                            var offSetFromTop = leftSideTop.y - pos.y;
                            pos.y = sidePointsRight[sidePointsRight.Count - 1].y - offSetFromTop;
                        }
                        else
                        {
                            pos.y = leftSideBottom.y + (pos.y - leftSideBottom.y) * rightSideScaler;
                        }
                    }

                    sidePointsRight[i] = pos;
                    counter++;
                }

                #region TopSideDeformation
                // temp experiment
                var a = sidePointsLeft.Last();
                var b = sidePointsLeft[sidePointsLeft.Count - 2];

                var forcedRatio = (a.y - b.y) / (b.x - a.x);
                if (Mathf.Approximately(forcedRatio, 0))
                {

                }
                else
                {
                    var aRight = sidePointsRight[sidePointsRight.Count - 1];
                    var bRight = sidePointsRight[sidePointsRight.Count - 2];

                    var xTarget = (aRight.y - bRight.y) / forcedRatio;

                    var posTemp = sidePointsRight[sidePointsRight.Count - 1];
                    posTemp.x = sidePointsRight[sidePointsRight.Count - 2].x - xTarget;
                    sidePointsRight[sidePointsRight.Count - 1] = posTemp;
                }
                #endregion



                //top points
                topPoints = new List<Vector3>();
                topPoints.Add(leftSideTop);

                var middleZPos = panelPointsAroundWheel[Mathf.FloorToInt(panelPointsAroundWheel.Count / 2f)].z;
                var zPercentage = (middleZPos - leftSideTop.z) / (endPointTopRight.z - leftSideTop.z);

                var middleYPos = Mathf.Lerp(leftSideTop.y, endPointTopRight.y, zPercentage);

                topPoints.Add(new Vector3(leftSideTop.x, middleYPos, middleZPos));
                topPoints.Add(new Vector3(leftSideTop.x, endPointTopRight.y, endPointTopRight.z));


                #region DefinePanelLeftRightNextToWheel
                // fill panel  Sides upwards untill Wheel Top
                var range = Mathf.FloorToInt(panelPointsAroundWheel.Count / 2f);
                if (flatWheelTop) range -= 1;
                for (int i = 0; i < range; i++)
                {
                    // towards body
                    CombineMeshes.Combine(mesh_RightPanel, TriangleGenerator.Generate(new Vector3[] { sidePointsLeft[i], panelPointsAroundWheel[panelPointsAroundWheel.Count - 2 - i], panelPointsAroundWheel[panelPointsAroundWheel.Count - 1 - i] }, colorData.BottomLeftUV, Vector3.right));
                    CombineMeshes.Combine(mesh_RightPanel, TriangleGenerator.Generate(new Vector3[] { sidePointsLeft[i], sidePointsLeft[i + 1], panelPointsAroundWheel[panelPointsAroundWheel.Count - 2 - i] }, colorData.BottomLeftUV, Vector3.right));

                    // towards front
                    CombineMeshes.Combine(mesh_RightPanel, TriangleGenerator.Generate(new Vector3[] { panelPointsAroundWheel[i], sidePointsRight[i + 1], sidePointsRight[i] }, colorData.BottomRightUV, Vector3.right));
                    CombineMeshes.Combine(mesh_RightPanel, TriangleGenerator.Generate(new Vector3[] { panelPointsAroundWheel[i], panelPointsAroundWheel[i + 1], sidePointsRight[i + 1] }, colorData.BottomRightUV, Vector3.right));
                }
                #endregion


                #region DefinePanelAboveWheel
                var wheelTopIndex = Mathf.FloorToInt(panelPointsAroundWheel.Count / 2f);
                runtimeData.WheelTopPoint = panelPointsAroundWheel[wheelTopIndex];

                var count = leftSide.Length;

                var leftPositions = sidePointsLeft.GetRange(sidePointsLeft.Count - count, count).ToArray();
                var rightPositions = sidePointsRight.GetRange(sidePointsRight.Count - count, count).ToArray();
                var middles = new List<List<Vector3>>();

                var positions = Utility.CreateDoubbleArray_Vector3(flatWheelTop ? 5 : 3, leftPositions.Length);

                for (int i = 1; i > -2; i--) // 1 to -1
                {
                    var wheelPoint = flatWheelTop ? panelPointsAroundWheel[wheelTopIndex + i] : panelPointsAroundWheel[wheelTopIndex];
                    var closestPoint = Line_Line_Intersection.GetClosestPointOnLineSegment(leftPositions[0].ZY(), rightPositions[0].ZY(), wheelPoint.ZY());
                    var zProgress = Vector2.Distance(leftPositions[0].ZY(), closestPoint) / Vector2.Distance(leftPositions[0].ZY(), rightPositions[0].ZY());

                    var middle = new List<Vector3>();
                    for (int j = 0; j < leftPositions.Length; j++)
                        middle.Add(Vector3.Lerp(leftPositions[j], rightPositions[j], zProgress));

                    middles.Add(middle);

                    if (flatWheelTop == false) break;
                }

                positions[0] = leftPositions;
                for (int i = 0; i < middles.Count; i++) positions[i + 1] = middles[i].ToArray();
                positions[positions.Length - 1] = rightPositions;

                var halfWayPoint = Mathf.CeilToInt(positions.Length / 2f);
                var leftHalfPostions = new Vector3[halfWayPoint][];
                var rightHalfPostions = new Vector3[halfWayPoint][];

                for(int i = 0; i < positions.Length; i++)
                {
                    if(i < halfWayPoint)
                    {
                        leftHalfPostions[i] = positions[i];
                    }

                    var rightSideStart = halfWayPoint - 1;
                    if (i >= rightSideStart)
                    {
                        rightHalfPostions[i - rightSideStart] = positions[i];
                    }
                }

                CombineMeshes.Combine(mesh_RightPanel, GeneralMeshGenerator.CreateMultiBridgeHardEdged(leftHalfPostions, false, true, true).OverrideUVs(colorData.TopLeftUV, 0));
                CombineMeshes.Combine(mesh_RightPanel, GeneralMeshGenerator.CreateMultiBridgeHardEdged(rightHalfPostions, false, true, true).OverrideUVs(colorData.TopRightUV, 0));
                #endregion



                #region DefinePanelsAroundWheelInward
                wheelFenderPositionsForward = new Vector3[2];
                wheelFenderPositionsBackward = new Vector3[2];

                var inwardsOffset = -Vector3.right * (wheelData.Width + wheelData.FreeSpace + (leftSideBottom.x - rightWheelPos.x));
                for (int i = 0; i < panelPointsAroundWheel_Inwards.Count; i++)
                {
                    panelPointsAroundWheel_Inwards[i] += inwardsOffset;

                    // prevent reaching outside of the body, incas ethe wheels are purposefully placed outside the frame
                    var pos = panelPointsAroundWheel_Inwards[i];
                    pos.x = Mathf.Min(pos.x, leftSideBottom.x);
                    panelPointsAroundWheel_Inwards[i] = pos;
                }



                for (int i = 0; i < panelPointsAroundWheel_Inwards.Count - 1; i++)
                {
                    var normal = Vector3.zero;
                    var first = i == 0;
                    var last = i == panelPointsAroundWheel_Inwards.Count - 2;
                    if (first)
                    {
                        normal = Vector3.back;
                    }
                    else if (last)
                    {
                        normal = Vector3.forward;
                    }
                    else
                    {
                        var dirToNext = panelPointsAroundWheel_Inwards[i + 1] - panelPointsAroundWheel_Inwards[i];
                        dirToNext.x = 0;
                        normal = new Vector3(0, dirToNext.z, -dirToNext.y).normalized;
                    }

                    CombineMeshes.Combine(mesh_RightPanel, TriangleGenerator.Generate(new Vector3[] { panelPointsAroundWheel[i], panelPointsAroundWheel_Inwards[i + 1], panelPointsAroundWheel[i + 1] }, bodyData.ColorSettings.WheelCompartmentUV, normal));
                    CombineMeshes.Combine(mesh_RightPanel, TriangleGenerator.Generate(new Vector3[] { panelPointsAroundWheel[i], panelPointsAroundWheel_Inwards[i], panelPointsAroundWheel_Inwards[i + 1] }, bodyData.ColorSettings.WheelCompartmentUV, normal));

                    //close behind wheel
                    var middlePoint = Vector3.Lerp(panelPointsAroundWheel_Inwards[0], panelPointsAroundWheel_Inwards[panelPointsAroundWheel_Inwards.Count - 1], 0.5f);
                    CombineMeshes.Combine(mesh_RightPanel, TriangleGenerator.Generate(new Vector3[] { middlePoint, panelPointsAroundWheel_Inwards[i + 1], panelPointsAroundWheel_Inwards[i] }, bodyData.ColorSettings.WheelCompartmentUV, Vector3.right));

                    if (first)
                    {
                        wheelFenderPositionsForward[0] = panelPointsAroundWheel[i];
                        wheelFenderPositionsForward[1] = panelPointsAroundWheel_Inwards[i];
                    }
                    else if (last)
                    {
                        wheelFenderPositionsBackward[0] = panelPointsAroundWheel[i + 1];
                        wheelFenderPositionsBackward[1] = panelPointsAroundWheel_Inwards[i + 1];
                    }
                }
                #endregion
            }
            else
            {
                topPoints = new List<Vector3>() { leftSideTop, endPointTopRight };
                sidePointsLeft = new List<Vector3>(leftSide);


                sidePointsRight = new List<Vector3>();
                sidePointsRight.AddRange(sidePointsLeft);
                for (int i = 0; i < sidePointsRight.Count; i++)
                {
                    var pos = sidePointsRight[i];
                    pos.z = endPointTopRight.z;

                    if (i > sidePointsRight.Count - leftSide.Length) // only affect the upper body shape segments
                        pos.y = leftSideBottom.y + (pos.y - leftSideBottom.y) * (panelHeightLeft / panelHeightRight);

                    sidePointsRight[i] = pos;
                }

                for (int i = 0; i < sidePointsLeft.Count - 1; i++)
                {
                    var upDir = (sidePointsRight[i + 1] - sidePointsLeft[i]).normalized;
                    var rightDir = (sidePointsRight[i] - sidePointsLeft[i]).normalized;

                    var normal = Vector3.Cross(upDir, rightDir).normalized;

                    CombineMeshes.Combine(mesh_RightPanel, QuadGenerator_3D.Generate(new Vector3[] { sidePointsLeft[i], sidePointsLeft[i + 1], sidePointsRight[i + 1], sidePointsRight[i] }, Vector2Int.one, normal));
                }

                mesh_RightPanel.OverrideUVs(colorData.BottomRightUV, 0);
            }



            var right = mesh_RightPanel;
            CarGenerator.AddBodySidePart(right);

            var left = mesh_RightPanel;
            left.FlipOverX();
            CarGenerator.AddBodySidePart(left);

            runtimeData.TopPoints = topPoints.ToArray();
            runtimeData.SidePointsRight = sidePointsRight.ToArray();
            runtimeData.SidePointsLeft = sidePointsLeft.ToArray();
            runtimeData.WheelFenderPositionsForward = wheelFenderPositionsForward;
            runtimeData.WheelFenderPositionsBackward = wheelFenderPositionsBackward;
            runtimeData.PointsAroundWheel_Inwards = panelPointsAroundWheel_Inwards.ToArray();
            return runtimeData;
        }

        public struct PanelColorUVs
        {
            public Vector2 TopLeftUV;
            public Vector2 TopRightUV;
            public Vector2 BottomLeftUV;
            public Vector2 BottomRightUV;

            public PanelColorUVs(Vector2 _topLeftUV, Vector2 _topRightUV, Vector2 _bottomLeftUV, Vector2 _bottomRightUV)
            {
                TopLeftUV = _topLeftUV;
                TopRightUV = _topRightUV;
                BottomLeftUV = _bottomLeftUV;
                BottomRightUV = _bottomRightUV;
            }
        }

        public struct PanelData
        {
            public Vector3[] TopPoints;
            public Vector3[] SidePointsRight;
            public Vector3[] SidePointsLeft;

            public Vector3[] WheelFenderPositionsForward;
            public Vector3[] WheelFenderPositionsBackward;

            public Vector3[] PointsAroundWheel_Inwards;

            public Vector3 WheelTopPoint;
        }
    }
}
