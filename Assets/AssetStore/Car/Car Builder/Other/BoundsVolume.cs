using UnityEngine;

namespace ProceduralCarBuilder
{
    public class BoundsVolume : MonoBehaviour
    {
        [SerializeField] private bool _transformIsFloor = default;
        [SerializeField] private Vector3 _size = default;
        [SerializeField] private Vector3 _offset = default;

        private bool _initialized = default;
        private Bounds _bounds = default;

        public Vector3 Offset { get => _offset; set => _offset = value; }

        public Bounds LocalBounds
        {
            get
            {
                if (_initialized == false) Initialize();

                return _bounds;
            }
        }

        private void Initialize()
        {
            _bounds = new Bounds(_size * 0.5f, _size);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + _offset + (_transformIsFloor ? Vector3.up * _size.y * 0.5f : Vector3.zero), _size);
        }
    }
}
