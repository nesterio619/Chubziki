using Components.ProjectileSystem;
using Core.Interfaces;
using UnityEngine;

namespace Components.RamProvider
{
    public class ProjectileRamProvider : RamProvider<CollisionHandler>
    {
        public int ownWeight;

        private Rigidbody _ramRigidbody;

        private Projectile _projectile;

        private int _obstacleMass;

        private int _obstacleSpeed;

        private void Start()
        {
            if (transform.parent == null) return;

            _projectile = GetComponent<Projectile>();
            _ramRigidbody = GetComponent<Rigidbody>();
        }

        public void PushObstacle(Collider colliderToPush)
        {
            var pushable = colliderToPush.GetComponent<IPushable>()
                           ?? colliderToPush.GetComponentInParent<IPushable>();

            if (pushable != null)
            {
                IPushable.PerformPush(pushable, RamDirection(colliderToPush.transform.position), GetPushForce());
            }
        }

        private protected override Vector3 RamDirection(Vector3 otherPosition)
        {
            float yOffset = 0;

            otherPosition.y = transform.position.y + yOffset;

            return (otherPosition + transform.position).normalized;
        }

        public override int GetDamageAmount
        {
            get
            {
               return _projectile.AttackDamage;
            }
        }
        private int GetSpeed() => Mathf.RoundToInt(_ramRigidbody.velocity.magnitude);

        private float GetPushForce()
        {
            var originalPushForce = RamForceFormula(ownWeight, _obstacleMass, GetSpeed(), _obstacleSpeed);
            return (DefaultPushForce * originalPushForce);
        }
    }
}