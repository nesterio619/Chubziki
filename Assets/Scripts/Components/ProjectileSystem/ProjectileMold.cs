using Core.ObjectPool;
using MelenitasDev.SoundsGood;
using UnityEngine;
using UnityEngine.Serialization;

namespace Components.ProjectileSystem
{
    [CreateAssetMenu(fileName = "ProjectileMold ", menuName = "Projectile/DefaultProjectileMold")]
    public class ProjectileMold : ScriptableObject
    {
        [Space(10)] public int damage;

        [Space(8)] public float defaultSpeed;

        [Space(20)] public float lifetimeAfterHit;

        [Tooltip("Determines if object gets stuck on impact")]
        public bool IsSharp;

        [Space(20)] public bool pushTarget;
        public int ownWeight;
        [Range(0f, 100f)] public float pushForceTarget;
        
        [Space(10)]
        [Tooltip("Base on which the projectile model is mounted")]
        public PrefabPoolInfo ProjectileBase;
        [Tooltip("Model or form of projectile")]
        public PrefabPoolInfo ModelOfProjectile;

        [Space(10)]
        [Tooltip("Particles that are emitted upon collision with an enemy or obstacles")]
        public PrefabPoolInfo ParticlesOnHitting;
        
        [Tooltip("The time it takes for the particles to fade out completely by scaling")]
        public float DurationScaling;
    }
}