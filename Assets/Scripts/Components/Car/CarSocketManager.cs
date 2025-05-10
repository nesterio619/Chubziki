using Actors.Molds;
using Hypertonic.Modules.UltimateSockets.PlaceableItems;
using Hypertonic.Modules.UltimateSockets.Sockets;
using ProceduralCarBuilder;
using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class CarSocketManager
{
    [SerializeField] private SocketGenerator _hoodGenerator;
    [SerializeField] private SocketGenerator _roofGenerator;
    [SerializeField] private SocketGenerator _trunkGenerator;
    [SerializeField] private SocketGenerator _leftSideGenerator;
    [SerializeField] private SocketGenerator _rightSideGenerator;
    [SerializeField] private SocketGenerator _backBumperGenerator;
    [SerializeField] private SocketGenerator _frontBumperGenerator;

    [SerializeField] private CarPartReferences partReferences;
    [SerializeField] private CarMold carMold;
    [SerializeField] private EquipmentSocket socketPrefab;

    public void Initialize(CarPartReferences carPartReferences, CarMold carMold, EquipmentSocket socketPrefab)
    {
        partReferences = carPartReferences;
        this.carMold = carMold;
        this.socketPrefab = socketPrefab;

        _hoodGenerator = InitializeGenerator(_hoodGenerator, partReferences.Hood, new Vector3(-90,0));
        _roofGenerator = InitializeGenerator(_roofGenerator, partReferences.PropAnchor_Roof.MiddlePoint, new Vector3(-90, 0));
        _trunkGenerator = InitializeGenerator(_trunkGenerator, partReferences.TrunkBonnet, new Vector3(-90, 180));
        _leftSideGenerator = InitializeGenerator(_leftSideGenerator, partReferences.BodySide, new Vector3(0,-90));
        _rightSideGenerator = InitializeGenerator(_rightSideGenerator, partReferences.BodySide, new Vector3(0, 90));
        _frontBumperGenerator = InitializeGenerator(_frontBumperGenerator, partReferences.LicensePlate, Vector3.zero);
        _backBumperGenerator = InitializeGenerator(_backBumperGenerator, partReferences.PropAnchor_BackTrunk.MiddlePoint, new Vector3(0,180));
    }

    private SocketGenerator InitializeGenerator(SocketGenerator generator, Transform parentTransform, Vector3 defaultRotationEuler)
    {
        if (generator == null || generator.SocketTransform == null)
            generator = new SocketGenerator(parentTransform, socketPrefab, defaultRotationEuler);
        return generator;
    }

    public void GenerateSockets(CarData carData)
    {
        if (carData == null) return;

        var equipment = carMold.Equipment;

        _hoodGenerator.GenerateSockets(CarBoundsGenerator.GetHoodBounds(carData), equipment.HoodEquipment);
        _roofGenerator.GenerateSockets(CarBoundsGenerator.GetRoofBounds(carData), equipment.RoofEquipment);
        _trunkGenerator.GenerateSockets(CarBoundsGenerator.GetTrunkBounds(carData), equipment.TrunkEquipment);
        _leftSideGenerator.GenerateSockets(CarBoundsGenerator.GetSideBounds(carData,leftSide:true), equipment.LeftSideEquipment);
        _rightSideGenerator.GenerateSockets(CarBoundsGenerator.GetSideBounds(carData, leftSide: false), equipment.RightSideEquipment);
        _frontBumperGenerator.GenerateSockets(CarBoundsGenerator.GetFrontBumperBounds(carData), equipment.FrontBumperEquipment);
        _backBumperGenerator.GenerateSockets(CarBoundsGenerator.GetBackBumperBounds(carData),equipment.BackBumperEquipment);
    }

    public void ShowSockets(bool show)
    {
        _hoodGenerator.ShowSockets(show);
        _roofGenerator.ShowSockets(show);
        _trunkGenerator.ShowSockets(show);
        _leftSideGenerator.ShowSockets(show);
        _rightSideGenerator.ShowSockets(show);
        _frontBumperGenerator.ShowSockets(show);
        _backBumperGenerator.ShowSockets(show);
    }

    public void LoadAllEquipment()
    {
        _hoodGenerator.LoadEquipment();
        _roofGenerator.LoadEquipment();
        _trunkGenerator.LoadEquipment();
        _leftSideGenerator.LoadEquipment();
        _rightSideGenerator.LoadEquipment();
        _frontBumperGenerator.LoadEquipment();
        _backBumperGenerator.LoadEquipment();
    }
}