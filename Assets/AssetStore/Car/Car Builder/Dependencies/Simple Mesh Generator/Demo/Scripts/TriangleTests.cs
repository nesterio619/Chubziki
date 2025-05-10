using SimpleMeshGenerator;
using UnityEngine;

public class TriangleTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = TriangleGenerator.Generate( new Vector3[] { new Vector3(0,2,3), new Vector3(1,5,-2), new Vector3(0,0,0)}, Vector2.zero, Vector3.forward).Clone();

        Instantiate(_dummyPrefab, new Vector3(4, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = TriangleGenerator.Generate_Hollow(new Vector3[] { new Vector3(0, 2, 3), new Vector3(1, 5, -2), new Vector3(0, 0, 0) }, Vector3.zero, 0.2f).Clone();

        Instantiate(_dummyPrefab, new Vector3(8, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = TriangleGenerator.Generate_Hollow(new Vector3[] { new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(1, 0, 0) }, Vector3.zero, 0.2f).Clone();

    }
}
