using SimpleMeshGenerator;
using UnityEngine;

namespace ProceduralCarBuilder
{
    public class Demo2 : MonoBehaviour
    {
        [SerializeField] [TextArea(10, 20)] private string _notes;

        [SerializeField] private CarSettings _carSettings = default;

        [Space]
        [SerializeField] private GameObject _roofProp = default;
        [SerializeField] private GameObject _trunkProp = default;
        [SerializeField] private GameObject _frontProp = default;
        [SerializeField] private GameObject _hoodProp = default;
        [SerializeField] private GameObject _light = default;

        void Start()
        {
            var carData = _carSettings.GenerateData();


            #region EXAMPLE: Make car fit prop

            var customboundsVolume = _trunkProp.GetComponentInChildren<BoundsVolume>();
            var bounds = customboundsVolume == null ? _trunkProp.GetComponentInChildren<MeshRenderer>().bounds : customboundsVolume.LocalBounds;
            carData.EnsureBackTrunkEncapsulatesVolume(bounds.size);


            customboundsVolume = _roofProp.GetComponentInChildren<BoundsVolume>();
            bounds = customboundsVolume == null ? _roofProp.GetComponentInChildren<MeshRenderer>().bounds : customboundsVolume.LocalBounds;
            carData.EnsureRoofEncapsulatesVolume(bounds.size);
            #endregion


            // generate Car after data has been modified
            var car = CarGenerator.Generate(carData);


            var propCopy = Instantiate(_trunkProp);
            propCopy.transform.SetParentReset(car.PropAnchor_BackTrunk.MiddlePoint);
            propCopy.transform.position = car.PropAnchor_BackTrunk.WorldSpaceBounds.Below_Center;

            propCopy = Instantiate(_roofProp);
            propCopy.transform.SetParentReset(car.PropAnchor_Roof.MiddlePoint);
            propCopy.transform.position = car.PropAnchor_Roof.WorldSpaceBounds.Below_Center;


            #region EXAMPLE: just put item in the car no manipulation of prop or car

            propCopy = Instantiate(_frontProp);
            propCopy.transform.SetParentReset(car.PropAnchor_FrontTrunk.MiddlePoint);
            propCopy.transform.position = car.PropAnchor_FrontTrunk.WorldSpaceBounds.Below_Center;

            propCopy = Instantiate(_light);
            propCopy.SetActive(true);
            propCopy.transform.SetParent(car.HeadLight_Left, Vector3.forward * 0.2f, Quaternion.Euler(32, 0, 0), Vector3.one);

            propCopy = Instantiate(_light);
            propCopy.SetActive(true);
            propCopy.transform.SetParent(car.HeadLight_Right, Vector3.forward * 0.2f, Quaternion.Euler(32, 0, 0), Vector3.one);

            #endregion


            #region EXAMPLE: Make prop fit the car (And use of the BoundsVolume)

            customboundsVolume = _hoodProp.GetComponentInChildren<BoundsVolume>();
            bounds = customboundsVolume == null ? _hoodProp.GetComponentInChildren<MeshRenderer>().bounds : customboundsVolume.LocalBounds;

            propCopy = Instantiate(_hoodProp);
            propCopy.transform.SetParentReset(car.PropAnchor_HoodOrnament.MiddlePoint);

            var t = propCopy.transform;
            car.FitObjectIntoAnchorSpace(ref t, bounds.size, car.PropAnchor_HoodOrnament, true);

            #endregion

        }

    }

}
