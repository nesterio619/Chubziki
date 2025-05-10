using Core.ObjectPool;
using UnityEngine;

namespace Components.ProjectileSystem.AttackPattern
{
    public abstract class AttackPattern : PooledGameObject
    {
        [SerializeField] protected bool ShowVisualPattern;

        public abstract bool CanAttack { get; }

        public virtual void ShowPattern(bool state)
        {
            ShowVisualPattern = state;
        }

        public abstract void PerformAttack();

        public abstract void Initialize(Transform parent, Transform firePoint, Collider[] ignoredColliders, LayerMask projectileLayerMask);


    }
}