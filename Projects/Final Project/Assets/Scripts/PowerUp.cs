using UnityEngine;

/// <summary>
/// PowerUp component that notifies the GameManager when clicked.
/// </summary>
public class PowerUp : MonoBehaviour
{
    private GameManager gameManager; // Reference to the GameManager

    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    /// <summary>
    /// When the power-up is clicked, we tell the GameManager to activate the power-up.
    /// </summary>
    private void OnMouseDown()
    {
        gameManager.ActivatePowerUp();
    }
}
