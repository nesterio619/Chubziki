using UnityEngine;
using SimpleMeshGenerator;

namespace ProceduralCarBuilder
{
    public class LicensePlate
    {
        private static Mesh _targetMesh;

        public static void Generate(Vector3 bottomBackCenter, float bumperHeight, float bumperWidth)
        {
            if (_targetMesh == null)
            {
                _targetMesh = new Mesh();
            }
            else
            {
                _targetMesh.Clear();
            }

            var data = CarGenerator.ActiveDataSet.LicensePlateData;
            var height = Mathf.Min(data.Height, bumperHeight);
            var width = Mathf.Min(data.Width, bumperWidth);


            var heightPosOffset = Vector3.up * Mathf.Abs(height - bumperHeight) * 0.5f;
            var heightOffset = Vector3.up * height;
            var forwardOffset = Vector3.forward * data.Tickness;
            var widthOffset = Vector3.right * width * 0.5f;

            var posA = bottomBackCenter + widthOffset + heightPosOffset;
            var posB = bottomBackCenter - widthOffset + heightPosOffset;

            // top side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, posB + heightOffset, posB + heightOffset + forwardOffset, posA + heightOffset + forwardOffset }, Vector2Int.one, Vector3.up));

            // outer side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset + forwardOffset, posB + heightOffset + forwardOffset, posB + forwardOffset, posA + forwardOffset }, Vector2Int.one, Vector3.forward));

            // bottom side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posB, posA, posA + forwardOffset, posB + forwardOffset }, Vector2Int.one, Vector3.down));

            // inner side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, posB + heightOffset, posB, posA }, Vector2Int.one, -Vector3.forward, true));

            // right side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posA + heightOffset, posA + heightOffset + forwardOffset, posA + forwardOffset, posA }, Vector2Int.one, Vector3.right));

            // left side
            CombineMeshes.Combine(_targetMesh, QuadGenerator_3D.Generate(new Vector3[] { posB + heightOffset, posB + heightOffset + forwardOffset, posB + forwardOffset, posB }, Vector2Int.one, -Vector3.right, true));


            _targetMesh.OverrideUVs(data.ColorSettings.BodyUV, 0);
            CarGenerator.AddBodySidePart(_targetMesh);

            var centerFrontPos = Vector3.Lerp(posA, posB, 0.5f);
            centerFrontPos += Vector3.forward * data.Tickness;
            centerFrontPos += heightOffset * 0.5f;

            CarGenerator.CreateLicensePlateRef(centerFrontPos, width, height);
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
            [Range(0,2)] public float Width = 1f;
            [Range(0,1)] public float Height = 0.05f;
            [Range(0.01f,0.1f)] public float Thickness = 0.05f;
        }


        public class Data
        {
            public float Width;
            public float Height;
            public float Tickness;

            public ColorSettings ColorSettings = new ColorSettings();

            public static Data Create(Settings settings, ColorSettings colorSettings)
            {
                var data = new Data();

                data.Width = settings.Width;
                data.Height = settings.Height;
                data.Tickness = settings.Thickness;

                data.ColorSettings = colorSettings;

                return data;
            }


            public static Data Blend(Data a, Data b, float progress, Data targetData = null)
            {
                progress = Mathf.Clamp01(progress);

                var dataBlend = targetData;
                if (dataBlend == null) dataBlend = new Data();

                dataBlend.Width = Mathf.Lerp(a.Width, b.Width, progress);
                dataBlend.Height = Mathf.Lerp(a.Height, b.Height, progress);
                dataBlend.Tickness = Mathf.Lerp(a.Tickness, b.Tickness, progress);

                ColorSettings.Blend(a.ColorSettings, b.ColorSettings, progress, ref dataBlend.ColorSettings);

                return dataBlend;
            }
        }
    }
}
