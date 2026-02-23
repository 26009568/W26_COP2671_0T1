using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void OnMouseDown()
    {
        gameManager.ActivatePowerUp();
    }
}
