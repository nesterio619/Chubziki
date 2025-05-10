using Core.ObjectPool;
using UnityEngine;

namespace Actors.Molds
{

    [CreateAssetMenu(fileName = "ChubzikWeaponMold", menuName = "Actors/Molds/ChubzikWeaponMold")]
    public class ChubzikWeaponMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }

        public PrefabPoolInfo AttackPattern;

        public RuntimeAnimatorController WeaponAnimatorController;

        public float MassWeaponModifier;
    }
}