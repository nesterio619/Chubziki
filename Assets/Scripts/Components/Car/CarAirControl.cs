using Core;
using ProceduralCarBuilder;
using System;
using UnityEngine;


namespace Components.Car
{
    [Serializable]
    public class CarAirControl
    {
        private const float Flip_Velocity_Threshold = 0.1f; // Max vertical speed to allow flipping
        private const float Flip_Orientation_Threshold = 0.2f; // Max dot product to allow flipping

        public bool InvertPitch;

        [Header("Torque Limits")]
        [Tooltip("Minimum torque applied for pitch (forward/back) rotation in air.")]
        [SerializeField] public float minPitchTorque = 200f;
        [Tooltip("Maximum torque applied for pitch rotation in air.")]
        [SerializeField] public float maxPitchTorque = 500f;
        [Tooltip("Minimum torque applied for roll (side) rotation in air.")]
        [SerializeField] public float minRollTorque = 200f;
        [Tooltip("Maximum torque applied for roll rotation in air.")]
        [SerializeField] public float maxRollTorque = 500f;

        [Tooltip("Force that is used to flip the car")]
        [SerializeField] public float flipForce = 500f;

        [SerializeField] public float downforce = 200f;


        [Header("Sensitivity Curves\n" +
            "   X axis - 0 is 0 degrees, 1 is 180 degrees\n" +
            "   Y axis - 0 is min torque, 1 is max torque")]
        [Space(7)]
        [Tooltip("Torque sensitivity curve for pitch (forward/back tilt) based on angle.")]
        [SerializeField] public AnimationCurve pitchSensitivityCurve;
        [Tooltip("Torque sensitivity curve for roll (side tilt) based on angle.")]
        [SerializeField] public AnimationCurve rollSensitivityCurve;

        private Rigidbody _rigidbody;
        private Transform _transform;
        private float _halfCarWidth;

        private Func<Vector2> _inputGetter;
        private Func<bool> _groundedGetter;

        public void Initialize(Rigidbody rigidbody, Transform transform, float carBodyWidth, Func<Vector2> inputGetter, Func<bool> groundedGetter)
        {
            _rigidbody = rigidbody;
            _transform = transform;
            _halfCarWidth = carBodyWidth * 0.5f;
            _inputGetter = inputGetter;
            _groundedGetter = groundedGetter;

            Player.Instance.OnFixedUpdateEvent += ApplyAirControl;
        }

        public void ApplyAirControl()
        {
            ApplyDownforce();

            if (_groundedGetter.Invoke()) return;

            Vector2 input = _inputGetter.Invoke();
            var pitchInput = input.y;
            var rollInput = input.x;
 
            if (ShouldFlipCar(rollInput))
            {
                ApplyFlipForce(rollInput);
            }
            else
            {
                float pitchAngle = GetNormalizedAngle(_rigidbody.rotation.eulerAngles.x);
                float rollAngle = GetNormalizedAngle(_rigidbody.rotation.eulerAngles.z);

                float appliedPitchTorque = CalculateTorque(pitchAngle, pitchSensitivityCurve, minPitchTorque, maxPitchTorque, pitchInput);
                float appliedRollTorque = CalculateTorque(rollAngle, rollSensitivityCurve, minRollTorque, maxRollTorque, rollInput);

                ApplyTorque(appliedPitchTorque, appliedRollTorque, pitchInput, rollInput);
            }
        }

        private float GetNormalizedAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }

        private float CalculateTorque(float angle, AnimationCurve sensitivityCurve, float minTorque, float maxTorque, float input)
        {
            float absAngle = Mathf.Abs(angle) / 180f;
            float factor = Mathf.Clamp01(sensitivityCurve.Evaluate(absAngle));
            return Mathf.Lerp(minTorque, maxTorque, factor) * input;
        }

        private bool ShouldFlipCar(float rollInput)
        {
            return Mathf.Abs(_rigidbody.velocity.y) < Flip_Velocity_Threshold && // If car is not midair
                   Vector3.Dot(_transform.up, Vector3.up) <= Flip_Orientation_Threshold; // and car is upside down or perpendicular to ground
        }

        private void ApplyFlipForce(float rollInput)
        {
            if(rollInput == 0) return;

            Vector3 carSide = _transform.right * _halfCarWidth * (rollInput > 0 ? 1 : -1); // Determine car side by input

            // Add force at car side to flip the car
            _rigidbody.AddForceAtPosition(-_transform.up * flipForce, _transform.position + carSide, ForceMode.Force);
        }

        private void ApplyTorque(float pitchTorque, float rollTorque, float pitchInput, float rollInput)
        {
            // Apply the torques to the Rigidbody to rotate the car in mid-air.
            if (pitchInput!=0f)
            {
                if (InvertPitch) pitchTorque *= -1;
                _rigidbody.AddTorque(_transform.right * pitchTorque, ForceMode.Force);
            }

            if (rollInput!=0f)
            {
                _rigidbody.AddTorque(_transform.forward * -rollTorque, ForceMode.Force);
            }
        }

        private void ApplyDownforce()
        {
            var dot = Vector3.Dot(_transform.up, Vector3.up);
            var downforceModifier = Mathf.Clamp01(dot);

            _rigidbody.AddForce(downforceModifier * downforce * -_transform.up);
        }

        public void Dispose()
        {
            Player.Instance.OnFixedUpdateEvent -= ApplyAirControl;
        }
    }
}