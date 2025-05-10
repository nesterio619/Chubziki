using Actors.Constructors;
using Core.ObjectPool;
using Core.Utilities;
using Regions;
using UnityEngine;
using UnityEngine.Events;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "PressureButtonMold", menuName = "Actors/Molds/PressureButtonMold")]
    public class PressureButtonMold : Mold
    {
        public override PrefabPoolInfo PrefabPoolInfoGetter => prefabPoolInfo;

        [field: SerializeField] private PrefabPoolInfo prefabPoolInfo { get; set; }
    }
}
