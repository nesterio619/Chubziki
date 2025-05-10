using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// TriggerHandler must be on object with Collider with trigger checkbox true
/// </summary>
/// 
public class TriggerHandler : ColliderHandlerBase
{
    public UnityEvent<Collider> OnEnter = null;
    public UnityEvent<Collider> OnStay = null;
    public UnityEvent<Collider> OnExit = null;

    [SerializeField] protected Collider Collider;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (LayerAllowed(other.gameObject))
        {
            OnEnter?.Invoke(other);
        }
    }

    protected void OnTriggerStay(Collider other)
    {
        if (LayerAllowed(other.gameObject))
        {
            OnStay?.Invoke(other);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (LayerAllowed(other.gameObject))
        {
            OnExit?.Invoke(other);
        }
    }
}
