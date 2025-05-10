using Components;
using Components.Particles;
using Components.ProjectileSystem;
using Core.ObjectPool;
using MelenitasDev.SoundsGood;
using UnityEngine;
using UnityEngine.Serialization;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "WeaponMold", menuName = "Actors/Molds/WeaponMold")]
    public class ScriptableWeaponMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }

        public SFX SoundOnShoot;

        public PrefabPoolInfo attackPatternPool_PrefabPoolInfo;

        public bool ShootIsLooping;

        public bool ShootOnStart;
        
        
    }
}