using System;
using System.Collections;
using System.Collections.Generic;
using Actors;
using Components.Car.CarLogic;
using Core;
using Core.Utilities;
using NWH.WheelController3D;
using UnityEngine;
using Upgrades;

namespace Components.Car
{
    public sealed class CarDriving : MonoBehaviour, IDisposable
    {
        [SerializeField] private WheelLogic wheelLogic;
        [SerializeField] private WheelTurning wheelTurning;
        [SerializeField] private CarParticles _carParticlesManager;
        [SerializeField] private CarAirControl _airControl;

        [Header("Wheels references")]
        [SerializeField]
        private List<WheelController> wheelControllers = new();
        [SerializeField]
        private List<WheelController> wheelControllersForHandBrake = new();
        [SerializeField]
        private MeshFilter[] wheelModels;

        [SerializeField]
        private CarCollisionHandler carCollisionHandler;

        private WheelAnimator wheelAnimator = new();

        //==-- Braking --==//
        private bool _brake = false;

        private bool _lastBrake;
        //==--         --==//
        public float CarSpeed { private set; get; }

        private Rigidbody _carRigidbody;

        private bool _isInitialized = false;

        private Vector2 _input;

        #region Initialization

        public void Initialize(Rigidbody carRigidbody, UpgradesList upgradeList, CarActor carActor)
        {
            this._carRigidbody = carRigidbody;
            if (!_isInitialized)
                _carParticlesManager.Initialize(wheelControllersForHandBrake.ToArray(), carCollisionHandler, carActor);
            wheelLogic.Initialize(carRigidbody, gameObject, upgradeList, wheelControllers.ToArray(), wheelControllersForHandBrake.ToArray(), _carParticlesManager);
            wheelTurning.UpdateUpgrades(upgradeList);

            if (!_isInitialized)
                wheelAnimator.Initialize(wheelModels, wheelControllers.ToArray());

            Player.Instance.OnFixedUpdateEvent += OnExternalFixedUpdate;

            _isInitialized = true;
        }

        public void InitializeAirControl(Rigidbody carRigidbody, float carBodyWidth)
        {
            _airControl.Initialize(carRigidbody, transform, carBodyWidth, GetAirInput, AllWheelsGrounded);
        }
        public Vector2 GetAirInput() => _input;
        public bool AllWheelsGrounded() => wheelLogic.AreAllWheelsGrounded();

        public void AllowMovement(bool allow)
        {
            if (allow)
                wheelLogic.RecoverTraction();
            else
                wheelLogic.Handbrake();
        }

        public void UpdateUpgrades(UpgradesList upgradeList)
        {
            wheelLogic.UpdateUpgrades(upgradeList);
            wheelTurning.UpdateUpgrades(upgradeList);

        }

        private void OnDestroy() => Dispose();

        public void Dispose()
        {
            wheelLogic.Dispose();
            wheelAnimator.Dispose();
            _carParticlesManager.Dispose();
            _airControl.Dispose();
            _isInitialized = false;

            if (Player.Instance != null)
                Player.Instance.OnFixedUpdateEvent -= OnExternalFixedUpdate;
        }

        #endregion

        private void OnExternalFixedUpdate()
        {
            if (wheelLogic != null)
            {
                CarSpeed = wheelLogic.CurrentCarSpeed;

                if (_brake)
                {
                    wheelLogic.ThrottleOff();
                    wheelLogic.DeceleratingCar = false;
                    wheelLogic.Handbrake();
                }

                if (_lastBrake && !_brake)
                {
                    wheelLogic.RecoverTraction();
                }
            }

            _lastBrake = _brake;

            _brake = false;
        }

        private void HandleTorque(float verticalInput = 0, float horizontalInput = 0f)
        {
            if (wheelLogic.AreAllWheelsGrounded())
                return;
        }

        public void ChangeStateDamageCar(bool isDamaged) => _carParticlesManager.ToggleCarSmoke(isDamaged); // Bullshit code. Need to move this into CarActor and check if it's already activated

        public void ResetState()
        {
            _brake = false;
            _lastBrake = false;
            _input = Vector2.zero; 
            CarSpeed = 0f; 
            wheelLogic.ThrottleOff(); 
            wheelLogic.RecoverTraction(); 
            wheelTurning.ResetSteeringAngle(); 
            ChangeStateDamageCar(false); 
            Debug.Log("CarDriving state reset");
        }
        #region OnInput

        internal void OnVerticalInput(float moveVertical)
        {
            _input.y = moveVertical;

            if (wheelLogic == null)
                return;

            if (!moveVertical.Equals(0f))
                HandleTorque(moveVertical);

            if (moveVertical!=0)
            {
                wheelLogic.DeceleratingCar = false;
                wheelLogic.GoDirection(moveVertical);
            }
            else
            {
                wheelLogic.ThrottleOff();
            }

            if (moveVertical == 0 && !_brake && !wheelLogic.DeceleratingCar)
                wheelLogic.DeceleratingCar = true;
        }

        internal void OnHorizontalInput(float moveHorizontal)
        {
            _input.x = moveHorizontal;

            if (wheelTurning == null)
                return;

            if (moveHorizontal.Equals(0))
                wheelTurning.ResetSteeringAngle();
            else
            {
                wheelTurning.TurningWheels(moveHorizontal);
                HandleTorque(0, moveHorizontal);
            }
        }

        public void Brake()
        {
            _brake = true;
        }
        #endregion

        public void MoveForwardForSeconds(float seconds) => StartCoroutine(MoveForwardForSecondsCoroutine(seconds));
        
        public IEnumerator MoveForwardForSecondsCoroutine(float seconds)
        {
            float timer = 0;
            do
            {
                MoveForward();
                timer += Time.deltaTime;
                yield return null;

            } while (timer < seconds);
        }

        public void MoveForward()
        {
            OnVerticalInput(1);
        }

        public void MoveStop()
        {
            OnVerticalInput(0);
        }

        public int GetCarSpeedInKm() => Mathf.RoundToInt(MathUtils.GetSpeedInKilometersPerHour(_carRigidbody));
        
    }
}
