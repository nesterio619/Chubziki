using NWH.WheelController3D;
using UnityEngine;
using Upgrades;
using Upgrades.CarUpgrades;

namespace Components.Car.CarLogic
{
    [System.Serializable]
    public sealed class WheelTurning
    {
        [SerializeField] private WheelController frontLeftController;
        [SerializeField] private WheelController frontRightController;
        [Space(10)]
        //[Range(10, 45)] 
        [SerializeField] private int maxSteeringAngle = 27; // The maximum angle that the tires can reach while rotating the steering wheel.
        //[Range(1f, 10f)] 
        [SerializeField] internal float steeringSpeed = 5f; // How fast the steering wheel turns.

        private float GetSteeringSpeed => Time.deltaTime * steeringSpeed;

        private float _steeringAxis;

        //Turning in direction from -1 to 1
        public void TurningWheels(float direction)
        {
            switch (direction)
            {
                case > 0:
                    _steeringAxis += GetSteeringSpeed;
                    break;
                case < 0:
                    _steeringAxis -= GetSteeringSpeed;
                    break;
            }

            // Prevent over-steering
            if (Mathf.Abs(_steeringAxis) > Mathf.Abs(direction))
                _steeringAxis = direction;

            // Set new steering direction
            var steeringAngle = _steeringAxis * maxSteeringAngle;
            frontLeftController.SteerAngle = Mathf.Lerp(frontLeftController.SteerAngle, steeringAngle, steeringSpeed);
            frontRightController.SteerAngle = Mathf.Lerp(frontRightController.SteerAngle, steeringAngle, steeringSpeed);
        }

        //smoothly changing streeringAxix to 0
        public void ResetSteeringAngle()
        {
            switch (_steeringAxis)
            {
                case 0:
                    return;
                case < 0f:
                    _steeringAxis += GetSteeringSpeed;
                    break;
                case > 0f:
                    _steeringAxis -= GetSteeringSpeed;
                    break;
            }

            // If the steering direction is too low, we set it to 0
            if (Mathf.Abs(frontLeftController.SteerAngle) < 1f)
                _steeringAxis = 0f;

            // Set new steering direction
            var steeringAngle = _steeringAxis * maxSteeringAngle;
            frontLeftController.SteerAngle = Mathf.Lerp(frontLeftController.SteerAngle, steeringAngle, steeringSpeed);
            frontRightController.SteerAngle = Mathf.Lerp(frontRightController.SteerAngle, steeringAngle, steeringSpeed);
        }

        public void UpdateUpgrades(UpgradesList baseScriptableStats)
        {
            maxSteeringAngle = (int)baseScriptableStats.CarUpgradesList[(int)UpgradesList.Upgrades.Control].GetValue((int)ControlCarUpgrade.ControlUpgrades.MaxSteeringAngle);
            steeringSpeed = (int)baseScriptableStats.CarUpgradesList[(int)UpgradesList.Upgrades.Control].GetValue((int)ControlCarUpgrade.ControlUpgrades.SteeringSpeed);
        }

        public float GetMaxSteering()
        {
            return maxSteeringAngle;
        }

    }
}


