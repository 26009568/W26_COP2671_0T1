using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Private Vars
   private float speed = 10;
   
   private float turnSpeed = 100f;

   private  float horizontalInput;
  private float forwardInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        //This is where we get player movement
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");
        //We'll move the vehicle forward
        transform.Translate(Vector3.forward * Time.deltaTime * speed * forwardInput);
        //We turn the Vechile
         transform.Rotate(Vector3.up, Time.deltaTime * turnSpeed * horizontalInput);
    }
}
