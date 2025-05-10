using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertsDrawer : MonoBehaviour
{
    [SerializeField] private int _specificVertIndex = -1;

    [SerializeField] private bool _forward;
    [SerializeField] private bool _backwards;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);

        var mesh = GetComponent<MeshFilter>().sharedMesh;

        for (int i = 0; i < Mathf.Min(mesh.vertices.Length, 4); i++)
        {
            Gizmos.DrawSphere(transform.TransformPoint(mesh.vertices[i]), 0.1f);
        }

        if(_forward)
        {
            _forward = false;
            _specificVertIndex++;
        }

        if (_backwards)
        {
            _backwards = false;
            _specificVertIndex--;
        }

        if (_specificVertIndex >= 0 && _specificVertIndex < mesh.vertices.Length)
            Gizmos.DrawWireSphere(transform.TransformPoint(mesh.vertices[_specificVertIndex]), 0.15f);
    }
}