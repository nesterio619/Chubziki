using SimpleMeshGenerator;
using UnityEngine;

public class SphereTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(5, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = SphereGenerator.Generate(1f, 66).Clone();

        Instantiate(_dummyPrefab, new Vector3(10, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = SphereGenerator.Generate(1f, 6, true).Clone();
    }
}
