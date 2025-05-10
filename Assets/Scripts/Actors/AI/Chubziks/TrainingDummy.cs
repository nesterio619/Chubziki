using Actors.Molds;
using Components;
using Core.Interfaces;
using Core.Utilities;
using MelenitasDev.SoundsGood;
using Regions;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Actors.AI.Chubziks
{
	public class TrainingDummy : AIActor, IMoving
	{
		[SerializeField] private Vector3 numbersOffset;
		[SerializeField] private DamageNumbersPro.DamageNumber damageNumbers;
		[SerializeField] protected RagdollComponent ragdollComponent;
		[SerializeField] protected BoxCollider boxCollider;

		private Sound _soundDie;
		
		private float _currentTime;
		private float _minimalStunTime = 1f;

		private bool _isVisible = false;
		public bool IsVisible { get => _isVisible; set { _isVisible = value; } }
		public bool IsOutOfSector { get => !_actorSector.IsInsideBounds(transform.position);}
		public bool IsSectorLoaded { get => _actorSector.IsLoaded; }

		private Sector _actorSector;
		private Transform _locationParent;


		public override void LoadActor(Mold actorMold)
		{
            base.LoadActor(actorMold);

			_locationParent = transform.parent;
			var surviveMold = (TrainingDummyMold)actorMold;
			_stunTime = surviveMold.DefaultStunTime;
			
			ResetHealth();
		}

		private void ResetDummyState()
		{
			GetUp();
			ResetHealth();
		}

		public override void ReturnToPool()
		{
			ragdollComponent.ResetRagdollToInitialState();
			SwitchGraphic(false);
			VisibleActorsManager.RemoveActingObject(this);
			base.ReturnToPool();
		}



		#region IFalling implementation

		public override void ChangeHealthBy(int changeAmount)
		{
			if (!IsStanding || changeAmount == 0) return;

			_currentHealth = Mathf.Max(0, _currentHealth + changeAmount);
			if (_currentHealth <= 0) _currentHealth = 0;

			DamageNumbersPro.DamageNumber pooldeNumber = damageNumbers.Spawn();

			Vector3 textPos = transform.position;
			textPos += numbersOffset;
			pooldeNumber.transform.position = textPos;
			pooldeNumber.number = changeAmount;
			_lastDamage = changeAmount;
			if (_currentHealth <= 0)
				Die();
		}

        public override void Fall(Vector3 pushForce)
        {
            if (!_isStanding) return;
			boxCollider.enabled = false;
			_isStanding = false;
			ragdollComponent.SwitchRagdoll(true);
			actorRigidbody.AddForce(pushForce, IPushable.CurrentForceMode);

			if (_currentHealth > 0)
			{
				float timeStun = _minimalStunTime + _stunTime / (_maxHeath / Mathf.Abs(_lastDamage));
				StartCoroutine(GetUpTimer(timeStun));
			}
			_lastDamage = 0;
		}

        public void Die()
		{
			Fall(Vector3.zero);
			UtilitiesProvider.WaitAndRun(ResetDummyState, false, _stunTime);
			_soundDie = new Sound(SFX.TrainingDummyDie);
			_soundDie.SetFollowTarget(transform);
			_soundDie.Play();
		}

        public override void GetUp()
		{
			boxCollider.enabled = true;
			base.GetUp();
			ragdollComponent.ResetRagdollToInitialState();
		}
		#endregion

		private void OnDrawGizmos()
		{
			Debug.DrawRay(transform.position, Vector3.down,Color.green);
		}

		#region IMovingVariablesImplementation
		public Actor GetActor()
		{
			return this;
		}

		public Bounds GetBounds()
		{
			return boxCollider.bounds;
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

