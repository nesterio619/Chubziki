using SimpleMeshGenerator;
using UnityEngine;

public class OctagonTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = OctagonGenerator.Generate(1f, Vector3.zero).Clone();

        Instantiate(_dummyPrefab, new Vector3(4, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = OctagonGenerator.Generate_Hollow(1f, 0.8f, Vector3.zero).Clone();

        Instantiate(_dummyPrefab, new Vector3(8, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = OctagonGenerator.Generate_Detailed(1f, 12, Vector3.zero).Clone();

        Instantiate(_dummyPrefab, new Vector3(12, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = OctagonGenerator.Generate_Hollow_Detailed(1f, 0.8f, 14, Vector3.zero).Clone();
    }
}
