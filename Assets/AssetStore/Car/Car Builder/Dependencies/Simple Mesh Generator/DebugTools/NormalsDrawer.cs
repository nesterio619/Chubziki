using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalsDrawer : MonoBehaviour
{
    [SerializeField] private int _min = 0;
    [SerializeField] private int _max = 10;
    [SerializeField] private int _offset = 0;
    [SerializeField][Range(0,1)] private float _length = 0.333f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var mesh = GetComponent<MeshFilter>().sharedMesh;


        if (mesh.vertices.Length != mesh.normals.Length)
        {
            Debug.Log(mesh.vertices.Length + "_" + mesh.normals.Length);
        }

        var min = _min + _offset;
        var max = _max + _offset;

        if (min < 0)
            min = 0;

        for (int i = Mathf.Min(mesh.vertices.Length, min); i < Mathf.Min(mesh.vertices.Length, max); i++)
        {
            var start = transform.TransformPoint(mesh.vertices[i]);
            var end = start + transform.TransformDirection(mesh.normals[i]) * _length;

            Gizmos.DrawLine(start, end);
        }
    }
}
