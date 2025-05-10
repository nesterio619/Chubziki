using SimpleMeshGenerator;
using UnityEngine;

public class CircleTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CircleGenerator.Generate( 0.5f, Vector3.zero, 30, 2).Clone();

        Instantiate(_dummyPrefab, new Vector3(4, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CircleGenerator.GenerateHollow(1, 0.3f, Vector3.zero, 30, 10).Clone();

        Instantiate(_dummyPrefab, new Vector3(4, 8, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CircleGenerator.GenerateHollow(2, 1.3f, Vector3.zero, 30, 10).Clone();

        Instantiate(_dummyPrefab, new Vector3(8, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CircleGenerator.GenerateHollow(1, 0.3f, Vector3.zero, 10, 10).Clone();

        Instantiate(_dummyPrefab, new Vector3(12, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CircleGenerator.Generate_Detailed(1, Vector3.zero).Clone();
    }
}
