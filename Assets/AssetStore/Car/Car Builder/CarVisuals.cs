using Actors;
using Core.Utilities;
using Hypertonic.Modules.UltimateSockets.Sockets;
using NWH.WheelController3D;
using ProceduralCarBuilder;
using UnityEngine;

[ExecuteInEditMode]
public class CarVisuals : MonoBehaviour
{
    [SerializeField] private CarActor carActor;
    [Space]
    [SerializeField] private CarSettings carSettings;
    [Space]
    [SerializeField] private BoxCollider frontBumperCollider;
    [SerializeField] private BoxCollider backBumperCollider;
    [SerializeField] private Transform enginePoint;
    [Space]
    [SerializeField] private Collider ramCollider;
    [SerializeField] private WheelController[] wheelControllers;

    [Space(15)]
    [SerializeField] private EquipmentSocket socketPrefab;
    [SerializeField] private CarSocketManager socketManager;

    public CarData CarData { get; private set; }

    private MeshCollider _carBodyMeshCollider;
    private TemporaryCarInitializer _carInitializer;
    private CarPartReferences _carPartReferences;

    private void Awake()
    {
        _carInitializer = GetComponent<TemporaryCarInitializer>();
        _carPartReferences = GetComponent<CarPartReferences>();
        _carBodyMeshCollider = GetComponent<MeshCollider>();

        if (socketManager != null && socketPrefab != null)
            socketManager.Initialize(_carPartReferences, carActor.CarMold, socketPrefab);
        else
            Debug.LogError("Couldn't find socketManager or socketPrefab for the car called:" +
                           transform.parent.gameObject.name);

        _carInitializer.SetPartReferences(_carPartReferences);

        var actorTransform = carActor.ActorPresetTransform ?? carActor.transform;
        var rotation = actorTransform.rotation;
        actorTransform.rotation = Quaternion.identity;

        GenerateCar(carSettings);

        actorTransform.rotation = rotation;
    }

    private void OnEnable()
    {
        carSettings.OnSettingsChange += GenerateCar;
        carActor.CarMold.OnMoldChange += GenerateSockets;
    }

    private void OnDisable()
    {
        carSettings.OnSettingsChange -= GenerateCar;
        carActor.CarMold.OnMoldChange -= GenerateSockets;
    }

    private void GenerateCar(CarSettings carSettings)
    {
        CarGenerator.CarInitializerInstance = _carInitializer;
        CarGenerator.MeshGenerationHelper = _carInitializer.MeshGenerationHelper;

        _carBodyMeshCollider.enabled = false;
        ramCollider.enabled = false;

        CarData = carSettings.GenerateData(true, true);
        CarGenerator.Generate(CarData, transform, false);

        if(_carBodyMeshCollider.sharedMesh.IsValid())
        {
            _carBodyMeshCollider.enabled = true;
            ramCollider.enabled = true;
        }

        if (wheelControllers.Length > 0)
            for (int i = 0; i < wheelControllers.Length; i++)
            {
                var wheel = wheelControllers[i];
                var mesh = _carInitializer.Wheels[i];
                var offset = wheel.NonRotatingVisual.transform.localPosition;

                wheel.enabled = false;
                wheel.Radius = CarData.WheelData.Radius;
                wheel.transform.position = mesh.transform.position - offset;
                wheel.enabled = true;
            }

        MoveExternalTransforms();

        GenerateSockets();

        ShowSockets(!Application.isPlaying);
    }

    private void MoveExternalTransforms()
    {
        if (enginePoint != null)
            enginePoint.position = _carPartReferences.PropAnchor_FrontTrunk.MiddlePoint.position;

        if (frontBumperCollider != null)
        {
            frontBumperCollider.transform.position = _carPartReferences.LicensePlate.position;
            frontBumperCollider.size = new Vector3(
                CarData.BodyData.TotalWidth + CarData.BodyData.SlantedShapeSidewaysOffset * 2,
                frontBumperCollider.size.y, frontBumperCollider.size.z);
        }

        if (backBumperCollider != null)
        {
            var bumperData = CarData.BumperDataBack;
            var bodyData = CarData.BodyData;
            var offset = new Vector3(0, -bodyData.BodyHeight + bumperData.Height / 2, -bumperData.Thickness - bodyData.TrunkLength / 2);

            backBumperCollider.transform.position = _carPartReferences.PropAnchor_BackTrunk.MiddlePoint.position + offset;
            backBumperCollider.size = new Vector3(
                CarData.BodyData.TotalWidth + CarData.BodyData.SlantedShapeSidewaysOffset * 2,
                frontBumperCollider.size.y, frontBumperCollider.size.z);
        }
    }

    private void GenerateSockets()
    {
        if(socketManager != null && socketPrefab != null)
            socketManager.GenerateSockets(CarData);
    }

    public void ShowSockets(bool show)
    {
        if(socketManager != null && socketPrefab != null)
            socketManager.ShowSockets(show);
    }

    public void LoadAllEquipment()
    {
        if(socketManager != null && socketPrefab != null)
            socketManager.LoadAllEquipment();
    }
}