using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrianglesDrawer : MonoBehaviour
{
    [SerializeField] private int _triangleCount = 1;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);

        var mesh = GetComponent<MeshFilter>().sharedMesh;

        for (int i = 0; i < Mathf.Min(mesh.triangles.Length, _triangleCount * 3); i++)
        {
            var vertIndex = mesh.triangles[i];
            Debug.Log(i + "_" + vertIndex);
            Gizmos.DrawSphere(transform.TransformPoint(mesh.vertices[vertIndex]), 0.1f);
        }
    }
}