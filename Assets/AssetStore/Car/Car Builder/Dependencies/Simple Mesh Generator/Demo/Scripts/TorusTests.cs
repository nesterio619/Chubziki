using SimpleMeshGenerator;
using UnityEngine;

public class TorusTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = TorusGenerator.Generate(3f, 1, 12, 12).Clone();

        Instantiate(_dummyPrefab, new Vector3(10, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = TorusGenerator.Generate(2f, 0.4f, 20, 12).Clone();
    }
}
