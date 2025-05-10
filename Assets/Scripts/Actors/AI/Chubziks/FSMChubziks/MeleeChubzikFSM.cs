using Actors.AI.Chubziks.Base;
using Actors.Molds;
using Components.Animation;
using Components.Particles;
using Components.ProjectileSystem.AttackPattern;
using Core;
using Core.Interfaces;
using Core.ObjectPool;
using Regions;
using RSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Actors.AI.Chubziks
{
    public class MeleeChubzikFSM : ChubzikActor, IStateBehaviour
    {
        #region AttackVariables
        [SerializeField] private LayerMask attackLayer;
        [SerializeField] private Transform attackPoint;
        #endregion


        #region Conditions
        [Condition] public bool CanAttack => _currentMeleeAttackPattern.CanAttack;
        [Condition] public bool IsAttacking => _isAttackingAnimation;
        [Condition] public bool IsPlayerTooClose => _currentMeleeAttackPattern != null && Vector3.Distance(currentTarget.position, transform.position) < _currentMeleeAttackPattern.DistanceToAttack && navMeshAgent.enabled;
        [Condition] public bool IsStunned => !_isStanding;
        [Condition] public bool IsLogicActive => _isLogicActive;
        [Condition] public bool IsPlayerInSameLocation => assignedLocation.Bounds.Contains(currentTarget.position) || isStoppedReturningToOwnLocation;

        [Condition] public bool StopReturnInLocation => returnToSector && Vector3.Distance(transform.parent.position, transform.position) < 2;
        #endregion

        private MeleeAttackPattern _currentMeleeAttackPattern;

        private void Awake()
        {
            navMeshAgent.enabled = false;
            _defaultSpeed = navMeshAgent.speed;
        }


        private void RestartNavMesh()
        {
            navMeshAgent.enabled = false;
            navMeshAgent.enabled = true;
        }

        public override void LoadActor(ChubzikMold actorMold, ChubzikModel chubzikModel, AttackPattern attackPattern)
        {

            if (actorMold is not ChubzikMeleeMold)
            {
                Debug.Log("Wrong mold for chubzik melee");
                return;
            }

            currentTarget = Player.Instance.PlayerCarGameObject.transform;

            _currentMeleeAttackPattern = attackPattern as MeleeAttackPattern;

            base.LoadActor(actorMold, chubzikModel, attackPattern);
        }

        public override void ToggleLogic(bool stateToSet)
        {
            if (IsVisible)
                return;

            base.ToggleLogic(stateToSet);

            if (stateToSet)
                RestartNavMesh();

            if (_currentMeleeAttackPattern != null)
            {
                if (stateToSet)
                    _currentMeleeAttackPattern.SetupPattern(attackPoint);
                else
                    _currentMeleeAttackPattern.StopAttackCooldown();
            }

            _isLogicActive = stateToSet;
        }

        public override LayerMask GetAttackLayer()
        {
            return attackLayer;
        }

        protected override void AnimationFalse()
        {
            base.AnimationFalse();
            StopReturnToOwnLocation();
        }

        /// <summary>
        /// States
        /// </summary>
        #region States

        #region Active
        [State]
        private void Active()
        {

        }
        #endregion

        #region Idle
        [State]
        private void EnterIdle()
        {
            actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Idle);
        }

        [State]
        private void Idle()
        {
            Vector3 dir = currentTarget.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, 120 * Time.deltaTime);


        }

        [State]
        private void ExitIdle()
        {
            isStoppedReturningToOwnLocation = false;
        }
        #endregion

        #region Move

        [State]
        private void EnterMove()
        {
            actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Disturb);
            navMeshAgent.isStopped = false;
        }

        [State]
        private void Move()
        {
            if (outsideSectorCoroutine == null && !returnToSector && !IsInsideOwnLocation && !isStoppedReturningToOwnLocation)
                outsideSectorCoroutine = StartCoroutine(ReturnToSectorDelay());

            if (IsInsideOwnLocation && outsideSectorCoroutine != null)
            {
                StopCoroutine(outsideSectorCoroutine);
                outsideSectorCoroutine = null;
            }

            Vector3 destination = Player.Instance.PlayerCarGameObject.transform.position;
            if (returnToSector)
            {
                if (IsInsideOwnLocation && IsPlayerInSameLocation)
                    returnToSector = false;
                else
                    destination = transform.parent.position;
            }

            navMeshAgent.SetDestination(destination);
        }

        [State]
        private void ExitMove()
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();

            if (IsInsideOwnLocation) returnToSector = false;
        }

        private IEnumerator ReturnToSectorDelay()
        {
            yield return new WaitForSeconds(outsideSectorTimeLimit);

            returnToSector = true;
            outsideSectorCoroutine = null;
        }

        #endregion

        #region Attack

        [State]
        private void EnterAttack()
        {
            _isAttackingAnimation = true;

            actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Attack);
        }



        #endregion

        #region Stun
        [State]
        private void EnterStunned()
        {
            if (_currentHealth > 0)
                currentStunParticle = PooledParticle.TryToLoadAndPlay(particleStunPool, null);

            _isAttackingAnimation = false;
            navMeshAgent.enabled = false;
            actorAnimationController.StopAnimation();
        }

        [State]
        private void Stunned()
        {
            if (currentStunParticle == null)
                return;

            currentStunParticle.transform.position = actorRigidbody.position + stunParticlesPoisitionOffset;
        }

        [State]
        private void ExitStunned()
        {
            if (currentStunParticle != null)
                currentStunParticle.StopParticle();
            currentStunParticle = null;
            navMeshAgent.enabled = true;

        }
        #endregion

        #endregion

        #region ChubzikGetAttacked

        protected override void StopReturnToOwnLocation()
        {
            if (outsideSectorCoroutine != null)
                StopCoroutine(outsideSectorCoroutine);
            outsideSectorCoroutine = null;

            isStoppedReturningToOwnLocation = true;
            returnToSector = false;
        }

        #endregion

        protected override void ChubzikPerformingAttack()
        {
            _currentMeleeAttackPattern.PerformAttack();
        }

        protected override void ReturnToPoolAttackPattern()
        {
            if (_currentMeleeAttackPattern != null)
            {
                _currentMeleeAttackPattern.ReturnToPool();
                _currentMeleeAttackPattern = null;
            }
        }

    }
}