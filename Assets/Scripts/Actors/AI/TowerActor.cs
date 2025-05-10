using Actors.Molds;
using AI;
using Components.ProjectileSystem.AttackPattern;
using Core.Enums;
using Core.Interfaces;
using Core.ObjectPool;
using Core.Utilities;
using System;
using UnityEngine;

namespace Actors.AI
{
    public class TowerActor : AIActor
    {
        [field: SerializeField] public PrefabPoolInfo attackPoolPattern_PrefabPoolInfo { get; protected set; }
        [SerializeField] private AimProvider.AimingUserData aimData;

        private RangedAttackPattern _rangeAttackPattern;
        private Collider[] _ignoredColliders;
        private Action _stopAiming;

        private bool _isPushedAfterDeath;

        public Transform FirePoint => aimData.FirePoint;

        public override void LoadActor(Mold actorMold)
        {
            _ignoredColliders = GetComponentsInChildren<Collider>();
            base.LoadActor(actorMold);
            actorRigidbody.isKinematic = true;
            _isStanding = true;
            _isPushedAfterDeath = false;

            _rangeAttackPattern = ObjectPooler.TakePooledGameObject(attackPoolPattern_PrefabPoolInfo).GetComponent<RangedAttackPattern>();
            _rangeAttackPattern.Initialize(transform, aimData.FirePoint, _ignoredColliders, UnityLayers.EnemyProjectile.GetIndex());

            //_aimProvider = new AimProvider(FirePoint,_rangeAttackPattern,offsetOfTargetPosition,targetType);
            //_aimProvider.RotationGetter += RotateFirePoint;
        }

        public override void ReturnToPool()
        {
            _stopAiming?.Invoke();
            _rangeAttackPattern.ReturnToPool();

            base.ReturnToPool();
        }

        private void RotateFirePoint(Quaternion targetRotation)
        {
            FirePoint.rotation = Quaternion.Lerp(FirePoint.rotation, targetRotation, _rangeAttackPattern.RotationSpeed);

            TryShoot(targetRotation);
        }

        private void TryShoot(Quaternion targetRotation)
        {
            if (Quaternion.Angle(targetRotation, Quaternion.LookRotation(aimData.FirePoint.forward)) < _rangeAttackPattern.MinimalAngleToShoot)
            {
                _rangeAttackPattern.SetShootLoop(true);
                _rangeAttackPattern.PerformAttack();
            }
            else
            {
                _rangeAttackPattern.SetShootLoop(false);
            }
        }

        public override void ToggleLogic(bool stateToSet)
        {
            if (stateToSet)
                AimProvider.StartSearchAndAim(aimData, _rangeAttackPattern, RotateFirePoint, out _stopAiming);
            else
                _stopAiming?.Invoke();
        }


        public override void Fall(Vector3 pushForce)
        {
            if (!actorRigidbody.isKinematic && !_isPushedAfterDeath)
            {
                _isPushedAfterDeath = true;
                actorRigidbody.AddForce(pushForce, IPushable.CurrentForceMode);
            }
        }

        public override void ChangeHealthBy(int changeAmount)
        {
            base.ChangeHealthBy(changeAmount);

            if (CurrentHealth > 0)
                return;

            _isStanding = false;
            actorRigidbody.isKinematic = false;
            ToggleLogic(false);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (_rangeAttackPattern != null)
                Gizmos.DrawWireSphere(FirePoint.position, _rangeAttackPattern.FiringRadius);
        }

    }
}