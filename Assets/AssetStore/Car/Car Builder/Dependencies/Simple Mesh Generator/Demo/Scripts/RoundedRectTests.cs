using SimpleMeshGenerator;
using UnityEngine;

public class RoundedRectTests : MonoBehaviour
{
    [SerializeField] private GameObject _dummyPrefab = default;

    void OnEnable()
    {
        Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = RoundedRectGenerator.GenerateFlatSided(Vector2.one, 0.25f, 0.25f).Clone();

        Instantiate(_dummyPrefab, new Vector3(2, 0, 0), Quaternion.identity).GetComponent<MeshFilter>().mesh = RoundedRectGenerator.GenerateFlatSided_Hollow(Vector2.one * 2.5f, 0.25f, 0.25f, Vector2.one * 0.3f).Clone();        
    }
}
