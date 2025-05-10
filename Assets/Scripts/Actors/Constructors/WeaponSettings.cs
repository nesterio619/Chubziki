using Core.ObjectPool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSettings : PooledGameObject
{
    public Renderer[] Renderers;

    public Vector3 PositionInHand;

    public Vector3 RotationInHand;

    public Hand WhichHand;
}
