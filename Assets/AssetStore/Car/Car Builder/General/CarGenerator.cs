using UnityEngine;
using SimpleMeshGenerator;
using Core.Utilities;

namespace ProceduralCarBuilder
{
    public static class CarGenerator
    {
        public const float MinimumDistance = 0.005f;
        public static CarData ActiveDataSet = default;
        public static TemporaryCarInitializer CarInitializerInstance = default;

        private static Material _matBodyRuntimeCopy = null;
        private static Material _matGlassRuntimeCopy = null;
        private static Material _matWheelRuntimeCopy = null;
        private static Material _matHoodDecalRuntimeCopy = null;
        private static Material _matBodyTopDecalRuntimeCopy = null;

        public static Transform MeshGenerationHelper;


        public static CarPartReferences Generate(CarData data, Transform carTranform = null, bool createNewCar = true)
        {
            if (MeshGenerationHelper == null)
            {
                MeshGenerationHelper = new GameObject("_meshGenerationHelper").transform;
                MeshGenerationHelper.parent = carTranform;
                CarInitializerInstance.MeshGenerationHelper = MeshGenerationHelper;
            }

            ActiveDataSet = data;
            if (CarInitializerInstance == null && createNewCar == false)
            {
                Debug.LogWarning("CarGenerator has no active Car Reference, thus forces \"createNewCar\" to be true");
                createNewCar = true;
            }

            PrepareMaterials(data.MaterialsData, data.HoodDecalColor, data.BodyTopDecalColor, createNewCar);

            if (createNewCar)
            {
                CarInitializerInstance = new GameObject("Car").AddComponent<TemporaryCarInitializer>();
                CarInitializerInstance.Initialize(true);
                CarInitializerInstance.SetupMeshRenderers(_matBodyRuntimeCopy, _matWheelRuntimeCopy, _matGlassRuntimeCopy, _matBodyTopDecalRuntimeCopy, _matHoodDecalRuntimeCopy);
            }
            else
            {
                CarInitializerInstance.Initialize(false);
            }

            CarInitializerInstance.ColorTextureInternal = CreateColorTexture(data.Colors, createNewCar, true);
            CarInitializerInstance.ColorTextureExternal = CreateColorTexture(data.Colors, createNewCar, false);

            UpdateMaterials(CarInitializerInstance.ColorTextureInternal);


            #region Scale Management
            var carParent = CarInitializerInstance.transform.parent;
            var carLocalPos = CarInitializerInstance.transform.localPosition;
            var carLocalRotation = CarInitializerInstance.transform.localRotation;

            CarInitializerInstance.transform.position = Vector3.zero;
            CarInitializerInstance.transform.localRotation = Quaternion.identity;
            CarInitializerInstance.transform.localScale = Vector3.one;
            #endregion


            #region CreateMeshes
            //body extends / interioir
            var bodyData = Body.Generate(data);

            // wheels
            Wheels.Generate(bodyData);

            // front / nose
            var frontData = Front.Generate(bodyData, data.BodyData.HoodHeightPercentage);

            // roof
            var roofData = Roof.Generate(bodyData);

            // windows
            WindShield.Generate(roofData, bodyData, true);
            WindShield.Generate(roofData, bodyData, false);

            // mirrors
            WingMirror.Generate(roofData, bodyData);

            // doors
            var doorsData = Doors.Generate(roofData, bodyData, frontData.WheelPanel_RightSide.WheelTopPoint.y);

            //back / trunk
            var backData = Back.Generate(bodyData, doorsData);

            Underside.Generate(frontData, backData);

            #endregion

            PostGeneration(frontData, backData);
            CarInitializerInstance.UpdatePartReferences();
            CarInitializerInstance.UpdateAnchors(roofData, frontData, backData);

            #region Scale Management
            CarInitializerInstance.transform.SetParent(carParent, false);
            CarInitializerInstance.transform.localPosition = carLocalPos;
            CarInitializerInstance.transform.localRotation = carLocalRotation;
            CarInitializerInstance.transform.localScale = Vector3.one;
            #endregion

            return CarInitializerInstance.CarPartReferences;
        }


        public static void AddBodySidePart(Mesh mesh)
        {
            CombineMeshes.Combine(CarInitializerInstance.BodySide.sharedMesh, mesh);
        }

        public static void AddBodyTopPart(Mesh mesh)
        {
            CombineMeshes.Combine(CarInitializerInstance.BodyTop.sharedMesh, mesh);
        }

        public static void AddLight(Mesh mesh, bool front, bool right, Vector3 position)
        {
            if (front)
            {
                if (right)
                {
                    CarInitializerInstance.HeadLight_Right.transform.position = position;
                    CombineMeshes.Combine(CarInitializerInstance.HeadLight_Right.sharedMesh, mesh.SetOrigin(position));
                }
                else
                {
                    CarInitializerInstance.HeadLight_Left.transform.position = position;
                    CombineMeshes.Combine(CarInitializerInstance.HeadLight_Left.sharedMesh, mesh.SetOrigin(position));
                }
            }

        }

        public static void AddWindow(Mesh mesh, bool debugMode = false)
        {
            CombineMeshes.Combine(CarInitializerInstance.Windows.sharedMesh, mesh);

            if (debugMode)
            {
                GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().mesh = mesh.Clone();
            }
        }

        public static void AddTrunk(Mesh mesh, Vector3 position, bool outer)
        {
            if (outer)
            {
                CarInitializerInstance.TrunkOuterSide.transform.position = position;
                CombineMeshes.Combine(CarInitializerInstance.TrunkOuterSide.sharedMesh, mesh);
            }
            else
            {
                CarInitializerInstance.TrunkInnerSide.transform.position = position;
                CombineMeshes.Combine(CarInitializerInstance.TrunkInnerSide.sharedMesh, mesh);        
            }   
        }

        public static void AddHood(Mesh mesh, Vector3 position, bool outer)
        {
            if (outer)
            {
                CarInitializerInstance.HoodOuterside.transform.position = position;
                CombineMeshes.Combine(CarInitializerInstance.HoodOuterside.sharedMesh, mesh);
            }
            else
            {
                CarInitializerInstance.HoodInnerSide.transform.position = position;
                CombineMeshes.Combine(CarInitializerInstance.HoodInnerSide.sharedMesh, mesh);
            }
        }

        public static void CreateLicensePlateRef(Vector3 pos, float width, float height)
        {
            if (CarInitializerInstance.LicensePlate == null)
            {
                CarInitializerInstance.LicensePlate = new GameObject("LicensePlate Ref").transform;
            }

            CarInitializerInstance.LicensePlate.SetParent(CarInitializerInstance.transform, false);

            if (pos.IsNaN()) return;

            CarInitializerInstance.LicensePlate.position = pos;

            CarInitializerInstance.LicensePlateDimensions = new Vector2(width, height);
        }


        private static Texture2D _tempColorTextureInternal = null;
        private static Texture2D _tempColorTextureExternal = null;

        private static Texture2D CreateColorTexture(Color[] colors, bool createNew, bool internalUse)
        {
            var textureColors = new Color[colors.Length];

            var tempTexture = internalUse ? _tempColorTextureInternal : _tempColorTextureExternal;

            if (createNew || tempTexture == null || textureColors.Length != tempTexture.width)
            {
                tempTexture = new Texture2D(colors.Length, 1, TextureFormat.ARGB32, false, true);

                tempTexture.filterMode = FilterMode.Point;
                tempTexture.wrapMode = TextureWrapMode.Clamp;
            }

            for (int i = 0; i < colors.Length; i++)
            {
                if (internalUse && QualitySettings.activeColorSpace == ColorSpace.Linear)
                {
                    textureColors[i] = colors[i].linear;
                }
                else
                {
                    textureColors[i] = colors[i];
                }
            }

            tempTexture.SetPixels(textureColors);
            tempTexture.Apply(false);
            return tempTexture;
        }

        private static void PrepareMaterials(CarSettings.MaterialSettings settings, Color hoodDecalColor, Color bodyTopDecalColor, bool createNew)
        {
            if (settings.MatBody == null) Debug.LogError("No materials defined");

            if (createNew || _matBodyRuntimeCopy == null)
            {         
                _matBodyRuntimeCopy = new Material(settings.MatBody);
                _matGlassRuntimeCopy = new Material(settings.MatGlass);
                _matWheelRuntimeCopy = new Material(settings.MatWheel);

                _matHoodDecalRuntimeCopy = settings.IsHoodDecalEnabled ? new Material(settings.MatHoodDecal) : null;
                _matBodyTopDecalRuntimeCopy = settings.IsBodyTopDecalEnabled ? new Material(settings.MatBodyTopDecal) : null;
            }
            else
            {
                _matBodyRuntimeCopy.CopyPropertiesFromMaterial(settings.MatBody);
                _matGlassRuntimeCopy.CopyPropertiesFromMaterial(settings.MatGlass);
                _matWheelRuntimeCopy.CopyPropertiesFromMaterial(settings.MatWheel);

                if (_matHoodDecalRuntimeCopy != null) _matHoodDecalRuntimeCopy.CopyPropertiesFromMaterial(settings.MatHoodDecal);
                if (_matBodyTopDecalRuntimeCopy != null) _matBodyTopDecalRuntimeCopy.CopyPropertiesFromMaterial(settings.MatBodyTopDecal);
            }

            #region Decals
            var colorProperty = Shader.PropertyToID("_Color");
            var textureProperty = Shader.PropertyToID("_MainTex");

            if (_matHoodDecalRuntimeCopy != null)
            {
                _matHoodDecalRuntimeCopy.SetColor(colorProperty, hoodDecalColor);
                if (settings.TextureOverrideHood != null) _matHoodDecalRuntimeCopy.SetTexture(textureProperty, settings.TextureOverrideHood);
            }

            if (_matBodyTopDecalRuntimeCopy != null)
            {
                _matBodyTopDecalRuntimeCopy.SetColor(colorProperty, bodyTopDecalColor);
                if (settings.TextureOverrideBodyTop != null) _matBodyTopDecalRuntimeCopy.SetTexture(textureProperty, settings.TextureOverrideBodyTop);
            }
            #endregion
        }


        private static void UpdateMaterials(Texture2D texture)
        {
            _matBodyRuntimeCopy.mainTexture = texture;
            _matBodyRuntimeCopy.SetTexture("_EmissionMap", texture);
            _matGlassRuntimeCopy.mainTexture = texture;
            _matWheelRuntimeCopy.mainTexture = texture;
        }


        private static void PostGeneration(Front.RuntimeData frontData, Back.RunTimeData backData)
        {
            // Body top UV Mapping for Decals
            var min = new Vector2(frontData.WheelPanel_RightSide.SidePointsRight.Last().x, frontData.WheelPanel_RightSide.SidePointsRight.Last().z);
            var max = new Vector2(-backData.WheelPanel_RightSide.SidePointsLeft.Last().x, backData.WheelPanel_RightSide.SidePointsLeft.Last().z);

            CarInitializerInstance.BodyTop.sharedMesh.BoxUVChannel_XZ(min, max, 1);
            CarInitializerInstance.HoodOuterside.sharedMesh.BoxUVChannel_XZ(min, max, 1);

            // fixing Pivots
            CarInitializerInstance.HoodOuterside.sharedMesh.SetOrigin(CarInitializerInstance.HoodOuterside.transform.position);
            CarInitializerInstance.TrunkOuterSide.sharedMesh.SetOrigin(CarInitializerInstance.TrunkOuterSide.transform.position);

            CarInitializerInstance.HoodInnerSide.sharedMesh.SetOrigin(CarInitializerInstance.HoodOuterside.transform.position);
            CarInitializerInstance.TrunkInnerSide.sharedMesh.SetOrigin(CarInitializerInstance.TrunkOuterSide.transform.position);
        }
    }
}
