using UnityEngine;

/// <summary>
/// Controls the camera's movement and rotation to follow the player.
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary>
    /// Speed at which the camera zooms in and out.
    /// </summary>
    [Tooltip("Speed at which the camera zooms in and out.")]
    public float ZoomSpeed;

    /// <summary>
    /// Transform of the player to follow.
    /// </summary>
    [Header("Player To Follow")]
    [Tooltip("Transform of the player to follow.")]
    public Transform target;

    [Header("Follow Properties")]
    /// <summary>
    /// Distance between the camera and the player.
    /// </summary>
    [Tooltip("Distance between the camera and the player.")]
    public float distance = 10.0f;

    /// <summary>
    /// Smoothness of the camera's movement.
    /// </summary>
    [Tooltip("Smoothness of the camera's movement.")]
    public float smoothness = 0.15f;

    /// <summary>
    /// Minimum distance the camera can be from the player.
    /// </summary>
    [Tooltip("Minimum distance the camera can be from the player.")]
    public float min_distance = 5.0f;

    /// <summary>
    /// Maximum distance the camera can be from the player.
    /// </summary>
    [Tooltip("Maximum distance the camera can be from the player.")]
    public float max_distance = 20.0f;

    /// <summary>
    /// Horizontal offset for the camera's follow position.
    /// </summary>
    [Tooltip("Horizontal offset for the camera's follow position.")]
    public float X_follow = 0.0f;

    /// <summary>
    /// Vertical offset for the camera's follow position.
    /// </summary>
    [Tooltip("Vertical offset for the camera's follow position.")]
    public float Y_follow = 3.0f;

    /// <summary>
    /// Depth offset for the camera's follow position.
    /// </summary>
    [Tooltip("Depth offset for the camera's follow position.")]
    public float Z_follow = 0.0f;

    [Header("Rotation Properties")]
    /// <summary>
    /// Indicates if the camera should rotate with input.
    /// </summary>
    [Tooltip("Indicates if the camera should rotate with input.")]
    public bool rotateCamera = true;

    /// <summary>
    /// Speed at which the camera rotates.
    /// </summary>
    [Tooltip("Speed at which the camera rotates.")]
    public float rotateSpeed = 5.0f;

    /// <summary>
    /// Minimum angle the camera can rotate to.
    /// </summary>
    [Tooltip("Minimum angle the camera can rotate to.")]
    public float minAngle = -45.0f;

    /// <summary>
    /// Maximum angle the camera can rotate to.
    /// </summary>
    [Tooltip("Maximum angle the camera can rotate to.")]
    public float maxAngle = -10.0f;

    /// <summary>
    /// Reference to the main camera.
    /// </summary>
    private Camera PlayerCamera;

    /// <summary>
    /// Current horizontal rotation angle.
    /// </summary>
    private float currentX = 0.0f;

    /// <summary>
    /// Current vertical rotation angle.
    /// </summary>
    private float currentY = 0.0f;

    /// <summary>
    /// Current rotation of the camera.
    /// </summary>
    private Quaternion rotation;

    /// <summary>
    /// Direction vector for camera movement.
    /// </summary>
    private Vector3 dir;

    /// <summary>
    /// Offset vector for the camera's position.
    /// </summary>
    private Vector3 offset;

    /// <summary>
    /// Locks and hides the cursor, initializes camera components.
    /// </summary>
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerCamera = Camera.main;
        offset = PlayerCamera.transform.position;
    }

    /// <summary>
    /// Handles camera input and zoom functionality.
    /// </summary>
    private void Update()
    {
        // Handle zooming in and out
        if (distance <= max_distance && distance >= min_distance)
        {
            distance += -(Input.GetAxis("Mouse ScrollWheel")) * ZoomSpeed;
        }

        if (distance > max_distance) { distance = max_distance; }
        if (distance < min_distance) { distance = min_distance; }

        // Handle camera rotation input
        currentX += Input.GetAxis("Mouse X") * rotateSpeed;
        currentY += Input.GetAxis("Mouse Y") * rotateSpeed;

        currentY = Mathf.Clamp(currentY, minAngle, maxAngle);
    }

    /// <summary>
    /// Updates camera follow and rotation based on input.
    /// </summary>
    private void FixedUpdate()
    {
        if (rotateCamera)
        {
            dir = new Vector3(0, 0, -distance);
            rotation = Quaternion.Euler(-currentY, currentX, 0);
            PlayerCamera.transform.position = Vector3.Lerp(PlayerCamera.transform.position, target.position + rotation * dir, smoothness);
            PlayerCamera.transform.LookAt(new Vector3(target.position.x + X_follow, target.position.y + Y_follow, target.position.z + Z_follow));
        }
        else
        {
            var targetRotation = Quaternion.LookRotation(target.position - PlayerCamera.transform.position);
            PlayerCamera.transform.position = Vector3.Lerp(PlayerCamera.transform.position, target.position + offset, smoothness);
            PlayerCamera.transform.rotation = Quaternion.Slerp(PlayerCamera.transform.rotation, targetRotation, smoothness);
        }
    }
}