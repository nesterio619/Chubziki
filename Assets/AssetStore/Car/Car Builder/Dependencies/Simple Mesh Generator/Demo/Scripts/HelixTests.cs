using SimpleMeshGenerator;
using UnityEngine;

public class HelixTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = DoubleHelixGenerator.Generate(5, 30, 12, 1).Clone();
    }
}
