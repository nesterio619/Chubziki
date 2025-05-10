using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// </summary>
namespace SimpleMeshGenerator
{
    public class Demo2 : MonoBehaviour
    {
        [SerializeField] private GameObject _dummyPrefab = default;
        [Space]

        [SerializeField] private bool _createFanDemo = false;
        [SerializeField] private bool _extrudeDemo = false;
        [SerializeField] private bool _bridgeDemo = false;
        [SerializeField] private bool _ridgeDemo1 = false;
        [SerializeField] private bool _ridgeDemo2 = false;
        [SerializeField] private bool _ridgeDemo3 = false;

        [Space]
        [SerializeField] private Transform _groupA = default;
        [SerializeField] private Transform _groupB = default;

        void OnEnable()
        {
            _groupA.gameObject.SetActive(true);
            _groupB.gameObject.SetActive(true);

            if (_createFanDemo)
            {
                var meshA = GeneralMeshGenerator.CreateFan(GetPoints(_groupA), GetPoints(_groupA)[0], Vector2.zero, Vector3.forward).Clone();
                var meshB = GeneralMeshGenerator.CreateFan(GetPoints(_groupB), GetPoints(_groupB)[2], Vector2.zero, Vector3.forward, true).Clone();

                var filter = Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>();
                filter.mesh = meshA.SetOrigin(filter.transform.position);

                filter = Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>();
                filter.mesh = meshB.SetOrigin(filter.transform.position);
            }
            else if(_extrudeDemo)
            {
                _groupB.gameObject.SetActive(false);

                var mesh = new Mesh();

                var extrudedPoints = new Vector3[0];
                CombineMeshes.Combine(mesh, GeneralMeshGenerator.Extrude(GetPoints(_groupA), Vector3.forward, 1, true, out extrudedPoints, false));
                var inwardsPoints = GeneralMeshGenerator.ExtrudePoints(extrudedPoints, 0.5f, Vector3.forward, Vector3.up, new GameObject().transform, true, false);

                CombineMeshes.Combine(mesh, GeneralMeshGenerator.CreateBridgeHardEdged(extrudedPoints, inwardsPoints.Value, true, true, true));

                CombineMeshes.Combine(mesh, GeneralMeshGenerator.Extrude(inwardsPoints.Value, Vector3.forward, 3, true, out extrudedPoints, false));
                CombineMeshes.Combine(mesh, GeneralMeshGenerator.CreateFan(extrudedPoints, extrudedPoints[0], Vector2.zero, Vector3.forward, true));

                var filter = Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>();
                filter.mesh = mesh.SetOrigin(filter.transform.position);
            }
            else if(_bridgeDemo)
            {
                var mesh = GeneralMeshGenerator.CreateBridgeHardEdged(GetPoints(_groupA), GetPoints(_groupB), true, true, true);

                var filter = Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>();
                filter.mesh = mesh.SetOrigin(filter.transform.position);
            }
            else if(_ridgeDemo1)
            {
                _groupB.gameObject.SetActive(false);

                var innerRidge = new Vector3[0];

                var mesh = GeneralMeshGenerator.TryCreateRidge
                (
                    GetPoints(_groupA),
                    new Vector2[] { new Vector2(0, 0), new Vector2(1, 0) },
                    Vector3.forward,
                    0.4f,
                    3f,
                    Vector3.forward,
                    Vector3.up,
                    true,
                    false,
                    new GameObject().transform,
                    out innerRidge
                ).Clone();

                var filter = Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>();
                filter.mesh = mesh.SetOrigin(filter.transform.position);
            }
            else if (_ridgeDemo2)
            {
                _groupB.gameObject.SetActive(false);

                var innerRidge = new Vector3[0];

                var mesh = GeneralMeshGenerator.TryCreateRidge
                (
                    GetPoints(_groupA),
                    new Vector2[] { new Vector2(0, 0), new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.5f), new Vector2(1, 0) },
                    Vector3.back,
                    0.4f,
                    3f,
                    Vector3.back,
                    Vector3.up,
                    true,
                    true,
                    new GameObject().transform,
                    out innerRidge
                ).Clone();

                var filter = Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>();
                filter.mesh = mesh.SetOrigin(filter.transform.position);
            }
            else if (_ridgeDemo3)
            {
                _groupB.gameObject.SetActive(false);

                var innerRidge = new Vector3[0];

                var angledForward = Vector3.Lerp(Vector3.forward, Vector3.up, 0.5f).normalized;

                var mesh = GeneralMeshGenerator.TryCreateRidge
                (
                    GetPoints(_groupA),
                    new Vector2[] { new Vector2(0, 0), new Vector2(0.2f, 0.5f), new Vector2(0.8f, 0.5f), new Vector2(1, 0) },
                    angledForward,
                    0.4f,
                    3f,
                    Vector3.forward,
                    Vector3.up,
                    false,
                    true,
                    new GameObject().transform,
                    out innerRidge
                ).Clone();


                var filter = Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<MeshFilter>();                
                filter.mesh = mesh.SetOrigin(filter.transform.position);
            }
        }

        private void OnDrawGizmos()
        {
            DebugUtility.DebugMode = true;
            if (_groupA.gameObject.activeSelf) DebugUtility.DrawPointsPath(_groupA, Color.red, true);
            if (_groupB.gameObject.activeSelf) DebugUtility.DrawPointsPath(_groupB, Color.blue, true);
        }

        private Vector3[] GetPoints(Transform transformHolder)
        {
            var points = new Vector3[transformHolder.childCount];
            for (int i = 0; i < transformHolder.childCount; i++)
            {
                points[i] = transformHolder.GetChild(i).position;
            }

            return points;
        }

    }
}
