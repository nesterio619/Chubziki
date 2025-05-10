using UnityEngine;
using SimpleMeshGenerator;
using System;
using Random = UnityEngine.Random;



#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ProceduralCarBuilder
{
    [CreateAssetMenu(fileName = "CarSettings", menuName = "CarBuilder/CarSettings", order = 1)]
    public class CarSettings : ScriptableObject
    {
        public enum DataStyle
        {
            Min,
            Average,
            Max,
            Random
        }

        [Header("Pivot")]
        [SerializeField] [Range(0, 1)] private float _bodyPivotPercentage = 0.333f;

        [Header("Materials")]
        [SerializeField] private MaterialSettings _materialSettings = default;

        [Header("Colors")]
        [SerializeField] private ColorSettings _colorSettings = default;

        [Header("Body")]
        [SerializeField] private Body.Settings _bodySettings = default;

        [Header("Wheels")]
        [SerializeField] private Wheels.Settings _wheelSettings = default;

        [Header("Backside")]
        [SerializeField] private Back.Settings _backSettings = default;

        [Header("Front")]
        [SerializeField] private Front.Settings _frontSettings = default;

        [Header("Nose/ Grill / Lights")]
        [SerializeField] private NoseGrillLightsSettings _noseGrillLightsSettings = default;

        [Header("Roof")]
        [SerializeField] private Roof.Settings _roofSettings = default;

        [Header("Windows")]
        [SerializeField] private Window.Settings _windowSettings = default;

        [Header("Door")]
        [SerializeField] private Doors.Settings _doorSettings = default;

        [Header("Wing Mirror")]
        [SerializeField] private WingMirror.Settings _wingMirrorSettings = default;

        [Header("Bumper Front")]
        [SerializeField] private Bumper.Settings _bumperSettingsFront = default;

        [Header("Bumper Back")]
        [SerializeField] private Bumper.Settings _bumperSettingsBack = default;

        [Header("LicencePlate")]
        [SerializeField] private LicensePlate.Settings _licensePlateSettings = default;

        public Action<CarSettings> OnSettingsChange;
        private void OnValidate()
        {
            OnSettingsChange?.Invoke(this);
        }

        /// <summary>
        /// </summary>
        /// <param name="dataStyle"></param>    Contol way data is read / mixed
        /// <param name="safeMode"></param>     Automatically perform a SanityCheckData() after processing all data
        /// <param name="tweaking"></param>     Re-use "random" results when tweaking the car. Example: If there are 3 wheel types to choose from in the DataSet, you dont want to randomly get one of them each frame when making continuous  edits 
        /// <param name="targetData"></param>   optional Data holder to be filled in, instead of generating a new CarData()
        /// <returns></returns>
        public CarData GenerateData(bool safeMode = true, bool tweaking = false, CarData targetData = null)
        {
            _colorSettings.Initialize();
            var data = targetData == null ? new CarData() : targetData;

            data.PivotPercentage = _bodyPivotPercentage;
            data.MaterialsData = _materialSettings;
            data.Colors = _colorSettings.Colors;
            data.HoodDecalColor = _colorSettings.HoodDecalColor;
            data.BodyTopDecalColor = _colorSettings.BodyTopDecalColor;

            data.WindowData = Window.Data.Create(_windowSettings, _colorSettings.WindowColorSettings);

            data.BumperDataFront = Bumper.Data.Create(_bumperSettingsFront, _colorSettings.BumperFrontColorSettings);
            data.BumperDataBack = Bumper.Data.Create(_bumperSettingsBack, _colorSettings.BumperBackColorSettings);

            data.LightsData = Lights.Data.Create(_noseGrillLightsSettings.LightsSettings, _colorSettings.LightsColorSettings);

            data.LicensePlateData = LicensePlate.Data.Create(_licensePlateSettings, _colorSettings.LicensePlateColorSettings);

            data.BackData = Back.Data.Create(_backSettings, _colorSettings.BackColorSettings);

            data.FrontData = Front.Data.Create(_frontSettings, _colorSettings.FrontColorSettings);

            data.RoofData = Roof.Data.Create(_roofSettings, _colorSettings.RoofColorSettings);

            data.NoseData = Nose.Data.Create(_noseGrillLightsSettings.NoseSettings, _colorSettings.NoseColorSettings);

            data.DoorData = Doors.Data.Create(_doorSettings, _colorSettings.DoorColorSettings);

            data.BodyData = Body.Data.Create(_bodySettings, _colorSettings.BodyColorSettings);
            data.WheelData = Wheels.Data.Create(_wheelSettings,_colorSettings.WheelsColorSettings);
            data.WingMirrorData = WingMirror.Data.Create(_wingMirrorSettings, _colorSettings.WingMirrorColorSettings);

            if (safeMode) data = CarData.SanityCheckData(data);

            return data;
        }


        

        public static float GetData(Vector2 range, DataStyle dataStyle)
        {
            switch (dataStyle)
            {
                case DataStyle.Min:
                    return range.x;
                case DataStyle.Average:
                    return Mathf.Lerp(range.x, range.y, 0.5f);
                case DataStyle.Max:
                    return range.y;
                case DataStyle.Random:
                    return range.GetRandom();
            }

            return range.GetRandom();
        }

        public static int GetData(Vector2Int range, DataStyle dataStyle)
        {
            switch (dataStyle)
            {
                case DataStyle.Min:
                    return range.x;
                case DataStyle.Average:
                    return Mathf.RoundToInt(Mathf.Lerp(range.x, range.y, 0.5f));
                case DataStyle.Max:
                    return range.y;
                case DataStyle.Random:
                    return Mathf.RoundToInt(Mathf.Lerp(range.x, range.y, Random.value));
            }

            return Mathf.RoundToInt(Mathf.Lerp(range.x, range.y, Random.value));
        }

        public static int GetData(int[] range, DataStyle dataStyle)
        {
            switch (dataStyle)
            {
                case DataStyle.Min:
                    return range[0];
                case DataStyle.Max:
                    return range[range.Length - 1];
                case DataStyle.Average:
                case DataStyle.Random:
                    return range[Random.Range(0, range.Length)];
            }

            return range[Random.Range(0, range.Length)];
        }

        [System.Serializable]
        public class MaterialSettings
        {
            [Header("Body")]
            [SerializeField] private Material _matBody = default;
            [SerializeField] private Material _matGlass = default;
            [SerializeField] private Material _matWheel = default;

            [Header("Hood Decal")]
            [SerializeField] private bool _isHoodDecalEnabled = false;
            [SerializeField] private Material _matHoodDecal = default;
            [SerializeField] private Texture2D _textureOverrideHood = default;

            [Header("BodyTopSide Decal")]
            [SerializeField] private bool _isBodyTopDecalEnabled = false;
            [SerializeField] private Material _matBodyTopDecal = default;
            [SerializeField] private Texture2D _textureOverrideBodyTop = default;

            public Material MatBody { get => _matBody; }
            public Material MatGlass { get => _matGlass; }
            public Material MatWheel { get => _matWheel; }

            public bool IsHoodDecalEnabled { get => _isHoodDecalEnabled; }
            public Material MatHoodDecal { get => _matHoodDecal; }
            public Texture2D TextureOverrideHood { get => _textureOverrideHood; }

            public bool IsBodyTopDecalEnabled { get => _isBodyTopDecalEnabled; }
            public Material MatBodyTopDecal { get => _matBodyTopDecal; }
            public Texture2D TextureOverrideBodyTop { get => _textureOverrideBodyTop; }
        }


        [System.Serializable]
        public class ColorSettings
        {
            [SerializeField] private Color[] _colors = new Color[] { Color.red, Color.yellow, Color.grey, Color.cyan };
            [Header("DecalColors")]
            [SerializeField] private Color _hoodDecalColor = Color.white;
            [SerializeField] private Color _bodyTopDecalColor = Color.white;
            [Space]
            [SerializeField] private Body.ColorSettings _bodyColorSettings = default;
            [SerializeField] private Wheels.ColorSettings _wheelsColorSettings = default;
            [SerializeField] private Back.ColorSettings  _backColorSettings = default;
            [SerializeField] private Front.ColorSettings _frontColorSettings = default;
            [SerializeField] private Nose.ColorSettings _noseColorSettings = default;
            [SerializeField] private Roof.ColorSettings _roofColorSettings = default;
            [SerializeField] private Window.ColorSettings _windowColorSettings = default;
            [SerializeField] private Doors.ColorSettings _doorColorSettings = default;
            [SerializeField] private Lights.ColorSettings _lightsColorSettings = default;
            [SerializeField] private WingMirror.ColorSettings _wingMirrorSettings = default;
            [SerializeField] private Bumper.ColorSettings _bumperFrontColorSettings = default;
            [SerializeField] private Bumper.ColorSettings _bumperBackColorSettings = default;
            [SerializeField] private LicensePlate.ColorSettings _licensePlateColorSettings = default;

            public Color[] Colors { get => _colors; }
            public Color HoodDecalColor { get => _hoodDecalColor; }
            public Color BodyTopDecalColor { get => _bodyTopDecalColor; }

            public Body.ColorSettings BodyColorSettings { get => _bodyColorSettings; }
            public Wheels.ColorSettings WheelsColorSettings { get => _wheelsColorSettings; }
            public Back.ColorSettings BackColorSettings { get => _backColorSettings; }
            public Front.ColorSettings FrontColorSettings { get => _frontColorSettings; }
            public Nose.ColorSettings NoseColorSettings { get => _noseColorSettings; }
            public Roof.ColorSettings RoofColorSettings { get => _roofColorSettings; }
            public Window.ColorSettings WindowColorSettings { get => _windowColorSettings; }
            public Doors.ColorSettings DoorColorSettings { get => _doorColorSettings; }
            public Lights.ColorSettings LightsColorSettings { get => _lightsColorSettings; }
            public WingMirror.ColorSettings WingMirrorColorSettings { get => _wingMirrorSettings; }
            public Bumper.ColorSettings BumperFrontColorSettings { get => _bumperFrontColorSettings; }
            public Bumper.ColorSettings BumperBackColorSettings { get => _bumperBackColorSettings; }
            public LicensePlate.ColorSettings LicensePlateColorSettings { get => _licensePlateColorSettings; }

            public void Initialize()
            {
                _bodyColorSettings.UpdateValues(_colors);
                _wheelsColorSettings.UpdateValues(_colors);
                _backColorSettings.UpdateValues(_colors);
                _frontColorSettings.UpdateValues(_colors);
                _noseColorSettings.UpdateValues(_colors);
                _roofColorSettings.UpdateValues(_colors);
                _windowColorSettings.UpdateValues(_colors);
                _doorColorSettings.UpdateValues(_colors);
                _lightsColorSettings.UpdateValues(_colors);
                _wingMirrorSettings.UpdateValues(_colors);
                _bumperFrontColorSettings.UpdateValues(_colors);
                _bumperBackColorSettings.UpdateValues(_colors);
                _licensePlateColorSettings.UpdateValues(_colors);
            }


            public static Vector2 GetColorUV(Color[] _colors, int id)
            {
                var uv = Vector2.zero;
                GetColorUV(_colors, id, ref uv);

                return uv;
            }

            public static void GetColorUV(Color[] _colors, int id, ref Vector2 target)
            {
                if (_colors.Length == 0)
                {
                    target.x = 0;
                    target.y = 0;
                }

                var stepSize = 1f / _colors.Length;

                id = Mathf.Clamp(id, 0, _colors.Length);
                target.x = stepSize * 0.5f + id * stepSize;
                target.y = 0.5f;
            }
        }


        [System.Serializable]
        public class NoseGrillLightsSettings
        {
            [Header("Nose")]
            public Nose.Settings NoseSettings = default;
            public Lights.Settings LightsSettings = default;
        }
    }
}
