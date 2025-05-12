using Core.Utilities;
using UnityEngine;
using MelenitasDev.SoundsGood;

namespace Components.RamProvider
{
    public class EquipmentRamProvider : RigidbodyRamProvider
    {
        protected override void Start()
        {
            impactSound = new Sound(SFX.carImpact).SetFollowTarget(transform);

            impactSound.SetFollowTarget(transform).SetRandomClip(true);
        }

        public void Initialize(float pushForce, int damage, Rigidbody rigidbody, Transform carPosition)
        {
            DefaultPushForce = pushForce;
            DefaultDamage = damage;
            ramRigidbody = rigidbody;
            positionOfObject = carPosition;

            UtilitiesProvider.ForceAddListener(ref CurrentColliderHandler.OnEnter, PushObstacle);
        }

        private protected override Vector3 RamDirection(Vector3 otherPosition)
        {
            Vector3 baseDirection = base.RamDirection(otherPosition);

            Quaternion tiltRotation = Quaternion.AngleAxis(-GetRandomExtraPushAngle(), transform.right);
            Vector3 tiltedDirection = tiltRotation * baseDirection;

            return tiltedDirection.normalized;
        }
    }
}