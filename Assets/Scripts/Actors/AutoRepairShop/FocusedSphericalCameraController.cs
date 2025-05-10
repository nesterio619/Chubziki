using UnityEngine;

namespace Actors.AutoRepairShop
{
    public class FocusedSphericalCameraController : MonoBehaviour
    {
        [SerializeField] private float minRadius = 4f;
        [SerializeField] private float maxRadius = 10f;
        [SerializeField] private float minYRotation = 10f;
        [SerializeField] private float maxYRotation = 80f;

        [Space(10)]
        [SerializeField] private float startRadius = 5f;
        [SerializeField] private Vector2 startRotation = new Vector2(45f, 30f);

        [SerializeField] private float rotationSpeed = 100f; 
        [SerializeField] private float zoomSpeed = 0.01f; 
        [SerializeField] private float smoothTime = 0.2f; 

        public Transform _target;

        private Vector2 _currentRotation;
        private Vector2 _targetRotation;
        private Vector2 _rotationSmoothVelocity;

        private Vector3 _currentPosition;
        private Vector3 _positionSmoothVelocity;

        private float _currentRadius;
        private float _targetRadius;
        private float _radiusSmoothVelocity;

        private void Start()
        {
            _currentRadius = Mathf.Clamp(startRadius, minRadius, maxRadius);
            _targetRadius = _currentRadius;

            startRotation.y = Mathf.Clamp(startRotation.y, minYRotation, maxYRotation);
            _currentRotation = startRotation;
            _targetRotation = startRotation;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            transform.position = CalculateCameraPosition();
            transform.rotation = CalculateCameraRotation();
        }

        public void HandleCameraMovement(float horizontal, float vertical)
        {
            if (_target == null) return;

            _targetRotation.x -= horizontal * rotationSpeed * Time.deltaTime;
            _targetRotation.y += vertical * rotationSpeed * Time.deltaTime;

            _targetRotation.y = Mathf.Clamp(_targetRotation.y, minYRotation, maxYRotation);
        }
        public void HandleCameraZoom(float zoom)
        {
            if (_target == null) return;

            _targetRadius -= zoom * zoomSpeed;
            _targetRadius = Mathf.Clamp(_targetRadius, minRadius, maxRadius);
        }
        
        private Vector3 CalculateCameraPosition()
        {
            ApplySmootDamp();

            float yaw = _currentRotation.x * Mathf.Deg2Rad;
            float pitch = _currentRotation.y * Mathf.Deg2Rad;

            Vector3 cameraOffset = new Vector3(
                Mathf.Sin(yaw) * Mathf.Cos(pitch),
                Mathf.Sin(pitch),
                Mathf.Cos(yaw) * Mathf.Cos(pitch)
            ) * _currentRadius;

            return _currentPosition + cameraOffset;
        }

        private Quaternion CalculateCameraRotation()
        {
            return Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation((_currentPosition - transform.position).normalized),
                smoothTime); 
        }

        private void ApplySmootDamp()
        {
            _currentRotation = Vector2.SmoothDamp(
                _currentRotation,
                _targetRotation,
                ref _rotationSmoothVelocity,
                smoothTime);

            _currentRadius = Mathf.SmoothDamp(
                _currentRadius,
                _targetRadius,
                ref _radiusSmoothVelocity,
                smoothTime);

            _currentPosition = Vector3.SmoothDamp(
                _currentPosition,
                _target.position,
                ref _positionSmoothVelocity,
                smoothTime);
        }

        public void SetTarget(Transform target)
        {
            if(_target==null) _currentPosition = target.position;
            _target = target;
        }

        public void SetRadius(float radius)
        {
            _targetRadius = radius;
        }
    }
}