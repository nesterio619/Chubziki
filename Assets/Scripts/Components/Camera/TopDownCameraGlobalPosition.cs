using UnityEngine;

namespace Components.Camera
{
    public class TopDownCameraGlobalPosition
    {
        private Transform _cameraGlobalTransformParent;
        private Transform _targetTransform;

        private Vector3 _globalPositionVelocity = Vector3.zero;
        private Vector3 _desiredCameraPosition;

        public TopDownCameraGlobalPosition(Transform cameraGlobalTransformParent, Transform targetTransform)
        {
            _cameraGlobalTransformParent = cameraGlobalTransformParent;
            _targetTransform = targetTransform;
        }

        public void SetTarget(Transform targetTransform)
        {
            _targetTransform = targetTransform;
        }
        //Updates X , Z  coordinate of Global Camera position
        public void SetGlobalCameraPosition(Vector3 offsetPosition, Vector3 carVelocity, Vector2 screenAspectRatio, float smoothSpeed, float maxSmoothSpeed)
        {
            UpdateDesiredCameraPosition(offsetPosition, carVelocity, screenAspectRatio);

            var currentPosition = _cameraGlobalTransformParent.position;

            var distance = Vector3.Distance(currentPosition, _desiredCameraPosition);
            var dynamicSmoothSpeed = Mathf.Lerp(smoothSpeed, maxSmoothSpeed, distance);
            
            _cameraGlobalTransformParent.position = 
                Vector3.SmoothDamp(currentPosition, _desiredCameraPosition, ref _globalPositionVelocity, dynamicSmoothSpeed);
        }

        private void UpdateDesiredCameraPosition(Vector3 offsetPosition, Vector3 carVelocity, Vector2 screenAspectRatio)
        {
            var currentPosition = _targetTransform.position;
            _desiredCameraPosition = new Vector3(
                currentPosition.x + offsetPosition.x + (carVelocity.x / screenAspectRatio.x),
                _cameraGlobalTransformParent.position.y,
                currentPosition.z + offsetPosition.z + (carVelocity.z / screenAspectRatio.y)
            );
        }
    }
}
