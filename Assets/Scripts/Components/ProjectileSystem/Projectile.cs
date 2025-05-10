using Components.Particles;
using Components.RamProvider;
using Core.Enums;
using Core.Extensions;
using Core.Interfaces;
using Core.ObjectPool;
using Core.Utilities;
using UnityEngine;

namespace Components.ProjectileSystem
{
    public class Projectile : PooledGameObject, IAttacker
    {
        private ProjectileMold _projectileMold;

        [SerializeField] private LayerMask beforeCollideIncludeLayers;
        [SerializeField] private LayerMask afterCollideIncludeLayers;
        [SerializeField] private TriggerHandler projectileTrigger;
        [SerializeField] private ProjectileRamProvider projectileRamProvider;

        private PooledGameObject _projectileModel;

        private Collider[] _ignoredColliders;

        private Vector3 _direction;

        private Collider _projectileCollider;
        private Rigidbody _currentRigidbody;

        private UnityLayers _projectileLayer;

        private bool _interactable;

        private AnimatedPooledParticle _projectileParticles;

        public int AttackDamage => _projectileMold.damage;

        private bool _isInitialized;

        public void Initialize(ProjectileMold projectileMold, Collider[] ignoredColliders, LayerMask layerOfProjectile)
        {
            if (_isInitialized)
                return;

            _ignoredColliders = ignoredColliders;

            _projectileMold = projectileMold;
            projectileRamProvider.DefaultPushForce = projectileMold.pushForceTarget;
            projectileRamProvider.ownWeight = _projectileMold.ownWeight;

            poolName = projectileMold.ProjectileBase.PoolName;

            _projectileModel = ObjectPooler.TakePooledGameObject(projectileMold.ModelOfProjectile);

            _projectileModel.transform.SetParent(transform, false);

            _projectileModel.transform.localPosition = Vector3.zero;
            _projectileModel.transform.localRotation = Quaternion.identity;

            _projectileModel.gameObject.layer = layerOfProjectile;
            _projectileLayer = _projectileModel.gameObject.GetObjectLayer();

            _projectileCollider = _projectileModel.GetComponent<Collider>();
            projectileRamProvider = GetComponent<ProjectileRamProvider>();
            _currentRigidbody = GetComponent<Rigidbody>();

            IgnoreCollisionWithOwner(_projectileCollider);

            projectileRamProvider.CurrentColliderHandler.SetCollider(_projectileCollider);
            projectileTrigger.OnEnter.AddListener(OnProjectileCollision);
            projectileRamProvider.CurrentColliderHandler.OnEnter.AddListener(OnProjectileCollision);

            _currentRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _currentRigidbody.isKinematic = false;
            _currentRigidbody.useGravity = false;

            _projectileCollider.isTrigger = false;
            _projectileCollider.enabled = true;
            _projectileCollider.includeLayers = beforeCollideIncludeLayers;

            InitializeParticles();

            _interactable = true;

            _isInitialized = true;
        }

        private void InitializeParticles()
        {
            _projectileParticles = _projectileModel.GetComponentInChildren<AnimatedPooledParticle>();

            if (_projectileParticles != null)
            {
                _projectileParticles.SetDefaultState();
                _projectileParticles.SetDurationScaling(_projectileMold.DurationScaling);
            }
        }

        private void StopParticles()
        {
            if (_projectileParticles != null)
                _projectileParticles.EndAnimation();
        }

        private void OnProjectileCollision(Collision collision) =>
            OnProjectileCollision(collision.collider);

        private void OnProjectileCollision(Collider collider)
        {
            if (!_interactable || collider == null) return;

            StopParticles();

            UnityLayers hitLayer = collider.gameObject.GetObjectLayer();
            Impact(hitLayer);

            if (_projectileMold.ParticlesOnHitting != null)
                PooledParticle.TryToLoadAndPlay(_projectileMold.ParticlesOnHitting, _projectileCollider.transform.position);

            DamageObstacle(collider);

            if (projectileRamProvider != null && _projectileMold.pushTarget) projectileRamProvider.PushObstacle(collider);

            IgnoreCollisionWithOwner(collider);
        }

        private void Impact(UnityLayers hitLayer)
        {
            bool shouldReturnInstantly =
                (_projectileLayer == UnityLayers.FriendlyProjectile && hitLayer == UnityLayers.EnemyProjectile) ||
                (_projectileLayer == UnityLayers.EnemyProjectile && hitLayer == UnityLayers.FriendlyProjectile) ||
                _projectileMold.lifetimeAfterHit == 0;

            if (shouldReturnInstantly)
            {
                ReturnToPool();
                return;
            }

            _projectileCollider.includeLayers = afterCollideIncludeLayers;
            _currentRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            _interactable = false;

            if (_projectileMold.IsSharp)
            {
                _projectileCollider.enabled = false;
                _currentRigidbody.isKinematic = true;
            }
            else
            {
                _projectileCollider.isTrigger = false;
                _currentRigidbody.useGravity = true;
            }

            projectileRamProvider.CurrentColliderHandler.OnEnter.RemoveListener((collision) =>
            {
                OnProjectileCollision(collision.collider);
            });

            UtilitiesProvider.WaitAndRun(ReturnToPool, false, _projectileMold.lifetimeAfterHit);
        }

        private void DamageObstacle(Collider colliderToDamage)
        {
            var damageable = colliderToDamage.GetComponent<IDamageable>()
                           ?? colliderToDamage.GetComponentInParent<IDamageable>();

            if (damageable != null)
                damageable.ChangeHealthBy(-AttackDamage);

        }

        public void Launch(Vector3 direction)
        {
            _direction = direction;

            transform.rotation = Quaternion.LookRotation(_direction.normalized);

            _currentRigidbody.AddForce(_direction.normalized * _projectileMold.defaultSpeed, ForceMode.Impulse);
        }

        private void IgnoreCollisionWithOwner(Collider collider)
        {
            if (_ignoredColliders == null)
                return;

            foreach (Collider ignoredCollider in _ignoredColliders)
            {
                Physics.IgnoreCollision(_projectileCollider, ignoredCollider);
            }
        }

        public override void ReturnToPool()
        {
            if (!_isInitialized) return;

            if (_currentRigidbody != null) _currentRigidbody.isKinematic = true;

            projectileTrigger.OnEnter.RemoveListener(OnProjectileCollision);
            projectileRamProvider.CurrentColliderHandler.OnEnter.RemoveListener(OnProjectileCollision);

            _projectileParticles = null;

            _isInitialized = false;

            _projectileModel.ReturnToPool();

            base.ReturnToPool();
        }
    }
}