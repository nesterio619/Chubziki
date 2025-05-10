using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RockGenerator
{
    [AddComponentMenu("ROCKGEN 2024/RockMeshCombiner")]
    public class RockMeshCombiner : MonoBehaviour
    {
        public bool addAttributeToName = true;

        public GameObject combinedObject;

        public void RockMeshCombine()
        {
            MeshFilter[] childMeshFilters = GetComponentsInChildren<MeshFilter>();

            CombineInstance[] combineInstances = new CombineInstance[childMeshFilters.Length];
            Mesh combinedMesh = new Mesh();

            for (int i = 0; i < childMeshFilters.Length; i++)
            {
                Mesh meshCopy = Instantiate(childMeshFilters[i].sharedMesh);
                meshCopy.name = childMeshFilters[i].sharedMesh.name + "_Copy";

                combineInstances[i].mesh = meshCopy;

                Matrix4x4 transformMatrix = transform.worldToLocalMatrix * childMeshFilters[i].transform.localToWorldMatrix;

                Vector3[] vertices = combineInstances[i].mesh.vertices;
                for (int v = 0; v < vertices.Length; v++)
                {
                    vertices[v] = transformMatrix.MultiplyPoint(vertices[v]);
                }

                combineInstances[i].mesh.vertices = vertices;

                // Recalculate normals
                combineInstances[i].mesh.RecalculateNormals();

                combineInstances[i].transform = Matrix4x4.identity;
            }

            combinedMesh.CombineMeshes(combineInstances, true, true);

            if (addAttributeToName)
                combinedObject = new GameObject($"{this.gameObject.name} Combined Mesh");
            else
                combinedObject = new GameObject($"{this.gameObject.name}");
            combinedObject.transform.SetParent(transform);

            combinedObject.AddComponent<MeshFilter>().sharedMesh = combinedMesh;
            combinedObject.AddComponent<MeshRenderer>().sharedMaterial = childMeshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;

            combinedObject.transform.localPosition = Vector3.zero;
            combinedObject.transform.localRotation = Quaternion.identity;
            combinedObject.transform.localScale = Vector3.one;
            combinedObject.transform.parent = null;
#if UNITY_EDITOR
            Selection.activeGameObject = combinedObject;
#endif
        }
    }
}
