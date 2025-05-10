using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace INab.WorldAlchemy
{
    [Serializable]
    public abstract class MaskObjectBase
    {
        [Header("General Settings"), Tooltip("Transform of the mask, used for positioning and scaling.")]
        public Transform maskTransform;

        [Header("Follow Settings")]
        [Tooltip("Enables the mask to follow a target transform.")]
        public bool enableFollow;

        [Tooltip("The target transform that the mask should follow.")]
        public Transform followTarget;

        [Tooltip("Offset distance towards the camera from the target position. Default: 0.3")]
        public float followOffsetDistance;

        [Tooltip("Enables smooth transition effect while following the target.")]
        public bool enableSmoothTransition;

        [Tooltip("Time taken for the smooth follow effect to stabilize. Default: 0.02 seconds")]
        public float smoothFollowDuration;

        private Vector3 followVelocity; // Internal use for smooth damping calculation

        [Header("Scaling Settings")]
        [Tooltip("Enables dynamic scaling of the mask based on the presence of objects between the mask and the camera.")]
        public bool enableDynamicScaling;

        [Tooltip("Layer mask used to detect objects that interfere between the camera and the mask.")]
        public LayerMask detectionLayer;

        [Tooltip("Mask scale when no obstruction is detected. Default: 0.01")]
        public float minScale;

        [Tooltip("Mask scale when obstruction is detected. Default: 1")]
        public float maxScale;

        [Tooltip("Time taken for the mask scale to adjust smoothly. Default: 0.05 seconds")]
        public float scaleAdjustmentDuration;

        private float scaleVelocity; // Internal use for smooth scaling calculation

        private Vector3 lastMaskPosition; // Tracks the last position of the mask for dynamic adjustments


        [Header("Debugging")]
        [Tooltip("Toggles visualization of the raycast used for detection.")]
        public bool enableDebugRaycast;

        protected void AdjustTransformScale(Transform transform, float targetScale)
        {
            transform.localScale = Vector3.one * targetScale;
        }

        public void InitializeMaskScale()
        {
            if (enableDynamicScaling) AdjustTransformScale(maskTransform, minScale);
        }

        public void UpdateFollowTarget(Transform cameraTransform)
        {
            if (!enableFollow) return;

            Vector3 targetDirection = cameraTransform.position - maskTransform.position;
            Vector3 normalizedDirection = targetDirection.normalized;

            Vector3 newPosition = followTarget.position + normalizedDirection * followOffsetDistance;

            if (enableSmoothTransition)
            {
                maskTransform.position = Vector3.SmoothDamp(maskTransform.position, newPosition, ref followVelocity, smoothFollowDuration);
            }
            else
            {
                maskTransform.position = newPosition;
            }
        }

        public void CheckForObstructions(Vector3 cameraPosition)
        {
            lastMaskPosition = maskTransform.position;

            if (!enableDynamicScaling) return;

            Vector3 directionToCamera = lastMaskPosition - cameraPosition;
            if (enableDebugRaycast)
            {
                Debug.DrawRay(cameraPosition, directionToCamera.normalized * (directionToCamera.magnitude + followOffsetDistance), Color.cyan);
            }

            RaycastHit hit;
            if (Physics.Raycast(cameraPosition, directionToCamera.normalized, out hit, directionToCamera.magnitude + followOffsetDistance, detectionLayer))
            {
                AdjustMaskScale(maxScale);
            }
            else
            {
                AdjustMaskScale(minScale);
            }
        }

        protected virtual void AdjustMaskScale(float targetScale)
        {
            float currentScale = maskTransform.localScale.x;
            float newScale = Mathf.SmoothDamp(currentScale, targetScale, ref scaleVelocity, scaleAdjustmentDuration);

            AdjustTransformScale(maskTransform, newScale);
        }
    }

    public abstract class SeeThroughHelperBase : MonoBehaviour
    {
        private Transform m_CameraTransform;
        private Vector3 previousCameraPosition;

        /// <summary>
        /// Set the camera transform for the see-through helper.
        /// </summary>
        /// <param name="cameraTransform"></param>
        public void SetCameraTransform(Transform cameraTransform)
        {
            m_CameraTransform = cameraTransform;
        }

        protected void StartCall(MaskObjectBase maskObject)
        {
            maskObject.InitializeMaskScale();
        }

        protected void LateUpdateCall(MaskObjectBase maskObject)
        {
            maskObject.UpdateFollowTarget(m_CameraTransform);
        }

        protected void UpdateCall(MaskObjectBase maskObject)
        {
            maskObject.CheckForObstructions(previousCameraPosition);
        }

        protected virtual void Update()
        {
            previousCameraPosition = m_CameraTransform.position;
        }
    }
}