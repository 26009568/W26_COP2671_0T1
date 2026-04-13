using UnityEngine;
using Platformer.Mechanics;

public class MainMenu : MonoBehaviour
{
    public GameObject menuUI;
    public PlayerController player;

    void Start()
    {
        Time.timeScale = 0f;

        if (menuUI != null)
        {
            menuUI.SetActive(true);
        }

        if (player != null)
        {
            player.enabled = false;
        }
    }

    public void StartGame()
    {
        Debug.Log("START BUTTON CLICKED");

        Time.timeScale = 1f;

        if (menuUI != null)
        {
            menuUI.SetActive(false);
        }

        if (player != null)
        {
            player.enabled = true;
        }
    }
}