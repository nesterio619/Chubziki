using UnityEngine;

namespace Components.Camera
{
    public class TopDownCameraTilt
    {
        private Transform _cameraRollingParent;
        public TopDownCameraTilt(Transform cameraRollingParent)
        {
            _cameraRollingParent = cameraRollingParent;
        }
        public void SetTiltAngle(Vector3 tiltAngleToSet)
        {
            _cameraRollingParent.localRotation = Quaternion.Euler(tiltAngleToSet.x, tiltAngleToSet.y, tiltAngleToSet.z);
        }
    }
}

