using System.Collections.Generic;
using Actors;
using UnityEngine;

public class PlatformAttacher : MonoBehaviour
{
    Vector3 _lastPlatformPosition;

    [SerializeField]
    private List<Rigidbody> movingTransforms = new();

    public Vector3 sizePhys;

    private Rigidbody _rbod;

    private void Start()
    {
        _rbod = GetComponent<Rigidbody>();

    }

    private void FixedUpdate()
    {

        Collider[] col = Physics.OverlapBox(transform.position + Vector3.up, sizePhys);

        movingTransforms.Clear();

        foreach (var item in col)
        {
            item.transform.root.TryGetComponent(out Rigidbody rb);

            if (rb != null)

                AttachToPlatform(rb);
        }

        Vector3 platformMovement = _rbod.position - _lastPlatformPosition;

        foreach (var item in movingTransforms)
        {
            item.MovePosition(item.position + platformMovement);
        }

        _lastPlatformPosition = _rbod.position;
    }



    /*private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent(out Rigidbody movingRigidbody))
        {
            Debug.Log("Complete Adding" + collision.gameObject.name) ;
            AttachToPlatform(movingRigidbody.transform);
        }else
        {
            Debug.Log("False Adding" + collision.gameObject.name);

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Complete removing" + collision.gameObject.name);

        RemoveFromPlatform(collision.transform);

    }*/

    public void AttachToPlatform(Rigidbody transform)
    {
        if (movingTransforms.Contains(transform))
        {
            return;
        }

        movingTransforms.Add(transform);
    }

    public void RemoveFromPlatform(Rigidbody transform)
    {
        movingTransforms.Remove(transform);
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawCube(transform.position + Vector3.up, sizePhys * 2);
    }
}
