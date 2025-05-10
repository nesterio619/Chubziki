using System;
using Actors.Molds;
using Core;
using Core.InputManager;
using Core.Utilities;
using MoreMountains.Feedbacks;
using QuestsSystem.Base;
using UnityEngine;
using Upgrades;
using Lofelt.NiceVibrations;
using MelenitasDev.SoundsGood;
using Core.SaveSystem;
using Components.Car;
using Core.Extensions;

namespace Actors
{
    public sealed class PlayerCarActor : CarActor
    {
        [SerializeField] private MMF_Player CameraShaker;
        [SerializeField] private MMF_Player EffectsOnMeleeAttacking;

        [Header("Damage numbers")]
        [SerializeField] private DamageNumbersPro.DamageNumber damageNumbers;
        [SerializeField] private DamageNumbersPro.DamageNumber healNumbers;
        [SerializeField] private Vector3 numbersOffset;

        [Header("Steering while mowing down")]
        [SerializeField][Tooltip("If car is somewhere between this rotation- we reverse steering direction")] private float angleWidth;
        [SerializeField][Tooltip("We offset the angle to check other directions, except forward")] private float angleOffset;

        public MovementDirectionLimiter MovementDirectionLimiter = new MovementDirectionLimiter();

        public bool HorizontalInput { get; private set; }
        public bool VerticalInput { get; private set; }

        public Pose LastSafePose => _positionTracker.LastTrackedPose;

        private bool _reverseSteering;
        private bool _isPlayerDead = false;
        private Sound _gettingDamageSound;

        private PositionTracker _positionTracker;

        private void Start()
        {
            _gettingDamageSound = new Sound(SFX.DamagePlayerCar).SetFollowTarget(CarDriving.transform);
            _gettingDamageSound.SetVolume(1);
        }

        private protected override void Initialize(CarMold carMold)
        {
            EquipmentManager.OnEquipmnetDamaging += EffectsOnMeleeAttack;
            GetComponent<CarCollisionHandler>().BodyCarRamProvider.OnDamagingObject += EffectsOnMeleeAttack;

            SaveManager.Progress.LoadEquipmentInto(carMold.Equipment);

            base.Initialize(carMold);

            InputManager.OnVerticalAxis += OnVerticalInput;
            InputManager.OnHorizontalAxis += OnPlayerSteer;
            InputManager.OnStopOrAlternativeUse += CarDriving.Brake;
            OnNoHealthLeft += OnPlayerDeath;

            SceneManager.OnSceneChangeTriggered_BeforeAnimation_Event += MovementDirectionLimiter.RestrictAllMovement;

            Player.Instance.OnUpdateEvent += carInfo.UpdateDebugCommands;

            _positionTracker = new PositionTracker(transform);

            UtilitiesProvider.WaitAndRun(() =>
            {
                var savedTransform = SaveManager.Progress.PlayerTransformData;
                if (savedTransform.TryGetPlayerPose(out Pose pose))
                    transform.ApplyPose(pose);

            }, true);
        }

        private void Update()
        {
            _positionTracker.Tick();
        }

        protected override UpgradeLevelContainer GetUpgradeLevelContainer(CarMold carMold)
        {
            return UpgradeConfigs.LoadCarUpgradesIndexes(UpgradeConfigs.UPGRADES_CONFIG_NAME);
        }

        public override void ReturnToPool()
        {
            InputManager.OnVerticalAxis -= OnVerticalInput;
            InputManager.OnHorizontalAxis -= OnPlayerSteer;
            InputManager.OnStopOrAlternativeUse -= CarDriving.Brake;
            SceneManager.OnSceneChangeTriggered_BeforeAnimation_Event -= MovementDirectionLimiter.RestrictAllMovement;

            CarMold.ResetEquipment();
            base.ReturnToPool();

            if (Player.Instance != null)
                Player.Instance.OnUpdateEvent -= carInfo.UpdateDebugCommands;
        }

        protected override void OnDestroy() 
        {
            base.OnDestroy();
            ReturnToPool();
        }

        private void OnVerticalInput(float verticalInput)
        {
            VerticalInput = verticalInput != 0;

            if (_isPlayerDead)
            {
                CarDriving.OnVerticalInput(0);
                return;
            }

            if (MovementDirectionLimiter.RestrictedReverseMovement && verticalInput < 0 || MovementDirectionLimiter.RestrictedForwardMovement && verticalInput > 0)
                verticalInput = 0;

            CarDriving.OnVerticalInput(verticalInput);
        }

        private void OnPlayerSteer(float steeringDirection)
        {
            HorizontalInput = steeringDirection != 0;

            if (_isPlayerDead) return;

            if (MovementDirectionLimiter.RestrictedLeftTurn && steeringDirection < 0 || MovementDirectionLimiter.RestrictedRightTurn && steeringDirection > 0)
                steeringDirection = 0;

            CarDriving.OnHorizontalInput(_reverseSteering ? -steeringDirection : steeringDirection);

            if (steeringDirection == 0)
                IsGoingDown();
        }

        private void IsGoingDown()
        {
            var yAxis = carControllerRigidBody.transform.eulerAngles.y;

            _reverseSteering = yAxis < angleOffset + angleWidth / 2
                               && yAxis > angleOffset - angleWidth / 2;
        }

        protected override void OnHealthChanged(int changeAmount)
        {
            base.OnHealthChanged(changeAmount);

            ShowDamageNumber(changeAmount);

            if (changeAmount < 0)
            {
                _gettingDamageSound.Play();
            }

            CameraShaker.PlayFeedbacks();
        }

        private void EffectsOnMeleeAttack(Collider pushingCollider, float powerOfPush)
        {
            EffectsOnMeleeAttacking.PlayFeedbacks();
            HapticPatterns.PlayConstant(10, 10, 10);
        }


        void ShowDamageNumber(int changeAmount)
        {
            if (changeAmount == 0)
                return;

            //Damage numbers
            Vector3 textPos = transform.position;
            textPos += numbersOffset;

            DamageNumbersPro.DamageNumber pooldeNumber;

            if (changeAmount < 0)
            {
                pooldeNumber = damageNumbers.Spawn();
            }
            else
                pooldeNumber = healNumbers.Spawn();

            pooldeNumber.transform.position = textPos;
            pooldeNumber.number = changeAmount;
        }

        public void Respawn(Vector3 respawnPosition, Quaternion respawnRotation = default)
        {
            TeleportUtilities.TeleportAnimation(transform, respawnPosition, respawnRotation, ResetHealth);
        }

        private void OnPlayerDeath()
        {
            if (_isPlayerDead) return;

            _isPlayerDead = true;

            Player.Instance.RespawnPlayer();

            _isPlayerDead = false;
        }
    }
    public class MovementDirectionLimiter
    {
        public bool RestrictedReverseMovement { get; private set; }
        public bool RestrictedForwardMovement { get; private set; }
        public bool RestrictedLeftTurn { get; private set; }
        public bool RestrictedRightTurn { get; private set; }

        public void SetMovementRestrictions(bool restrictReverse = false, bool restrictForward = false, bool restrictLeft = false, bool restrictRight = false)
        {
            RestrictedReverseMovement = restrictReverse;
            RestrictedForwardMovement = restrictForward;
            RestrictedLeftTurn = restrictLeft;
            RestrictedRightTurn = restrictRight;
        }
        public void RestrictAllMovement() => SetMovementRestrictions(true, true, true, true);
    }
}