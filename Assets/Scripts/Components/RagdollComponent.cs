using UnityEngine;

namespace Components
{
    public class RagdollComponent : MonoBehaviour
    {
        [SerializeField][HideInInspector] private RagdollPart[] _ragdollParts;
        [SerializeField][HideInInspector] private TransformData[] _initialTransforms;
        [SerializeField][HideInInspector] private bool _ragdollPartsSet = false;
        [SerializeField][HideInInspector] private bool _ragdollPartsInitialStateStored = false;

        private void Start() => SwitchRagdoll(false);
        public void SwitchRagdoll(bool enable)
        {
            if (!_ragdollPartsSet)
            {
                Debug.LogError($"Ragdoll Parts not assigned {gameObject.name}");
                return;
            }

            foreach (var part in _ragdollParts)
            {
                part.Collider.enabled = enable;
                part.Rigidbody.isKinematic = !enable;
            }
        }

        // Function to reset the ragdoll parts to their original state
        public void ResetRagdollToInitialState()
        {
            if (!_ragdollPartsSet)
            {
                Debug.LogError($"Ragdoll Parts not assigned for {gameObject.name}");
                return;
            }

            for (int i = 0; i < _ragdollParts.Length; i++)
            {
                var initialTransform = _initialTransforms[i];
                var ragdollPart = _ragdollParts[i];

                ragdollPart.Rigidbody.transform.localPosition = initialTransform.Position;
                ragdollPart.Rigidbody.transform.localRotation = initialTransform.Rotation;
                ragdollPart.Rigidbody.isKinematic = true;
                ragdollPart.Collider.enabled = false;
            }
        }

        public void AddForceToAll(Vector3 force)
        {
            if (!_ragdollPartsSet)
            {
                Debug.LogError($"Ragdoll Parts not assigned {gameObject.name}");
                return;
            }

            foreach (var part in _ragdollParts)
            {
                part.Rigidbody.AddForce(force, ForceMode.Acceleration);
            }
        }

        public void SetRagdollParts(RagdollPart[] parts)
        {
            _ragdollParts = parts;
            _ragdollPartsSet = true;

            SaveInitialTransforms();
            _ragdollPartsInitialStateStored = true;
        }

        public bool RagdollParsSet() => _ragdollPartsSet;
        public bool RagdollParsInitialStateStored() => _ragdollPartsInitialStateStored;

        // Save the initial positions and rotations of all ragdoll parts
        private void SaveInitialTransforms()
        {
            if (_ragdollParts == null || _ragdollParts.Length == 0)
            {
                Debug.LogWarning("Ragdoll parts are not set. Cannot save initial transforms.");
                return;
            }

            _initialTransforms = new TransformData[_ragdollParts.Length];

            for (int i = 0; i < _ragdollParts.Length; i++)
            {
                var ragdollPart = _ragdollParts[i];
                _initialTransforms[i] = new TransformData(
                    ragdollPart.Rigidbody.transform.localPosition,
                    ragdollPart.Rigidbody.transform.localRotation
                );
            }
        }
    }

    [System.Serializable]
    public struct RagdollPart
    {
        public Rigidbody Rigidbody;
        public Collider Collider;

        public RagdollPart(Rigidbody rigidbody, Collider collider)
        {
            Rigidbody = rigidbody;
            Collider = collider;
        }
    }

    [System.Serializable]
    public struct TransformData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public TransformData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
