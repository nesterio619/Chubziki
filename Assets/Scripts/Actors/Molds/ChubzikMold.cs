using Core.ObjectPool;
using UnityEngine;
using UnityEngine.Serialization;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "ChubzikMold", menuName = "Actors/Molds/ChubzikMold")]
    public class ChubzikMold : AIActorMold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => ChubzikAIMold.PrefabPoolInfoGetter;

        public override int MaxHealth => ChubzikAIMold.HealthDefault;

        public override float Mass => ChubzikAIMold.MassDefault;

        public ChubzikAIMold ChubzikAIMold;

        public ArmorMold ArmorMold;

        public ChubzikWeaponMold WeaponPrefabPool;
        
        
    
    }
}

