using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public static ScoreUI instance;

    public int points = 0;
    public TextMeshProUGUI scoreText;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ResetPoints(); // starts at 0 automatically
    }

    public void AddPoint()
    {
        points++;
        UpdateScoreText();
    }

    public void ResetPoints()
    {
        points = 0;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText == null)
        {
            Debug.LogError("ScoreUI ERROR: scoreText is not assigned in Inspector!");
            return;
        }

        scoreText.text = "Points: " + points;
    }
}