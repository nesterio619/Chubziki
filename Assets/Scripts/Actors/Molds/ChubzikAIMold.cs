using Actors.Molds;
using Core.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Actors.Molds
{
    
    [CreateAssetMenu(fileName = "ChubzikAIMold", menuName = "Actors/Molds/ChubzikAIMold")]
    public class ChubzikAIMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }

        public int HealthDefault;
        public float MassDefault;
        public float StunTimeDefault;
        public float SpeedDefault;
    }
}