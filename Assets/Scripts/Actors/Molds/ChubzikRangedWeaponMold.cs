using UnityEngine;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "ChubzikRangedWeaponMold", menuName = "Actors/Molds/ChubzikRangedWeaponMold")]
    public class ChubzikRangedWeaponMold : ChubzikWeaponMold
    {
        [Range(0,1)]
        public float StartAimingPercentFromMaxAimingDistance;

        [Range(0, 1)]
        public float TooCloseistancePercent;
    }

}