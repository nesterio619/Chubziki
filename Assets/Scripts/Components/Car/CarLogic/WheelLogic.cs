using System;
using System.Collections.Generic;
using Core;
using Core.Utilities;
using NWH.WheelController3D;
using UnityEngine;
using Upgrades;
using Upgrades.CarUpgrades;

namespace Components.Car.CarLogic
{
    [System.Serializable]
    public sealed class WheelLogic : ICarHandBrake, IDisposable
    {
        //=--            Editor-controlled variables            --=//
        
        private CarParticles _carParticles;
        
        [Header("Speed")]
        //[Range(20, 190)]
        [SerializeField] internal int maxSpeed = 120;
        
        //[Range(10, 120)]
        [SerializeField] private int maxReverseSpeed = 45;

        //[Range(10, 300)]
        [SerializeField] private int accelerationMultiplier = 100;

        //[Range(1, 10)] 
        [SerializeField] private int decelerationMultiplier = 2;
        
        [Header("Brake")]

        //[Range(100, 1000)]
        [SerializeField] private int brakeForce = 350;

        
        
        //=------------------------------------------------------=//
        
        private const float THROTTLE_SPEED = 10f;

        private const float SIDE_MOVING_FOR_DRIFT = 2.5f;
        private const float DREFTING_AXIS_SPEED = 0.6666f;

        #region Movement variables

        public float CurrentCarSpeed { get; private set; }
        
        private float _localVelocityZ;
        private float _localVelocityX;
        
        private float _driftingAxis;
        private float _throttleAxis;
        
        private bool _isTractionLocked;
        
        [HideInInspector] public bool DeceleratingCar;
        private float _currentTimeDecelerating;

        #endregion

        #region Objects

        private Rigidbody _carRigidbody;
        private GameObject _carMainObjectRefernce;
        
        private WheelController[] _wheelControllers;
        private WheelController[] _wheelControllersForHandbrake;
        
        private CarSound _carSound = new();

        #endregion

        #region Events

        public delegate void OnDriftDelegate(bool isDrift);
        public event OnDriftDelegate OnDrift;

        public delegate void OnTractionDelegate(bool isTraction, float xLocalVelocity, float carSpeed);
        public event OnTractionDelegate OnTraction;

        public delegate void OnMotorTorque(float motorTorque);
        public event OnMotorTorque OnMotor;

        #endregion

        public void Initialize(Rigidbody carRigidbody, GameObject currentCar, UpgradesList upgradeList
            , WheelController[] wheelControllers, WheelController[] wheelControllersForHandbrake, CarParticles carParticles)
        {
            //=-- Set references
            _carRigidbody = carRigidbody;
            _carMainObjectRefernce = currentCar;

            _wheelControllers = wheelControllers;
            _wheelControllersForHandbrake = wheelControllersForHandbrake;
            
            //=-- Initialize components
            _carSound.Initialize(this, _carMainObjectRefernce);
            
            UpdateUpgrades(upgradeList);


			// Init wheel effects
			_carParticles = carParticles;
            OnTraction += _carParticles.ToggleDriftTracer;
            OnDrift += _carParticles.ToggleDriftSmoke;
            
            //=-- Start updating wheels
            Player.Instance.OnFixedUpdateEvent += OnExternalFixedUpdate;
        }

        public void Dispose()
        {
            if (Player.Instance == null) return;
            
            Player.Instance.OnFixedUpdateEvent -= OnExternalFixedUpdate;
            OnTraction -= _carParticles.ToggleDriftTracer;
            OnDrift -= _carParticles.ToggleDriftSmoke;

            _carSound.Dispose();
        }

        public void UpdateUpgrades(UpgradesList upgradeList)
        {
            if (upgradeList == null || _carRigidbody == null)
                return;

            //SURVIVE
            _carRigidbody.mass = (int)upgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Survive].GetValue((int)SurviveCarUpgrade.SurviveUpgrades.Weight);

            

            //SPEED
            maxSpeed = (int)upgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Speed].GetValue((int)SpeedCarUpgrade.SpeedUpgrades.MaxForwardSpeed);
            maxReverseSpeed = (int)upgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Speed].GetValue((int)SpeedCarUpgrade.SpeedUpgrades.MaxReverseSpeed);

            //POWER
            accelerationMultiplier = (int)upgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Power].GetValue((int)PowerCarUpgrade.PowerUpgrades.SpeedAcceleration);
            decelerationMultiplier = (int)upgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Power].GetValue((int)PowerCarUpgrade.PowerUpgrades.SpeedDecceleration);
            brakeForce = (int)upgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Power].GetValue((int)PowerCarUpgrade.PowerUpgrades.BrakeForce);

            var forwardGrip = Mathf.Clamp(
                upgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Speed].GetValue((int)SpeedCarUpgrade.SpeedUpgrades.ForwardGrip),
                0, 2);

            var sidewaysGrip = Mathf.Clamp(
                upgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Control].GetValue((int)ControlCarUpgrade.ControlUpgrades.SidewaysGrip),
                0, 2);

            var sidewaysStiffnes = Mathf.Clamp(
                upgradeList.CarUpgradesList[(int)UpgradesList.Upgrades.Control].GetValue((int)ControlCarUpgrade.ControlUpgrades.SidewaysStiffnes),
                0, 2);

            foreach (var wheel in _wheelControllers)
            {
                wheel.forwardFriction.grip = forwardGrip;
                wheel.sideFriction.grip = sidewaysGrip;
                wheel.sideFriction.stiffness = sidewaysStiffnes;
            }
        }

        private void OnExternalFixedUpdate()
        {
            CurrentCarSpeed = MathUtils.GetSpeedInKilometersPerHour(_carRigidbody);

            // Save the local velocity of the car in the x axis. Used to know if the car is drifting.
            _localVelocityX = _carMainObjectRefernce.transform.InverseTransformDirection(_carRigidbody.velocity).x;
            // Save the local velocity of the car in the z axis. Used to know if the car is going forward or backwards.
            _localVelocityZ = _carMainObjectRefernce.transform.InverseTransformDirection(_carRigidbody.velocity).z;

            OnMotor?.Invoke(_wheelControllers[0].MotorTorque);

            if (_currentTimeDecelerating >= 0.1 && DeceleratingCar)
            {
                DecelerateCarFull();
                _currentTimeDecelerating = 0;
            }

            _currentTimeDecelerating += Time.deltaTime;


        }

        public void GoDirection(float pushPower)
        {
            DriftCar();

            // The following part sets the throttle power to 1 smoothly.
            _throttleAxis += (Time.deltaTime * THROTTLE_SPEED);

            if (_throttleAxis > pushPower)
                _throttleAxis = pushPower;

            //If the car is going backwards, then apply brakes in order to avoid strange
            //behaviours. If the local velocity in the 'z' axis is less than -1f, then it
            //is safe to apply positive torque to go forward.

            float currentMaxSpeed = 0;

            if (pushPower > 0)
                currentMaxSpeed = maxSpeed;
            else
                currentMaxSpeed = maxReverseSpeed;

            if (_localVelocityZ < -1f && pushPower > 0 || _localVelocityZ > 1f && pushPower < 0)
            {
                Brakes();
            }
            else
            {
                if (CurrentCarSpeed < currentMaxSpeed)
                {
                    //Apply positive torque in all wheels to go forward if maxSpeed has not been reached.
                    foreach (var wheel in _wheelControllers)
                    {
                        if (wheel.IsGrounded)
                            wheel.MotorTorque = accelerationMultiplier * _throttleAxis;

                        // Apply breaks so wheels don't accumulate angular velocity midair
                        wheel.BrakeTorque = wheel.IsGrounded ? 0 : brakeForce;
                    }
                }
                else
                {
                    // If the maxSpeed has been reached, then stop applying torque to the wheels.
                    // IMPORTANT: The maxSpeed variable should be considered as an approximation; the speed of the car
                    // could be a bit higher than expected.

                    ThrottleOff();
                }
            }
        }

        private void DecelerateCar()
        {
            switch (_throttleAxis)
            {
                // The following part resets the throttle power to 0 smoothly.
                case 0f:
                    return;
                case > 0f:
                    _throttleAxis -= (Time.deltaTime * THROTTLE_SPEED);
                    break;
                case < 0f:
                    _throttleAxis += (Time.deltaTime * THROTTLE_SPEED);
                    break;
            }

            if (Mathf.Abs(_throttleAxis) < 0.15f)
                _throttleAxis = 0f;
        }

        internal void ThrottleOff()
        {
            foreach (var item in _wheelControllers)
            {
                item.MotorTorque = 0;
                item.BrakeTorque = 0;
            }
        }

        private void Brakes()
        {
            foreach (var item in _wheelControllers)
                item.BrakeTorque = brakeForce;
        }


        internal void OnDrifting(bool isDrift)
        {
            OnDrift?.Invoke(AreAllWheelsGrounded() ? isDrift : false);

            if (AreAllWheelsGrounded())
                OnTraction?.Invoke(_isTractionLocked, _localVelocityX, CurrentCarSpeed);
            else
                OnTraction?.Invoke(false, 0, 0);
        }

        public void Handbrake()
        {
            DriftCar();

            //Here we tell rear wheels to brake
            foreach (var item in _wheelControllersForHandbrake)
            {
                item.BrakeTorque = brakeForce;
            }

            // Whenever the player uses the handbrake, it means that the wheels are locked, so we set 'isTractionLocked = true'
            // and, as a consequense, the car starts to emit trails to simulate the wheel skids.
            _isTractionLocked = true;
        }

        public void RecoverTraction()
        {
            _isTractionLocked = false;
        }

        public Vector3 GetLocalVelocity()
        {
            return new Vector3(_localVelocityX, 0, _localVelocityZ);
        }


        #region Uncontrolled automated actions

        public void DriftCar()
        {
            //If the forces aplied to the rigidbody in the 'x' asis are greater than
            //SIDE_MOVING_FOR_DRIFT, it means that the car lost its traction, then the car will start emitting particle systems.
            if (Mathf.Abs(GetLocalVelocity().x) > SIDE_MOVING_FOR_DRIFT)
            {
                OnDrifting(true);
            }
            else
            {
                OnDrifting(false);
            }
        }

        public void DecelerateCarFull()
        {
            DriftCar();

            DecelerateCar();

            _carRigidbody.velocity *= (1f / (1f + (0.025f * decelerationMultiplier)));

            ThrottleOff();

            // If the magnitude of the car's velocity is less than 0.25f (very slow velocity), then stop the car completely
            if (_carRigidbody.velocity.magnitude < 0.25f)
            {
                _carRigidbody.velocity = Vector3.zero;
                DeceleratingCar = false;
            }
        }

        #endregion
        
        public bool AreAllWheelsGrounded()
        {
            foreach (var wheel in _wheelControllers)
            {
                if (!wheel.IsGrounded)
                    return false;
            }
            return true;
        }

        public bool AnyWheelGrounded()
        {
            foreach (var wheel in _wheelControllers)
            {
                if (wheel.IsGrounded)
                    return true;
            }
            return false;
        }
    }
}
