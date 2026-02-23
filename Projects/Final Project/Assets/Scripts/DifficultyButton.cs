using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the difficulty button behavior. On click, it sets the game's difficulty and starts the game.
/// </summary>
public class DifficultyButton : MonoBehaviour
{
    private Button button;              // Reference to the button component
    private GameManager gameManager;    // Reference to the GameManager
    public int difficulty;             // Difficulty level (used to modify spawn rate)

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(SetDifficulty);
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    /// <summary>
    /// Called when the difficulty button is clicked. Logs the button name and starts the game with the specified difficulty.
    /// </summary>
    public void SetDifficulty()
    {
        gameManager.StartGame(difficulty);
    }
}
