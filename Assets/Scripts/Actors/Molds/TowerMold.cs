using Core.ObjectPool;
using UnityEngine;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "TowerMold", menuName = "Actors/Molds/TowerMold")]
    public class TowerMold : AIActorMold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabTurretPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabTurretPoolInfo { get; set; }

        public override int MaxHealth => TowerHealth;

        public override float Mass => TowerMass;

        public int TowerHealth;

        public float TowerMass;
    }
}

