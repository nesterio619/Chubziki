using Core.ObjectPool;
using UnityEngine;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "ArmorMold", menuName = "Actors/Molds/ArmorMold")]
    public class ArmorMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }

        public int HealthArmorModifier;

        public float MassArmorModifier; 

        public float SpeedArmorModifier;
    }

}