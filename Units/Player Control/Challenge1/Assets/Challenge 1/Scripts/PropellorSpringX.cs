using UnityEngine;

public class PropellorSpringX : MonoBehaviour
{
    public float rotateSpeed = 0.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      transform.Rotate(Vector3.forward * rotateSpeed );  
    }
}
