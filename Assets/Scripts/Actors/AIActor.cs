using System.Collections;
using Actors.Molds;
using Core.Interfaces;
using Core.Utilities;
using Regions;
using UnityEngine;

namespace Actors
{
    public class AIActor : Actor, IDestructible
    {
        [SerializeField] protected Rigidbody actorRigidbody;
        [SerializeField] protected LayerMask groundLayer;
        [SerializeField] private LayerMask actorLayers;

        protected Location assignedLocation;
        protected bool IsInsideOwnLocation => assignedLocation.Bounds.Contains(transform.position);

        public override void LoadActor(Mold actorMold)
        {
            if (actorMold is not AIActorMold)
            {
                Debug.LogWarning("Wrong mold");
                return;
            }
            var surviveMold = (AIActorMold)actorMold; 

            _maxHeath = surviveMold.MaxHealth;
            _currentHealth = _maxHeath;
            actorRigidbody.mass = surviveMold.Mass;
        }

        public void SetLocation(Location location) => assignedLocation = location;

        public bool IsAlive => _currentHealth > 0;

		#region IDestructible implementation
		public int MaxHealth => _maxHeath;
        public int CurrentHealth => _currentHealth;
		public float DefaultStunTime => _stunTime;
		public bool IsStanding => _isStanding;

		protected int _maxHeath;
        protected int _currentHealth;
        protected float _stunTime;
        protected bool _isStanding = true;
        protected float _lastDamage;


		public Rigidbody GetRigidbody()
		{
			return actorRigidbody;
		}

		public virtual void ChangeHealthBy(int changeAmount)
		{
			_currentHealth += changeAmount;
			_lastDamage = changeAmount;
		}

        public void ResetHealth() => _currentHealth = _maxHeath;

        public virtual void ApplySinglePushForce(Vector3 pushForce, ForceMode forceMode) => Fall(pushForce);
        
        public virtual Vector3 GetVelocity() => actorRigidbody.velocity;

		public virtual void Fall(Vector3 pushForce)
		{
			if (!_isStanding) return;
			_isStanding = false;
			actorRigidbody.AddForce(pushForce, IPushable.CurrentForceMode);

			if (_currentHealth > 0)
			{
				float timeStun = _stunTime / (_maxHeath / Mathf.Abs(_lastDamage));
				UtilitiesProvider.WaitAndRun(GetUp, false, timeStun);
			}
			_lastDamage = 0;
		}

		public virtual void GetUp()
		{
			var actorRigidbodyPosition = actorRigidbody.position;
			if(Physics.Raycast(actorRigidbodyPosition, Vector3.down, out RaycastHit hit, 15, groundLayer))
				transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
			
			_isStanding = true;
		}
		
		private float _timeLeftBeforeGetUp;
		protected IEnumerator GetUpTimer(float time)
		{
			_timeLeftBeforeGetUp = 0;

			while (!_isStanding && LogicIsEnabled)
			{
				if (Physics.Raycast(actorRigidbody.position, Vector3.up, out RaycastHit hit, 10, actorLayers))
					_timeLeftBeforeGetUp = 0;

				_timeLeftBeforeGetUp += Time.deltaTime;

				if (_timeLeftBeforeGetUp >= time)
				{
					GetUp();
					break;
				}

				yield return null;
			}

		}
		#endregion
	}
}




