using Actors.Molds;
using Components.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CarEquipmentManager : IDisposable
{
    [SerializeField] private MeleeEquipment defaultFrontBumper;
    [SerializeField] private MeleeEquipment defaultBackBumper;

    public Action<Collider, float> OnEquipmentTriggered;

    public Action<Collider, float> OnEquipmnetDamaging;

    private CarDriving _carDriving;
    private Transform _positionOfCar;
    private Rigidbody _ramRigidbody;

    private CarMold _carMold;
    private EquipmentMold[] _frontBumperEquipment;
    private EquipmentMold[] _backBumperEquipment;

    private List<EquipmentActor> _currentEquipment = new();

    public void Initialize(CarDriving carDriving, Transform positionOfCar, Rigidbody ramRigidbody, CarMold carMold)
    {
        _carDriving = carDriving;
        _positionOfCar = positionOfCar;
        _ramRigidbody = ramRigidbody;

        _carMold = carMold;
        _frontBumperEquipment = carMold.Equipment.FrontBumperEquipment;
        _backBumperEquipment = carMold.Equipment.BackBumperEquipment;

        Equip(defaultFrontBumper);
        Equip(defaultBackBumper);
    }

    public void Equip(EquipmentActor equipment)
    {
        if (equipment == null)
        {
            Debug.LogError($"CarActor called '{_carDriving.gameObject.name}' couldn't find default equipment for front and back bumpers");
            return;
        }

        if (equipment is MeleeEquipment meleeEquipment)
            meleeEquipment.Initialize(null, _carDriving, _ramRigidbody, _positionOfCar, OnEquipmentTriggered, OnEquipmnetDamaging);

        else if (equipment is TurretEquipment turretEquipment)
            turretEquipment.Initialize(_positionOfCar, _ramRigidbody);

        equipment.gameObject.layer = _carDriving.gameObject.layer;

        _currentEquipment.Add(equipment);

        TryActivateDefaultBumper();
    }
    public void Unequip(EquipmentActor equipment)
    {
        _currentEquipment.Remove(equipment);

        TryActivateDefaultBumper();
    }

    private void TryActivateDefaultBumper()
    {
        if (_frontBumperEquipment != null)
            defaultFrontBumper.gameObject.SetActive(!_frontBumperEquipment.Any(e => e != null));

        if (_backBumperEquipment != null)
            defaultBackBumper.gameObject.SetActive(!_backBumperEquipment.Any(e => e != null));
    }

    public void Dispose()
    {
        foreach (var item in _currentEquipment)
            item.ReturnToPool();
    }
}
