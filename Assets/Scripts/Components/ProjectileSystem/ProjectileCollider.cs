using UnityEngine;

namespace Components.ProjectileSystem
{
    public class ProjectileCollider : TriggerHandler
    {
        public Collider CurrentCollider
        {
            get
            {
                return Collider;
            }
        }

        [SerializeField] private Rigidbody currentRigidbody;

        public Rigidbody CurrentRigidbody
        {
            get
            {
                return currentRigidbody;
            }
        }
    }
}