using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public int points = 0;
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        scoreText.text = "Points: " + points;
    }

    public void AddPoints(int amount)
    {
        points += amount;
        scoreText.text = "Points: " + points;
    }
}