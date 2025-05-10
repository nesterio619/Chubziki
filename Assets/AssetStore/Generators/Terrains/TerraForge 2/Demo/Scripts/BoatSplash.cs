using UnityEngine;

/// <summary>
/// Controls the emission of splash particles based on the boat's velocity.
/// </summary>
public class BoatSplash : MonoBehaviour
{
    /// <summary>
    /// Reference to the splash particle system.
    /// </summary>
    [Tooltip("Reference to the splash particle system.")]
    public ParticleSystem splashParticles;

    /// <summary>
    /// Velocity threshold for emitting particles.
    /// </summary>
    [Tooltip("Velocity threshold for emitting particles.")]
    public float splashVelocityThreshold = 1f;

    /// <summary>
    /// Reference to the boat's Rigidbody component.
    /// </summary>
    private Rigidbody boatRigidbody;

    /// <summary>
    /// Gets the Rigidbody component attached to the boat GameObject.
    /// </summary>
    private void Start()
    {
        // Get the Rigidbody component attached to the boat GameObject
        boatRigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Checks the boat's velocity and controls the splash particles accordingly.
    /// </summary>
    private void Update()
    {
        // Check if the boat's velocity magnitude exceeds the threshold
        if (boatRigidbody.velocity.magnitude >= splashVelocityThreshold && !splashParticles.isPlaying)
        {
            // Play splash particles
            splashParticles.Play();
        }
        else if (boatRigidbody.velocity.magnitude < splashVelocityThreshold && splashParticles.isPlaying)
        {
            // Stop splash particles
            splashParticles.Stop();
        }
    }
}