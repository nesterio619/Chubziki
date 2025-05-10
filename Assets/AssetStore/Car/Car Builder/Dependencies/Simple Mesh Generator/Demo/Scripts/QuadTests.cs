using SimpleMeshGenerator;
using UnityEngine;

public class QuadTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = QuadGenerator_2D.Generate(Vector2.one, Vector2Int.one).Clone();

        Instantiate(_dummyPrefab, new Vector3(4, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = QuadGenerator_2D.Generate(Vector2.one * 3, Vector2Int.one * 8).Clone();

        Instantiate(_dummyPrefab, new Vector3(0, 4, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = QuadGenerator_2D.Generate_Hollow(Vector2.one * 3, Vector2Int.one * 6, 0.5f).Clone();

        Instantiate(_dummyPrefab, new Vector3(0, 8, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = QuadGenerator_2D.Generate_Hollow(Vector2.one * 3, Vector2Int.one, 0.25f).Clone();
    }
}
