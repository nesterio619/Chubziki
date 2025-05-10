using UnityEngine;

namespace Components.Animation
{
    public class PlatformAnimation : MonoBehaviour
    {
        [SerializeField]
        private CurveAnimatorController.AxisCurveMovement CurveMovement;

        [SerializeField]
        private GameObject Actor;

        private float _currentTime;

        private float _totalTime;

        private void Start()
        {
            _totalTime = CurveMovement.XCurve.keys[CurveMovement.XCurve.length - 1].time;
        }

        private void Update()
        {
            Vector3 pos;

            pos.x = CurveMovement.XCurve.Evaluate(_currentTime);

            pos.y = CurveMovement.YCurve.Evaluate(_currentTime);

            pos.z = CurveMovement.ZCurve.Evaluate(_currentTime);

            Actor.transform.localPosition = pos;

            Actor.transform.rotation = Quaternion.Euler(CurveMovement.XRotationCurve.Evaluate(_currentTime), CurveMovement.YRotationCurve.Evaluate(_currentTime), CurveMovement.ZRotationCurve.Evaluate(_currentTime));


            if (_currentTime >= _totalTime)
            {
                _currentTime = 0;
            }

            _currentTime += Time.deltaTime;

        }


    }
}