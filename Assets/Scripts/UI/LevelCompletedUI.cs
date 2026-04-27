using UnityEngine;

public class LevelCompletedUI : MonoBehaviour
{
    public static LevelCompletedUI instance;
    public GameObject levelCompleteText;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        levelCompleteText.SetActive(false);
    }

    public void Show()
    {
        levelCompleteText.SetActive(true);
        Time.timeScale = 0f;
    }
}