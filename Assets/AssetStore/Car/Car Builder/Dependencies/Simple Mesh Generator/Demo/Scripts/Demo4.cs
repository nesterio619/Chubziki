using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// </summary>
namespace SimpleMeshGenerator
{
    public class Demo4 : MonoBehaviour
    {
        [SerializeField] [TextArea(10, 20)] private string _notes;

        [SerializeField] private GameObject _dummyPrefab = default;

        [Space]
        [SerializeField] private bool _saveMeshAsAsset;
        [Space]
        [SerializeField] private bool _saveMeshToClipboard;
        [Space]
        [SerializeField] private bool _saveMeshAsJSON;
        [Space]
        [SerializeField] private bool _saveAndGenerate;

        private Mesh _meshA;
        private Mesh _meshB;

        private void Start()
        {
            _meshA = QuadGenerator_2D.Generate(Vector2.one, Vector2Int.one).Clone();
            _meshB = RectangleGenerator.Generate(Vector3.one * 2).Clone();
        }

        private void Update()
        {
            if (_saveMeshAsAsset)
            {
                _saveMeshAsAsset = false;
                ProjectUtility.SaveMeshAsAsset(_meshA, "Assets/DummyFile", "testtt");
            }

            if (_saveMeshToClipboard)
            {
                _saveMeshToClipboard = false;
                ProjectUtility.SaveMeshToClipboard(_meshA, "testtt");
            }

            if (_saveMeshAsJSON)
            {
                _saveMeshAsJSON = false;
                ProjectUtility.SaveMeshAsJSON(_meshA, Application.streamingAssetsPath, "testtt");
            }


            if (_saveAndGenerate)
            {
                _saveAndGenerate = false;
                var dataString = ProjectUtility.ConvertMeshesToString(new Mesh[] { _meshA, _meshB }, false);
                ProjectUtility.GenerateMeshesFromFile(dataString, transform);
            }
        }
    }
}
