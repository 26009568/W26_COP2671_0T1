using UnityEngine;

/// <summary>
/// Target behavior: moves upward with a random force/torque and awards points when clicked.
/// Triggers Game Over if missed (unless it's marked as "Bad").
/// </summary>
public class Target : MonoBehaviour
{
    private Rigidbody targetRb;       // Rigidbody component on the target
    private float minSpeed = 12f;     // Minimum upward force
    private float maxSpeed = 16f;     // Maximum upward force
    private float maxTorque = 0.5f;   // Maximum random torque
    private float xRange = 4f;        // Horizontal spawn range
    private float ySpawnPos = -2f;    // Vertical spawn position
    
    private GameManager gameManager;  // Reference to the GameManager
    
    public int pointValue;            // How many points this target is worth
    public ParticleSystem explosionParticle; // Particle effect to play on hit
    public AudioClip hitSound;        // Sound to play on hit

    void Start()
    {
        // Set up components and initial forces
        transform.position = RandomSpawnPos();
        targetRb = GetComponent<Rigidbody>();
        targetRb.AddForce(RandomForce(), ForceMode.Impulse);
        targetRb.AddTorque(RandomTorque(), RandomTorque(), RandomTorque(), ForceMode.Impulse);
        
        // Cache the reference to GameManager
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    
    /// <summary>
    /// Called when the target is clicked. Awards points, plays sound and particle effect, then destroys the target.
    /// </summary>
    private void OnMouseDown()
    {
        if (gameManager.isGameActive)
        {
            // Play the hit sound at the camera's position
            AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position);
            
            // Destroy the target and update the score
            Destroy(gameObject);
            gameManager.UpdateScore(pointValue);
            
            // Spawn explosion particles
            Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);
        }
    }

    /// <summary>
    /// If the target leaves the screen (hits the trigger below), trigger Game Over if it's not a "Bad" target.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Ignore collisions with power-ups or borders
        if (other.gameObject.CompareTag("PowerUp") || other.gameObject.CompareTag("border")) return;

        Destroy(gameObject);

        // If it's not a bad target, the player loses
        if (!gameObject.CompareTag("Bad"))
        {
            gameManager.GameOver();
        }
    }


    /// <summary>
    /// Returns a random upward force vector within the specified speed range.
    /// </summary>
    private Vector3 RandomForce()
    {
        return Vector3.up * Random.Range(minSpeed, maxSpeed);
    }

    /// <summary>
    ///  Returns a random torque value within the specified range.
    /// </summary>
    private float RandomTorque()
    {
        return Random.Range(-maxTorque, maxTorque);
    }

    /// <summary>
    /// Returns a random spawn position along the horizontal range at the specified Y position.
    /// </summary>
    private Vector3 RandomSpawnPos()
    {
        return new Vector3(Random.Range(-xRange, xRange), ySpawnPos, 0);
    }
}
