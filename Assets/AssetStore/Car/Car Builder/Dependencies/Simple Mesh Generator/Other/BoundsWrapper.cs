using UnityEngine;

namespace SimpleMeshGenerator
{
    [System.Serializable]
    public class BoundsWrapper
    {
        [HideInInspector] [SerializeField] private Bounds _bounds = new Bounds();

        private Vector3 _above_FrontLeft;
        private Vector3 _above_FrontRight;
        private Vector3 _above_BackLeft;
        private Vector3 _above_BackRight;
        private Vector3 _above_Center;

        private Vector3 _below_FrontLeft;
        private Vector3 _below_FrontRight;
        private Vector3 _below_BackLeft;
        private Vector3 _below_BackRight;
        private Vector3 _below_Center;

        private float _ceiling;
        private float _floor;

        private Vector3 _forward = Vector3.forward;
        private Vector3 _up = Vector3.up;

        public Vector3 Above_FrontLeft { get => _above_FrontLeft; }
        public Vector3 Above_FrontRight { get => _above_FrontRight; }
        public Vector3 Above_BackLeft { get => _above_BackLeft; }
        public Vector3 Above_BackRight { get => _above_BackRight; }
        public Vector3 Above_Center { get => _above_Center; }


        public Vector3 Below_FrontLeft { get => _below_FrontLeft; }
        public Vector3 Below_FrontRight { get => _below_FrontRight; }
        public Vector3 Below_BackLeft { get => _below_BackLeft; }
        public Vector3 Below_BackRight { get => _below_BackRight; }
        public Vector3 Below_Center { get => _below_Center; }

        public Vector3 Forward { get => _forward; }
        public Vector3 Up { get => _up; }

        public float Ceiling { get => _ceiling; }
        public float Floor { get => _floor; }

        public BoundsWrapper (Vector3 center, Vector3 size)
        {
            _bounds.center = center;
            _bounds.size = size;
            UpdateCorners();
        }

        public BoundsWrapper(Bounds bounds)
        {
            _bounds.center = bounds.center;
            _bounds.size = bounds.size;
            UpdateCorners();
        }

        public BoundsWrapper() {}

        public Vector3 Center
        {
            get { return _bounds.center; }
            set
            {
                _bounds.center = value;
                UpdateCorners();
            }
        }

        public Vector3 Size
        {
            get { return _bounds.size; }
            set
            {
                _bounds.size = value;
                UpdateCorners();
            }
        }

        public void Encapsulate(Vector3 point)
        {
            _bounds.Encapsulate(point);
            UpdateCorners();
        }

        public void SetDirections(Vector3 forward, Vector3 up)
        {
            _forward = forward;
            _up = up;
        }

        private void UpdateCorners()
        {
            _above_FrontLeft = _bounds.max.FlipXClone();
            _above_FrontRight = _bounds.max;
            _above_BackLeft = _bounds.min.ReplaceYClone(_bounds.max.y);
            _above_BackRight = _bounds.max.ReplaceZClone(_bounds.min.z);
            _above_Center = (_above_FrontLeft + _above_FrontRight + _above_BackLeft + _above_BackRight) / 4;

            _below_FrontLeft = _bounds.min.ReplaceZClone(_bounds.max.z);
            _below_FrontRight = _bounds.max.ReplaceYClone(_bounds.min.y);
            _below_BackLeft = _bounds.min;
            _below_BackRight = _bounds.min.FlipXClone();
            _below_Center = (_below_FrontLeft + _below_FrontRight + _below_BackLeft + _below_BackRight) / 4;

            _ceiling = _bounds.max.y;
            _floor = _bounds.min.y;
        }

        public static void DebugRayBounds(Bounds value)
        {
            var color = Color.white;
            Debug.DrawLine(value.min, value.max, color, 10);
        }
    }
}
