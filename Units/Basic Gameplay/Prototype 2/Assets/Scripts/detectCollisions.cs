using UnityEngine;

public class detectCollisions : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public int scoreCounter = 0;
    void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
        Destroy(other.gameObject);
        scoreCounter++;
        Debug.Log("Score: " + scoreCounter);
    }


}