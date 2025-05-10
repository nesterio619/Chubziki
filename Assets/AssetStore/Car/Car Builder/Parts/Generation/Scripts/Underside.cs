using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class Underside
    {
        private static Mesh _targetMesh;
        private static Mesh _panelMesh;

        public static void Generate(Front.RuntimeData frontData, Back.RunTimeData backData)
        {
            if (_panelMesh == null)
            {
                _panelMesh = new Mesh();
                _targetMesh = new Mesh();
            }
            else
            {
                _panelMesh.Clear();
                _targetMesh.Clear();
            }

            var bodyData = CarGenerator.ActiveDataSet.BodyData;
            var innerFendersOnCar = frontData.WheelPanel_RightSide.WheelFenderPositionsBackward.Length > 0;

            if (innerFendersOnCar)
            {
                var middlePoint_Right = Vector3.Lerp(frontData.WheelPanel_RightSide.WheelFenderPositionsBackward[0], backData.WheelPanel_RightSide.WheelFenderPositionsForward[0], 0.5f);

                var rightPanel = _panelMesh;

                // front
                var panel = QuadGenerator_3D.Generate(new Vector3[]
                {
                    middlePoint_Right,
                    frontData.WheelPanel_RightSide.WheelFenderPositionsBackward[0],
                    frontData.WheelPanel_RightSide.WheelFenderPositionsBackward[1],
                    middlePoint_Right.ReplaceXClone(frontData.WheelPanel_RightSide.WheelFenderPositionsBackward[1].x)
                },
                Vector2Int.one, Vector3.down);
                CombineMeshes.Combine(rightPanel, panel);


                panel = QuadGenerator_3D.Generate(new Vector3[]
                {
                    frontData.WheelPanel_RightSide.WheelFenderPositionsForward[0],
                    frontData.WheelPanel_RightSide.WheelFenderPositionsForward[0].ReplaceZClone(frontData.zPosExtend),
                    frontData.WheelPanel_RightSide.WheelFenderPositionsForward[1].ReplaceZClone(frontData.zPosExtend),
                    frontData.WheelPanel_RightSide.WheelFenderPositionsForward[1]
                },
                Vector2Int.one, Vector3.down);
                CombineMeshes.Combine(rightPanel, panel);


                panel = QuadGenerator_3D.Generate(new Vector3[]
                {
                    middlePoint_Right.ReplaceXClone(frontData.WheelPanel_RightSide.WheelFenderPositionsBackward[1].x),
                    middlePoint_Right.ReplaceXClone(frontData.WheelPanel_RightSide.WheelFenderPositionsBackward[1].x).ReplaceZClone(frontData.zPosExtend),
                    middlePoint_Right.ReplaceXClone(0).ReplaceZClone(frontData.zPosExtend),
                    middlePoint_Right.ReplaceXClone(0),
                },
                Vector2Int.one, Vector3.down);
                CombineMeshes.Combine(rightPanel, panel);

                CombineMeshes.Combine(_targetMesh, rightPanel);
                CombineMeshes.Combine(_targetMesh, rightPanel.FlipOverX());


                // back
                rightPanel.Clear();

                panel = QuadGenerator_3D.Generate(new Vector3[]
                {
                    backData.WheelPanel_RightSide.WheelFenderPositionsBackward[0].ReplaceZClone(backData.zPosExtend),
                    backData.WheelPanel_RightSide.WheelFenderPositionsBackward[0],
                    backData.WheelPanel_RightSide.WheelFenderPositionsBackward[1],
                    backData.WheelPanel_RightSide.WheelFenderPositionsBackward[1].ReplaceZClone(backData.zPosExtend),
                },
                Vector2Int.one, Vector3.down);
                CombineMeshes.Combine(rightPanel, panel);


                panel = QuadGenerator_3D.Generate(new Vector3[]
                {
                    backData.WheelPanel_RightSide.WheelFenderPositionsForward[0],
                    backData.WheelPanel_RightSide.WheelFenderPositionsForward[0].ReplaceZClone(middlePoint_Right.z),
                    backData.WheelPanel_RightSide.WheelFenderPositionsForward[1].ReplaceZClone(middlePoint_Right.z),
                    backData.WheelPanel_RightSide.WheelFenderPositionsForward[1]
                },
                Vector2Int.one, Vector3.down);
                CombineMeshes.Combine(rightPanel, panel);


                panel = QuadGenerator_3D.Generate(new Vector3[]
                {
                    middlePoint_Right.ReplaceXClone(0).ReplaceZClone(backData.zPosExtend),
                    middlePoint_Right.ReplaceXClone(backData.WheelPanel_RightSide.WheelFenderPositionsForward[1].x).ReplaceZClone(backData.zPosExtend),
                    middlePoint_Right.ReplaceXClone(backData.WheelPanel_RightSide.WheelFenderPositionsForward[1].x),
                    middlePoint_Right.ReplaceXClone(0),
                },
                Vector2Int.one, Vector3.down);
                CombineMeshes.Combine(rightPanel, panel);

                CombineMeshes.Combine(_targetMesh, rightPanel);
                CombineMeshes.Combine(_targetMesh, rightPanel.FlipOverX());
            }
            else
            {
                var points = new Vector3[]
                {
                    backData.WheelPanel_RightSide.SidePointsLeft[0],
                    frontData.WheelPanel_RightSide.SidePointsRight[0],
                    frontData.WheelPanel_RightSide.SidePointsRight[0].FlipXClone(),
                    backData.WheelPanel_RightSide.SidePointsLeft[0].FlipXClone()
                };

                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(points, Vector2Int.one * 4, Vector3.down));
            }

            CarGenerator.AddBodySidePart(_targetMesh.OverrideUVs(bodyData.ColorSettings.UnderSideUV, 0));
        }
    }
}
