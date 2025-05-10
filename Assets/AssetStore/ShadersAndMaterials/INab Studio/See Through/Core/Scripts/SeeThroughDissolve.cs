using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace INab.WorldAlchemy
{
    [ExecuteAlways]
    /// <summary>
    /// See through dissolve with up to 6 different sphere masks at a time. Local ShaderType and auto detection is on.
    /// </summary>
    public class SeeThroughDissolve : WorldDissolveBase
    {
        #region staticProperties

        /// <summary>
        /// Defines the maximum number of masks that can be active on a material simultaneously.
        /// </summary>
        public static int MaxMasksSeeThrough = 6;

        private static string useStableScreenRadius = "_USESTABLESCREENRADIUS";


        #endregion

        #region privateProperties

        private List<Dictionary<Renderer, int>> renderersDictionaries = new List<Dictionary<Renderer, int>>();

        [SerializeField, Tooltip("List of masks used in the world dissolve effect. Masks at the top have higher priority.")]
        private List<Mask> masksList = new List<Mask>();

        public Transform CameraTransform
        {
            get
            {
                return cameraTransform;
            }
            private set { }

        }
        [SerializeField, Tooltip("Camera transform used for mask collider in see-through effect.")]
        private Transform cameraTransform;

        [SerializeField, Tooltip("Whether to use screen-stable radius.")]
        private bool useScreenStableRadius = false;
        public bool UseScreenStableRadius
        {
            get { return useScreenStableRadius; }
            private set { }
        }

        [SerializeField, Range(0, 0.7f), Tooltip("Multiplier for screen radius.")]
        private float screenRadiusMultiplier = 0.2f;
        public float ScreenRadiusMultiplier
        {
            get { return screenRadiusMultiplier; }
            set
            {
                screenRadiusMultiplier = value;
                UpdateMaterialsProperties(ScreenRadiusMultiplier);
            }
        }

        [SerializeField, Tooltip("Whether to use depth offset.")]
        private bool useDepthOffset = true;
        public bool UseDepthOffset
        {
            get { return useDepthOffset; }
            set
            {
                useDepthOffset = value;
                UpdateMaterialsProperties(UseDepthOffset);
            }
        }

        [SerializeField, Range(-2, 2), Tooltip("Depth offset value.")]
        private float depthOffset = 0.6f;
        public float DepthOffset
        {
            get { return depthOffset; }
            set
            {
                depthOffset = value;
                UpdateMaterialsProperties(DepthOffset);
            }
        }

        [SerializeField, Range(0, 3), Tooltip("Smoothness of the offset.")]
        private float offsetSmoothness = 0.8f;
        public float OffsetSmoothness
        {
            get { return offsetSmoothness; }
            set
            {
                offsetSmoothness = value;
                UpdateMaterialsProperties(OffsetSmoothness);
            }
        }

        [SerializeField, Tooltip("Forces shaders properties (e.g. edge,ember smoothness, width, etc...) to maintain the same look no matter the distance to the camera. Works best when there is only one active mask.")]
        private bool stabilizeDistanceLook = false;
        public bool StabilizeDistanceLook
        {
            get { return stabilizeDistanceLook; }
            private set
            {
                stabilizeDistanceLook = value;
                UpdateMaterialsProperties(StabilizeDistanceLook);
            }
        }
        #endregion

        #region sdfVectors

        private Vector4[] positions = new Vector4[MaxMasksSeeThrough];
        private Vector4[] scales = new Vector4[MaxMasksSeeThrough];

        #endregion

        #region autoDetection


        private void OnEnable()
        {
            // We are udpating sdf parameters e.g. scales and positions of masks before start to ensure every
            // mask that is triggering when the game starts gets property values
            UpdateParameters();
        }

        public override void Start()
        {
            base.Start();

            // Fills lists with default values
            for (int i = 0; i < MaxMasksSeeThrough; i++)
            {
                var dictionary = new Dictionary<Renderer, int>();
                renderersDictionaries.Add(dictionary);
            }

            // Set default scale values to 0 in all materials udpated in editor
            float[] floatScales = new float[MaxMasksSeeThrough];

            for (int i = 0; i < MaxMasksSeeThrough; i++)
            {
                floatScales[i] = 0;
            }

            foreach (var material in materialsList)
            {
                material.SetFloatArray("Scales", floatScales);
            }

        }

        private void UpdatePropertyBlockScales(Renderer renderer, int id, float value)
        {
            MaterialPropertyBlock mtb = new MaterialPropertyBlock(); ;
            renderer.GetPropertyBlock(mtb);

            List<float> floatScales = new List<float>();
            mtb.GetFloatArray("Scales", floatScales);

            if (floatScales.Count < 1)
            {
                for (int i = 0; i < MaxMasksSeeThrough; i++)
                {
                    floatScales.Add(0);
                }
            }
            floatScales[id] = value;
            mtb.SetFloatArray("Scales", floatScales);

            renderer.SetPropertyBlock(mtb);
        }

        public void UpdateMaskScale(int id)
        {
            foreach (var item in renderersDictionaries[id])
            {
                UpdatePropertyBlockScales(item.Key, id, scales[id].x);
            }
        }

        protected override void TriggerEnter(Renderer renderer, int id = 0)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                // Add material to materials dictionary without updating material keywords
                if (!materialsDictionary.ContainsKey(material))
                {
                    materialsDictionary.Add(material, 1);
                }
                else
                {
                    materialsDictionary[material]++;
                }
            }

            if (!renderersDictionaries[id].ContainsKey(renderer))
            {
                renderersDictionaries[id].Add(renderer, 1);
            }
            else
            {
                renderersDictionaries[id][renderer]++;
            }

            UpdatePropertyBlockScales(renderer, id, scales[id].x);
        }

        protected override void TriggerExit(Renderer renderer, int id = 0)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                // Remove material from materials dictionary without updating material keywords
                if (materialsDictionary.ContainsKey(material))
                {
                    materialsDictionary[material]--;

                    if (materialsDictionary[material] <= 0)
                    {
                        materialsDictionary.Remove(material);
                    }
                }
            }

            if (renderersDictionaries[id].ContainsKey(renderer))
            {
                renderersDictionaries[id][renderer]--;

                if (renderersDictionaries[id][renderer] <= 0)
                {
                    // We are removing renderer only when not using alpha fade
                    renderersDictionaries[id].Remove(renderer);
                    UpdatePropertyBlockScales(renderer, id, 0);
                }
            }

        }

        #endregion

        #region overrideFunctions

        public override void UpdateMaterialsParameters()
        {
            //foreach (var mat in materialsDictionary)
            //{
            //    var material = mat.Key;
            //    material.SetVector("_CameraPosition", cameraTransform.position);
            //}

            base.UpdateMaterialsParameters();
        }

        protected override void UpdateMaterialsProperties()
        {
            base.UpdateMaterialsProperties();

            foreach (var mat in materialsDictionary)
            {
                var material = mat.Key;
                material.SetFloat("_" + nameof(ScreenRadiusMultiplier), ScreenRadiusMultiplier);
                material.SetFloat("_" + nameof(OffsetSmoothness), OffsetSmoothness);
                material.SetFloat("_" + nameof(DepthOffset), DepthOffset);
                material.SetInt("_" + nameof(UseDepthOffset), UseDepthOffset ? 1 : 0);
                material.SetInt("_" + nameof(StabilizeDistanceLook), StabilizeDistanceLook ? 1 : 0);

            }
        }

        public override void OnValidate()
        {
            base.OnValidate();

            copyEditorLists = false;

            if (UseDisplacement)
            {
                ChangeUseDisplacement(false);
            }


            if (type != Type.Sphere) ChangeType(Type.Sphere);

            int id = 0;
            foreach (var mask in masksList)
            {
                //if (mask == null) return;
                mask.ID = id;
                id++;

                mask.SeeThroughMask = true;
            }

            useAutoDetection = true;

            if (cameraTransform != null) UpdateCameraTransform(cameraTransform);
        }

        protected override void MasksTriggerSubscribe()
        {
            foreach (var mask in masksList)
            {
                mask.OnMaskTriggerEnter += TriggerEnter;
                mask.OnMaskTriggerExit += TriggerExit;
            }
        }

        protected override void MasksTriggerUnSubscribe()
        {
            foreach (var mask in masksList)
            {
                mask.OnMaskTriggerEnter -= TriggerEnter;
                mask.OnMaskTriggerExit -= TriggerExit;
            }
        }

        protected override void UpdateParameters()
        {
            int i = 0;

            // Automatically checks if we have enought masks to cover active masks number 
            int numberOfMasks = Mathf.Min(ActiveMasks, masksList.Count);

            for (int defaultIndex = numberOfMasks; defaultIndex < MaxMasksSeeThrough; defaultIndex++)
            {
                ResetToDefaultVectors(ref positions[defaultIndex]);
            }

            foreach (var mask in masksList)
            {
                if (i + 1 > numberOfMasks) break;

                if (mask != null) SetCommonSDFVectors(mask, ref positions[i], ref scales[i]);

                i++;
            }
        }

        protected override void CheckMasksForNulls()
        {
            List<Mask> newMasksList = new List<Mask>();
            foreach (var mask in masksList)
            {
                if (mask != null)
                {
                    newMasksList.Add(mask);
                }
            }
            masksList = newMasksList;
        }

        protected override void UpdateMasksDetection()
        {
            foreach (var mask in masksList)
            {
                mask.DetectionLayerMask = detectionLayerMask;
                mask.UseAutoDetection = useAutoDetection;
            }
        }

        protected override void UpdateCommonProperties(Material material)
        {
            // We update positions in editor and in runtime on each material from materials dictionary
            material.SetVectorArray("Positions", positions);

            // We update scales only in editor
            if (!Application.isPlaying)
            {
                float[] floatScales = new float[MaxMasksSeeThrough];

                for (int i = 0; i < MaxMasksSeeThrough; i++)
                {
                    floatScales[i] = scales[i].x;
                }

                material.SetFloatArray("Scales", floatScales);
            }
        }

        #endregion

        #region publicFunctions

        /// <summary>
        /// Update the screen stable radius property.
        /// </summary>
        /// <param name="useScreenStableRadius"></param>
        public void ChangeUseScreenStableRadius(bool useScreenStableRadius)
        {
            this.useScreenStableRadius = useScreenStableRadius;

            if (UseScreenStableRadius)
            {
                foreach (var material in materialsDictionary)
                {
                    DisableScreenStableRadius(material.Key);
                    EnableScreenStableRadius(material.Key);
                }
            }
            else
            {
                foreach (var material in materialsDictionary)
                {
                    DisableScreenStableRadius(material.Key);
                }
            }
        }

        private void DisableScreenStableRadius(Material material)
        {
            foreach (var keyword in material.enabledKeywords)
            {
                if (keyword.name == useStableScreenRadius)
                {
                    material.DisableKeyword(useStableScreenRadius);
                }
            }
        }

        private void EnableScreenStableRadius(Material material)
        {
            material.EnableKeyword(useStableScreenRadius);
        }

        /// <summary>
        /// Updates the camera transform used for mask collider in see-through effect.
        /// </summary>
        /// <param name="newCameraTransform"></param>
        public void UpdateCameraTransform(Transform newCameraTransform)
        {
            cameraTransform = newCameraTransform;

            foreach (var mask in masksList)
            {
                if (cameraTransform) mask.CameraTransform = cameraTransform;
            }
        }

        /// <summary>
        /// Adds a mask to the dissolve effect, subscribing to its events if auto detection is used.
        /// </summary>
        /// <param name="mask">The mask to add.</param>
        public void AddMask(Mask mask)
        {
            if (mask == null) return;

            mask.Type = Type;
            mask.DetectionLayerMask = detectionLayerMask;
            mask.UseAutoDetection = useAutoDetection;
            mask.SeeThroughMask = true;


            {
                mask.UpdateCollider();
                mask.OnMaskTriggerEnter += TriggerEnter;
                mask.OnMaskTriggerExit += TriggerExit;
            }

            if (cameraTransform) mask.CameraTransform = cameraTransform;

            masksList.Add(mask);
            mask.ID = masksList.Count - 1;
        }

        /// <summary>
        /// Removes a specific mask from the dissolve effect, unsubscribing from its events.
        /// </summary>
        /// <param name="mask">The mask to remove.</param>
        /// <returns>True if the mask was successfully removed, false otherwise.</returns>
        public bool RemoveMask(Mask mask)
        {
            if (mask == null) return false;
            mask.SeeThroughMask = false;

            mask.DestroyCollider();
            mask.OnMaskTriggerEnter -= TriggerEnter;
            mask.OnMaskTriggerExit -= TriggerExit;

            return masksList.Remove(mask);
        }

        /// <summary>
        /// Removes a mask at a specific index from the dissolve effect, unsubscribing from its events.
        /// </summary>
        /// <param name="index">The index of the mask to remove.</param>
        /// <returns>True if a mask was removed, false otherwise.</returns>
        public bool RemoveMask(int index)
        {
            if (masksList.Count < index) return false;

            var mask = masksList[index];
            if (mask != null)
            {
                mask.DestroyCollider();
                mask.OnMaskTriggerEnter -= TriggerEnter;
                mask.OnMaskTriggerExit -= TriggerExit;
            }

            masksList.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes all masks from the dissolve effect, unsubscribing from their events.
        /// </summary>
        public void RemoveAllMasks()
        {
            foreach (var mask in masksList)
            {
                if (mask != null)
                {
                    mask.DestroyCollider();
                    mask.OnMaskTriggerEnter -= TriggerEnter;
                    mask.OnMaskTriggerExit -= TriggerExit;
                }
            }
            masksList.Clear();
        }

        /// <summary>
        /// Checks if a specific mask is part of the dissolve effect.
        /// </summary>
        /// <param name="mask">The mask to check for.</param>
        /// <returns>True if the mask is part of the dissolve effect, false otherwise.</returns>
        public bool ContainsMask(Mask mask)
        {
            if (mask == null) return false;

            return masksList.Contains(mask);
        }

        /// <summary>
        /// Returns the count of masks currently part of the dissolve effect.
        /// </summary>
        /// <returns>The number of masks in the dissolve effect.</returns>
        public int MasksListCount()
        {
            return masksList.Count;
        }

        #endregion

        #region notUsedOverrides

        protected override void UpdateMasksType()
        {
            foreach (var mask in masksList)
            {
                mask.Type = Type.Sphere;
            }
        }

        protected override void UpdateRotation(Material material)
        {
            // To Delete
        }

        protected override void UpdateUpVector(Material material)
        {
            // To Delete
        }

        protected override void UpdateRoundCone(Material material)
        {
            // To Delete
        }

        #endregion
    }
}