using System.Collections.Generic;
using UnityEngine;
using SimpleMeshGenerator;
using static LinearAlgebra.Line_Line_Intersection;

namespace ProceduralCarBuilder
{
    public class Window
    {
        private static Mesh _frameMesh;
        private static Mesh _glassMesh;
        private static WindowSet _windowSet = new WindowSet();

        public static WindowSet GenerateWindow(GeneralMeshGenerator.Axis2D axis, Vector3[] outerPoints, float spacing, float windowIndentation, Vector2 housingUV, bool flip, bool flat)
        {
            if (_frameMesh == null)
            {
                _frameMesh = new Mesh();
                _glassMesh = new Mesh();
            }
            else
            {
                _frameMesh.Clear();
                _glassMesh.Clear();
            }

            var data = CarGenerator.ActiveDataSet.WindowData;
            List<Vector3> lineDirections = new List<Vector3>();

            var innerPoints = new Vector3[outerPoints.Length];
            for (int i = 0; i < outerPoints.Length; i++)
            {
                innerPoints[i] = outerPoints[i];

                var dir = outerPoints.GetValueAt(i + 1) - outerPoints[i];
                lineDirections.Add(dir.normalized);
            }


            for (int i = 0; i < lineDirections.Count; i++)
            {
                Line GetLine(int startIndex)
                {
                    var dir = lineDirections.GetValueAt(startIndex).ToAxis(axis).normalized;
                    var line = new Line();
                    line.Start = new Vector2(outerPoints.GetValueAt(startIndex).ToAxis(axis)[0] - dir.x * 10, outerPoints.GetValueAt(startIndex).ToAxis(axis)[1] - dir.y * 10);
                    line.End = new Vector2(outerPoints.GetValueAt(startIndex + 1).ToAxis(axis)[0] + dir.x * 10, outerPoints.GetValueAt(startIndex + 1).ToAxis(axis)[1] + dir.y * 10);

                    var offset = new Vector2(dir.y, -dir.x) * spacing * (flip ? -1 : 1);
                    line.Start += offset;
                    line.End += offset;
                    return line;
                }

                var lineA = GetLine(i);
                var lineB = GetLine(i + 1);
                var result = IsIntersecting2D(lineA, lineB);
                if (result.Key == false)
                {
                    Debug.LogError("No Point Found");
                }
                else
                {
                    var pos = result.Value;

                    if (flat)
                    {
                        innerPoints[i] = new Vector3(pos.x, pos.y, outerPoints[0].z);
                    }
                    else
                    {
                        var bottom = outerPoints[outerPoints.Length - 1];
                        var top = outerPoints[0];

                        var height = 0f;
                        var heightFromBottom = 0f;

                        switch (axis)
                        {
                            case GeneralMeshGenerator.Axis2D.XY:

                                height = (top - bottom).y;
                                heightFromBottom = pos.y - bottom.y;

                                var z = Mathf.Lerp(bottom.z, top.z, Mathf.Clamp01(heightFromBottom / height));
                                innerPoints[i] = new Vector3(pos.x, pos.y, z);

                                break;

                            case GeneralMeshGenerator.Axis2D.XZ:


                                break;


                            case GeneralMeshGenerator.Axis2D.ZY:

                                height = (top - bottom).y;
                                heightFromBottom = pos.y - bottom.y;

                                var x = Mathf.Lerp(bottom.x, top.x, Mathf.Clamp01(heightFromBottom / height));
                                innerPoints[i] = new Vector3(x, pos.y, pos.x);

                                break;
                        }
                    }
                }
            }


            CombineMeshes.Combine(_frameMesh, GeneralMeshGenerator.CreateBridgeHardEdged(innerPoints, outerPoints, true, false, true));

            #region inwards sides
            var normal = Utility.CalculateNormal(outerPoints[0], outerPoints[1], outerPoints[0], outerPoints.Last());
            var innerPointsIndented = new List<Vector3>(innerPoints);
            for (int i = 0; i < innerPointsIndented.Count; i++)
            {
                innerPointsIndented[i] -= normal * windowIndentation;
            }

            //windowFrameIndented
            CombineMeshes.Combine(_frameMesh, GeneralMeshGenerator.CreateBridgeHardEdged(new List<Vector3>(innerPoints), innerPointsIndented, true, true, true));
            _frameMesh.OverrideUVs(housingUV, 0);
            #endregion

            var window = GeneralMeshGenerator.CreateFan(innerPointsIndented, innerPointsIndented[0], data.ColorSettings.GlassUV, normal, true);

            _windowSet.Glass = window;
            _windowSet.Frame = _frameMesh;
            return _windowSet;
        }

        public struct WindowSet
        {
            public Mesh Glass;
            public Mesh Frame;
        }



        [System.Serializable]
        public class ColorSettings
        {
            [SerializeField] [Range(0, 16)] private int _frameID = 0;
            [SerializeField] [Range(0, 16)] private int _glassID = 0;

            [System.NonSerialized] public Vector2 FrameUV = Vector2.zero;
            [System.NonSerialized] public Vector2 GlassUV = Vector2.zero;

            public void UpdateValues(Color[] carColors)
            {
                CarSettings.ColorSettings.GetColorUV(carColors, _frameID, ref FrameUV);
                CarSettings.ColorSettings.GetColorUV(carColors, _glassID, ref GlassUV);
            }

            public static void Blend(ColorSettings a, ColorSettings b, float progress, ref ColorSettings target)
            {
                progress = Mathf.Clamp01(progress);

                target.FrameUV = Utility.Vector2Lerp_HardSwitch(a.FrameUV, b.FrameUV, progress);
                target.GlassUV = Utility.Vector2Lerp_HardSwitch(a.GlassUV, b.GlassUV, progress);
            }      
        }


        [System.Serializable]
        public class Settings
        {
            [Range(0,0.5f)] public float WindowSpacing = 0.1f;
            [Range(0,0.1f)] public float WindowIndentation = 0.1f;
            public bool GenerateSideBackWindows = true;
            public bool GenerateBackWindow = true;
        }


        public class Data
        {
            public float Width;

            public bool GenerateSideBackWindows = false;
            public bool GenerateBackWindow = false;
            public float WindowSpacing;
            public float WindowIndentation;

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.WindowSpacing = settings.WindowSpacing;
                data.WindowIndentation = settings.WindowIndentation;

                data.GenerateSideBackWindows = settings.GenerateSideBackWindows;
                data.GenerateBackWindow = settings.GenerateBackWindow;

                data.ColorSettings = colorSettings;

                return data;
            }


            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.Width = Mathf.Lerp(a.Width, b.Width, progress);

                dataBlend.WindowSpacing = Mathf.Lerp(a.WindowSpacing, b.WindowSpacing, progress);
                dataBlend.WindowIndentation = Mathf.Lerp(a.WindowIndentation, b.WindowIndentation, progress);

                dataBlend.GenerateSideBackWindows = Utility.BoolLerp(a.GenerateSideBackWindows, b.GenerateSideBackWindows, progress);
                dataBlend.GenerateBackWindow = Utility.BoolLerp(a.GenerateBackWindow, b.GenerateBackWindow, progress);

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }
    }
}
