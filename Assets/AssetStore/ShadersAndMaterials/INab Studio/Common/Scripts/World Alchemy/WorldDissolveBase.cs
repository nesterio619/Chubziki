using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace INab.WorldAlchemy
{
    /// <summary>
    /// Base abstract class for managing world dissolve effects. 
    /// It provides core functionality for different types of dissolving effects 
    /// based on Signed Distance Fields (SDFs).
    /// </summary>
    public abstract class WorldDissolveBase : MonoBehaviour
    {
        #region Static Properties

        /// <summary>
        /// List of shader keyword names corresponding to different mask Signed Distance Field (SDF) types.
        /// These types include plane, box, sphere, ellipse, solid angle, and round cone.
        /// </summary>
        private static List<string> typeKeywords = new List<string>()
        {
            "_TYPE_PLANE",
            "_TYPE_BOX",
            "_TYPE_SPHERE",
            "_TYPE_ELLIPSE",
            "_TYPE_SOLID_ANGLE",
            "_TYPE_ROUND_CONE"
        };

        /// <summary>
        /// List of shader keyword names for varying numbers of active masks in the material, ranging from 0 to 4.
        /// </summary>
        private static List<string> masksNumberKeywords = new List<string>()
        {
            "_MASKSNUMBER__0",
            "_MASKSNUMBER__1",
            "_MASKSNUMBER__2",
            "_MASKSNUMBER__3",
            "_MASKSNUMBER__4",
            "_MASKSNUMBER__5",
            "_MASKSNUMBER__6"
        };

        /// <summary>
        /// Shader keyword for enabling 
        /// effects.
        /// </summary>
        private static string useDisplacementKeyword = "_USEDISPLACEMENT";

        /// <summary>
        /// Defines the maximum number of masks that can be active on a material simultaneously.
        /// </summary>
        public static int MaxMasks = 4; 

        #endregion

        #region Private Properties

        [SerializeField, Tooltip("Active mask count. Lower numbers will enhance performance.")]
        private int activeMasks = 1;

        public int ActiveMasks
        {
            get { return activeMasks; }
            private set { }
        }

        [SerializeField, Tooltip("Type of SDF mask. Choose from Plane, Box, Sphere, etc.")]
        protected Type type = Type.Sphere;

        public Type Type
        {
            get { return type; }
            private set { }
        }

        [SerializeField, Tooltip("All world dissolve materials.")]
        protected List<Material> materialsList = new List<Material>();

        [Tooltip("Dictionary of shared materials.")]
        protected Dictionary<Material, int> materialsDictionary = new Dictionary<Material, int>();

        [SerializeField, Tooltip("Editor list usage at startup. Disable for auto detection.")]
        protected bool copyEditorLists = true;

        [SerializeField, Tooltip("Enables automatic material detection. Not suitable for 'Round Cone' dissolve type.")]
        protected bool useAutoDetection = false;

        public bool UseAutoDetection
        {
            get { return useAutoDetection; }
            private set { }
        }

        [SerializeField, Tooltip("Layer mask of objects to auto detect.")]
        protected LayerMask detectionLayerMask;

        [SerializeField, Tooltip("Automatically updates material properties from materials list if enabled.")]
        private bool controlMaterialsProperties = false;

        public bool ControlMaterialsProperties
        {
            get { return controlMaterialsProperties; }
            private set { }
        }

        [SerializeField, Tooltip("Dissolve shader style. For 'Smooth', set material to transparent.")]
        private DissolveType dissolveType = DissolveType.Burn;

        public DissolveType DissolveType
        {
            get { return dissolveType; }
            private set { }
        }

        #endregion

        // Properties accessible via inspactor and code that are applied to all materials attached to WorldDissolve script. 
        #region dissolveTypeProperties

        [SerializeField, Tooltip("Inverts the dissolve effect.")]
        private bool invert = false;
        public bool Invert
        {
            get { return invert; }
            set
            {
                invert = value;
                UpdateMaterialsProperties(Invert);
            }
        }

        [SerializeField, Tooltip("Guide texture for the dissolve pattern.")]
        private Texture2D guideTexture = null;
        public Texture2D GuideTexture
        {
            get { return guideTexture; }
            set
            {
                guideTexture = value;
                UpdateMaterialsProperties(GuideTexture);
            }
        }

        [SerializeField, Tooltip("Tiling rate of the guide texture.")]
        private float guideTiling = 1;
        public float GuideTiling
        {
            get { return guideTiling; }
            set
            {
                guideTiling = value;
                UpdateMaterialsProperties(GuideTiling);
            }
        }

        [SerializeField, Range(0, 1), Tooltip("Intensity of the guide texture effect.")]
        private float guideStrength = 0.5f;
        public float GuideStrength
        {
            get { return guideStrength; }
            set
            {
                guideStrength = value;
                UpdateMaterialsProperties(GuideStrength);
            }
        }

        // ================= Back Color =================

        [SerializeField, Tooltip("Enables a background color for the dissolve.")]
        private bool useBackColor = false;
        public bool UseBackColor
        {
            get { return useBackColor; }
            set
            {
                useBackColor = value;
                UpdateMaterialsProperties(UseBackColor);
            }
        }

        [SerializeField, ColorUsage(true, true), Tooltip("Background color for the dissolve effect.")]
        private Color backColor = Color.black;
        public Color BackColor
        {
            get { return backColor; }
            set
            {
                backColor = value;
                UpdateMaterialsProperties(BackColor);
            }
        }

        // ================= Burn =================

        [SerializeField, Range(0, 1), Tooltip("Controls the sharpness of the burn effect.")]
        private float burnHardness = 0.5f;
        public float BurnHardness
        {
            get { return burnHardness; }
            set
            {
                burnHardness = value;
                UpdateMaterialsProperties(BurnHardness);
            }
        }

        [SerializeField, Range(0, 2), Tooltip("Determines the offset of the burn effect.")]
        private float burnOffset = 0.5f;
        public float BurnOffset
        {
            get { return burnOffset; }
            set
            {
                burnOffset = value;
                UpdateMaterialsProperties(BurnOffset);
            }
        }

        [SerializeField, Range(0, 1), Tooltip("Offset for the ember effect within the burn area.")]
        private float emberOffset = 0.1f;
        public float EmberOffset
        {
            get { return emberOffset; }
            set
            {
                emberOffset = value;
                UpdateMaterialsProperties(EmberOffset);
            }
        }

        [SerializeField, Range(0, 1), Tooltip("Smoothness of the ember edges.")]
        private float emberSmoothness = 0.1f;
        public float EmberSmoothness
        {
            get { return emberSmoothness; }
            set
            {
                emberSmoothness = value;
                UpdateMaterialsProperties(EmberSmoothness);
            }
        }

        [SerializeField, Range(0, 1), Tooltip("Width of the ember effect.")]
        private float emberWidth = 0;
        public float EmberWidth
        {
            get { return emberWidth; }
            set
            {
                emberWidth = value;
                UpdateMaterialsProperties(EmberWidth);
            }
        }

        [SerializeField, ColorUsage(true, true), Tooltip("Color of the ember effect.")]
        private Color emberColor = new Color(10f, 1.8f, 0.2f);
        public Color EmberColor
        {
            get { return emberColor; }
            set
            {
                emberColor = value;
                UpdateMaterialsProperties(EmberColor);
            }
        }

        [SerializeField, ColorUsage(true, true), Tooltip("Primary color for the burn effect.")]
        private Color burnColor = Color.black;
        public Color BurnColor
        {
            get { return burnColor; }
            set
            {
                burnColor = value;
                UpdateMaterialsProperties(BurnColor);
            }
        }

        // ================= Smooth =================

        [SerializeField, Tooltip("Whether to use dithering to fake transparency.")]
        private bool useDithering = false;
        public bool UseDithering
        {
            get { return useDithering; }
            set
            {
                useDithering = value;
                UpdateMaterialsProperties(UseDithering);
            }
        }

        [SerializeField, ColorUsage(true, true), Tooltip("Color of the edge.")]
        private Color edgeColor = Color.blue;
        public Color EdgeColor
        {
            get { return edgeColor; }
            set
            {
                edgeColor = value;
                UpdateMaterialsProperties(EdgeColor);
            }
        }

        [SerializeField, Range(0, 0.2f), Tooltip("Width of the edge.")]
        private float edgeWidth = 0;
        public float EdgeWidth
        {
            get { return edgeWidth; }
            set
            {
                edgeWidth = value;
                UpdateMaterialsProperties(EdgeWidth);
            }
        }

        [SerializeField, Range(0, 1), Tooltip("Smoothness of the edge.")]
        private float edgeSmoothness = 0.05f;
        public float EdgeSmoothness
        {
            get { return edgeSmoothness; }
            set
            {
                edgeSmoothness = value;
                UpdateMaterialsProperties(EdgeSmoothness);
            }
        }

        [SerializeField, Tooltip("Determines if the albedo is affected.")]
        private bool affectAlbedo = false;
        public bool AffectAlbedo
        {
            get { return affectAlbedo; }
            set
            {
                affectAlbedo = value;
                UpdateMaterialsProperties(AffectAlbedo);
            }
        }

        [SerializeField, ColorUsage(true, true), Tooltip("Color of the glare effect.")]
        private Color glareColor = Color.blue;
        public Color GlareColor
        {
            get { return glareColor; }
            set
            {
                glareColor = value;
                UpdateMaterialsProperties(GlareColor);
            }
        }

        [SerializeField, Range(0, 3), Tooltip("Strength of the guide texture for the glare effect.")]
        private float glareGuideStrength = 1;
        public float GlareGuideStrength
        {
            get { return glareGuideStrength; }
            set
            {
                glareGuideStrength = value;
                UpdateMaterialsProperties(GlareGuideStrength);
            }
        }

        [SerializeField, Range(0, 1), Tooltip("Width of the glare effect.")]
        private float glareWidth = 0;
        public float GlareWidth
        {
            get { return glareWidth; }
            set
            {
                glareWidth = value;
                UpdateMaterialsProperties(GlareWidth);
            }
        }

        [SerializeField, Range(0, 2), Tooltip("Smoothness of the glare effect.")]
        private float glareSmoothness = 0.3f;
        public float GlareSmoothness
        {
            get { return glareSmoothness; }
            set
            {
                glareSmoothness = value;
                UpdateMaterialsProperties(GlareSmoothness);
            }
        }

        [SerializeField, Range(-2, 2), Tooltip("Offset for the glare effect.")]
        private float glareOffset = 0;
        public float GlareOffset
        {
            get { return glareOffset; }
            set
            {
                glareOffset = value;
                UpdateMaterialsProperties(GlareOffset);
            }
        }


        #endregion

        #region displacementProperties

        [SerializeField, ColorUsage(true, true), Tooltip("Emission color for the dissolve effect based on displacement.")]
        private Color displacementColor = Color.black;
        public Color DisplacementColor
        {
            get { return displacementColor; }
            set
            {
                displacementColor = value;
                UpdateMaterialsProperties(DisplacementColor);
            }
        }

        [SerializeField, Tooltip("Enables displacement effects.")]
        private bool useDisplacement = false;
        public bool UseDisplacement
        {
            get { return useDisplacement; }
            private set { }
        }

        [SerializeField, Tooltip("Applies displacement per vertex for more detailed effects.")]
        private bool displacementPerVertex = false;
        public bool DisplacementPerVertex
        {
            get { return displacementPerVertex; }
            set
            {
                displacementPerVertex = value;
                UpdateMaterialsProperties(DisplacementPerVertex);
            }
        }

        [SerializeField, Range(0, 10), Tooltip("Smoothness of the displacement effect.")]
        private float displacementSmoothness = 1;
        public float DisplacementSmoothness
        {
            get { return displacementSmoothness; }
            set
            {
                displacementSmoothness = value;
                UpdateMaterialsProperties(DisplacementSmoothness);
            }
        }

        [SerializeField, Tooltip("Offset value for displacement.")]
        private float displacementOffset = 0.5f;
        public float DisplacementOffset
        {
            get { return displacementOffset; }
            set
            {
                displacementOffset = value;
                UpdateMaterialsProperties(DisplacementOffset);
            }
        }

        [SerializeField, Tooltip("Axis for rotational displacement.")]
        private Vector3 rotationAxis = Vector3.up;
        public Vector3 RotationAxis
        {
            get { return rotationAxis; }
            set
            {
                rotationAxis = value;
                UpdateMaterialsProperties(RotationAxis);
            }
        }

        [SerializeField, Range(-180, 180), Tooltip("Minimum rotation angle for displacement.")]
        private float rotationMin = 0;
        public float RotationMin
        {
            get { return rotationMin; }
            set
            {
                rotationMin = value;
                UpdateMaterialsProperties(RotationMin);
            }
        }

        [SerializeField, Range(-180, 180), Tooltip("Maximum rotation angle for displacement.")]
        private float rotationMax = 0;
        public float RotationMax
        {
            get { return rotationMax; }
            set
            {
                rotationMax = value;
                UpdateMaterialsProperties(RotationMax);
            }
        }

        [SerializeField, Tooltip("Offset for randomizing position in displacement.")]
        private Vector3 randomPositionOffset = Vector3.zero;
        public Vector3 RandomPositionOffset
        {
            get { return randomPositionOffset; }
            set
            {
                randomPositionOffset = value;
                UpdateMaterialsProperties(RandomPositionOffset);
            }
        }

        [SerializeField, Tooltip("Direct position offset for displacement.")]
        private Vector3 positionOffset = Vector3.zero;
        public Vector3 PositionOffset
        {
            get { return positionOffset; }
            set
            {
                positionOffset = value;
                UpdateMaterialsProperties(PositionOffset);
            }
        }

        [SerializeField, Tooltip("Scale factor for displacement.")]
        private float scale = 1;
        public float Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                UpdateMaterialsProperties(Scale);
            }
        }

        [SerializeField, Tooltip("Offset for normals in displacement.")]
        private Vector3 normalOffset = Vector3.zero;
        public Vector3 NormalOffset
        {
            get { return normalOffset; }
            set
            {
                normalOffset = value;
                UpdateMaterialsProperties(NormalOffset);
            }
        }

        #endregion

        #region unityFunctions

        public virtual void Start()
        {
            if (enabled == false) return;

            MasksTriggerSubscribe();

            if (copyEditorLists && !useAutoDetection)
            {
                CopyEditorLists();
            }
        }

        public virtual void Update()
        {
            CheckMasksForNulls();
        }

        public void LateUpdate()
        {
           // TODO: see whether we need to add an option to disable this call when using SeeThroughHelper based on users feedback.
            ForceUpdateMasksParameters();
        }

        public virtual void OnValidate()
        {
            if (enabled == false || gameObject.activeSelf == false) return;

            if (useAutoDetection)
            {
                copyEditorLists = false;
            }

            // We use CopyEditorLists() only in editor
            if (!Application.isPlaying)
            {
                CopyEditorLists();
            }

            // Automatically changes all materials's properties attached to this script.
            if (controlMaterialsProperties)
            {
                UpdateMaterialsProperties();
            }

            CheckMasksForNulls();

            // Update detection tags in each one of the masks attached to the script.
            UpdateMasksDetection();
        }

        public void OnDestroy()
        {
            MasksTriggerUnSubscribe();
        }

        #endregion

        #region protectedFunctions

        /// <summary>
        /// Used to fill hash sets, dictionaries, etc. with lists accessible via inspector to make all of the script logic work on faster enumerators.
        /// </summary>
        protected virtual void CopyEditorLists()
        {
            // Materials from list get passed to materials dictionary.
            materialsDictionary.Clear();
            foreach (var material in materialsList)
            {
                if (material != null)
                {
                    AddMaterial(material);
                }
            }
        }


        /// <summary>
        /// Udpates properties for each material from dictionary.
        /// </summary>
        protected void UpdateMaterialsDictionaryProperties()
        {
            foreach (var material in materialsDictionary)
            {
                SetVectorProperties(material.Key);
            }
        }

        /// <summary>
        /// In each update ensures that there is no null mask attached.
        /// </summary>
        protected abstract void CheckMasksForNulls();

        /// <summary>
        /// Updates sdf vectors, vfx graph parameters, etc...
        /// </summary>
        protected abstract void UpdateParameters();

        /// <summary>
        /// Update type of mask of mask/masks attached to the script.
        /// </summary>
        protected abstract void UpdateMasksType();

        /// <summary>
        /// Update detection tag of mask/masks attached to the script.
        /// </summary>
        protected abstract void UpdateMasksDetection();

        #endregion

        #region publicFunctions

        /// <summary>
        /// Updates all properties related to SDFs (e.g. positions, rotations, etc.). It udpates properties for each material from dictionary. Called in LateUpdate.
        /// Call when masks transform is changed.
        /// </summary>
        public void ForceUpdateMasksParameters()
        {
            UpdateParameters();
            UpdateMaterialsParameters();
        }

        /// <summary>
        /// All properties related to SDFs are updated (e.g. positions, rotations, etc.). It udpates properties for each material from dictionary. Called in OnUpdate.
        /// </summary>
        public virtual void UpdateMaterialsParameters()
        {
            // All properties related to SDFs are updated. We use materials from dictionary, since materials from list are automatically passed to dictionary in OnValidate.
            UpdateMaterialsDictionaryProperties();
        }

        /// <summary>
        /// Refreshes all properties and keywords for the materials managed by this script.
        /// Ensures the latest material settings are applied, including mask types.
        /// </summary>
        public virtual void RefreshWorldDissolve()
        {
            // Masks 
            UpdateMasksType();
            UpdateMasksDetection();

            // Materials parameters
            UpdateParameters();
            UpdateMaterialsParameters();

            // Materials keywords
            ChangeActiveMasks(activeMasks);
            ChangeType(type);
            ChangeUseDisplacement(useDisplacement);

            OnValidate();
        }

        /// <summary>
        /// Removes a material from the materials dictionary. If 'forceRemove' is true, the material is removed regardless of its count.
        /// </summary>
        /// <param name="material">The material to be removed.</param>
        /// <param name="forceRemove">Forces removal of the material even if it has multiple entries.</param>
        /// <returns>True if the material was successfully removed, false otherwise.</returns>
        public bool RemoveMaterial(Material material, bool forceRemove)
        {
            if (materialsDictionary.ContainsKey(material))
            {
                materialsDictionary[material]--;

                if (materialsDictionary[material] <= 0 || forceRemove)
                {
                    DisableMaterialTypes(material);
                    DisableMaterialMasksNumber(material);
                    return materialsDictionary.Remove(material);
                }
            }

            return false;
        }

        /// <summary>
        /// Adds a material to the materials dictionary to manage its usage. 
        /// Prevents duplication and manages multiple entries of the same material.
        /// </summary>
        /// <param name="material">The material to be added.</param>
        public void AddMaterial(Material material)
        {
            if (!materialsDictionary.ContainsKey(material))
            {
                materialsDictionary.Add(material, 1);
                EnableMaterialType(material);
                EnableMaterialMasksNumber(material);
            }
            else
            {
                materialsDictionary[material]++;
            }
        }

        /// <summary>
        /// Updates the number of active masks.
        /// </summary>
        /// <param name="newActiveMasks">The new number of active masks to apply.</param>
        public virtual void ChangeActiveMasks(int newActiveMasks)
        {
            activeMasks = newActiveMasks;

            foreach (var material in materialsDictionary)
            {
                DisableMaterialMasksNumber(material.Key);
                EnableMaterialMasksNumber(material.Key);
            }
        }

        /// <summary>
        /// Changes the type of world dissolve mask used, updating all materials managed by the script.
        /// </summary>
        /// <param name="newType">The new mask type to apply.</param>
        public void ChangeType(Type newType)
        {
            type = newType;

            foreach (var material in materialsDictionary)
            {
                DisableMaterialTypes(material.Key);
                EnableMaterialType(material.Key);
            }

            UpdateMasksType();
        }

        /// <summary>
        /// Toggles the use of displacement in the dissolve effect, applying changes to all materials managed by the script.
        /// </summary>
        /// <param name="useDisplacement">Whether to enable or disable displacement.</param>
        public void ChangeUseDisplacement(bool useDisplacement)
        {
            this.useDisplacement = useDisplacement;

            if (useDisplacement)
            {
                foreach (var material in materialsDictionary)
                {
                    DisableMaterialDisplacement(material.Key);
                    EnableMaterialDisplacement(material.Key);
                }
            }
            else
            {
                foreach (var material in materialsDictionary)
                {
                    DisableMaterialDisplacement(material.Key);
                }
            }
        }

        #endregion

        #region autoDetectFunctions

        // Using instanced materials instead of shared materials breaks SRP batching and leads to CPU overhead. 
        // It creates new material and it needs to be destroyed manually. Thus not using shared materials here is a bad idea.

        /// <summary>
        /// Adds renderer's shared materials to materials dictionary.
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void TriggerEnter(Renderer renderer, int id = 0)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                AddMaterial(material);
            }
        }

        /// <summary>
        /// Removes renderer's shared materials to materials dictionary.
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void TriggerExit(Renderer renderer, int id = 0)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                RemoveMaterial(material, false);
            }
        }

        /// <summary>
        /// Subscribe to OnMaskTrigger events of mask/masks attached to the script.
        /// </summary>
        protected abstract void MasksTriggerSubscribe();

        /// <summary>
        /// Unsubscribe to OnMaskTrigger events of mask/masks attached to the script.
        /// </summary>
        protected abstract void MasksTriggerUnSubscribe();

        #endregion

        #region sdfVectors

        protected void ResetToDefaultVectors(ref Vector4 position)
        {
            position = new Vector4(0, 999999, 0, 0);
        }

        /// <summary>
        /// Sets default values for vectors to avoid glitches when not all of the arrays are used, but are passed to shader.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <param name="right"></param>
        /// <param name="up"></param>
        protected void ResetToDefaultVectors(ref Vector4 position, ref Vector4 forward, ref Vector4 right, ref Vector4 up)
        {
            ResetToDefaultVectors(ref position);
            //position = new Vector4(0, 999999, 0, 0);

            forward = Vector3.forward;
            up = Vector3.up;
            right = Vector3.right;
        }

        /// <summary>
        /// Set common vectors values used in sdf calculations.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        protected void SetCommonSDFVectors(Mask mask, ref Vector4 position, ref Vector4 scale)
        {
            var maskTransform = mask.transform;

            position = maskTransform.position;
            // Using abs to avoid negative scales
            //scale = new Vector4(Mathf.Abs(maskTransform.lossyScale.x), Mathf.Abs(maskTransform.lossyScale.y), Mathf.Abs(maskTransform.lossyScale.z), 0);
            scale = new Vector4((maskTransform.lossyScale.x), (maskTransform.lossyScale.y), (maskTransform.lossyScale.z), 0);
        }

        /// <summary>
        /// Set neccessary vectors values used in sdf calculations, based on mask type.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="forward"></param>
        /// <param name="right"></param>
        /// <param name="up"></param>
        /// <param name="position_2"></param>
        protected void SetSDFVectors(Mask mask, ref Vector4 position, ref Vector4 scale,
            ref Vector4 forward, ref Vector4 right, ref Vector4 up, ref Vector4 position_2)
        {
            var maskTransform = mask.transform;

            SetCommonSDFVectors(mask, ref position, ref scale);

            switch (type)
            {
                case Type.Plane:
                    up = maskTransform.up;
                    break;
                case Type.Box:
                    forward = maskTransform.forward;
                    up = maskTransform.up;
                    right = maskTransform.right;
                    scale = scale * 0.5f; // Divide box scale by 2
                    break;
                case Type.Sphere:
                    break;
                case Type.Ellipse:
                    forward = maskTransform.forward;
                    up = maskTransform.up;
                    right = maskTransform.right;
                    break;
                case Type.SolidAngle:
                    forward = maskTransform.forward;
                    up = maskTransform.up;
                    right = maskTransform.right;

                    float angle = Mathf.Deg2Rad * mask.angle * 0.5f;
                    scale = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), (mask.transform.lossyScale.y)); // Mathf.Abs
                    break;
                case Type.RoundCone:

                    var startTransform = mask.startTransform;
                    var endTransform = mask.endTransform;

                    if (startTransform == null || endTransform == null)
                        break;

                    position = startTransform.position;
                    position_2 = endTransform.position;

                    Vector3 radiuses = new Vector3(startTransform.lossyScale.x, endTransform.lossyScale.x, 0);
                    scale = radiuses;
                    break;
            }

        }


        /// <summary>
        /// Sets all sdf vectors properties. 
        /// </summary>
        /// <param name="material"></param>
        protected void SetVectorProperties(Material material)
        {
            if (material == null) return;
            UpdateCommonProperties(material);

            switch (type)
            {
                case Type.Plane:
                    UpdateUpVector(material);
                    break;
                case Type.Box:
                    UpdateRotation(material);
                    break;
                case Type.Sphere:
                    break;
                case Type.Ellipse:
                    UpdateRotation(material);
                    break;
                case Type.SolidAngle:
                    UpdateRotation(material);
                    break;
                case Type.RoundCone:
                    UpdateRoundCone(material);
                    break;
            }
        }

        protected abstract void UpdateCommonProperties(Material material);
        protected abstract void UpdateRotation(Material material);
        protected abstract void UpdateUpVector(Material material);
        protected abstract void UpdateRoundCone(Material material);

        #endregion

        #region materialsKeywords

        /// <summary>
        /// Disables all type keywords in material.
        /// </summary>
        /// <param name="material"></param>
        private void DisableMaterialTypes(Material material)
        {
            foreach (var keyword in material.enabledKeywords)
            {
                if (typeKeywords.Contains(keyword.name))
                {
                    material.DisableKeyword(keyword);
                }
            }
        }

        /// <summary>
        /// Disables all mask number keywords in material.
        /// </summary>
        /// <param name="material"></param>
        private void DisableMaterialMasksNumber(Material material)
        {
            foreach (var keyword in material.enabledKeywords)
            {
                if (masksNumberKeywords.Contains(keyword.name))
                {
                    material.DisableKeyword(keyword);
                }
            }
        }

        private void DisableMaterialDisplacement(Material material)
        {
            foreach (var keyword in material.enabledKeywords)
            {
                if (keyword.name == useDisplacementKeyword)
                {
                    material.DisableKeyword(useDisplacementKeyword);
                }
            }
        }

        /// <summary>
        /// Enables type keyword based on current mask type.
        /// </summary>
        /// <param name="material"></param>
        private void EnableMaterialType(Material material)
        {
            material.EnableKeyword(typeKeywords[(int)type]);
        }

        /// <summary>
        /// Enables mask number keyword based on current number of active masks.
        /// </summary>
        /// <param name="material"></param>
        private void EnableMaterialMasksNumber(Material material)
        {
            material.EnableKeyword(masksNumberKeywords[activeMasks]);
        }

        private void EnableMaterialDisplacement(Material material)
        {
            material.EnableKeyword(useDisplacementKeyword);
        }

        #endregion

        #region materialsProperties

        protected void UpdateMaterialsProperties(Color color)
        {
            foreach (var material in materialsDictionary)
            {
                material.Key.SetColor("_" + nameof(color), color);
            }
        }

        protected void UpdateMaterialsProperties(float vector)
        {
            foreach (var material in materialsDictionary)
            {
                material.Key.SetFloat("_" + nameof(vector), vector);
            }
        }

        protected void UpdateMaterialsProperties(Vector3 vector)
        {
            foreach (var material in materialsDictionary)
            {
                material.Key.SetVector("_" + nameof(vector), vector);
            }
        }

        protected void UpdateMaterialsProperties(bool value)
        {
            foreach (var material in materialsDictionary)
            {
                material.Key.SetInt("_" + nameof(value), value ? 1 : 0);
            }
        }

        protected void UpdateMaterialsProperties(Texture value)
        {
            if (value == null) return;
            foreach (var material in materialsDictionary)
            {
                material.Key.SetTexture("_" + nameof(value), value);
            }
        }

        /// <summary>
        /// Updates all materials's dissolve type properties attached to this script.
        /// </summary>
        protected virtual void UpdateMaterialsProperties()
        {
            if (dissolveType == DissolveType.Burn)
            {
                foreach (var mat in materialsDictionary)
                {
                    var material = mat.Key;
                    material.SetInt("_" + nameof(Invert), Invert ? 1 : 0);
                    material.SetTexture("_" + nameof(GuideTexture), GuideTexture);
                    material.SetFloat("_" + nameof(GuideTiling), GuideTiling);
                    material.SetFloat("_" + nameof(GuideStrength), GuideStrength);
                    material.SetInt("_" + nameof(UseBackColor), UseBackColor ? 1 : 0);
                    material.SetColor("_" + nameof(BackColor), BackColor);

                    material.SetFloat("_" + nameof(BurnHardness), BurnHardness);
                    material.SetFloat("_" + nameof(BurnOffset), BurnOffset);

                    material.SetFloat("_" + nameof(EmberOffset), EmberOffset);
                    material.SetFloat("_" + nameof(EmberSmoothness), EmberSmoothness);
                    material.SetFloat("_" + nameof(EmberWidth), EmberWidth);

                    material.SetColor("_" + nameof(EmberColor), EmberColor);
                    material.SetColor("_" + nameof(BurnColor), BurnColor);

                }
            }
            else if (dissolveType == DissolveType.Smooth)
            {
                foreach (var mat in materialsDictionary)
                {
                    var material = mat.Key;
                    material.SetInt("_" + nameof(Invert), Invert ? 1 : 0);
                    material.SetTexture("_" + nameof(GuideTexture), GuideTexture);
                    material.SetFloat("_" + nameof(GuideTiling), GuideTiling);
                    material.SetFloat("_" + nameof(GuideStrength), GuideStrength);
                    material.SetInt("_" + nameof(UseBackColor), UseBackColor ? 1 : 0);
                    material.SetColor("_" + nameof(BackColor), BackColor);

                    material.SetInt("_" + nameof(UseDithering), UseDithering ? 1 : 0);
                    material.SetColor("_" + nameof(EdgeColor), EdgeColor);
                    material.SetFloat("_" + nameof(EdgeWidth), EdgeWidth);
                    material.SetFloat("_" + nameof(EdgeSmoothness), EdgeSmoothness);
                    material.SetInt("_" + nameof(AffectAlbedo), AffectAlbedo ? 1 : 0);
                    material.SetColor("_" + nameof(GlareColor), GlareColor);
                    material.SetFloat("_" + nameof(GlareGuideStrength), GlareGuideStrength);
                    material.SetFloat("_" + nameof(GlareWidth), GlareWidth);
                    material.SetFloat("_" + nameof(GlareSmoothness), GlareSmoothness);
                    material.SetFloat("_" + nameof(GlareOffset), GlareOffset);
                }
            }
            else if (dissolveType == DissolveType.DisplacementOnly)
            {
                foreach (var mat in materialsDictionary)
                {
                    var material = mat.Key;
                    material.SetInt("_" + nameof(Invert), Invert ? 1 : 0);
                    material.SetColor("_" + nameof(DisplacementColor), DisplacementColor);
                }
            }

            if (UseDisplacement)
            {
                foreach (var mat in materialsDictionary)
                {
                    var material = mat.Key;
                    material.SetInt("_" + nameof(DisplacementPerVertex), DisplacementPerVertex ? 1 : 0);
                    material.SetFloat("_" + nameof(DisplacementSmoothness), DisplacementSmoothness);
                    material.SetFloat("_" + nameof(DisplacementOffset), DisplacementOffset);

                    material.SetVector("_" + nameof(RotationAxis), RotationAxis);
                    material.SetFloat("_" + nameof(RotationMin), RotationMin);
                    material.SetFloat("_" + nameof(RotationMax), RotationMax);
                    material.SetVector("_" + nameof(RandomPositionOffset), RandomPositionOffset);
                    material.SetVector("_" + nameof(PositionOffset), PositionOffset);
                    material.SetFloat("_" + nameof(Scale), Scale);
                    material.SetVector("_" + nameof(NormalOffset), NormalOffset);

                }
            }
        }

        #endregion
    }
}