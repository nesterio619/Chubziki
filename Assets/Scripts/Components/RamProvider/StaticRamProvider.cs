using Core.Interfaces;
using Core.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Components.RamProvider
{
    public class StaticRamProvider : RamProvider<TriggerHandler>
    {
        [field:Space()]
        [field: SerializeField][Range(0,200)] private float minPushForce = 10.0f;

        [field: SerializeField][Range(0,200)] private float maxPushForce = 50.0f;

        private List<TrappedRigidbodyGameObject> pushableObjects = new();

        public override int GetDamageAmount
        {
            get
            {
                return AttackDamage;
            }
        }


        private void Start()
        {
            UtilitiesProvider.ForceAddListener(ref CurrentColliderHandler.OnEnter, ActivateTrap);

            UtilitiesProvider.ForceAddListener(ref CurrentColliderHandler.OnExit, RemoveColliderAfterExitingTrigger);
        }

        private void ActivateTrap(Collider colliderToAttack)
        {
            var pushable = FindIPushable(colliderToAttack.gameObject);

            if (pushable == null)
                return;

            var transfer = TryFindTrappedRigidbody(pushable.GetRigidbody());

            if (transfer != null)
            {
                transfer.ChildColliders.Add(colliderToAttack);
                return;
            }
            else
                pushableObjects.Add(new TrappedRigidbodyGameObject(pushable.GetRigidbody(), colliderToAttack));

            float targetSpeed = MathUtils.GetSpeedInKilometersPerHour(pushable.GetRigidbody());

            if (pushable is IDamageable damageable)
                StartAttacking(damageable, MultiplyDamageBySpeed(AttackDamage, targetSpeed));

            IPushable.PerformPush(pushable, RamDirection(colliderToAttack.transform.position), GetPushForce(targetSpeed), ForceMode.VelocityChange);
        }

        private IPushable FindIPushable(GameObject gameObjectData)
        {
            var actor = gameObjectData.GetComponent<IPushable>();

            if (actor == null)
                actor = gameObjectData.transform.parent.GetComponent<IPushable>();

            return actor;
        }

        private void RemoveColliderAfterExitingTrigger(Collider colliderToAttack)
        {

            var rigidbodyToAttack = TryFindTrappedRigidbody(colliderToAttack.transform.GetComponentInParent<Rigidbody>());

            if (rigidbodyToAttack == null)
                return;

            if (rigidbodyToAttack.RemoveAndCheckOnLast(colliderToAttack))
            {
                pushableObjects.Remove(rigidbodyToAttack);
            }


        }

        public TrappedRigidbodyGameObject TryFindTrappedRigidbody(Rigidbody checkObject)
        {
            foreach (var trappedObject in pushableObjects)
            {
                if (trappedObject.TrappedRigidbody == checkObject)
                {
                    return trappedObject;
                }
            }

            return null;
        }

        private float GetPushForce(float speed)
        {
            float currentPushForce = DefaultPushForce * speed;

            if (currentPushForce > maxPushForce)
            {
                currentPushForce = maxPushForce;
            }

            if (currentPushForce < minPushForce)
            {
                currentPushForce = minPushForce;
            }

            return currentPushForce;
        }

        private override protected Vector3 RamDirection(Vector3 pushedObjectPosition)
        {
            var pushDirection = base.RamDirection(pushedObjectPosition);
            pushDirection.y = 0;

            return pushDirection.normalized;
        }

        private void StartAttacking(IDamageable target, int damage) => target.ChangeHealthBy(-damage);
    }
}