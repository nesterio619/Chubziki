using Core.InputManager;
using Hypertonic.Modules.UltimateSockets.XR;
using System;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectGrabber : MonoBehaviour
{
    private const float Ray_Length = 100;

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float offsetLength;
    [SerializeField] private float smoothTime;

    public Action<Transform> OnGrab;
    public Action<Transform> OnRelease;

    private Ray _ray;
    private RaycastHit _raycastHit;
    private Camera _camera;

    private IGrabbableItem _grabbedObject;
    private Transform _grabbedTransform;

    private Vector3 _smoothVelocity;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update() 
    {
        Vector3 screenPoint;
        if (InputManager.CurrentInputDevice == InputDeviceType.Keyboard)
            screenPoint = Mouse.current.position.ReadValue();
        else
            screenPoint = GamepadCursor.CurrentCursorPosition;

        _ray = _camera.ScreenPointToRay(screenPoint);

        if (_grabbedObject == null)
        {
            if (InputManager.leftMouseButton.WasPressedThisFrame() || GamepadCursor.WasPressedThisFrame())
            {
                if (!Physics.Raycast(_ray, out _raycastHit, Ray_Length, layerMask)) return;

                GrabObject();
            }
        }
        else
        {
            if (InputManager.leftMouseButton.WasReleasedThisFrame() || GamepadCursor.WasReleasedThisFrame())
            {
                ReleaseObject();
                return;
            }

            if (!Physics.Raycast(_ray, out _raycastHit, Ray_Length)) return;

            MoveObject();
        }
    }

    private void GrabObject()
    {
        var hitObject = _raycastHit.collider.gameObject;
        if (hitObject.TryGetComponent(out IGrabbableItem item))
        {
            _grabbedObject = item;
            _grabbedObject.Grab();

            _grabbedTransform = _raycastHit.transform;

            OnGrab?.Invoke(_grabbedTransform);
            GamepadCursor.SetImage(GamepadCursorImage.Grab);
        }
    }

    private void MoveObject()
    {
        if (_grabbedObject == null) return;

        var offset = -_ray.direction * offsetLength;
        var targetPosition = _raycastHit.point + offset;

        _grabbedTransform.position = Vector3.SmoothDamp(
            _grabbedTransform.position, 
            targetPosition, 
            ref _smoothVelocity, 
            smoothTime
        );
    }

    private void ReleaseObject()
    {
        _grabbedObject.Release();
        _grabbedObject = null;

        OnRelease?.Invoke(_grabbedTransform);
        _grabbedTransform = null;

        GamepadCursor.SetImage(GamepadCursorImage.Normal);
    }

    public void EnableGrabbing(bool enable)
    {
        enabled = enable;

        if (!enable && _grabbedTransform != null)
            ReleaseObject();
    }
}