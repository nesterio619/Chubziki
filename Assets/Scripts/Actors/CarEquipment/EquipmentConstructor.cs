using Actors.Constructors;
using Actors.Molds;
using Components.RamProvider;
using Core;
using System.Collections;
using System.Collections.Generic;
using UI.Canvas;
using UI.Popup;
using UnityEngine;

public class EquipmentConstructor : ObjectConstructor<EquipmentActor>
{
    private static EquipmentConstructor _instance = new();

    public static EquipmentConstructor Instance
    {
        get
        {
            return _instance;
        }

        private set
        {
            if (_instance == null)
            {
                _instance = value;
            }
        }

    }

    [SerializeField] private MeleeEquipment defaultMeleeEquipment;

    [SerializeField] private List<MeleeEquipment> meleeEquipmentPrefabs = new();

    public override EquipmentActor Load(Mold moldType, Transform transform)
    {
        var pooledObject = TakeFromPool(moldType, transform).GetComponent<EquipmentActor>();

        if (pooledObject == null)
            Debug.Log(pooledObject.gameObject.name);

        var equipmentMold = moldType as EquipmentMold;
        pooledObject.SetMold(equipmentMold);

        return pooledObject;
    }
}
