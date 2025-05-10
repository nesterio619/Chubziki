using SimpleMeshGenerator;
using UnityEngine;

public class RoundedQuadTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = RoundedQuadGenerator.Generate(Vector2.one, 0.25f, false).Clone();

        Instantiate(_dummyPrefab, new Vector3(5, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = RoundedQuadGenerator.Generate_Hollow(Vector2.one * 4, 0.5f, Vector2.one * 0.5f, false).Clone();
    }
}
