using UnityEngine;

public class Target : MonoBehaviour
{
    private Rigidbody targetRb;
    private float minSpeed = 12f;
    private float maxSpeed = 16f;
    private float maxTorque = 0.5f;
    private float xRange = 4f;
    private float ySpawnPos = -2f;
    
    private GameManager gameManager;
    public int pointValue;
    public ParticleSystem explosionParticle;
    public AudioClip hitSound;

    void Start()
    {
        targetRb = GetComponent<Rigidbody>();
        targetRb.AddForce(RandomForce(), ForceMode.Impulse);
        targetRb.AddTorque(RandomTorque(), RandomTorque(), RandomTorque(), ForceMode.Impulse);
        transform.position = RandomSpawnPos();
        
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void OnMouseDown()
    {
        if (gameManager.isGameActive)
        {
            AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position);
            Destroy(gameObject);
            gameManager.UpdateScore(pointValue);
            Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
    if (other.gameObject.CompareTag("PowerUp")) return; // Ignore power-ups

    Destroy(gameObject);

    if (!gameObject.CompareTag("Bad"))
    {
        gameManager.GameOver();
    }
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
