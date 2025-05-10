using UnityEngine;

public class BF_RotateWithMouse : MonoBehaviour
{
    public Camera cam;
    public Transform target;  // The Transform to rotate around
    public float rotationSpeed = 5.0f;

    private void Start()
    {
        // Rotate the camera around the target while keeping the look-at point constant
        Vector3 cameraToTarget = cam.transform.position - target.position;
        Quaternion rotation = Quaternion.AngleAxis(0, Vector3.up);
        cameraToTarget = rotation * cameraToTarget;
        // Update camera position and rotation
        cam.transform.position = target.position + cameraToTarget;
        cam.transform.LookAt(target);
    }

    private void Update()
    {
        // Check if the right mouse button is held down
        if (Input.GetMouseButton(1))
        {
            // Calculate rotation based on mouse input
            float mouseX = Input.GetAxis("Mouse X");
            float rotationAmount = mouseX * rotationSpeed * Time.deltaTime;

            // Rotate the camera around the target while keeping the look-at point constant
            Vector3 cameraToTarget = cam.transform.position - target.position;
            Quaternion rotation = Quaternion.AngleAxis(rotationAmount, Vector3.up);
            cameraToTarget = rotation * cameraToTarget;

            // Update camera position and rotation
            cam.transform.position = target.position + cameraToTarget;
            cam.transform.LookAt(target);
        }
    }
}