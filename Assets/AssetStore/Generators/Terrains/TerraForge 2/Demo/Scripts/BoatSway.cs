using UnityEngine;

/// <summary>
/// Controls the swaying and yaw wiggling of the boat to simulate realistic movement.
/// </summary>
public class BoatSway : MonoBehaviour
{
    /// <summary>
    /// Speed of the boat's sway.
    /// </summary>
    [Tooltip("Speed of the boat's sway.")]
    public float swaySpeed = 1f;

    /// <summary>
    /// Amount of boat sway.
    /// </summary>
    [Tooltip("Amount of boat sway.")]
    public float swayAmount = 1f;

    /// <summary>
    /// Amount of yaw wiggle.
    /// </summary>
    [Tooltip("Amount of yaw wiggle.")]
    public float yawWiggleAmount = 1f;

    /// <summary>
    /// Starting local Y position of the boat.
    /// </summary>
    private float startYPosition;

    /// <summary>
    /// Current offset for boat sway.
    /// </summary>
    private float currentSwayOffset;

    /// <summary>
    /// Current offset for yaw wiggle.
    /// </summary>
    private float currentYawOffset;

    /// <summary>
    /// Stores the starting local Y position of the boat.
    /// </summary>
    private void Start()
    {
        // Store the starting local Y position of the boat
        startYPosition = transform.localPosition.y;
    }

    /// <summary>
    /// Updates the boat's sway and yaw wiggle based on time and speed.
    /// </summary>
    private void Update()
    {
        // Calculate the new sway offset based on time and speed
        currentSwayOffset = Mathf.Sin(Time.time * swaySpeed) * swayAmount;

        // Calculate the new yaw offset based on time and speed
        currentYawOffset = Mathf.Sin(Time.time * swaySpeed) * yawWiggleAmount;

        // Apply the sway offset to the boat's local position
        Vector3 newLocalPosition = transform.localPosition;
        newLocalPosition.y = startYPosition + currentSwayOffset;
        transform.localPosition = newLocalPosition;

        // Apply the yaw offset to the boat's local rotation
        Vector3 newLocalRotationEuler = transform.localEulerAngles;
        newLocalRotationEuler.z = currentYawOffset;
        transform.localEulerAngles = newLocalRotationEuler;
    }
}