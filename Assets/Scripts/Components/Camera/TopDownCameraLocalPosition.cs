using UnityEngine;

namespace Components.Camera
{
    public class TopDownCameraLocalPosition
    {
        private Transform _cameraLocalPositionParent;
        private Vector3 _localPositionVelocity = Vector3.zero;
        private Vector2 _cameraPositionRectangle;

        public TopDownCameraLocalPosition(Transform cameraLocalPositionParent)
        {
            _cameraLocalPositionParent = cameraLocalPositionParent;
        }
        //Updates X , Z  coordinate of Local Camera position for better Road view
        public void SetLocalCameraPosition(Vector3 velocity, Vector2 screenAspectRatio, float smoothSpeed, float returnSmoothSpeed)
        {
            UpdateRectangle(velocity, screenAspectRatio, smoothSpeed);

            var localPosition = _cameraLocalPositionParent.localPosition;
            Vector3 targetPosition = new Vector3(_cameraPositionRectangle.x, localPosition.y, _cameraPositionRectangle.y);
            
            _cameraLocalPositionParent.localPosition =
                Vector3.SmoothDamp(localPosition, targetPosition, ref _localPositionVelocity, returnSmoothSpeed);
        }

        private void UpdateRectangle(Vector3 velocity, Vector2 screenAspectRatio, float smoothSpeed)
        {
            var direction = velocity.normalized;
            var speed = velocity.magnitude;

            _cameraPositionRectangle = new Vector2(direction.x * speed * screenAspectRatio.x * smoothSpeed,
                             direction.z * speed * screenAspectRatio.y * smoothSpeed);
        }
    }
}
