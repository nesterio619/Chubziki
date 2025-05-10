using Core.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Components
{
    public class ObjectInPlace : MonoBehaviour
    {
        [SerializeField] private float allowedDistance = 0.2f;
        [SerializeField] private float fallThresholdAngle = 10f;
        
        [SerializeField] private Transform startPosition;
        [SerializeField] private Transform pressedPosition;
        [SerializeField] private Axis detectedAxis;

        public UnityEvent OnTooClose = null;
        public UnityEvent OnTooFar = null;
        public UnityEvent OnFall = null;

        private bool _isTooFar = false;
        
        private bool IsTooFar
        {
            get => _isTooFar;

            set
            {
                _isTooFar = value;

                if (_isTooFar)
                    OnTooFar?.Invoke();
                else
                    OnTooClose?.Invoke();
            }
        }

        protected virtual void FixedUpdate()
        {
            CheckDistance();
            CheckTilt();
        }

        //protected virtual void FixedUpdate() => CheckDistance();
        
        public void CheckDistance()
        {
            bool isCurrentDistanceClose = MathUtils.ObjectIsTooClose(startPosition.localPosition, pressedPosition.localPosition, detectedAxis, allowedDistance);

            if (!isCurrentDistanceClose && !_isTooFar)
            {
                IsTooFar = true;
            }
            else if (isCurrentDistanceClose && _isTooFar)
            {
                IsTooFar = false;
            }
        }
        
        private void CheckTilt()
        {
            bool isLeaned = MathUtils.ObjectIsTilted(transform, fallThresholdAngle, detectedAxis);
            
            if (isLeaned)
                OnFall?.Invoke();
        }
    }
}
