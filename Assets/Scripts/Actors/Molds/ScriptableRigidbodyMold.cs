using Core.ObjectPool;
using UnityEngine;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "RigidbodyMold", menuName = "Actors/Molds/RigidbodyMold")]
    public class ScriptableRigidbodyMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }

        [field: SerializeField] public int Weight { get; protected set; }
        [field: SerializeField] public bool KinematicUntilFirstTouch { get; protected set; }
    }
}

