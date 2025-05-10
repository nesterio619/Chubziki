using Components;
using UnityEngine;
using Components.Animation;
using Actors.Molds;
using System.Collections;
using Core.ObjectPool;
using Core.Interfaces;
using Regions;
using RSM;
using UnityEngine.AI;
using Components.ProjectileSystem.AttackPattern;
using System.Collections.Generic;
using Components.Particles;

namespace Actors.AI.Chubziks.Base
{
    /// <summary>
    /// Base class for all Chubziks in game 
    /// </summary>

    public abstract class ChubzikActor : AIActor, IMoving
    {
        private const float Delay_Of_Spawning_Corpse = 5;

        [SerializeField] private protected ActorAnimatorController.DefaultAnimations defaultAnimation;
        [SerializeField] private protected ActorAnimatorController actorAnimationController;
        [SerializeField] private protected Collider hitBox;
        [SerializeField] private protected RagdollComponent ragdollComponent;

        [SerializeField] private PrefabPoolInfo corpsePool;
        [SerializeField] protected SkinnedMeshRenderer skinnedMeshRenderer;

        protected Transform currentTarget;

        private Vector3 _lastModelPosition = Vector3.zero;


        #region IMovingVariables
        private bool _isVisible = false;
        public bool IsVisible { get => _isVisible; set { _isVisible = value; } }
        public bool IsOutOfSector { get => !_actorSector.IsInsideBounds(transform.position); }
        public bool IsSectorLoaded { get => _actorSector.IsLoaded; }

        private Transform _locationParent;
        private Sector _actorSector;
        #endregion

        private float _minimalStunTime = 1f;

        protected ChubzikModel _chubzikModel;
        protected WeaponSettings _weaponSettings;

        protected bool _isLogicActive = false;

        protected bool _isAttackingAnimation = false;

        protected float _defaultSpeed;

        protected float outsideSectorTimeLimit = 5;
        protected bool returnToSector;
        protected Coroutine outsideSectorCoroutine;
        protected bool isStoppedReturningToOwnLocation;

        [SerializeField] protected StateMachine chubzikStateMachine;
        [SerializeField] protected NavMeshAgent navMeshAgent;

        [SerializeField] protected PrefabPoolInfo particleStunPool;
        protected PooledParticle currentStunParticle;
        protected Vector3 stunParticlesPoisitionOffset = Vector3.up;

        [SerializeField] protected Vector2 randomSizeBounds = new Vector2(0.95f, 1.05f);

        public virtual void LoadActor(ChubzikMold actorMold, ChubzikModel chubzikModel, AttackPattern attackPattern)
        {
            List<Renderer> renderers = new();

            _chubzikModel = chubzikModel;

            _chubzikModel.transform.localScale = Vector3.one * Random.Range(randomSizeBounds.x, randomSizeBounds.y);

            if (actorMold.WeaponPrefabPool.PrefabPoolInfoGetter != null)
            {
                _weaponSettings = ChubzikConstructor.CreateWeaponModel(actorMold.WeaponPrefabPool.PrefabPoolInfoGetter, _chubzikModel.LeftHand, _chubzikModel.RightHand);

                foreach (var item in _weaponSettings.Renderers)
                {
                    renderers.Add(item);
                }
            }

            foreach (var item in _chubzikModel.Renderers)
            {
                renderers.Add(item);
            }

            Renderers = renderers.ToArray();

            var actorAnimator = _chubzikModel.Animator as DefaultAnimatorController;

            if (actorAnimator != null)
            {
                actorAnimator.transform.SetParent(transform);
                actorAnimator.AttackAnimationTrigger += ChubzikPerformingAttack;

                actorAnimationController = actorAnimator;
                actorAnimationController.AddEventOnEndAnimation(ActorAnimatorController.DefaultAnimations.Attack, AnimationFalse);
            }


            actorRigidbody = _chubzikModel.ModelRigidbody;
            skinnedMeshRenderer = _chubzikModel.BodySkinnedMeshRenderer;
            ragdollComponent = _chubzikModel.ModelRagollComponent;
            
            LoadActor(actorMold);

            actorRigidbody.mass += actorMold.WeaponPrefabPool.MassWeaponModifier;


            if (_chubzikModel.ArmorEquipment != null)
            {
                _maxHeath += actorMold.ArmorMold.HealthArmorModifier;
                actorRigidbody.mass += actorMold.ArmorMold.MassArmorModifier;
                navMeshAgent.speed = actorMold.ChubzikAIMold.SpeedDefault + actorMold.ArmorMold.SpeedArmorModifier;
            }



            _currentHealth = _maxHeath;
            _isStanding = true;
            hitBox.enabled = true;
            navMeshAgent.enabled = true;
        }

        public override void LoadActor(Mold actorMold)
        {
            base.LoadActor(actorMold);

            var chubzikMold = (ChubzikMold)actorMold;
            _stunTime = chubzikMold.ChubzikAIMold.StunTimeDefault;
            if (assignedLocation != null)
                assignedLocation.AttackLocation += ActingOnGetAttack;

        }

        #region IFalling implementation

        [ContextMenu("Kill chubzik in PlayMode")]
        public void KYSChubzik()
        {
            ChangeHealthBy(-20);
        }

        public override void ChangeHealthBy(int changeAmount)
        {
            if (!IsStanding || changeAmount == 0) return;

            _currentHealth = Mathf.Max(0, _currentHealth + changeAmount);

            if (changeAmount < 0)
                CheckingLocationOfPlayerAndChubzik();

            if (_currentHealth <= 0)
            {
                Fall(Vector3.zero);
                _currentHealth = 0;
                SpawnCorpse();
            }

            _lastDamage = changeAmount;
        }


        public override void GetUp()
        {
            hitBox.enabled = true;

            transform.position = actorRigidbody.position;
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100, groundLayer))
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);

            _isStanding = true;

            ragdollComponent.ResetRagdollToInitialState();
        }

        public override void Fall(Vector3 pushForce)
        {
            if (!_isStanding) return;
            hitBox.enabled = false;
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

        #endregion

        #region ChubzikGetAttacked

        public virtual void ActingOnGetAttack()
        {
            StopReturnToOwnLocation();
        }

        protected virtual void StopReturnToOwnLocation()
        {
        }

        protected void CheckingLocationOfPlayerAndChubzik()
        {
            //If a Chubzik is outside its designated location and gets attacked by the player while being outside,
            //it will start attacking the player instead of fleeing in own location 
            if (!IsInsideOwnLocation && !assignedLocation.Bounds.Contains(currentTarget.position))
            {
                StopReturnToOwnLocation();
            }

            //If a Chubzik is inside its designated location and gets attacked by the player while being outside,
            //all chubziks starts attacking the player instead of standing in own location 
            if (IsInsideOwnLocation && !assignedLocation.Bounds.Contains(currentTarget.position))
            {
                assignedLocation.AttackLocation?.Invoke();
            }
        }

        #endregion

        #region IMovingVariablesImplementation
        public Bounds GetBounds()
        {
            return hitBox != null ? hitBox.bounds : new Bounds();
        }

        public Transform GetLocationParent()
        {
            return _locationParent;
        }

        public Transform GetSelfTransform()
        {
            return transform;
        }


        public Actor GetActor()
        {
            return this;
        }

        public void UnloadIfOutOfBounds() => ReturnToPool();

        public void SetSector(Sector sector) => _actorSector = sector;

        #endregion

        public virtual Transform GetAttackPoint()
        {
            return transform;
        }

        public virtual LayerMask GetAttackLayer()
        {
            return gameObject.layer;
        }

        protected void SpawnCorpse()
        {
            if (corpsePool != null)
                StartCoroutine(SpawnCorpseWithDelay());
        }

        protected IEnumerator SpawnCorpseWithDelay()
        {
            float timeOfCheckMoving = 0;

            while (timeOfCheckMoving < Delay_Of_Spawning_Corpse)
            {
                if (_lastModelPosition == skinnedMeshRenderer.transform.position)
                {
                    timeOfCheckMoving += Time.deltaTime;
                }
                else
                {
                    _lastModelPosition = skinnedMeshRenderer.transform.position;
                    timeOfCheckMoving = 0;
                }

                yield return null;
            }


            var corpse = ObjectPooler.TakePooledGameObject(corpsePool).GetComponent<Corpse>();

            corpse.transform.position = transform.position;
            corpse.SetBonesPositionAndRotation(skinnedMeshRenderer.bones);
            corpse.SetSector(assignedLocation._mySector);
            ReturnToPool();
        }

        public override void ReturnToPool()
        {
            if (ragdollComponent != null)
                ragdollComponent.ResetRagdollToInitialState();

            SwitchGraphic(false);

            if (outsideSectorCoroutine != null)
                StopCoroutine(outsideSectorCoroutine);
            outsideSectorCoroutine = null;

            IsVisible = false;
            _isLogicActive = false;
            returnToSector = false;

            chubzikStateMachine.CheckAnyTransitions();

            navMeshAgent.enabled = false;
            
            if (_chubzikModel != null)
            {
                if (_weaponSettings != null)
                    _weaponSettings.ReturnToPool();

                ragdollComponent = null;
                actorRigidbody = null;
                actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Idle);
                actorAnimationController.RemoveEventToAnimation(ActorAnimatorController.DefaultAnimations.Attack, AnimationFalse);
                (actorAnimationController as DefaultAnimatorController).AttackAnimationTrigger -= ChubzikPerformingAttack;
                _chubzikModel.ReturnToPool();

            }

            ReturnToPoolAttackPattern();

            VisibleActorsManager.RemoveActingObject(this);

            if (assignedLocation != null)
                assignedLocation.AttackLocation -= ActingOnGetAttack;

            base.ReturnToPool();

            if (_chubzikModel != null)
            {
                _chubzikModel = null;
                Renderers = null;
            }
        }

        protected virtual void AnimationFalse()
        {
            _isAttackingAnimation = false;
        }

        protected abstract void ReturnToPoolAttackPattern();

        protected abstract void ChubzikPerformingAttack();


        protected void PlayDefaultAnimation() => actorAnimationController.SetAnimationByID(defaultAnimation);
        protected void PlayAttack() => actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Attack);
        protected void PlayDisturb() => actorAnimationController.SetAnimationByID(ActorAnimatorController.DefaultAnimations.Disturb);
    }
}