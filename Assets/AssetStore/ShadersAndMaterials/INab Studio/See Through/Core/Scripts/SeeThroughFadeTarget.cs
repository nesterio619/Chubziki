using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace INab.WorldAlchemy
{
    public class SeeThroughFadeTarget : MonoBehaviour
    {
        // General settings

        [Tooltip("Duration (in seconds) for the fade animation.")]
        public float duration = 0.5f;

        [Tooltip("List of GameObjects that trigger the fade effect when raycasted.")]
        public List<GameObject> triggerGameObjects = new List<GameObject>();

        // Opacity control

        [Tooltip("Enables the fade effect by changing the opacity of the opacityRenderers.")]
        public bool useOpacity;

        [Tooltip("Animation curve used to control the opacity changes during the fade.")]
        public AnimationCurve opacityCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("Renderers to change opacity on.")]
        public List<Renderer> opacityRenderers = new List<Renderer>();

        // Mask transform manipulation

        [Tooltip("Enables changing the mask transform during the fade effect.")]
        public bool useMaskTransform;

        [Tooltip("The transform of the mask to change when one of the triggerGameObjects gets obscured.")]
        public Transform maskTransform;

        [Tooltip("Enables scaling the target object during the fade effect.")]
        public bool useScale;

        public Vector3 startScale;
        public Vector3 endScale;

        [Tooltip("Enables changing the target object's local position during the fade effect.")]
        public bool usePosition;

        public Vector3 startPosition;
        public Vector3 endPosition;

        [Tooltip("Enables rotating the target object during the fade effect.")]
        public bool useRotation;

        public Vector3 startRotation;
        public Vector3 endRotation;

        // Private variables

        private float currentFadeLerp = 0f; // Current lerp value for fade effects

        private bool currentFrameDetectedFlag = false;
        private bool lastFrameDetectedFlag = false;
        private bool coroutineRunning = false;

        // Public methods

        /// <summary>
        /// Clears the list of opacityRenderers associated with the target.
        /// </summary>
        public void ClearRenderersList()
        {
            opacityRenderers.Clear();
        }

        /// <summary>
        /// Finds all opacityRenderers attached to the target object's children.
        /// </summary>
        public void FindRenderersInChildren()
        {
            opacityRenderers.AddRange(GetComponentsInChildren<Renderer>());
        }

        /// <summary>
        /// Finds all opacityRenderers attached to the target object itself.
        /// </summary>
        public void FindRenderersOnTarget()
        {
            opacityRenderers.AddRange(GetComponents<Renderer>());
        }

        /// <summary>
        /// Sets the flag indicating if an obstacle is detected.
        /// </summary>
        /// <param name="value">True if an obstacle is detected, False otherwise.</param>
        public void SetDetectedFlag(bool value)
        {
            currentFrameDetectedFlag = value;
        }

        /// <summary>
        /// Coroutine that handles the fade animation based on start and end lerp values.
        /// </summary>
        /// <param name="startLerp">Starting lerp value for the fade.</param>
        /// <param name="endLerp">Ending lerp value for the fade.</param>
        /// <returns>IEnumerator for the coroutine.</returns>
        private IEnumerator FadeCoroutine(float startLerp, float endLerp)
        {
            coroutineRunning = true;

            // Fade over the specified duration
            float timePassed = 0;
            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;

                // Calculate lerp value based on animation curve
                float sampleTime = timePassed / duration;
                float value = opacityCurve.Evaluate(sampleTime);

                currentFadeLerp = Mathf.Lerp(startLerp, endLerp, value);

                // Apply fade effects
                ChangeOpacity(currentFadeLerp);

                if (useMaskTransform)
                {
                    ChangeScale(currentFadeLerp);
                    ChangeRotation(currentFadeLerp);
                    ChangePosition(currentFadeLerp);
                }

                yield return null; // Wait for next frame
            }

            coroutineRunning = false;
        }

        // Private helper methods for applying fade effects

        private void ChangeOpacity(float lerp)
        {
            if (!useOpacity) return;

            lerp = 1 - lerp; // Invert lerp for opacity 
            MaterialPropertyBlock mtb = new MaterialPropertyBlock();
            mtb.SetFloat("_Opacity", lerp);
            foreach (var renderer in opacityRenderers)
            {
                renderer.SetPropertyBlock(mtb);
            }
        }

        private void ChangePosition(float lerp)
        {
            if (!usePosition) return;
            maskTransform.position = Vector3.Lerp(startPosition, endPosition, lerp);
        }

        private void ChangeRotation(float lerp)
        {
            if (!useRotation) return;
            maskTransform.rotation = Quaternion.Lerp(Quaternion.Euler(startRotation), Quaternion.Euler(endRotation), lerp);
        }

        private void ChangeScale(float lerp)
        {
            if (!useScale) return;
            maskTransform.localScale = Vector3.Lerp(startScale, endScale, lerp);
        }

        /// <summary>
        /// Checks for changes in obstacle detection and triggers fade coroutines accordingly.
        /// </summary>
        private void CheckForCallDetectionEvents()
        {
            if (currentFrameDetectedFlag == true && lastFrameDetectedFlag == false)
            {
                // Obstacle detected, fade out

                if (coroutineRunning)
                {
                    StopAllCoroutines();
                    StartCoroutine(FadeCoroutine(currentFadeLerp, 1));
                }
                else
                {
                    StartCoroutine(FadeCoroutine(0, 1));
                }
            }
            else if (currentFrameDetectedFlag == false && lastFrameDetectedFlag == true)
            {
                // No obstacle detected, fade in

                if (coroutineRunning)
                {
                    StopAllCoroutines();
                    StartCoroutine(FadeCoroutine(currentFadeLerp, 0));
                }
                else
                {
                    StartCoroutine(FadeCoroutine(1, 0));
                }
            }
        }

        private void Start()
        {
            if (useMaskTransform)
            {
                if (usePosition) maskTransform.position = startPosition;
                if (useRotation) maskTransform.rotation = Quaternion.Euler(startRotation);
                if (useScale) maskTransform.localScale = startScale;
            }
        }

        private void Update()
        {
            CheckForCallDetectionEvents();

            lastFrameDetectedFlag = currentFrameDetectedFlag;
        }
    }
}
