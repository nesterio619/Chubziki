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
    }
}