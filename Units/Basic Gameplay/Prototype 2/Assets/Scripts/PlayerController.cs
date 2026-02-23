using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float horizontalInput;
    [SerializeField] private float speed = 20.0f;
    [SerializeField] private GameObject projectilePrefab;
    public float xRange = 20.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < -xRange)
        {
            transform.position = new Vector3(-xRange, transform.position.y, transform.position.z);
        }
        else if (transform.position.x > xRange)
        {
            transform.position = new Vector3(xRange, transform.position.y, transform.position.z);
        }
        horizontalInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * Time.deltaTime * speed * horizontalInput);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate (projectilePrefab, transform.position, projectilePrefab.transform.rotation);
        }
    }
}
