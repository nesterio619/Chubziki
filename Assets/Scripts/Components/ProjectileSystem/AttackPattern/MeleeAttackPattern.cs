using Core.Extensions;
using Core.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Components.ProjectileSystem.AttackPattern
{
    public class MeleeAttackPattern : AttackPattern
    {

        [SerializeField] private MeleeAttackConfig meleeAttackConfig;

        private int _damage => meleeAttackConfig.Damage;
        private float _attackCooldown => meleeAttackConfig.AttackCooldown;
        private float _attackCooldownTimer;
        private float _attackRange => meleeAttackConfig.AttackRange;
        public float DistanceToAttack => meleeAttackConfig.DistanceToAttack;
        private Vector3 _attackSize => meleeAttackConfig.AttackSize;
        private LayerMask _attackLayerMask;

        private Coroutine cooldownCoroutine;

        public override bool CanAttack => _attackCooldownTimer > _attackCooldown;

        public override void Initialize(Transform parent, Transform firePoint, Collider[] ignoredColliders, LayerMask attackLayer)
        {
            transform.SetParent(parent);

            SetAttackLayer(attackLayer);
        }

        public void SetupPattern(Transform positonAndParent)
        {
            _attackCooldownTimer = _attackCooldown;

            gameObject.transform.position = positonAndParent.position;

            if (cooldownCoroutine == null)
                cooldownCoroutine = StartCoroutine(AttackCooldown());
        }

        private IEnumerator AttackCooldown()
        {
            while (true)
            {
                _attackCooldownTimer += Time.deltaTime;
                yield return null;

            }
        }

        public void SetAttackLayer(LayerMask attackLayer)
        {
            _attackLayerMask = attackLayer;
        }

        public void StopAttackCooldown()
        {
            if (cooldownCoroutine != null)
                StopCoroutine(cooldownCoroutine);
            cooldownCoroutine = null;
        }

        public override void ReturnToPool()
        {
            StopAttackCooldown();
            base.ReturnToPool();
        }
        public void AttackColliders(Collider[] colliders)
        {
            List<IDamageable> attackedTargets = new();

            foreach (var collider in colliders)
            {
                IDamageable damageable = collider.gameObject.transform.parent.GetComponent<IDamageable>();

                if (attackedTargets.Contains(damageable))
                    return;

                attackedTargets.Add(damageable);

                if (damageable != null)
                {
                    damageable.ChangeHealthBy(-_damage);
                }
            }
        }

        public override void PerformAttack()
        {
            if (!CanAttack)
            {
                Debug.LogWarning("Can not attack!");
                return;
            }

            transform.position = transform.parent.position + transform.parent.forward * _attackRange;
            _attackCooldownTimer = 0;

#if UNITY_EDITOR

            StartCoroutine(VisualizeTimer());
#endif
            Vector3 position = transform.position;
            position.y += _attackSize.y / 2;
            AttackColliders(Physics.OverlapBox(position, _attackSize / 2, Quaternion.identity, _attackLayerMask));
        }

#if UNITY_EDITOR
        private bool _visualizedCollier = false;
        private IEnumerator VisualizeTimer()
        {
            _visualizedCollier = true;
            yield return new WaitForSeconds(0.1f);
            _visualizedCollier = false;

        }

        private void OnDrawGizmos()
        {
            if (!ShowVisualPattern || !_visualizedCollier)
                return;

            Gizmos.color = Color.red;
            Vector3 position = transform.position;
            position.y += _attackSize.y / 2;
            Gizmos.DrawCube(position, _attackSize);
        }
#endif
    }
}