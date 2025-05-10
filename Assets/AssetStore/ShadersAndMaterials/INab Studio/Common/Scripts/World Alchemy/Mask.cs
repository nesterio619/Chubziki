using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace INab.WorldAlchemy
{
    /// <summary>
    /// Mask script.
    /// </summary>
    [ExecuteAlways]
    public class Mask : MonoBehaviour
    {
        #region UserExposed

        [SerializeField, Tooltip("Configuration settings for the mask's visual guides and collision detection.")]
        public MaskSettings maskSettings;

        [SerializeField, Range(1, 2), Tooltip("Scale factor for the collider, relative to the mask's size.")]
        public float colliderScale = 1f;

        // =========== Solid angle ===========
        [Tooltip("The angle of a solid cone mask.")]
        public float angle;

        [Range(0, 3), Tooltip("Adjusts the size of the collider and preview based on the angle of a solid cone mask.")]
        public float angleAdjust = 1;

        // =========== Round Cone ===========
        [Tooltip("Adds the radiusAdjust value to the radius of the round cone collider.")]
        public float radiusAdjust = 0;

        // Start and end transforms for a round cone mask
        public Transform startTransform;
        public Transform endTransform;

        #endregion

        #region Internal

        /// <summary>
        /// INTERAL: Determines the mask's shape and associated behavior.
        /// </summary>
        public Type Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                if (usePreview)
                {
                    UpdatePreview();
                }
            }
        }
        [SerializeField, Tooltip("Determines the mask's shape and associated behavior.")]
        private Type type = Type.Sphere;

        /// <summary>
        /// INTERAL: Enables the mask to be used as a see-through effect.
        /// </summary>
        public bool SeeThroughMask
        {
            get
            {
                return seeThroughMask;
            }
            set
            {
                seeThroughMask = value;
            }
        }

        [SerializeField]
        private bool seeThroughMask = false;

        /// <summary>
        /// INTERAL: Camera transform used to create mask collider for see-thrgouh effect.
        /// </summary>
        public Transform CameraTransform
        {
            get
            {
                return cameraTransform;
            }
            set
            {
                cameraTransform = value;
            }
        }

        [SerializeField]
        private Transform cameraTransform;

        /// <summary>
        /// INTERAL: The layer mask used for auto-detecting interactions with this mask.
        /// </summary>
        public LayerMask DetectionLayerMask
        {
            get
            {
                return detectionLayerMask;
            }
            set
            {
                detectionLayerMask = value;
            }
        }
        [SerializeField]
        private LayerMask detectionLayerMask;

        /// <summary>
        /// INTERAL: Enables automatic detection of objects using the specified tag.
        /// </summary>
        public bool UseAutoDetection
        {
            get
            {
                return useAutoDetection;
            }
            set
            {
                useAutoDetection = value;
            }
        }
        [SerializeField]
        protected bool useAutoDetection = false;

        /// <summary>
        /// INTERAL: The tag used for auto-detecting interactions with this mask.
        /// </summary>
        public bool HasMaskCollider
        {
            get
            {
                if (maskCollider != null) return true;
                else return false;
            }
            private set { }
        }

        [SerializeField]
        private GameObject maskCollider;
        [SerializeField]
        private Collider maskColliderComponent;

        /// <summary>
        /// INTERAL: The ID of the mask. Used for distinguishing between masks when using see-through auto detection.
        /// </summary>
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        [SerializeField]
        private int id = 0;

        /// <summary>
        /// INTERAL: Enables a visual preview of the mask in the scene.
        /// </summary>
        public bool UsePreview
        {
            get
            {
                return usePreview;
            }
            set
            {
                usePreview = value;
            }
        }
        [SerializeField, Tooltip("Enables a visual preview of the mask in the scene.")]
        protected bool usePreview = false;

        /// <summary>
        /// INTERAL: Indicates whether the mask has a preview renderer component.
        /// </summary>
        public bool HasMaskPreview
        {
            get
            {
                if (maskPreviewRenderer != null) return true;
                else return false;
            }
            private set { }
        }

        /// <summary>
        /// INTERAL: Disables the mask preview when the application is playing.
        /// </summary>
        public bool OnlyEditorPreview
        {
            get
            {
                return onlyEditorPreview;
            }
            set
            {
                onlyEditorPreview = value;
            }
        }
        [SerializeField, Tooltip("Disables the mask preview when the application is playing.")]
        protected bool onlyEditorPreview = false;

        /// <summary>
        /// INTERAL: Mask preview mesh filter component.
        /// </summary>
        public MeshFilter maskPreviewFilter;

        /// <summary>
        /// INTERAL: Mask preview mesh renderer component.
        /// </summary>
        public MeshRenderer maskPreviewRenderer;

        #endregion

        #region privateMethods

        private void Start()
        {
            if (onlyEditorPreview && Application.isPlaying)
            {
                if (maskPreviewRenderer) maskPreviewRenderer.enabled = false;
            }
            else
            {
                if (maskPreviewRenderer) maskPreviewRenderer.enabled = true;
            }
        }

        private void OnValidate()
        {
            TransformScaleClamp();
            ScaleCollider();
        }

        private void Update()
        {
            if (transform.hasChanged)
            {
                TransformScaleClamp();
                transform.hasChanged = false;
            }

            if (type == Type.RoundCone)
            {
                if (startTransform != null && endTransform != null)
                {
                    float radius = Mathf.Min(startTransform.lossyScale.x, endTransform.lossyScale.x) + radiusAdjust;
                    RoundConeColliderAdjust(startTransform.position, endTransform.position, radius);
                }
            }

            if (seeThroughMask)
            {
                if (CameraTransform != null && maskCollider != null)
                {
                    float worldScaleAdjust = 1f / maskCollider.transform.lossyScale.x;

                    float radius = transform.lossyScale.x;

                    //Debug.Log(radius);

                    var diff = transform.position - CameraTransform.position;
                    Vector3 normal = diff.normalized * transform.lossyScale.x;

                    RoundConeColliderAdjust(transform.position + normal, CameraTransform.position, radius * colliderScale, worldScaleAdjust);
                }
            }
        }

        /// <summary>
        /// Adjusts properties of the capsule collider based on start/end transforms for the round cone mask.
        /// </summary>
        private void RoundConeColliderAdjust(Vector3 startPos, Vector3 endPos, float radius, float worldScaleAdjust = 1)
        {
            if (maskCollider == null) return;

            if (maskColliderComponent == null)
            {
                maskColliderComponent = maskCollider.GetComponent<Collider>();
            }

            var middle = (startPos + endPos) / 2;
            maskCollider.transform.position = middle;

            var capsuleCollider = (CapsuleCollider)maskColliderComponent;
            capsuleCollider.radius = radius * worldScaleAdjust;

            var diff = startPos - endPos;
            capsuleCollider.height = diff.magnitude * worldScaleAdjust;

            maskCollider.transform.up = diff.normalized;
        }

        /// <summary>
        /// Adjusts the scale of the transform based on the selected mask type.
        /// </summary>
        private void TransformScaleClamp()
        {
            if (type == Type.SolidAngle)
            {
                float scale = transform.localScale.y * angleAdjust;
                transform.localScale = new Vector3(scale, transform.localScale.y, scale);
            }

            else if (type == Type.Sphere)
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.x, transform.localScale.x);
            }

            else if (type == Type.RoundCone)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }

        /// <summary>
        /// Scales the collider to match the mask's size.
        /// </summary>
        private void ScaleCollider()
        {
            if (maskCollider)
            {
                maskCollider.transform.localScale = new Vector3(colliderScale, colliderScale, colliderScale);
            }
        }

        #endregion

        #region UserExposedMethods

        /// <summary>
        /// Updates the collider based on the mask type. 
        /// </summary>
        public void UpdateCollider()
        {
            if (maskCollider) DestroyCollider();

            maskCollider = new GameObject("Mask Collider");

            maskCollider.AddComponent<MaskCollider>();
            maskCollider.transform.parent = transform;
            maskCollider.transform.localPosition = Vector3.zero;
            maskCollider.transform.localRotation = Quaternion.identity;

            maskCollider.transform.localScale = new Vector3(colliderScale, colliderScale, colliderScale);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif


            if (type == Type.Sphere)
            {
                if (seeThroughMask)
                {
                    maskColliderComponent = maskCollider.AddComponent<CapsuleCollider>();
                    maskColliderComponent.isTrigger = true;
                }
                else
                {
                    maskColliderComponent = maskCollider.AddComponent<SphereCollider>();
                    ((SphereCollider)maskColliderComponent).radius = 1;
                    maskColliderComponent.isTrigger = true;
                }
            }
            else if (type == Type.Box)
            {
                maskColliderComponent = maskCollider.AddComponent<BoxCollider>();
                maskColliderComponent.isTrigger = true;
            }
            else if (type == Type.RoundCone)
            {
                maskColliderComponent = maskCollider.AddComponent<CapsuleCollider>();
                maskColliderComponent.isTrigger = true;
            }
            else
            {
                maskColliderComponent = maskCollider.AddComponent<MeshCollider>();
                var collider = (MeshCollider)maskColliderComponent;
                collider.convex = true;
                collider.isTrigger = true;

                switch (type)
                {
                    case Type.Plane:
                        collider.sharedMesh = maskSettings.PlaneMesh;
                        break;
                    case Type.Ellipse:
                        collider.sharedMesh = maskSettings.SphereMesh;
                        break;
                    case Type.SolidAngle:
                        collider.sharedMesh = maskSettings.SolidAngleMesh;
                        break;
                        //case Type.RoundCone:
                        //    // Auto-detection feature doesn't support round cone
                        //    break;
                }
            }

            if (maskCollider.GetComponent<Rigidbody>() == null)
            {
                var rb = maskCollider.AddComponent<Rigidbody>();
                rb.isKinematic = true;
            }
            else
            {
                maskCollider.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        /// <summary>
        /// Destroys the existing collider.
        /// </summary>
        public void DestroyCollider()
        {
            if (Application.isPlaying)
            {
                Destroy(maskCollider.gameObject);
            }
            else
            {
                DestroyImmediate(maskCollider.gameObject);
            }

            maskCollider = null;
        }

        /// <summary>
        /// Creates start and end transforms for round cone masks.
        /// </summary>
        public void AddRoundConeTransforms()
        {
            // Create the start transform and configure its properties.
            GameObject start = new GameObject("Start Transform");
            start.transform.localPosition = transform.position;
            start.transform.localScale = new Vector3(1, 1, 1);
            start.AddComponent<RoundConeHelper>().Setup(this);
            startTransform = start.transform;

            // Create the end transform and configure its properties.
            GameObject end = new GameObject("End Transform");
            end.transform.localPosition = transform.position + new Vector3(2, 0, 0);
            end.transform.localScale = new Vector3(1, 1, 1);
            end.AddComponent<RoundConeHelper>().Setup(this);
            endTransform = end.transform;

            // Make the mask object parent of transforms
            startTransform.parent = transform;
            endTransform.parent = transform;
        }

        /// <summary>
        /// Destroys the transforms created for round cone masks.
        /// </summary>
        public void DestroyRoundConeTransforms()
        {
            if (Application.isPlaying)
            {
                Destroy(startTransform.gameObject);
                Destroy(endTransform.gameObject);
            }
            else
            {
                DestroyImmediate(startTransform.gameObject);
                DestroyImmediate(endTransform.gameObject);
            }

            startTransform = null;
            endTransform = null;
        }

        #endregion

        #region InternalMethods

        // Events for entering and exiting mask triggers
        public delegate void EnterTriggerEvent(Renderer renderer, int id = 0);
        public event EnterTriggerEvent OnMaskTriggerEnter;

        public delegate void ExitTriggerEvent(Renderer renderer, int id = 0);
        public event ExitTriggerEvent OnMaskTriggerExit;

        /// <summary>
        /// INTERAL: Invokes the OnMaskTriggerEnter event.
        /// </summary>
        /// <param name="collider"></param>
        public void CallTriggerEnter(Renderer renderer, int id = 0)
        {
            if (OnMaskTriggerEnter != null) OnMaskTriggerEnter(renderer,id);
        }

        /// <summary>
        /// INTERAL: Invokes the OnMaskTriggerExit event.
        /// </summary>
        /// <param name="collider"></param>
        public void CallTriggerExit(Renderer renderer, int id = 0)
        {
            if (OnMaskTriggerExit != null) OnMaskTriggerExit(renderer,id);
        }

        /// <summary>
        /// INTERAL: Updates the collider based on the mask type. 
        /// </summary>
        public void UpdatePreview()
        {
            DestroyPreview();

            if (gameObject.GetComponent<MeshFilter>() != null)
            {
                maskPreviewFilter = gameObject.GetComponent<MeshFilter>();
            }
            else
            {
                maskPreviewFilter = gameObject.AddComponent<MeshFilter>();
            }

            if (gameObject.GetComponent<MeshRenderer>() != null)
            {
                maskPreviewRenderer = gameObject.GetComponent<MeshRenderer>();
            }
            else
            {
                maskPreviewRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            maskPreviewRenderer.material = maskSettings.PreviewMaterial;
            maskPreviewRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            maskPreviewRenderer.receiveShadows = false;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif

            switch (type)
            {
                case Type.Plane:
                    maskPreviewFilter.sharedMesh = maskSettings.PreviewPlaneMesh;
                    break;
                case Type.Box:
                    maskPreviewFilter.sharedMesh = maskSettings.PreviewBoxMesh;
                    break;
                case Type.Sphere:
                    maskPreviewFilter.sharedMesh = maskSettings.PreviewSphereMesh;
                    break;
                case Type.Ellipse:
                    maskPreviewFilter.sharedMesh = maskSettings.PreviewSphereMesh;
                    break;
                case Type.SolidAngle:
                    maskPreviewFilter.sharedMesh = maskSettings.PreviewSolidAngleMesh;
                    break;
                case Type.RoundCone:
                    // Preview feature doesn't support round cone
                    break;
            }

        }

        /// <summary>
        /// INTERAL: Destroys the existing preview
        /// </summary>
        public void DestroyPreview()
        {
            if (Application.isPlaying)
            {
                Destroy(maskPreviewFilter);
                Destroy(maskPreviewRenderer);
            }
            else
            {
                DestroyImmediate(maskPreviewFilter);
                DestroyImmediate(maskPreviewRenderer);
            }

            maskPreviewFilter = null;
            maskPreviewRenderer = null;
        }

        #endregion

    }
}