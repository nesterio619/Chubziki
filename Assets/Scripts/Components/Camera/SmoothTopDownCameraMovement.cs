using Actors;
using Core;
using UnityEngine;

namespace Components.Camera
{
    public class SmoothTopDownCameraMovement : MonoBehaviour
    {
        [Header("Transform references")]
        [SerializeField] private Transform cameraGlobalPositionParent;
        [SerializeField] private Transform cameraLocalPositionParent;
        [SerializeField] private Transform cameraRollingParent;


        [Space]

        [Header("Movement Settings")]
        [Tooltip("Offset position from the _target")]
        [SerializeField] private Vector3 offset = new Vector3(0, 14, -10);

        [Tooltip("Smoothing speed for camera movement")]
        [SerializeField] private float smoothSpeed = 0.015f;
        [Tooltip("Max Smoothing speed for camera movement")]
        [SerializeField] private float maxSmoothSpeed = 0.02f;
        [Tooltip("Smoothing speed for camera returning back to player")]
        [SerializeField] private float returnSmoothSpeed = 0.6f;
        [Tooltip("Smoothing speed for camera movement")]
        [SerializeField] private float YsmoothSpeed = 0.015f;

        [Tooltip("Initial tilt angle for top-down view")]
        [SerializeField] private Vector3 initialTiltAngle = new Vector3(65, 0, 0);

        [Tooltip("Maximum Y offset for dynamic adjustment")]
        [SerializeField] private float maxOffsetY = 22.0f;

        [Tooltip("Speed threshold for maximum Y-offset")]
        [SerializeField] private float speedThreshold = 120.0f;

        [Space]
        [Header("Camera Reference")]
        [SerializeField] private UnityEngine.Camera mainCamera;

        [SerializeField] private Transform _targetActor;
        
        private Rigidbody _targetRigidBody;

        private Vector3  _targetVelocity => _targetActor != null ? _targetRigidBody.velocity : Vector3.zero;
        private float  _targetSpeed => _targetActor != null ?_targetRigidBody.velocity.magnitude : 0;

        private Vector2Int _screenAspectRatio;

        private TopDownCameraTilt _topDownCameraTilt;
        private TopDownCameraHeight _topDownCameraHeight;
        private TopDownCameraLocalPosition _topDownCameraLocalPosition;
        private TopDownCameraGlobalPosition _topDownCameraGlobalPosition;


        private void Start()
        {
            _screenAspectRatio = Core.Utilities.ScreenUtilities.GetAspectRatio();
            _topDownCameraHeight = new TopDownCameraHeight(cameraGlobalPositionParent, YsmoothSpeed);
            _topDownCameraTilt = new TopDownCameraTilt(cameraRollingParent);

            _topDownCameraLocalPosition = new TopDownCameraLocalPosition(cameraLocalPositionParent);
            _topDownCameraGlobalPosition = new TopDownCameraGlobalPosition(cameraGlobalPositionParent, _targetActor);


            _topDownCameraTilt.SetTiltAngle(initialTiltAngle);

            Core.Utilities.UtilitiesProvider.WaitAndRun(() =>
            {
                SetTarget(Player.Instance.PlayerCarGameObject.transform);
            }, true, 0.1f);
        }

        private void LateUpdate()
        {
            if (_targetActor == null) return;

            UpdatePosition();
        }

        public void SetTarget(Transform targetToSet)
        {
            _targetActor = targetToSet;
            GetTargetVelocity();
            _topDownCameraGlobalPosition.SetTarget(targetToSet);
        }
        
        private void GetTargetVelocity()
        {
            _targetRigidBody = _targetActor.gameObject.GetComponent<Rigidbody>();
        }
        
        private void UpdatePosition()
        {
            if (_topDownCameraGlobalPosition == null)
            {
                Debug.Log("_topDownCameraGlobalPosition == null");
            }
            //Update global position in scene
            _topDownCameraGlobalPosition.SetGlobalCameraPosition(offset, _targetVelocity, _screenAspectRatio, smoothSpeed, maxSmoothSpeed);
            
            // Update Y position
            _topDownCameraHeight.SetCameraHeight(offset.y, maxOffsetY, _targetSpeed, speedThreshold, _targetActor.transform.localPosition.y);
            
            // Update local position inside parent
            _topDownCameraLocalPosition.SetLocalCameraPosition(_targetVelocity, _screenAspectRatio, smoothSpeed, returnSmoothSpeed);
        }
    }
}
