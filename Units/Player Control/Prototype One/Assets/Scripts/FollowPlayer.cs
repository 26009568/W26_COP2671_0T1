using UnityEngine;

public class FollowPlayer1 : MonoBehaviour
{

    public GameObject player;
    private Vector3 offset = new Vector3(0, 5, -7);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    // //Updating after update method happens
    void LateUpdate()
    {
        transform.position = player.transform.position + offset;
    }
}
