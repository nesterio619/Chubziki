using UnityEngine;
using UnityEngine.Events;

public class CollisionHandler : ColliderHandlerBase
{
    public UnityEvent<Collision> OnEnter = null;
    public UnityEvent<Collision> OnStay = null;
    public UnityEvent<Collision> OnExit = null;

    //Collider on which UnityEvents will be triggered
    [SerializeField] private Collider targetTriggeredCollider;

    public void SetCollider(Collider collider)
    {
        targetTriggeredCollider = collider;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (LayerAllowed(collision.collider.gameObject) && CheckCollider(collision))
        {
            OnEnter?.Invoke(collision);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (LayerAllowed(collision.collider.gameObject) && CheckCollider(collision))
        {
            OnStay?.Invoke(collision);
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (LayerAllowed(collision.collider.gameObject) && CheckCollider(collision))
        {
            OnExit?.Invoke(collision);
        }
    }

    public bool CheckCollider(Collision collision)
    {
        foreach (var contact in collision.contacts)
            if (targetTriggeredCollider.gameObject == contact.thisCollider.gameObject)
                return true;

        return false;

    }

}
