using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace INab.WorldAlchemy
{
    /// <summary>
    /// Handles collider interactions for a Mask.
    /// </summary>
    public class MaskCollider : MonoBehaviour
    {
        private Mask mask;

        private void OnEnable()
        {
            mask = GetComponentInParent<Mask>();
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (mask == null) mask = GetComponentInParent<Mask>();

            if (mask.DetectionLayerMask == (mask.DetectionLayerMask | (1 << collider.gameObject.layer)))
            {
                var renderer = collider.gameObject.GetComponent<Renderer>();
                if (renderer == null) return;
                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                {
                    mask.CallTriggerEnter(renderer, mask.ID);
                }
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (mask == null) mask = GetComponentInParent<Mask>();

            if (mask.DetectionLayerMask == (mask.DetectionLayerMask | (1 << collider.gameObject.layer)))
            {
                var renderer = collider.gameObject.GetComponent<Renderer>();
                if (renderer == null) return;

                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                {
                    mask.CallTriggerExit(renderer, mask.ID);
                }
            }
        }

    }

}