using Core.Interfaces;
using Core.Utilities;
using System;
using UnityEngine;
using MelenitasDev.SoundsGood;


namespace Components.RamProvider
{
    public class RigidbodyRamProvider : RamProvider<TriggerHandler>
    {
        [Header("Object variables")]
        [SerializeField] protected Transform positionOfObject;
        [SerializeField] protected Rigidbody ramRigidbody;

        [Space(10f)]
        [Header("RamProvider settings")]
        [SerializeField] private float selfSlowdownModifier = 0.05f;

        #region PrivateInvisibleVariables
        protected int pushedObstacleMass;
        protected float pushedObstacleSpeed;
        #endregion

        public Action<Collider, float> OnPushObstacle;
        public Action<Collider, float> OnDamagingObject;

        protected Sound impactSound;
        

        public override int GetDamageAmount
        {
            get
            {
                float force = RamForceFormula(GetOwnWeight(), pushedObstacleMass, GetSpeed(), pushedObstacleSpeed);
                return MultiplyDamageByForce(AttackDamage,force);
            }
        }

        protected virtual float GetPushForce
        {
            get
            {
                var originalPushForce = RamForceFormula(GetOwnWeight(), pushedObstacleMass, GetSpeed(), pushedObstacleSpeed);

                return (DefaultPushForce * originalPushForce);
            }
        }

        protected float GetSpeed() => ramRigidbody.velocity.magnitude;

        protected virtual void Start()
        {
            impactSound = new Sound(SFX.carImpact).SetFollowTarget(transform);

            impactSound.SetFollowTarget(transform).SetRandomClip(true);

            UtilitiesProvider.ForceAddListener(ref CurrentColliderHandler.OnEnter, PushObstacle);
        }

        public void StartAttacking(IDamageable target) => target.ChangeHealthBy(-GetDamageAmount);

        public void PushObstacle(Collider colliderToPush)
        {
            var pushable = FindIPushable(colliderToPush.gameObject);

            if (pushable == null || pushable is not IPushable)
                return;

            var obstacleRigidbody = pushable.GetRigidbody();
            pushedObstacleMass = Mathf.RoundToInt(obstacleRigidbody.mass);
            pushedObstacleSpeed = pushable.GetVelocity().magnitude;

            ApplySelfSlowdown();

            if (!ShouldRam(GetOwnWeight(), pushedObstacleMass, GetSpeed(), pushedObstacleSpeed))
                return;

            if (pushable is IDamageable damageable)
            {
                StartAttacking(damageable);
                OnDamagingObject?.Invoke(colliderToPush, GetPowerOfHit(GetOwnWeight(), pushedObstacleMass, GetSpeed(), pushedObstacleSpeed));
            }

            IPushable.PerformPush(pushable, RamDirection(colliderToPush.transform.position), GetPushForce);

            PlayImpactSound();

            OnPushObstacle?.Invoke(colliderToPush, GetPowerOfHit(GetOwnWeight(), pushedObstacleMass, GetSpeed(), pushedObstacleSpeed));
        }


        private void PlayImpactSound()
        {
            float volume = GetPushForce / GetPowerOfHit(GetOwnWeight(), pushedObstacleMass, GetSpeed(), pushedObstacleSpeed);

            impactSound.SetVolume(volume);

            impactSound.Play();
        }


        private IPushable FindIPushable(GameObject gameObjectData)
        {
            var actor = gameObjectData.GetComponent<IPushable>();
            var parent = gameObjectData.transform.parent;

            if (actor == null && parent!=null)
                actor = gameObjectData.transform.parent.GetComponent<IPushable>();

            return actor;
        }

        private protected override Vector3 RamDirection(Vector3 otherPosition)
        {
            float yOffset = 0;
            otherPosition.y = transform.position.y + yOffset;

            return (otherPosition - positionOfObject.position).normalized;
        }

        private void ApplySelfSlowdown()
        {
            float selfSlowdownAmount = pushedObstacleMass * selfSlowdownModifier;

            DecreaseRamRigidbodyVelocity(selfSlowdownAmount);
        }

        private void DecreaseRamRigidbodyVelocity(float amount)
        {
            if (ramRigidbody == null) return;

            var currentVelocity = ramRigidbody.velocity;
            var velocityDirection = currentVelocity.normalized;

            var reduction = velocityDirection * amount;

            // Apply the reduction to the current velocity
            var newVelocity = currentVelocity - reduction;

            // Clamp the new velocity to ensure we don't reverse direction
            if (Vector3.Dot(newVelocity, currentVelocity) < 0)
                newVelocity = Vector3.zero;

            newVelocity = Vector3.Lerp(currentVelocity, newVelocity, 0.5f);

            ramRigidbody.velocity = newVelocity;
        }

        protected float GetOwnWeight() => ramRigidbody ? ramRigidbody.mass : 0;
    }
}