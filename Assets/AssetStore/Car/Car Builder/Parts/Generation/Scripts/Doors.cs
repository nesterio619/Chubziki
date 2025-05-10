using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class Doors
    {
        private enum WindowStyle
        {
            NoWindows,
            Single,
            Single_BackClosed,
            Double
        }

        public static RunTimeData Generate(Roof.RunTimeData roofData, Body.RunTimeData bodyData, float wheelTopPointY)
        {
            var windowData = CarGenerator.ActiveDataSet.WindowData;
            var doorData = CarGenerator.ActiveDataSet.DoorData;
            var doorWidth = Mathf.Min(doorData.Width, CarGenerator.ActiveDataSet.BodyData.BodyLength - CarGenerator.ActiveDataSet.WheelData.TotalWheelAreaHalf() - CarGenerator.MinimumDistance);
            var minimalDoorWidthPossible = bodyData.FrontRight.Last().z - roofData.Bounds.Floor;

            var seperationPointZ = bodyData.FrontRight.Last().z - doorWidth;
            var roofBackIsOverExtending = roofData.Bounds.Below_BackRight.z >= seperationPointZ - CarGenerator.MinimumDistance;
            var doorPanelIsSeperated = doorWidth > minimalDoorWidthPossible + CarGenerator.MinimumDistance;

            #region Panel

            if (wheelTopPointY > 0)
            {
                var bottomPanel = QuadGenerator_3D.Generate(new Vector3[] 
                {
                    bodyData.FrontRight[0],
                    bodyData.FrontRight[0].ReplaceYClone(wheelTopPointY),
                    bodyData.FrontRight[0].ReplaceYClone(wheelTopPointY).ReplaceZClone(seperationPointZ),
                    bodyData.FrontRight[0].ReplaceZClone(seperationPointZ)
                }, Vector2Int.one, Vector3.right, true).OverrideUVs(doorData.ColorSettings.Body_BottomUV, 0);
                CarGenerator.AddBodySidePart(bottomPanel);
                CarGenerator.AddBodySidePart(bottomPanel.FlipOverX());

                var bodyPoints = new Vector3[bodyData.FrontRight.Length];
                System.Array.Copy(bodyData.FrontRight, bodyPoints, bodyData.FrontRight.Length);
                bodyPoints[0].y = wheelTopPointY;

                var doorPanel = GeneralMeshGenerator.CreateBridgeHardEdged(bodyPoints, bodyPoints.ReplaceZClone(seperationPointZ));
                doorPanel.OverrideUVs(doorData.ColorSettings.Body_TopUV, 0);
                CarGenerator.AddBodySidePart(doorPanel);
                CarGenerator.AddBodySidePart(doorPanel.FlipOverX());
            }
            else
            {
                var doorPanel = GeneralMeshGenerator.CreateBridgeHardEdged(bodyData.FrontRight, bodyData.FrontRight.ReplaceZClone(seperationPointZ));
                doorPanel.OverrideUVs(doorData.ColorSettings.Body_TopUV, 0);
                CarGenerator.AddBodySidePart(doorPanel);
                CarGenerator.AddBodySidePart(doorPanel.FlipOverX());
            }
            #endregion


            #region Window
            WindowStyle windowStyle = WindowStyle.Double;
            if (doorPanelIsSeperated)
            {
                if (roofBackIsOverExtending)
                {
                    windowStyle = WindowStyle.Single;
                }
                else
                { 
                    if (windowData.GenerateSideBackWindows)
                    {
                        windowStyle = WindowStyle.Double;
                    }
                    else
                    {
                        windowStyle = WindowStyle.Single_BackClosed;
                    }
                }          
            }
            else
            {
                windowStyle = WindowStyle.Single; 
            }


            var points = new Vector3[4];
            var normal = Vector3.zero;
            Window.WindowSet windowSet = new Window.WindowSet();
            switch (windowStyle)
            {
                case WindowStyle.NoWindows:

                    points = new Vector3[] { roofData.Bounds.Below_BackRight, roofData.Bounds.Below_FrontRight, bodyData.FrontRight.Last(), bodyData.BackRight.Last() };

                    normal = Utility.CalculateNormal(points[0], points[1], points[0], points.Last());
                    var window = QuadGenerator_3D.Generate(points, Vector2Int.one, normal).OverrideUVs(windowData.ColorSettings.FrameUV, 0);
                    CarGenerator.AddBodySidePart(window);
                    CarGenerator.AddBodySidePart(window.FlipOverX());

                    break;

                case WindowStyle.Single:

                    points = new Vector3[] { roofData.Bounds.Below_BackRight, roofData.Bounds.Below_FrontRight, bodyData.FrontRight.Last(), bodyData.BackRight.Last() };

                    windowSet = Window.GenerateWindow(GeneralMeshGenerator.Axis2D.ZY, points, windowData.WindowSpacing, windowData.WindowIndentation, windowData.ColorSettings.FrameUV, false, false);
                    AddWindow();
                    AddWindowFlippedOverX();

                    windowSet = Window.GenerateWindow(GeneralMeshGenerator.Axis2D.ZY, points, windowData.WindowSpacing, windowData.WindowIndentation, windowData.ColorSettings.FrameUV, false, false);
                    AddWindow();
                    AddWindowFlippedOverX();
                    break;

                case WindowStyle.Single_BackClosed:

                    points = new Vector3[] {
                        roofData.Bounds.Below_BackRight.ReplaceZClone(seperationPointZ),
                        roofData.Bounds.Below_FrontRight,
                        bodyData.FrontRight.Last(),
                        bodyData.FrontRight.Last().ReplaceZClone(seperationPointZ)
                    };
                    windowSet = Window.GenerateWindow(GeneralMeshGenerator.Axis2D.ZY, points, windowData.WindowSpacing, windowData.WindowIndentation, windowData.ColorSettings.FrameUV, false, false);
                    AddWindow();
                    AddWindowFlippedOverX();

                    points = new Vector3[] {
                        roofData.Bounds.Below_BackRight,
                        roofData.Bounds.Below_BackRight.ReplaceZClone(seperationPointZ),
                        bodyData.BackRight.Last().ReplaceZClone(seperationPointZ),
                        bodyData.BackRight.Last()
                    };
                    normal = Utility.CalculateNormal(points[0], points[1], points[0], points[points.Length - 1]);
                    var backPanel = QuadGenerator_3D.Generate(points, Vector2Int.one, normal).OverrideUVs(windowData.ColorSettings.FrameUV, 0);
                    CarGenerator.AddBodySidePart(backPanel);
                    CarGenerator.AddBodySidePart(backPanel.FlipOverX());

                    break;

                case WindowStyle.Double:

                    points = new Vector3[] {
                        roofData.Bounds.Below_BackRight.ReplaceZClone(seperationPointZ),
                        roofData.Bounds.Below_FrontRight,
                        bodyData.FrontRight.Last(),
                        bodyData.FrontRight.Last().ReplaceZClone(seperationPointZ)
                    };

                    windowSet = Window.GenerateWindow(GeneralMeshGenerator.Axis2D.ZY, points, windowData.WindowSpacing, windowData.WindowIndentation, windowData.ColorSettings.FrameUV, false, false);
                    AddWindow();
                    AddWindowFlippedOverX();


                    points = new Vector3[] {
                        roofData.Bounds.Below_BackRight,
                        roofData.Bounds.Below_BackRight.ReplaceZClone(seperationPointZ),
                        bodyData.BackRight.Last().ReplaceZClone(seperationPointZ),
                        bodyData.BackRight.Last()
                    };

                    windowSet = Window.GenerateWindow(GeneralMeshGenerator.Axis2D.ZY, points, windowData.WindowSpacing, windowData.WindowIndentation, windowData.ColorSettings.FrameUV, false, false);  
                    AddWindow();
                    AddWindowFlippedOverX();

                    break;
            }
            #endregion

            #region Handle
            if (doorPanelIsSeperated && doorData.HandleEnabled)
            {
                //Handle
                var handleSize = doorData.HandleSize;
                var basePos = bodyData.FrontRight[1].ReplaceZClone(seperationPointZ);
                basePos += Vector3.forward * doorData.HandleOffsetSide;
                basePos += Vector3.down * doorData.HandleOffsetHeight;
                basePos += new Vector3(handleSize.x * 0.5f, handleSize.y * -0.5f, handleSize.z * 0.5f);

                var handle = RectangleGenerator.Generate(handleSize).AddPositionOffset(basePos);
                handle.OverrideUVs(doorData.ColorSettings.HandleUV, 0);

                CarGenerator.AddBodySidePart(handle);
                CarGenerator.AddBodySidePart(handle.FlipOverX());
            }
            #endregion

            var runtimeData = new RunTimeData();
            runtimeData.BodyMiddlePoint = bodyData.FrontRight.Last().ReplaceZClone(seperationPointZ);

            return runtimeData;

            void AddWindow()
            {
                CarGenerator.AddBodySidePart(windowSet.Frame);
                CarGenerator.AddWindow(windowSet.Glass);
            }

            void AddWindowFlippedOverX()
            {
                windowSet.Frame.FlipOverX();
                windowSet.Glass.FlipOverX();
                AddWindow();
            }
        }


        public struct RunTimeData
        {
            public Vector3 BodyMiddlePoint;
        }





        [System.Serializable]
        public class ColorSettings
        {
            [SerializeField] [Range(0, 16)] private int _body_TopID = 0;
            [SerializeField] [Range(0, 16)] private int _body_BottomID = 0;
            [Header("Handle")]
            [SerializeField] [Range(0, 16)] private int _handleID = 0;

            [System.NonSerialized] public Vector2 Body_TopUV = Vector2.zero;
            [System.NonSerialized] public Vector2 Body_BottomUV = Vector2.zero;
            [System.NonSerialized] public Vector2 HandleUV = Vector2.zero;

            public void UpdateValues(Color[] carColors)
            {
                CarSettings.ColorSettings.GetColorUV(carColors, _body_TopID, ref Body_TopUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _body_BottomID, ref Body_BottomUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _handleID, ref HandleUV);
            }

            public static void Blend(ColorSettings a, ColorSettings b, float progress, ref ColorSettings target)
            {
                progress = Mathf.Clamp01(progress);

                target.Body_TopUV = Utility.Vector2Lerp_HardSwitch(a.Body_TopUV, b.Body_TopUV, progress);
                target.Body_BottomUV = Utility.Vector2Lerp_HardSwitch(a.Body_BottomUV, b.Body_BottomUV, progress);
                target.HandleUV = Utility.Vector2Lerp_HardSwitch(a.HandleUV, b.HandleUV, progress);
            }
        }

        [System.Serializable]
        public class Settings
        {
            [Range(0, 2)] public float Width = 1f;
            [Header("Handle")]
            public bool HandleEnabled = true;
            public Vector3 HandleSize = new Vector3(0.025f, 0.05f, 0.1f);
            [Range(-3, 3)] public float HandleOffsetSide = 0.05f;
            [Range(-1, 1)] public float HandleOffsetHeight = 0.05f;
        }


        public class Data
        {
            public float Width;

            public bool HandleEnabled = false;
            public Vector3 HandleSize;
            public float HandleOffsetSide;
            public float HandleOffsetHeight;

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.Width = settings.Width;

                data.HandleEnabled = settings.HandleEnabled;
                data.HandleSize = settings.HandleSize;
                data.HandleOffsetSide = settings.HandleOffsetSide;
                data.HandleOffsetHeight = settings.HandleOffsetHeight;

                data.ColorSettings = colorSettings;

                return data;
            }


            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.Width = Mathf.Lerp(a.Width, b.Width, progress);

                dataBlend.HandleEnabled = Utility.BoolLerp(a.HandleEnabled, b.HandleEnabled, progress);
                dataBlend.HandleSize = Vector3.Lerp(a.HandleSize, b.HandleSize, progress);
                dataBlend.HandleOffsetSide = Mathf.Lerp(a.HandleOffsetSide, b.HandleOffsetSide, progress);
                dataBlend.HandleOffsetHeight = Mathf.Lerp(a.HandleOffsetHeight, b.HandleOffsetHeight, progress);

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }
    }
}