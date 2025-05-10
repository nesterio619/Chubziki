using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls the movement and turning of the boat.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BoatController : MonoBehaviour
{
    /// <summary>
    /// Base speed of the boat.
    /// </summary>
    [Tooltip("Base speed of the boat.")]
    public float speed = 1500f;

    /// <summary>
    /// Turning speed of the boat.
    /// </summary>
    [Tooltip("Turning speed of the boat.")]
    public float turnSpeed = 150f;

    /// <summary>
    /// Acceleration rate of the boat.
    /// </summary>
    [Tooltip("Acceleration rate of the boat.")]
    public float acceleration = 3f;

    /// <summary>
    /// Deceleration rate of the boat.
    /// </summary>
    [Tooltip("Deceleration rate of the boat.")]
    public float deceleration = 2f;

    /// <summary>
    /// Multiplier applied to the speed when shift key is held.
    /// </summary>
    [Tooltip("Multiplier applied to the speed when shift key is held.")]
    public float speedMultiplier = 2f;

    /// <summary>
    /// List of audio clips for collision with terrain.
    /// </summary>
    [Tooltip("List of audio clips for collision with terrain.")]
    public List<AudioClip> collisionSounds;

    public float collisionVolumeMultiplier = 1f;
    public AudioSource audioSource;

    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private Rigidbody rb;

    /// <summary>
    /// Initializes the Rigidbody and AudioSource components.
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Updates the boat's movement and turning based on user input.
    /// </summary>
    private void Update()
    {
        float movementInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        float effectiveSpeed = speed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            effectiveSpeed *= speedMultiplier;
        }

        if (movementInput > 0)
        {
            targetSpeed = effectiveSpeed;
        }
        else if (movementInput < 0)
        {
            targetSpeed = -effectiveSpeed / 2f;
        }
        else
        {
            targetSpeed = 0;
        }

        if (targetSpeed > currentSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, deceleration * Time.deltaTime);
        }

        Vector3 force = transform.forward * currentSpeed * Time.deltaTime;
        rb.AddForce(force, ForceMode.Acceleration);

        float turnTorque = turnInput * turnSpeed * Time.deltaTime;
        rb.AddTorque(Vector3.up * turnTorque, ForceMode.Acceleration);
    }

    /// <summary>
    /// Handles collision with objects tagged as "Terrain".
    /// Plays a random collision sound and applies bounce force.
    /// </summary>
    /// <param name="collision">Collision information.</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Terrain"))
        {
            // Play a random collision sound if the list is not empty
            if (collisionSounds.Count > 0 && audioSource != null)
            {
                AudioClip randomClip = collisionSounds[Random.Range(0, collisionSounds.Count)];
                float volume = rb.velocity.magnitude / speed;
                audioSource.PlayOneShot(randomClip, volume * collisionVolumeMultiplier);
            }
        }
    }
}