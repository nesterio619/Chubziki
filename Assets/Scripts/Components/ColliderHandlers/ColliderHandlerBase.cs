using Core.Enums;
using Core.Extensions;
using UnityEngine;

/// <summary>
/// ColliderHandler must be on object with rigidbody and in child object must be collider
/// </summary>
public abstract class ColliderHandlerBase : MonoBehaviour
{
    [SerializeField] private UnityLayers _allowedLayers = (UnityLayers)~0;

    protected bool LayerAllowed(GameObject gameObject)
    {
        // If all layers are selected, return true
        if (_allowedLayers == (UnityLayers)~0)
        {
            return true;
        }

        // Check if the object's layer is in the allowed layers
        var objectLayer = gameObject.GetObjectLayer();
        return _allowedLayers.HasFlag(objectLayer);
    }
}
