using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    // this is your in-game controls menu (the panel that pops up)
    public GameObject controlsPanel;

    // this is the pause icon (the || in the middle of the screen)
    public GameObject pauseSymbol;

    // tracks whether the game is currently paused
    bool isPaused = false;

    void Start()
    {
        // hide the menu at the start of the game
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        // hide the pause icon at the start
        if (pauseSymbol != null)
            pauseSymbol.SetActive(false);
    }

    void Update()
    {
        // P key = pause / unpause the game
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();   // if already paused → resume
            else
                PauseGame();    // if not paused → pause
        }

        // M key = open/close the controls menu
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            if (controlsPanel.activeSelf)
                CloseMenu();    // if menu open → close it
            else
                OpenMenu();     // if menu closed → open it
        }

        // ESC key = go back to main menu scene
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            GoToMenu();
        }
    }

    void PauseGame()
    {
        // mark game as paused
        isPaused = true;

        // show pause icon if it exists
        if (pauseSymbol != null)
            pauseSymbol.SetActive(true);

        // freeze the entire game (everything stops)
        Time.timeScale = 0f;
    }

    void ResumeGame()
    {
        // mark game as not paused
        isPaused = false;

        // hide pause icon
        if (pauseSymbol != null)
            pauseSymbol.SetActive(false);

        // resume normal time (game continues)
        Time.timeScale = 1f;
    }

    public void OpenMenu()
    {
        // show the controls menu
        controlsPanel.SetActive(true);

        // freeze gameplay while menu is open
        Time.timeScale = 0f;

        // also mark as paused
        isPaused = true;

        // hide pause icon so it doesn’t overlap menu
        if (pauseSymbol != null)
            pauseSymbol.SetActive(false);
    }

    public void CloseMenu()
    {
        // hide the controls menu
        controlsPanel.SetActive(false);

        // resume gameplay
        Time.timeScale = 1f;

        // mark as not paused
        isPaused = false;

        // make sure pause icon is off
        if (pauseSymbol != null)
            pauseSymbol.SetActive(false);
    }

    void GoToMenu()
    {
        // reset time so next scene isn't frozen
        Time.timeScale = 1f;

        // load your main menu scene (scene index 0)
        SceneManager.LoadScene(0);
    }
}