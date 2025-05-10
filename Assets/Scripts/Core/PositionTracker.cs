using Core.Extensions;
using UnityEngine;

namespace Core
{
    public class PositionTracker
    {
        private const float Default_Interval = 5;

        public Pose LastTrackedPose { get; private set; }

        private readonly Transform _trackedTransform;
        private readonly float _interval;
        private float _timer;

        public PositionTracker(Transform trackedTransform, float interval = Default_Interval)
        {
            _trackedTransform = trackedTransform;
            _interval = interval;

            UpdatePosition();
        }

        public void Tick()
        {
            _timer += Time.deltaTime;

            if (_timer >= _interval)
            {
                _timer = 0;
                UpdatePosition();
            }
        }

        private void UpdatePosition() => 
            LastTrackedPose = _trackedTransform.GetPose();
    }
}


