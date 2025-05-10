using Components.Particles;
using Core.ObjectPool;
using System;
using System.Collections;
using UnityEngine;

namespace Components.ProjectileSystem.AttackPattern
{
    public class RangedAttackPattern : AttackPattern
    {
        [SerializeField] private RangedAttackConfig rangeAttackConfig;

        public float FiringRadius => rangeAttackConfig.FiringRadius;
        public float RotationSpeed => rangeAttackConfig.RotationSpeed;
        //This is a imaginary cone within which shooting is possible, created from the aiming center and this angle.
        public float MinimalAngleToShoot => rangeAttackConfig.MinimalAngleToShoot;
        private bool _canAttack = true;
        public override bool CanAttack => _canAttack;

        private Transform _firePoint;
        private Collider[] _ignoredColliders;
        private LayerMask projectileLayerMask;
        private float _totalTimeBetweenShots;

        public Action OnShoot;

        private Coroutine _shootLoopCoroutine;
        private bool _isShootLoop;

        private Func<float> _getDistanceToTarget;
        public Func<bool> _beforeShootFunc;

        public override void Initialize(Transform parent,Transform firePoint, Collider[] ignoredColliders, LayerMask projectileLayerMask)
        {
            transform.SetParent(parent);
            _firePoint = firePoint;
            _ignoredColliders = ignoredColliders;
            transform.position = _firePoint.position;
            transform.rotation = _firePoint.rotation;
            this.projectileLayerMask = projectileLayerMask;
            _canAttack = true;
            _isShootLoop = false;

            _totalTimeBetweenShots = UnityEngine.Random.Range(
                rangeAttackConfig.totalTimeBetweenShots.x, 
                rangeAttackConfig.totalTimeBetweenShots.y
                );
        }


        public void SetShootLoop(bool shootLoop) => _isShootLoop = shootLoop;

        public bool GetLoopCoroutine()
        {
            return _shootLoopCoroutine != null;
        }

        public override void PerformAttack()
        {
            if (_shootLoopCoroutine != null)
                return;

            _shootLoopCoroutine = StartCoroutine(ShootingProcess());
        }

        private IEnumerator ShootingProcess()
        {
            do
            {
                yield return null;
                TypeOfAttack();

            } while (_isShootLoop);

            _isShootLoop = false;
            _shootLoopCoroutine = null;
        }

        private void TypeOfAttack()
        {
            if (!_canAttack)
                return;

            if (rangeAttackConfig.isBurstFire)
            {
                StartCoroutine(BurstShootWithCooldown());
            }
            else
            {
                StartCoroutine(OneShootWithCooldown());
            }
        }

        private IEnumerator OneShootWithCooldown()
        {

            _canAttack = false;
            LoadAndShootProjectile();

            yield return new WaitForSeconds(_totalTimeBetweenShots);

            _canAttack = true;
        }

        private IEnumerator BurstShootWithCooldown()
        {
            _canAttack = false;

            for (int i = 0; i < rangeAttackConfig.shotsPerBurst; i++)
            {
                LoadAndShootProjectile();
                if (i != rangeAttackConfig.shotsPerBurst - 1)
                    yield return new WaitForSeconds(_totalTimeBetweenShots);
            }

            yield return new WaitForSeconds(rangeAttackConfig.pauseBetweenBursts);
            _canAttack = true;

        }

        public void LoadAndShootProjectile()
        {
            if(_beforeShootFunc!=null && _beforeShootFunc.Invoke()==false) return;

            for (int i = 0; i < rangeAttackConfig.projectilesPerShoot; i++)
            {
                var projectile = ObjectPooler.TakePooledProjectile(rangeAttackConfig.projectileType.ProjectileBase);
                projectile.transform.position = _firePoint.position;
                projectile.transform.rotation = _firePoint.rotation;
                projectile.Initialize(rangeAttackConfig.projectileType, _ignoredColliders, projectileLayerMask);

                var distanceToTarget = _getDistanceToTarget == null ? 0 : _getDistanceToTarget();
                var percent = rangeAttackConfig.SpreadCurve.Evaluate(distanceToTarget / rangeAttackConfig.FiringRadius);

                var shootSpreadHorizontal = Mathf.Lerp(rangeAttackConfig.MinSpread.x, rangeAttackConfig.MaxSpread.x, percent);
                var shootSpreadVertical = Mathf.Lerp(rangeAttackConfig.MinSpread.y, rangeAttackConfig.MaxSpread.y, percent);

                Quaternion rotation = Quaternion.Euler(0,
                    UnityEngine.Random.Range(-shootSpreadHorizontal, shootSpreadHorizontal),
                    UnityEngine.Random.Range(-shootSpreadVertical, shootSpreadVertical));

                Vector3 newDirection = rotation * projectile.transform.forward;
                projectile.Launch(newDirection);

                if(rangeAttackConfig.shootParticlesPoolInfo!=null)
                    PooledParticle.TryToLoadAndPlay(rangeAttackConfig.shootParticlesPoolInfo, _firePoint);
            }

            if (rangeAttackConfig.ParticlesOnShootPool != null)
            {
                var particles = PooledParticle.TryToLoadAndPlay(rangeAttackConfig.ParticlesOnShootPool, _firePoint);

                particles.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }

            OnShoot?.Invoke();

            // Required to set position of projectiles with interpolated rigidbody
            Physics.SyncTransforms();
        }

        public float GetProjectileSpeed()
        {
            return rangeAttackConfig.projectileType.defaultSpeed;
        }

        public void SetDistanceFunc(Func<float> getDistanceToTarget) => _getDistanceToTarget = getDistanceToTarget;
        public void SetBeforeShootFunc(Func<bool> beforeShootFunc) => _beforeShootFunc = beforeShootFunc;


        public override void ReturnToPool()
        {
            if (_shootLoopCoroutine != null)
                StopCoroutine(_shootLoopCoroutine);

            _shootLoopCoroutine = null;

            base.ReturnToPool();
        }

        private void OnDrawGizmos()
        {
            if (!ShowVisualPattern)
                return;
            if (_firePoint != null)
                Gizmos.DrawRay(_firePoint.position, Vector3.forward);
        }
    }
}