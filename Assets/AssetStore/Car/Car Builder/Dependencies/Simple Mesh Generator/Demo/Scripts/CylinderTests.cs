using SimpleMeshGenerator;
using UnityEngine;

public class CylinderTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CylinderGenerator.Generate(0.5f, 2, 12, 5).Clone();

        Instantiate(_dummyPrefab, new Vector3(5, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CylinderGenerator.Generate(2.5f, 2, 50, 5).Clone();

        Instantiate(_dummyPrefab, new Vector3(10, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CylinderGenerator.Generate_Hollow(1.5f, 4, 0.5f, 10, 3, GeneralMeshGenerator.Axis.Y).Clone();

        Instantiate(_dummyPrefab, new Vector3(15, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CylinderGenerator.Generate_Hollow(1.5f, 4, 0.5f, 10, 3, GeneralMeshGenerator.Axis.X).Clone();

        Instantiate(_dummyPrefab, new Vector3(20, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CylinderGenerator.Generate_Hollow(1.5f, 4, 0.5f, 10, 3, GeneralMeshGenerator.Axis.Z).Clone();
    }
}
