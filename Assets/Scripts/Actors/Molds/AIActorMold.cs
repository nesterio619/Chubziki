using Actors.AI;
using Actors.Molds;
using Core.ObjectPool;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretMold", menuName = "Actors/Molds/TurretMold")]
public abstract class AIActorMold : Mold
{
    public abstract int MaxHealth { get; }

    public abstract float Mass { get; }
}
