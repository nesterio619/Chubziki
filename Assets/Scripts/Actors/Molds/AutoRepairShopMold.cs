using Core.ObjectPool;
using UnityEngine;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "AutoRepairShopMold", menuName = "Actors/Molds/AutoRepairShopMold")]
    public class AutoRepairShopMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }
    }
}
