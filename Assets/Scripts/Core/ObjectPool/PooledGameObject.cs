using System;
using UnityEngine;

namespace Core.ObjectPool
{
    public class PooledGameObject: MonoBehaviour
    {
        [HideInInspector] public string poolName;

        public bool IsBeingDestroyed {  get; protected set; } 

        public virtual void ReturnToPool() => ObjectPooler.ReturnPooledObject(this);

        protected virtual void OnDestroy()
        {
            IsBeingDestroyed = true;
        }
    }
}