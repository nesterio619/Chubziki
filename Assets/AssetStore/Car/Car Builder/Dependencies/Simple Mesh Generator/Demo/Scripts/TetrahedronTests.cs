using SimpleMeshGenerator;
using UnityEngine;

public class TetrahedronTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = TetrahedronGenerator.Generate(2f).Clone();

        Instantiate(_dummyPrefab, new Vector3(5, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = TetrahedronGenerator.GenerateHollow(2f, 0.4f).Clone();
    }
}
