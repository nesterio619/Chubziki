using SimpleMeshGenerator;
using UnityEngine;

public class PipeTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;
    [SerializeField] private Transform[] _path = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = PipeGenerator.Generate(_path, false, 0.1f, 24, true).Clone();
    }
}
