using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class WindShield
    {
        public static void Generate(Roof.RunTimeData roofData, Body.RunTimeData bodyData, bool front = true)
        {
            var windowData = CarGenerator.ActiveDataSet.WindowData;
            var windowSpacing = CarGenerator.ActiveDataSet.WindowData.WindowSpacing;
            var windowIndentation = CarGenerator.ActiveDataSet.WindowData.WindowIndentation;

            if (front)
            {
                var points = new Vector3[] { roofData.Bounds.Below_FrontRight, roofData.Bounds.Below_FrontLeft, bodyData.FrontLeft.Last(), bodyData.FrontRight.Last() };

                var windowSet = Window.GenerateWindow(GeneralMeshGenerator.Axis2D.XY, points, windowSpacing, windowIndentation, windowData.ColorSettings.FrameUV, true, false);
                CarGenerator.AddBodyTopPart(windowSet.Frame);
                CarGenerator.AddWindow(windowSet.Glass);
            }
            else
            {
                var points = new Vector3[] { roofData.Bounds.Below_BackLeft, roofData.Bounds.Below_BackRight, bodyData.BackRight.Last(), bodyData.BackLeft.Last() };

                if (windowData.GenerateBackWindow)
                {       
                    var windowSet = Window.GenerateWindow(GeneralMeshGenerator.Axis2D.XY, points, windowSpacing, windowIndentation, windowData.ColorSettings.FrameUV, false, false);
                    CarGenerator.AddBodyTopPart(windowSet.Frame);
                    CarGenerator.AddWindow(windowSet.Glass);
                }
                else
                {
                    var normal = Utility.CalculateNormal(points[1] - points[0], points[3] - points[0]);
                    var mesh = QuadGenerator_3D.Generate(points, Vector2Int.one, normal).OverrideUVs(CarGenerator.ActiveDataSet.RoofData.ColorSettings.BodyUV, 0);
                    CarGenerator.AddBodyTopPart(mesh);
                }
            }
        }
    }
}
