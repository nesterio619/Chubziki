using Actors;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using Hypertonic.Modules.UltimateSockets.XR;
using System;
using UnityEngine;

public abstract class EquipmentActor : Actor, IGrabbableItem
{   
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;

    [SerializeField, Range(-10, 100)] protected int damage;
    [SerializeField, Range(-10, 100)] protected float pushForce;

    public event IGrabbableItemEvent OnGrabbed;
    public event IGrabbableItemEvent OnReleased;

    [field:Space(10)]
    [field:SerializeField] public EquipmentMold EquipmentMold {  get; protected set; }

    protected CarEquipmentManager _currentEquipmentManager;

    protected bool _isGrabbed;
    protected Collider _equipmentCollider;
    protected Rigidbody _rigidbody;

    private void Awake()
    {
        _equipmentCollider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();

        if(EquipmentMold != null ) SetMold(EquipmentMold);
    }

    public void SetMold(EquipmentMold mold)
    {
        EquipmentMold = mold;

        pushForce = mold.PushForce;
        damage = mold.Damage;

        if (_rigidbody != null)
            _rigidbody.mass = mold.RigidbodyMass;
    }

    public void EnablePhysics(bool enable)
    {
        if(_rigidbody == null) return;

        _rigidbody.isKinematic = !enable;
        _equipmentCollider.isTrigger = !enable;
    }

    public bool IsGrabbing() => _isGrabbed;

    public virtual void Grab()
    {
        OnGrabbed?.Invoke();
        _isGrabbed = true;
        _equipmentCollider.enabled = false;

        EnablePhysics(false);
    }

    public virtual void Release()
    {
        OnReleased?.Invoke();
        _isGrabbed = false;
        _equipmentCollider.enabled = true;

        EnablePhysics(true);
    }
    public virtual void HandlePlacedInSocket(Socket socket, PlaceableItem placeableItem) 
    {
        EnablePhysics(false);

        var carActor = socket.GetComponentInParent<CarActor>();
        if (carActor == null) return;

        if(socket is EquipmentSocket equipmentSocket)
            equipmentSocket.EquipmentMoldsReference[socket.transform.GetSiblingIndex()] = EquipmentMold;

        _currentEquipmentManager = carActor.EquipmentManager;
        _currentEquipmentManager.Equip(this);
    }
    public virtual void HandleRemovedFromSocket(Socket socket, PlaceableItem placeableItem) 
    {
        if (socket is EquipmentSocket equipmentSocket)
            if (equipmentSocket.EquipmentMoldsReference.Length != 0)
                equipmentSocket.EquipmentMoldsReference[socket.transform.GetSiblingIndex()] = null;

        _currentEquipmentManager?.Unequip(this);
    }
}
