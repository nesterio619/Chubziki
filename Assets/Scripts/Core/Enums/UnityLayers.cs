
using System;

namespace Core.Enums
{
    [Flags]
    public enum UnityLayers
    {
        Default = 1 << 1,
        TransparentFX = 1 << 2,
        IgnoreRaycast = 1 << 3,
        Water = 1 << 4,
        UI = 1 << 5,
        Ragdoll = 1 << 6,
        Environment = 1 << 7,
        Wheel = 1 << 8,
        Traps = 1 << 9,
        PlayerTrigger = 1 << 10,
        AttackPattern = 1 << 11,
        Bounds = 1 << 13,
        ActorPlayer = 1 << 14,
        ActorEnemy = 1 << 15,
        ActorNeutral = 1 << 16,
        StartTrigger = 1 << 17,
        FriendlyProjectile = 1 << 18,
        EnemyProjectile = 1 << 19,
    }

}
