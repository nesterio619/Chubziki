using UnityEngine;

/// <summary>
/// Created by : Glenn korver
/// </summary>
namespace SimpleMeshGenerator
{
    public class Demo3 : MonoBehaviour
    {
        [SerializeField] [TextArea(10, 20)] private string _notes;

        [SerializeField] private GameObject _dummyPrefab = default;

        private void Start()
        {

            var baseMesh = QuadGenerator_2D.Generate(Vector2.one, Vector2Int.one).Clone();

            var shape1 = QuadGenerator_2D.Generate(new Vector2(0.5f, 2), Vector2Int.one).Clone();
            var shape2 = QuadGenerator_2D.Generate(new Vector2(4, 0.5f), Vector2Int.one).Clone();

            Vector3[] deltaVerts;
            Vector3[] deltaNormals;
            Vector3[] deltaTangents;

            var tempMesh = baseMesh.Clone();

            // make blendshape 1
            CombineMeshes.CreateBlendShape(tempMesh, shape1, out deltaVerts, out deltaNormals, out deltaTangents);
            CombineMeshes.AddBlendShapeFrame(tempMesh, "Shape1", 1, deltaVerts, deltaNormals, deltaTangents);

            // make blendshape 2
            CombineMeshes.CreateBlendShape(tempMesh, shape2, out deltaVerts, out deltaNormals, out deltaTangents);
            CombineMeshes.AddBlendShapeFrame(tempMesh, "Shape2", 1, deltaVerts, deltaNormals, deltaTangents);


            // make blendshape 3, which consists out of multiple frames
            CombineMeshes.CreateBlendShape(tempMesh, shape1, out deltaVerts, out deltaNormals, out deltaTangents);
            CombineMeshes.AddBlendShapeFrame(tempMesh, "ShapeCombined", 0.5f, deltaVerts, deltaNormals, deltaTangents);

            CombineMeshes.CreateBlendShape(tempMesh, shape2, out deltaVerts, out deltaNormals, out deltaTangents);
            CombineMeshes.AddBlendShapeFrame(tempMesh, "ShapeCombined", 1f, deltaVerts, deltaNormals, deltaTangents);

            // create
            Instantiate(_dummyPrefab, new Vector3(0, 0, 0), Quaternion.identity).GetComponent<SkinnedMeshRenderer>().sharedMesh = tempMesh;

            // Save to project
            ProjectUtility.SaveMeshAsAsset(tempMesh, "Assets/BlendshapeDummy", "BlendShapeMesh");
        }
    }
}
