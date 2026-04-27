using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    // reference to the text on screen that will display the timer
    public TextMeshProUGUI timerText;

    // stores how much time has passed since the game started
    float time;

    void Update()
    {
        // add the time passed since last frame (makes it count smoothly in seconds)
        time += Time.deltaTime;

        // convert total time into minutes
        int minutes = Mathf.FloorToInt(time / 60);

        // get remaining seconds after minutes are removed
        int seconds = Mathf.FloorToInt(time % 60);

        // update the UI text
        // seconds.ToString("00") makes sure it shows like 01, 02, 03 instead of 1, 2, 3
        timerText.text = minutes + ":" + seconds.ToString("00");
    }
}