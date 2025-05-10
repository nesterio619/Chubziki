using Actors;
using Actors.Molds;
using Components.Particles;
using Components.ProjectileSystem.AttackPattern;
using Core.Enums;
using Core.ObjectPool;
using Core.Utilities;
using MelenitasDev.SoundsGood;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Components
{
    public class ProjectileLauncher : Actor
    {
        [field: SerializeField] public Transform FirePoint { get; protected set; }

        [SerializeField] private PrefabPoolInfo attackPoolPattern_PrefabPoolInfo;

        [SerializeField] private bool shootOnStart = true;

        public Action OnShoot;

        private RangedAttackPattern _rangeAttackPattern;

        private PrefabPoolInfo _particlePoolOnShoot_PrefabPoolInfo = null;

        private Collider[] _ignoredColliders;

        public override void LoadActor(Mold actorMold)
        {
            if (actorMold is not ScriptableWeaponMold)
            {
                Debug.LogWarning("Wrong mold can not be initialized");
                return;
            }

            base.LoadActor(actorMold);

            var mold = (ScriptableWeaponMold)actorMold;

            if (mold.SoundOnShoot != SFX.None)
            {
                Sound fireSound = new Sound(mold.SoundOnShoot);
                fireSound.SetFollowTarget(FirePoint);
                OnShoot += () => fireSound.Play();
            }

            LoadActor(mold.ShootOnStart, mold.attackPatternPool_PrefabPoolInfo);

        }

        public void LoadActor(bool isShootOnStart, PrefabPoolInfo attackPatternPool_PrefabPoolInfo)
        {
            shootOnStart = isShootOnStart;
            _ignoredColliders = GetComponentsInChildren<Collider>();
            attackPoolPattern_PrefabPoolInfo = attackPatternPool_PrefabPoolInfo;

            /*if (particleOnShootPool_PrefabPoolInfo != null)
            {
                _particlePoolOnShoot_PrefabPoolInfo = particleOnShootPool_PrefabPoolInfo;
                OnShoot += CreateParticleOnShoot;
            }*/

            ToggleLogic(false);
        }

        public override void ToggleLogic(bool stateToSet)
        {
            if (stateToSet == true)
                LoadAttackPattern();
            else
                UnloadAttackPattern();
        }

        private void LoadAttackPattern()
        {
            if (_rangeAttackPattern != null && _rangeAttackPattern.gameObject.activeInHierarchy)
                return;

            _rangeAttackPattern = ObjectPooler.TakePooledGameObject(attackPoolPattern_PrefabPoolInfo)
                .GetComponent<RangedAttackPattern>();
            _rangeAttackPattern.Initialize(transform, FirePoint, _ignoredColliders, UnityLayers.EnemyProjectile.GetIndex());
            _rangeAttackPattern.OnShoot += OnShootInvoke;

            if (shootOnStart)
            {
                _rangeAttackPattern.SetShootLoop(true);
                _rangeAttackPattern.PerformAttack();
            }
        }

        private void UnloadAttackPattern()
        {
            if (_rangeAttackPattern == null)
                return;

            _rangeAttackPattern.SetShootLoop(false);
            _rangeAttackPattern.OnShoot -= OnShootInvoke;

            _rangeAttackPattern.ReturnToPool();
            _rangeAttackPattern = null;
        }

        #region Shooting

        private void OnShootInvoke()
        {
            OnShoot?.Invoke();
        }

        public void ShootingLoop()
        {

            _rangeAttackPattern.SetShootLoop(true);
            _rangeAttackPattern.PerformAttack();
        }

        public void ShootingLoopStop()
        {
            if (_rangeAttackPattern != null)
                _rangeAttackPattern.SetShootLoop(false);
        }

        public void ShootOnce()
        {
            _rangeAttackPattern.PerformAttack();
        }

        #endregion

    }
}