using Core.Utilities;
using SimpleMeshGenerator;
using UnityEngine;

namespace ProceduralCarBuilder
{
    public class CarPartReferences : MonoBehaviour
    {
        [Header("Car Parts")]
        public Transform WheelsFrontRight;
        public Transform WheelsFrontLeft;
        public Transform WheelsBackRight;
        public Transform WheelsBackLeft;
        [HideInInspector] public float WheelRadius;
        [HideInInspector] public float WheelWidth;     
        [Space]
        public Transform Windows;
        public Transform BodySide;
        public Transform BodyTop;
        public Transform TrunkBonnet;
        public Transform Hood;
        [Space]
        public Transform HeadLight_Right;
        public Transform HeadLight_Left;
        [Space]
        public Transform LicensePlate;
        [HideInInspector] public Vector2 LicensePlateDimensions;

        [Header("Prop Anchors")]
        public PropAnchor PropAnchor_HoodOrnament;
        public PropAnchor PropAnchor_Roof;
        public PropAnchor PropAnchor_FrontTrunk;
        public PropAnchor PropAnchor_BackTrunk;

        private void OnDrawGizmosSelected()
        {
            PropAnchor_HoodOrnament.GizmoBounds();
            PropAnchor_Roof.GizmoBounds();
            PropAnchor_FrontTrunk.GizmoBounds();
            PropAnchor_BackTrunk.GizmoBounds();
        }

        public void FitObjectIntoAnchorSpace(ref Transform targetObject, Vector3 objectSize, PropAnchor anchor, bool ignoreY = false)
        {
            var x = objectSize.x / Mathf.Max(anchor.WorldSpaceBounds.Size.x, CarGenerator.MinimumDistance);
            var y = objectSize.y / Mathf.Max(anchor.WorldSpaceBounds.Size.y, CarGenerator.MinimumDistance);
            var z = objectSize.z / Mathf.Max(anchor.WorldSpaceBounds.Size.z, CarGenerator.MinimumDistance);

            var scaler = 1f;

            if(ignoreY)     scaler = 1f / Mathf.Max(x, z);
            else            scaler = 1f / Mathf.Max(x, y, z);

            targetObject.localScale = Vector3.one * scaler;
        }
    }

    [System.Serializable]
    public class PropAnchor
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private BoundsWrapper _bounds;

        private BoundsWrapper _exposedBounds;

        public Transform MiddlePoint { get => _transform; }
        public BoundsWrapper WorldSpaceBounds {
            get
            {
                if (_exposedBounds == null) _exposedBounds = new BoundsWrapper(_bounds.Center, _bounds.Size);

                // Scaling the car/anchor makes the BoundsWrapper positions incorrect, thus re-position and re-size when needed
                var boundsIsInSync = Mathf.Approximately(_transform.position.x, _exposedBounds.Center.x) && Mathf.Approximately(_transform.position.y, _exposedBounds.Center.y) && Mathf.Approximately(_transform.position.z, _exposedBounds.Center.z);
                if (boundsIsInSync == false)
                {
                    _exposedBounds = new BoundsWrapper(_transform.position, Vector3.Scale(_bounds.Size, _transform.lossyScale));
                }

                return _exposedBounds;
            }
        }

        public PropAnchor(Transform parent, string name)
        {
            _transform = new GameObject(name).transform;
            _transform.SetParent(parent, false);
        }


        public void SetWorldBounds(BoundsWrapper bounds)
        {
            _bounds = bounds;

            if (bounds.Center.IsNaN()) return;

            _transform.localPosition = _bounds.Center;
            _transform.LookAt(_transform.position + _bounds.Forward, _bounds.Up);
        }

        public void GizmoBounds()
        {
            Gizmos.color = Color.cyan;
            var matrix = Gizmos.matrix;
            Gizmos.matrix = _transform.localToWorldMatrix;

            Gizmos.DrawWireCube(Vector3.zero, _bounds.Size);
            Gizmos.matrix = matrix;
        }
    }
}
