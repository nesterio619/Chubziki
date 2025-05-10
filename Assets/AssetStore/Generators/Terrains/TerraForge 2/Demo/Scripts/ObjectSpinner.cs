using UnityEngine;

/// <summary>
/// Rotates an object around its local axes at specified speeds.
/// </summary>
public class ObjectSpinner : MonoBehaviour
{
    /// <summary>
    /// Rotation speed around the X axis.
    /// </summary>
    [Tooltip("Rotation speed around the X axis.")]
    public float rotationSpeedX = 0f;

    /// <summary>
    /// Rotation speed around the Y axis.
    /// </summary>
    [Tooltip("Rotation speed around the Y axis.")]
    public float rotationSpeedY = 0f;

    /// <summary>
    /// Rotation speed around the Z axis.
    /// </summary>
    [Tooltip("Rotation speed around the Z axis.")]
    public float rotationSpeedZ = 0f;

    /// <summary>
    /// Updates the object's rotation based on the specified speeds.
    /// </summary>
    void Update()
    {
        // Calculate the rotation angles based on the speed and time.
        float rotationX = rotationSpeedX * Time.deltaTime;
        float rotationY = rotationSpeedY * Time.deltaTime;
        float rotationZ = rotationSpeedZ * Time.deltaTime;

        // Apply the rotation to the object.
        transform.Rotate(rotationX, rotationY, rotationZ);
    }
}