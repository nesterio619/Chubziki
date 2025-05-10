using Core.ObjectPool;
using UnityEngine;

namespace Actors.Molds
{
    [CreateAssetMenu(fileName = "ChubzikSniperMold", menuName = "Actors/Molds/ChubzikSniperMold")]
    public class ChubzikSniperMold : ChubzikMold
    {
        [Range(0,1)][Tooltip("This is a percentage of the shooting range of the RangedAttackPattern. The Chubzik moves to this distance for aiming. This is needed so that the Chubzik does not stop shooting immediately after leaving the aiming zone.")]
        public float StartAimingPercentFromMaxAimingDistance;

        [Range(0,1)]
        public float TooCloseistancePercent;
    }
}