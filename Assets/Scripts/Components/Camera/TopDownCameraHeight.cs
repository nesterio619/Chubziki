using UnityEngine;

namespace Components.Camera
{
    public class TopDownCameraHeight
    {
        private readonly Transform _cameraGlobalTransformParent;
        private readonly float _smoothSpeed;
        
        // Required for SmoothDamp, so that camera moves smoothly
        private float _currentHeight = 0f;
        private float _heightVelocity = 0f;
        
        public TopDownCameraHeight(Transform cameraGlobalTransformParent, float smoothSpeed)
        {
            _cameraGlobalTransformParent = cameraGlobalTransformParent;
            _smoothSpeed = Mathf.Max(smoothSpeed, 0.01f);
            
            _currentHeight = float.IsNaN(cameraGlobalTransformParent.localPosition.y) 
                ? 0f 
                : cameraGlobalTransformParent.localPosition.y;
        }

        public void SetCameraHeight(float YOffset, float maxOffsetY, float speed, float speedThreshold, float targetHeight)
        {
            if (float.IsNaN(YOffset) || float.IsNaN(maxOffsetY) || float.IsNaN(speed) || 
                float.IsNaN(speedThreshold) || float.IsNaN(targetHeight))
            {
                Debug.LogError("Invalid input parameters in SetCameraHeight: NaN detected.");
                return;
            }

            if (speedThreshold <= 0)
            {
                Debug.LogWarning("speedThreshold must be positive. Using default value.");
                speedThreshold = 1f; 
            }
            float targetOffsetY = GetDynamicOffsetY(YOffset, maxOffsetY, speed, speedThreshold);

            // Вычисляем желаемую высоту
            float desiredHeight = targetHeight + targetOffsetY;

            // Применяем сглаживание
            _currentHeight = Mathf.SmoothDamp(_currentHeight, desiredHeight, ref _heightVelocity, _smoothSpeed);
            if (float.IsNaN(_currentHeight) || float.IsInfinity(_currentHeight))
            {
                Debug.LogError($"SmoothDamp returned invalid value. Resetting _currentHeight. " +
                               $"desiredHeight={desiredHeight}, _smoothSpeed={_smoothSpeed}");
                _currentHeight = targetHeight;
            }
            // Обновляем позицию камеры
            var localPosition = _cameraGlobalTransformParent.localPosition; var newPosition = new Vector3(localPosition.x, _currentHeight, localPosition.z);
            if (IsValidVector(newPosition))
            {
                _cameraGlobalTransformParent.localPosition = newPosition;
            }
            else
            {
                Debug.LogError($"Attempted to set invalid localPosition: {newPosition}. Skipping.");
            }
        }

        private float GetDynamicOffsetY(float YOffset, float maxOffsetY, float speed, float speedThreshold)
        {
            return Mathf.Lerp(YOffset, maxOffsetY, Mathf.Clamp01(speed / speedThreshold));
        }
        private bool IsValidVector(Vector3 vector)
        {
            return !float.IsNaN(vector.x) && !float.IsNaN(vector.y) && !float.IsNaN(vector.z) &&
                   !float.IsInfinity(vector.x) && !float.IsInfinity(vector.y) && !float.IsInfinity(vector.z);
        }
    }
}