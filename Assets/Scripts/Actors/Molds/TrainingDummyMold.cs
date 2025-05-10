using Core.ObjectPool;
using UnityEngine;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "TrainingDummyMold", menuName = "Actors/Molds/TrainingDummyMold")]
    public class TrainingDummyMold : AIActorMold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabTrainingDummyPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabTrainingDummyPoolInfo { get; set; }

        public override int MaxHealth => TrainingDummyHealth;

        public override float Mass => TrainingDummyMass;

        public int TrainingDummyHealth;

        public float TrainingDummyMass;

        public float DefaultStunTime;
    }
}


