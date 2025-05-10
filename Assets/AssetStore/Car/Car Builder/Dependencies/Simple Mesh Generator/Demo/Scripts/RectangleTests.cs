using SimpleMeshGenerator;
using UnityEngine;

public class RectangleTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = RectangleGenerator.Generate(new Vector3(1,1,1)).Clone();

        Instantiate(_dummyPrefab, new Vector3(5, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = RectangleGenerator.Generate(new Vector3(2, 5, 1), 4, true).Clone();

        Instantiate(_dummyPrefab, new Vector3(10, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = RectangleGenerator.GenerateHollow(new Vector3(1, 1, 1), 0.1f).Clone();

        Instantiate(_dummyPrefab, new Vector3(15, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = RectangleGenerator.GenerateHollow(new Vector3(1, 3, 1), 0.1f, 2, true).Clone();
    }
}
