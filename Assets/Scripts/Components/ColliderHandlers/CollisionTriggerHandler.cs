using UnityEngine;
using UnityEngine.Events;
using Core.Enums;
using Core.Extensions;

namespace Components
{
    [ExecuteAlways]
    public class CollisionTriggerHandler : MonoBehaviour
    {
        public UnityEvent<Collider> OnEnter = null;
        public UnityEvent<Collider> OnStay = null;
        public UnityEvent<Collider> OnExit = null;

        [SerializeField] protected Collider Collider;
        [SerializeField] private UnityLayers _allowedLayers = (UnityLayers)~0; //All layers selected

        private void OnValidate()
        {
            if (Collider == null)
            {
                Debug.LogWarning("Collider is not assigned. Please assign a collider. " + gameObject.name);
            }
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (LayerAllowed(collision.collider.gameObject))
            {
                OnEnter?.Invoke(collision.collider);
            }
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            if (LayerAllowed(collision.collider.gameObject))
            {
                OnStay?.Invoke(collision.collider);
            }
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (LayerAllowed(collision.collider.gameObject))
            {
                OnExit?.Invoke(collision.collider);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (LayerAllowed(other.gameObject))
            {
                OnEnter?.Invoke(other);
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            if (LayerAllowed(other.gameObject))
            {
                OnStay?.Invoke(other);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (LayerAllowed(other.gameObject))
            {
                OnExit?.Invoke(other);
            }
        }

        protected bool LayerAllowed(GameObject gameObject)
        {
            // If all layers are selected, return true
            if (_allowedLayers == (UnityLayers)~0)
            {
                return true;
            }

            // Check if the object's layer is in the allowed layers
            var objectLayer = gameObject.GetObjectLayer();
            return _allowedLayers.HasFlag(objectLayer);
        }
    }
}
