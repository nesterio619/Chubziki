using UnityEngine;
using Core.Enums;

namespace Core.Extensions
{
    public static class GameObjectExtensions
    {
        public static UnityTags GetObjectTag(this GameObject gameObject)
        {
            switch (gameObject.tag)
            {
                case "EditorOnly":
                    return UnityTags.EditorOnly;
                case "MainCamera":
                    return UnityTags.MainCamera;
                case "EditorActor":
                    return UnityTags.MainCamera;
                default:
                    return UnityTags.Untagged;
            }
        }

        public static UnityLayers GetObjectLayer(this GameObject gameObject)
        {
            switch (gameObject.layer)
            {
                case 0:
                    return UnityLayers.Default;
                case 1:
                    return UnityLayers.TransparentFX;
                case 2:
                    return UnityLayers.IgnoreRaycast;
                case 4:
                    return UnityLayers.Water;
                case 5:
                    return UnityLayers.UI;
                case 6:
                    return UnityLayers.Ragdoll;
                case 7:
                    return UnityLayers.Environment;
                case 8:
                    return UnityLayers.Wheel;
                case 9:
                    return UnityLayers.Traps;
                case 10:
                    return UnityLayers.PlayerTrigger;
                case 11:
                    return UnityLayers.AttackPattern;

                case 13:
                    return UnityLayers.Bounds;
                case 14:
                    return UnityLayers.ActorPlayer;
                case 15:
                    return UnityLayers.ActorEnemy;
                case 16:
                    return UnityLayers.ActorNeutral;
                case 17:
                    return UnityLayers.StartTrigger;
                case 18:
                    return UnityLayers.FriendlyProjectile;
                case 19:
                    return UnityLayers.EnemyProjectile;
                default:
                    Debug.LogWarning($"Unknown layer: {gameObject.layer} on GameObject: {gameObject.name}");
                    return (UnityLayers)gameObject.layer;
            }
        }
    }
}
