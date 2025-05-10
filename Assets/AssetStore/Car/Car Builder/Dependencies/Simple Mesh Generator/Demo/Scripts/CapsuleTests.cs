using SimpleMeshGenerator;
using UnityEngine;

public class CapsuleTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CapsuleGenerator.Generate(0.5f, 3, 6).Clone();

        Instantiate(_dummyPrefab, new Vector3(5, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CapsuleGenerator.Generate(0.5f, 3, 7).Clone();

        Instantiate(_dummyPrefab, new Vector3(10, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = CapsuleGenerator.Generate(2f, 3, 30).Clone();
    }
}
