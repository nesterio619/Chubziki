using Actors;
using Actors.AI;
using Actors.AI.Chubziks;
using Actors.AI.Chubziks.Base;
using Components.ProjectileSystem.AttackPattern;
using Core;
using Core.Enums;
using Core.Interfaces;
using Core.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


namespace AI
{
    public static class AimProvider
    {
        private const float Logic_Update_Time = 0.025f;
        private const int Max_Registered_Targets = 20;
        private const float Debug_Ray_Length = 20;

        [Serializable]
        public struct AimingUserData
        {
            public Transform FirePoint;
            public Vector3 OffsetOfTargetPosition;
            public TargetTypes TargetType;

            [HideInInspector] public RangedAttackPattern RangeAttackPattern;

            public float FiringRadius => RangeAttackPattern.FiringRadius;
        }

        public static void StartSearchAndAim(AimingUserData aimingUserData, RangedAttackPattern rangeAttackPattern, Action<Quaternion> OnRotationRequiredAction, out Action stopAimingAction)
        {
            aimingUserData.RangeAttackPattern = rangeAttackPattern;

            var coroutine = Player.Instance.StartCoroutine(SearchAndAim(aimingUserData, OnRotationRequiredAction));

            stopAimingAction = () =>
            {
                aimingUserData.RangeAttackPattern.SetShootLoop(false);
                aimingUserData.RangeAttackPattern.SetBeforeShootFunc(null);
                
                if(Player.Instance != null)
                    Player.Instance.StopCoroutine(coroutine);
            };
        }

        private static IEnumerator SearchAndAim(AimingUserData data, Action<Quaternion> OnRotationRequiredAction)
        {
            IDamageable currentTarget = null;
            Rigidbody currentTargetRigidbody = null;
            Collider[] targetsInAttackDistance = new Collider[Max_Registered_Targets];

            data.RangeAttackPattern.SetDistanceFunc(GetDistanceToTarget);
            data.RangeAttackPattern.SetBeforeShootFunc(BeforeShoot);

            while (data.RangeAttackPattern != null)
            {
                if (currentTarget == null)
                {
                    yield return SearchTarget(data, targetsInAttackDistance,
                        result => currentTarget = result);

                    if (currentTarget != null)
                        currentTargetRigidbody = currentTarget.GetRigidbody();
                }

                if (currentTarget == null || currentTargetRigidbody == null
                    || TargetIsOutOfRange() || currentTarget.CurrentHealth <= 0)
                {
                    ResetTarget();
                }
                else
                    yield return AimAtTarget(data, currentTargetRigidbody.transform, currentTargetRigidbody, OnRotationRequiredAction);

                yield return new WaitForSeconds(Logic_Update_Time);
            }

            float GetDistanceToTarget()
            {
                if (currentTargetRigidbody == null) return 0;

                return Vector3.Distance(currentTargetRigidbody.transform.position, data.FirePoint.position);
            }

            bool TargetIsOutOfRange()
            {
                var allAxes = Axis.X | Axis.Y | Axis.Z;
                return !MathUtils.ObjectIsTooClose(currentTargetRigidbody.transform.position, data.FirePoint.position, allAxes, data.FiringRadius);
            }

            void ResetTarget()
            {
                currentTarget = null;
                currentTargetRigidbody = null;

                if (data.RangeAttackPattern != null)
                    data.RangeAttackPattern.SetShootLoop(false);
            }

            bool BeforeShoot()
            {
                if(currentTargetRigidbody == null) return false;

                var canSeeTarget = CanSeeTarget(data, currentTargetRigidbody.transform);

                if (!canSeeTarget) ResetTarget();

                return canSeeTarget;
            }
        }

        public static IEnumerator AimAtTarget(AimingUserData data, Transform currentTarget, Rigidbody targetRigidbody, Action<Quaternion> OnRotationRequiredAction)
        {
            if (currentTarget == null)
            {
                yield return null;
                yield break;
            }

            var targetPosition = currentTarget.position + data.OffsetOfTargetPosition;
            var targetVelocity = targetRigidbody == null ? Vector3.zero : targetRigidbody.velocity;
            var directionToTarget = targetPosition - data.FirePoint.position;
            var projectileSpeed = data.RangeAttackPattern.GetProjectileSpeed();

            Quaternion targetRotation = Quaternion.identity;

            if (projectileSpeed <= 0 || targetVelocity.magnitude <= 0)
            {
                // If the projectile or target is stationary, simply shoot at the current position
                targetRotation = Quaternion.LookRotation(directionToTarget.normalized);
            }
            else
            {
                bool directionFound = MathUtils.FindAngleToShoot(targetPosition, targetVelocity, data.FirePoint.position, projectileSpeed, out var direction);
                targetRotation = Quaternion.LookRotation(directionFound ? direction : directionToTarget.normalized);
            }

            OnRotationRequiredAction?.Invoke(targetRotation);

            Debug.DrawRay(data.FirePoint.position, data.FirePoint.forward * Debug_Ray_Length, Color.green, Logic_Update_Time);
            Debug.DrawRay(data.FirePoint.position, targetRotation * Vector3.forward * Debug_Ray_Length, Color.white, Logic_Update_Time);

            yield return null;
        }

        public static IEnumerator SearchTarget(AimingUserData data, Collider[] targetsInAttackDistance, Action<IDamageable> onTargetFound)
        {
            List<IDamageable> targetTransforms = new();

            int amountOfDetectedTargets = Physics.OverlapSphereNonAlloc(data.FirePoint.position, data.FiringRadius, targetsInAttackDistance, GetLayerFromType(data.TargetType));

            if (amountOfDetectedTargets > targetsInAttackDistance.Length)
                amountOfDetectedTargets = Max_Registered_Targets;

            for (int i = 0; i < amountOfDetectedTargets; i++)
            {
                var target = GetTransformIfValidTarget(targetsInAttackDistance[i].gameObject, data.TargetType);

                if (target == null) continue;
                if (!CanSeeTarget(data, targetsInAttackDistance[i].transform)) continue;

                onTargetFound?.Invoke(target);
                yield break;
            }

            onTargetFound?.Invoke(null);
            yield return null;
        }

        private static IDamageable GetTransformIfValidTarget(GameObject target, TargetTypes targetFlags)
        {
            if (targetFlags.HasFlag(TargetTypes.Enemy)
                && UtilitiesProvider.TrySearchComponentInObject(target, out ChubzikActor chubzikActor)
                && chubzikActor.IsAlive && chubzikActor.IsStanding)
                return chubzikActor;

            if (targetFlags.HasFlag(TargetTypes.Enemy)
                && UtilitiesProvider.TrySearchComponentInObject(target, out TowerActor towerActor)
                && towerActor.IsAlive && towerActor.IsStanding)
                return towerActor;

            if (targetFlags.HasFlag(TargetTypes.TrainingDummy)
            && UtilitiesProvider.TrySearchComponentInObject(target, out TrainingDummy dummyActor)
            && dummyActor.IsAlive && dummyActor.IsStanding)
                return dummyActor;

            if (targetFlags.HasFlag(TargetTypes.Friendly)
                && UtilitiesProvider.TrySearchComponentInObject(target, out PlayerCarActor playerActor))
                return playerActor;

            return null;
        }

        private static bool CanSeeTarget(AimingUserData data, Transform target)
        {
            var targetPosition = target.position + data.OffsetOfTargetPosition;
            var targetDirection = (targetPosition - data.FirePoint.position).normalized;
            var targetDistance = Vector3.Distance(data.FirePoint.position, targetPosition);
            
            return !Physics.Raycast(data.FirePoint.position, targetDirection, targetDistance, (int)UnityLayers.Environment);
        }

        private static LayerMask GetLayerFromType(TargetTypes targetType)
        {

            LayerMask layer = LayerMask.GetMask("Nothing");

            if (targetType.HasFlag(TargetTypes.Friendly))
            {
                layer |= (1 << LayerMask.NameToLayer("ActorPlayer"));
            }
            if (targetType.HasFlag(TargetTypes.Enemy))
            {
                layer |= (1 << LayerMask.NameToLayer("ActorEnemy"));
            }
            if (targetType.HasFlag(TargetTypes.TrainingDummy))
            {
                layer |= (1 << LayerMask.NameToLayer("ActorNeutral"));
            }

            return layer;
        }

        [Flags]
        public enum TargetTypes
        {
            Friendly = 1,
            Enemy = 2,
            TrainingDummy = 4
        }
    }
}