using UnityEngine;

public class Target : MonoBehaviour
{
    private Rigidbody targetRb;

    [Header("Movement Settings")]
    private float minSpeed = 12f;
    private float maxSpeed = 16f;
    private float maxTorque = 0.5f;
    private float xRange = 4f;
    private float ySpawnPos = -2f;

    private GameManager gameManager;

    [Header("Scoring & Effects")]
    public int pointValue;
    public ParticleSystem explosionParticle;
    public AudioClip hitSound;

    void Start()
    {
        // Get Rigidbody safely
        targetRb = GetComponent<Rigidbody>();

        if (targetRb == null)
        {
            Debug.LogError("Target needs a 3D Rigidbody on the same GameObject.", this);
            enabled = false;
            return;
        }

        // Find GameManager safely (no name dependency)
        gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("No GameManager found in scene.", this);
            enabled = false;
            return;
        }

        // Apply movement
        targetRb.AddForce(RandomForce(), ForceMode.Impulse);
        targetRb.AddTorque(RandomTorque(), RandomTorque(), RandomTorque(), ForceMode.Impulse);

        // Set spawn position
        transform.position = RandomSpawnPos();
    }

    private void OnMouseDown()
    {
        if (gameManager != null && gameManager.isGameActive)
        {
            if (hitSound != null && Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position);
            }

            if (explosionParticle != null)
            {
                Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);
            }

            gameManager.UpdateScore(pointValue);
            Destroy(gameObject);
        }
    }

  private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PowerUp")) return;

        if (gameManager != null && !gameObject.CompareTag("Bad"))
        {
            gameManager.GameOver();
        }

        Destroy(gameObject);
    }

    private Vector3 RandomForce()
    {
        return Vector3.up * Random.Range(minSpeed, maxSpeed);
    }

    private float RandomTorque()
    {
        return Random.Range(-maxTorque, maxTorque);
    }

    private Vector3 RandomSpawnPos()
    {
        return new Vector3(Random.Range(-xRange, xRange), ySpawnPos, 0);
    }
}