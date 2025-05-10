using Actors.Molds;
using Core.Interfaces;
using Regions;
using UnityEngine;
namespace Actors
{
    [RequireComponent(typeof(CollisionHandler))]
    public class RigidbodyActor : Actor, IPushable, IMoving
    {

        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }

        private bool _isVisible = false;
        public bool IsVisible { get => _isVisible; set { _isVisible = value; } }
        public bool IsOutOfSector => _actorSector != null && gameObject != null && !_actorSector.IsInsideBounds(transform.position);
        public bool IsSectorLoaded => _actorSector != null && gameObject != null && _actorSector.IsLoaded;

        private Sector _actorSector;
        private Transform _locationParent;

        [SerializeField] private Collider actorCollider;

        public override void LoadActor(Mold actorMold)
        {
            base.LoadActor(actorMold);

            _locationParent = transform.parent;
        }

        public void ApplySinglePushForce(Vector3 pushForce, ForceMode forceMode) => Rigidbody.AddForce(pushForce, forceMode);
        
        public Rigidbody GetRigidbody()
        {
            return Rigidbody;
        }

        public Vector3 GetVelocity() => Rigidbody.velocity;

        public void ToggleKinematic(bool stateToSet)
        {
            Rigidbody.isKinematic = stateToSet;
        }

        public override void ReturnToPool()
        {
            base.ReturnToPool();

            VisibleActorsManager.RemoveActingObject(this);

        }

        #region IMovingVariablesImplementation
        public Actor GetActor()
        {
            return this;
        }

        public Bounds GetBounds()
        {
            return actorCollider != null ? actorCollider.bounds : new Bounds();
        }

        public Transform GetLocationParent()
        {
            return _locationParent;
        }

        public Transform GetSelfTransform()
        {
            return transform;
        }

        public void UnloadIfOutOfBounds() => ReturnToPool();
        public void SetSector(Sector sector) => _actorSector = sector;

        #endregion


    }
}

