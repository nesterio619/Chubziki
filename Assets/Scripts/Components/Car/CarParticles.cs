using System;
using Actors;
using Actors.AI.Chubziks;
using Actors.AI.Chubziks.Base;
using Components.Particles;
using Core.Interfaces;
using Core.ObjectPool;
using NWH.WheelController3D;
using UnityEngine;
using UnityEngine.Serialization;

namespace Components.Car
{
    [System.Serializable]
    public sealed class CarParticles : IDisposable
    {
        [SerializeField] private PrefabPoolInfo particleCarSmoke_PrefabPoolInfo;
        [SerializeField] private PrefabPoolInfo particleDriftSmoke_PrefabPoolInfo;
        [SerializeField] private DynamicParticlesConfig pushObstacleParticlesConfig;
        [SerializeField] private PrefabPoolInfo hitEnvironmentParticles;


        [SerializeField] private TrailRenderer[] TireSkids;
        [SerializeField] private Transform EnginePosition;

        private PooledParticle[] _driftParticles; // May contain null objects
        private WheelController[] _driftWheelsPositions; // Never contains null object if _isInitialized
        private PooledParticle _engineSmokeParticle; // May be null
        private CarCollisionHandler _carCollisionHandler;

        private float _xVelocityForTrail = 5;
        private float _xSpeedForTrailt = 12;
        private float _sensitivityOfPushPower = 0.2f;

        private bool _isInitialized;

        public void Initialize(WheelController[] driftWheelsPositions, CarCollisionHandler carCollisionHandler, CarActor carActor)
        {
            if (_isInitialized)
            {
                Debug.LogError("Cannot initialize CarParticles twice");
                return;
            }
            _carCollisionHandler = carCollisionHandler;
            carActor.EquipmentManager.OnEquipmentTriggered += CreateImpactParticles;
            _carCollisionHandler.BodyCarRamProvider.OnPushObstacle += CreateImpactParticles;
            _carCollisionHandler.OnHittingEnvironment += CreateParticlesOnHittingEnvironment;

            _driftWheelsPositions = driftWheelsPositions;
            _driftParticles = new PooledParticle[driftWheelsPositions.Length];
            _isInitialized = true;
        }

        public void ToggleDriftSmoke(bool isDrift)
        {
            if (!_isInitialized || _driftParticles == null || _driftParticles.Length == 0) return;
            for (var index = 0; index < _driftParticles.Length; index++)
            {
                var particlesAreLoaded = _driftParticles[index] != null;
                if (!isDrift)
                {
                    if (particlesAreLoaded)
                        _driftParticles[index].StopParticle();
                    _driftParticles[index] = null;
                }
                else if (!particlesAreLoaded)
                    _driftParticles[index] =
                        PooledParticle.TryToLoadAndPlayLoop(particleDriftSmoke_PrefabPoolInfo, _driftWheelsPositions[index].transform);
            }
        }

        public void ToggleDriftTracer(bool isTractionLocked, float xLocalVelocity, float wheelSpeed)
        {
            var valueToSet = (isTractionLocked
                               || Mathf.Abs(xLocalVelocity) > _xVelocityForTrail) && Mathf.Abs(wheelSpeed) > _xSpeedForTrailt;

            for (int i = 0; i < TireSkids.Length; i++)
            {
                TireSkids[i].emitting = valueToSet;
            }
        }

        public void ToggleCarSmoke(bool isSmoking)
        {
            if (!_isInitialized) return;
            var particlesAreLoaded = _engineSmokeParticle != null;
            if (!isSmoking)
            {
                if (particlesAreLoaded)
                    _engineSmokeParticle.StopParticle();
                _engineSmokeParticle = null;
            }
            else if (!particlesAreLoaded)
                _engineSmokeParticle
                    = PooledParticle.TryToLoadAndPlayLoop(particleCarSmoke_PrefabPoolInfo, EnginePosition);
        }

        public void CreateImpactParticles(Collider colliderToPush, float powerOfPushing)
        {
            var particles = DynamicParticles.TryToLoadAndPlay(pushObstacleParticlesConfig);

            powerOfPushing *= _sensitivityOfPushPower;

            if (powerOfPushing > 1)
                powerOfPushing = 1;

            particles.ChangeParticlesColor(powerOfPushing);

            particles.gameObject.transform.position = colliderToPush.ClosestPoint(_carCollisionHandler.transform.position);
            particles.gameObject.transform.position += Vector3.up;

        }

        public void CreateParticlesOnHittingEnvironment(Collision collision)
        {
            var particles = PooledParticle.TryToLoadAndPlay(hitEnvironmentParticles, null);
            particles.gameObject.transform.position = collision.contacts[0].point;
            particles.transform.SetParent(_carCollisionHandler.transform);
        }

        public void Dispose()
        {
            _isInitialized = false;
            _driftWheelsPositions = null;
            _driftParticles = null;
        }
    }
}