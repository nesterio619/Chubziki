using UnityEngine;

/// <summary>
/// Controls the rotation of the object based on user input and returns it to its original position when there is no input.
/// </summary>
public class TiltController : MonoBehaviour
{
    /// <summary>
    /// Maximum rotation angle on the x-axis.
    /// </summary>
    [Tooltip("Maximum rotation angle on the x-axis.")]
    public float maxAngleX = 20f;

    /// <summary>
    /// Maximum rotation angle on the y-axis.
    /// </summary>
    [Tooltip("Maximum rotation angle on the y-axis.")]
    public float maxAngleY = 10f;

    /// <summary>
    /// Speed at which the object rotates based on user input.
    /// </summary>
    [Tooltip("Speed at which the object rotates based on user input.")]
    public float rotationSpeed = 50f;

    /// <summary>
    /// Speed at which the object returns to its original position when there is no input.
    /// </summary>
    [Tooltip("Speed at which the object returns to its original position when there is no input.")]
    public float returnSpeed = 30f;

    /// <summary>
    /// The original rotation of the object.
    /// </summary>
    private Quaternion originalRotation;

    /// <summary>
    /// Current rotation angle on the x-axis.
    /// </summary>
    private float currentAngleX = 0f;

    /// <summary>
    /// Current rotation angle on the y-axis.
    /// </summary>
    private float currentAngleY = 0f;

    /// <summary>
    /// Stores the original rotation of the object.
    /// </summary>
    void Start()
    {
        // Store the original rotation of the object
        originalRotation = transform.localRotation;
    }

    /// <summary>
    /// Updates the rotation of the object based on user input or returns it to its original position when there is no input.
    /// </summary>
    void Update()
    {
        // Get input from the left/right keys
        float input = Input.GetAxis("Horizontal");

        if (input != 0)
        {
            // Rotate the object on the x-axis based on input
            currentAngleX += input * rotationSpeed * Time.deltaTime;
            currentAngleX = Mathf.Clamp(currentAngleX, -maxAngleX, maxAngleX);

            // Rotate the object on the y-axis based on input
            currentAngleY += input * rotationSpeed * Time.deltaTime;
            currentAngleY = Mathf.Clamp(currentAngleY, -maxAngleY, maxAngleY);
        }
        else
        {
            // Return the object to its original position on the x-axis
            if (currentAngleX > 0)
            {
                currentAngleX -= returnSpeed * Time.deltaTime;
                if (currentAngleX < 0)
                {
                    currentAngleX = 0;
                }
            }
            else if (currentAngleX < 0)
            {
                currentAngleX += returnSpeed * Time.deltaTime;
                if (currentAngleX > 0)
                {
                    currentAngleX = 0;
                }
            }

            // Return the object to its original position on the y-axis
            if (currentAngleY > 0)
            {
                currentAngleY -= returnSpeed * Time.deltaTime;
                if (currentAngleY < 0)
                {
                    currentAngleY = 0;
                }
            }
            else if (currentAngleY < 0)
            {
                currentAngleY += returnSpeed * Time.deltaTime;
                if (currentAngleY > 0)
                {
                    currentAngleY = 0;
                }
            }
        }

        // Apply the rotation to the object
        Quaternion targetRotation = originalRotation * Quaternion.Euler(currentAngleX, currentAngleY, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}