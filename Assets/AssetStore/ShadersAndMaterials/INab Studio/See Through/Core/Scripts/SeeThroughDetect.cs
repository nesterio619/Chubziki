using System.Collections.Generic;
using UnityEngine;

namespace INab.WorldAlchemy
{
    /// <summary>
    /// Detects obstacles that obstruct the view of the camera.
    /// </summary>
    public class SeeThroughDetect : MonoBehaviour
    {
        [Tooltip("The fade targets that will be affected by obstruction detection.")]
        public List<SeeThroughFadeTarget> fadeTargets = new List<SeeThroughFadeTarget>();

        [Tooltip("The camera to use for raycasting.")]
        public Transform cameraTransform;

        [Tooltip("The target object whose visibility is being checked.")]
        public Transform targetTransform;

        [Tooltip("The layer mask that determines which objects will be detected by raycasts.")]
        public LayerMask detectionLayer;

        [Tooltip("Extends the raycast's length beyond the target position.")]
        public float targetOffsetDistance = 0.0f;

        [Tooltip("Extends the raycast's length beyond the camera position. Use when your camera might be inside a collider.")]
        public float cameraOffsetDistance = 0.0f;

        [Tooltip("Visualizes the raycast in the scene for debugging.")]
        public bool enableDebugRaycast = false;

        public void Update()
        {
            CheckForObstructions();
        }

        /// <summary>
        /// Performs raycasting to detect obstacles between the camera and target object.
        /// </summary>
        private void CheckForObstructions()
        {
            // Get relevant positions
            var cameraPosition = cameraTransform.position;
            var targetPosition = targetTransform.position;

            // Calculate ray direction
            Vector3 directionToCamera = targetPosition - cameraPosition;

            var origin = cameraPosition - directionToCamera.normalized * cameraOffsetDistance;
            var direction = directionToCamera.normalized;
            var distance = directionToCamera.magnitude + targetOffsetDistance + cameraOffsetDistance;

            // Visualize raycast for debugging (if enabled)
            if (enableDebugRaycast)
            {
                Debug.DrawRay(origin, direction * distance, Color.cyan);
            }

            // Perform raycast and check for collisions
            var hits = Physics.RaycastAll(origin, direction, distance, detectionLayer);

            // Reset the current frame detect flag for all fade targets
            foreach (var item in fadeTargets)
            {
                item.SetDetectedFlag(false);
            }

            // Mark any fade targets whose targets are hit by the raycast as obstructed in the current frame
            foreach (var hit in hits)
            {
                foreach (var item in fadeTargets)
                {
                    foreach (var targetObject in item.triggerGameObjects)
                    {
                        if (targetObject == hit.collider.gameObject)
                        {
                            item.SetDetectedFlag(true);
                            break; 
                        }
                    }
                }
            }
        }
    }
}
