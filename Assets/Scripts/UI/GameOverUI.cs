using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    // this lets other scripts easily access this UI (like a global reference)
    public static GameOverUI instance;

    // this is the actual UI panel shown when the player dies
    public GameObject panel;

    void Awake()
    {
        // store this object so other scripts can call GameOverUI.instance
        instance = this;
    }

    void Start()
    {
        // when the game starts, make sure the game over panel is hidden
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void Show(Vector3 worldPosition)
    {
        // safety check in case panel wasn't assigned in Unity
        if (panel == null)
        {
            Debug.LogError("GameOverUI: panel is null.");
            return;
        }

        // show the game over panel on screen
        panel.SetActive(true);
    }
}