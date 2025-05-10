using Core.ObjectPool;
using UnityEngine;

namespace Actors.Molds
{
    public abstract class Mold : ScriptableObject
    {
        public abstract PrefabPoolInfo PrefabPoolInfoGetter { get; }
    }
   
}
