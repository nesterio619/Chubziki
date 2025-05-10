using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class Bumper
    {
        private static Mesh _targetMesh;

        public static Data Generate(Data bumperData, Vector3[] acrossBottom, bool front, float maxHeight)
        {
            var wheelData = CarGenerator.ActiveDataSet.WheelData;
            var generatedData = new Data();
            var floor = acrossBottom[0].y;
            var ceiling = floor + maxHeight;

            generatedData.Height = Mathf.Min(maxHeight, bumperData.Height);
            generatedData.YPosOffSet = bumperData.YPosOffSet;
            generatedData.YPosOffSet = Mathf.Min(bumperData.YPosOffSet, ceiling - floor - generatedData.Height);

            generatedData.SlantedEdgeMode = bumperData.SlantedEdgeMode;
            generatedData.Thickness = bumperData.Thickness;

            // No bumper
            if (bumperData.IsEnabled == false || generatedData.Height < 0.01f)
            {
                generatedData.Height = 0;
                generatedData.YPosOffSet = 0;
                generatedData.Thickness = 0;

                return generatedData;
            }


            if (_targetMesh == null)
            {
                _targetMesh = new Mesh();
            }
            else
            {
                _targetMesh.Clear();
            }    

            if (front)
            {
                var wheel = CarGenerator.CarInitializerInstance.Wheels[0]; // front right
                var outerWheelPos = wheel.transform.position.z + wheelData.TotalWheelAreaHalf();
                var maxSideLength = acrossBottom[0].z - outerWheelPos;
                generatedData.SideLength = Mathf.Min(maxSideLength, bumperData.SideLength);
            }
            else
            {
                var wheel = CarGenerator.CarInitializerInstance.Wheels[3]; // back left
                var outerWheelPos = wheel.transform.position.z - wheelData.TotalWheelAreaHalf();
                var maxSideLength = outerWheelPos - acrossBottom[0].z;
                generatedData.SideLength = Mathf.Min(maxSideLength, bumperData.SideLength);
            }
   
           
            if (generatedData.YPosOffSet < 0)
            {
                generatedData.YPosOffSet = Mathf.Max(-generatedData.Height, generatedData.YPosOffSet);
            }


            Vector3 yOffset = Vector3.up * generatedData.YPosOffSet;
            var heightOffset = Vector3.up * generatedData.Height;

            var forwardDir = front ? Vector3.forward : Vector3.back;
            var forwardOffset = forwardDir * bumperData.Thickness;

            var rightDir = front ? Vector3.right : Vector3.left;
            var rightOffset = rightDir * bumperData.Thickness;

            var leftDir = front ? Vector3.left : Vector3.right;
            var leftOffset = leftDir * bumperData.Thickness;

            var posA = Vector3.zero;
            var posB = Vector3.zero;

            for (int i = 0; i < acrossBottom.Length - 1; i++)
            {
                posA = acrossBottom[i] + yOffset;
                posB = acrossBottom[i + 1] + yOffset;

                // top side
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, posB + heightOffset, posB + heightOffset + forwardOffset, posA + heightOffset + forwardOffset }, Vector2Int.one, Vector3.up));

                // outer side
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset + forwardOffset, posB + heightOffset + forwardOffset, posB + forwardOffset, posA + forwardOffset }, Vector2Int.one, forwardDir));

                // bottom side
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posB, posA, posA + forwardOffset, posB + forwardOffset }, Vector2Int.one, Vector3.down));
            }


            // right side Car

            posB = acrossBottom[0] + yOffset;
            posA = posB - forwardDir * generatedData.SideLength;

            // top side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, posB + heightOffset, posB + heightOffset + rightOffset, posA + heightOffset + rightOffset }, Vector2Int.one, Vector3.up));

            // outer side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset + rightOffset, posB + heightOffset + rightOffset, posB + rightOffset, posA + rightOffset }, Vector2Int.one, rightDir));

            // bottom side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posB, posA, posA + rightOffset, posB + rightOffset }, Vector2Int.one, Vector3.down));

            // close opening
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, posA + heightOffset + rightOffset, posA + rightOffset, posA }, Vector2Int.one, Vector3.back));



            // left side Car
            posB = acrossBottom[0].FlipXClone() + yOffset;
            posA = posB - forwardDir * generatedData.SideLength;

            // top side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posB + heightOffset, posA + heightOffset, posA + heightOffset + leftOffset, posB + heightOffset + leftOffset }, Vector2Int.one, Vector3.up));

            // outer side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posB + heightOffset + leftOffset, posA + heightOffset + leftOffset, posA + leftOffset, posB + leftOffset }, Vector2Int.one, leftDir));

            // bottom side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA, posB, posB + leftOffset, posA + leftOffset }, Vector2Int.one, Vector3.down));

            // close opening
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset + leftOffset, posA + heightOffset, posA, posA + leftOffset }, Vector2Int.one, Vector3.back));



            //Corner Right
            var basePos = acrossBottom[0] + yOffset;
            posA = basePos + rightOffset;
            posB = basePos + forwardOffset;
            if (bumperData.SlantedEdgeMode)
            {
                // top side
                CombineMeshes.Combine(_targetMesh, TriangleGenerator.Generate(posA + heightOffset, basePos + heightOffset, posB + heightOffset, Vector2.zero, Vector3.up));

                // outer side
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, posB + heightOffset, posB, posA }, Vector2Int.one, Vector3.Lerp(rightDir, forwardDir, 0.5f)));

                // bottom side
                CombineMeshes.Combine(_targetMesh, TriangleGenerator.Generate(posB, basePos, posA, Vector2.zero, Vector3.down));
            }
            else
            {
                // top side
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, basePos + heightOffset, posB + heightOffset, basePos + heightOffset + forwardOffset + rightOffset }, Vector2Int.one, Vector3.up));

                // outer side Right
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, basePos + heightOffset + forwardOffset + rightOffset, basePos + forwardOffset + rightOffset, posA }, Vector2Int.one, rightDir));

                // outer side Front
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { basePos + heightOffset + forwardOffset + rightOffset, posB + heightOffset, posB, basePos + forwardOffset + rightOffset }, Vector2Int.one, forwardDir));

                // bottom side
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA, basePos, posB, basePos + forwardOffset + rightOffset }, Vector2Int.one, Vector3.up, true));

            }


            //Corner left
            basePos = acrossBottom[0].FlipXClone() + yOffset;
            posA = basePos + leftOffset;
            posB = basePos + forwardOffset;
            if (bumperData.SlantedEdgeMode)
            {
                // top side
                CombineMeshes.Combine(_targetMesh, TriangleGenerator.Generate(posA + heightOffset, basePos + heightOffset, posB + heightOffset, Vector2.zero, Vector3.up, true));

                // outer side
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, posB + heightOffset, posB, posA }, Vector2Int.one, Vector3.Lerp(leftDir, forwardDir, 0.5f), true));

                // bottom side
                CombineMeshes.Combine(_targetMesh, TriangleGenerator.Generate(posB, basePos, posA, Vector2.zero, Vector3.down, true));
            }
            else
            {
                // top side
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, basePos + heightOffset, posB + heightOffset, basePos + heightOffset + forwardOffset + leftOffset }, Vector2Int.one, Vector3.up, true));

                // outer side Right
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, basePos + heightOffset + forwardOffset + leftOffset, basePos + forwardOffset + leftOffset, posA }, Vector2Int.one, leftDir, true));

                // outer side Front
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { basePos + heightOffset + forwardOffset + leftOffset, posB + heightOffset, posB, basePos + forwardOffset + leftOffset }, Vector2Int.one, forwardDir, true));

                // bottom side
                CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA, basePos, posB, basePos + forwardOffset + leftOffset }, Vector2Int.one, Vector3.down));

            }

            _targetMesh.OverrideUVs(bumperData.ColorSettings.BodyUV, 0);
            CarGenerator.AddBodySidePart(_targetMesh);

            if (front)
            {
                var plateCenterPos = (acrossBottom[0] + yOffset).ReplaceXClone(0);
                plateCenterPos += Vector3.forward * bumperData.Thickness;
                LicensePlate.Generate(plateCenterPos, generatedData.Height, CarGenerator.ActiveDataSet.BodyData.TotalWidth);
            }

            return generatedData;
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
            public bool IsEnabled = false;
            [Space]
            public bool SlantedEdgeMode = false;
            [Range(0.01f,0.2f)] public float Thickness = 0.1f;
            [Range(0,1)] public float Height = 0.2f;
            [Range(0,1)] public float SideLength = 0.2f;
            [Range(-0.5f,0.5f)] public float PosOffset = 0.2f;
        }

        public class Data
        {
            public bool IsEnabled = false;
            public bool SlantedEdgeMode = false;

            public float Thickness = 0;
            public float Height = 0;
            public float SideLength = 0;
            public float YPosOffSet = 0;

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.IsEnabled = settings.IsEnabled;

                data.SlantedEdgeMode = settings.SlantedEdgeMode;
                data.Thickness = settings.Thickness;
                data.Height = settings.Height;
                data.SideLength = settings.SideLength;
                data.YPosOffSet = settings.PosOffset;

                data.ColorSettings = colorSettings;

                return data;
            }


            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.IsEnabled = Utility.BoolLerp(a.IsEnabled, b.IsEnabled, progress);

                dataBlend.SlantedEdgeMode = Utility.BoolLerp(a.SlantedEdgeMode, b.SlantedEdgeMode, progress);

                dataBlend.Thickness = Mathf.Lerp(a.Thickness, b.Thickness, progress);
                dataBlend.Height = Mathf.Lerp(a.Height, b.Height, progress);
                dataBlend.SideLength = Mathf.Lerp(a.SideLength, b.SideLength, progress);
                dataBlend.YPosOffSet = Mathf.Lerp(a.YPosOffSet, b.YPosOffSet, progress);

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }
    }
}
