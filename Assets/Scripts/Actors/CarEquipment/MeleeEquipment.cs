using Components.Car;
using Components.RamProvider;
using Core.Utilities;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using System;
using UnityEngine;
using UnityEngine.Events;

public class MeleeEquipment : EquipmentActor
{
    [SerializeField] private Collider equpipmentCollider;

    [SerializeField] public EquipmentRamProvider EquipmentRamProvider;

    public TriggerHandler EquipmentTriggerHandler;

    public void Initialize(UnityAction<Collider> unityEvents, CarDriving carDriving, Rigidbody rigidbody, Transform carPosition, Action<Collider, float> OnPushObstacle, Action<Collider, float> OnDamagingObstacle)
    {
        if (unityEvents != null)
            UtilitiesProvider.ForceAddListener(ref EquipmentTriggerHandler.OnEnter, unityEvents);

        ToggleRenderersEnabled(true);

        EquipmentRamProvider.OnPushObstacle += OnPushObstacle;
        EquipmentRamProvider.OnDamagingObject += OnDamagingObstacle;

        EquipmentRamProvider.Initialize(pushForce, damage, rigidbody, carPosition);

        ToggleLogic(true);
    }

    public int GetDamageModifier()
    {
        return damage;
    }

    public float GetPushForceModifier()
    {
        return pushForce;
    }

    public Vector3 GetColliderSize()
    {
        return equpipmentCollider.bounds.size;
    }

    public Vector3 GetColliderCenter()
    {
        return transform.localPosition + equpipmentCollider.bounds.center;
    }
    public override void ReturnToPool()
    {
        base.ReturnToPool();

        EquipmentRamProvider.OnPushObstacle = null;

        EquipmentTriggerHandler.OnEnter.RemoveAllListeners();
    }
    public override void HandleRemovedFromSocket(Socket socket, PlaceableItem placeableItem)
    {
        base.HandleRemovedFromSocket(socket, placeableItem);

        ToggleLogic(false);
    }
}
