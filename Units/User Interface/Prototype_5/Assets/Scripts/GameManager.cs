using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool isGameActive;
    //List is more useful here for dynamic sizing
    public List<GameObject> targets;

    private bool hasPressedEnter = false;
    private float spawnRate = 1.0f;
    private int score;

    [Header("Game Settings")]
    [SerializeField] private float startGameTime = 60f;   // starting time each round
    private float gameTime;                               // current time
    private int highScore = 0;

    [Header("UI (TMP)")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI highScoreText;

    [Header("UI (Menus/Objects)")]
    public Button restartButton;
    public Button resumeButton;
    public Button quitButton;
    public GameObject titleScreen;
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject pressEnterText;
    public GameObject difficultyButtonsPanel;

    //[Header("Audio")]
    public AudioSource backgroundMusic;
    public Slider musicSlider;
    public AudioClip buttonSound;

    //[Header("Power Ups")]
    public GameObject powerUpPrefab;
    private GameObject activePowerUp;
    [SerializeField] private float powerUpDuration = 5f;
    private bool canSpawnPowerUp = true;
    private int nextPowerUpScore = 50;

    [Header("Spawn Control")]
    private readonly List<Vector3> recentSpawnPositions = new List<Vector3>();
    [SerializeField] private float minSpawnDistance = 1.5f;

    [Header("Crosshair/Gun UI")]
    public Texture2D crosshairTexture;
    public RectTransform gunImage;

    private bool isPaused = false;

    void Start()
    {
        // Make the game safe even if you forgot to assign something in the Inspector.
        SafeSetActive(pressEnterText, true, "pressEnterText");
        SafeSetActive(difficultyButtonsPanel, false, "difficultyButtonsPanel");
        SafeSetActive(pauseMenu, false, "pauseMenu");
        SafeSetActive(settingsMenu, false, "settingsMenu");

        SafeSetTMPActive(timerText, false, "timerText");
        SafeSetTMPActive(gameOverText, false, "gameOverText");
        SafeSetTMPActive(highScoreText, false, "highScoreText");
        SafeSetTMPActive(scoreText, false, "scoreText");

        if (restartButton != null) restartButton.gameObject.SetActive(false);

        // Music
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = 1.0f;
            if (!backgroundMusic.isPlaying) backgroundMusic.Play();
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = 1.0f;
            if (backgroundMusic != null) backgroundMusic.volume = 1.0f;
            musicSlider.onValueChanged.AddListener(AdjustMusicVolume);
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        gameTime = startGameTime;

        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore;
        }
    }

    void Update()
    {
        // Press Enter flow
        if (!hasPressedEnter && Input.GetKeyDown(KeyCode.Return))
        {
            hasPressedEnter = true;
            SafeSetActive(pressEnterText, false, "pressEnterText");
            SafeSetActive(difficultyButtonsPanel, false, "difficultyButtonsPanel");
            StartGame(1); // default difficulty
        }

        if (!isGameActive) return;

        // Escape key flow
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsMenu != null && settingsMenu.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                TogglePause();
            }
        }

        // Move gun image with mouse (only if assigned)
        if (gunImage != null)
        {
            Vector3 mousePosition = Input.mousePosition;
            gunImage.position = new Vector3(mousePosition.x, gunImage.position.y, gunImage.position.z);
        }
    }

    public void StartGame(int difficulty)
    {
        // Defensive checks
        if (targets == null || targets.Count == 0)
        {
            Debug.LogError("Targets list is empty on GameManager. Add target prefabs to the list.");
            return;
        }

        if (difficulty <= 0) difficulty = 1;

        spawnRate = 1.0f;           // reset spawn rate each new game
        spawnRate /= difficulty;

        isGameActive = true;
        isPaused = false;
        Time.timeScale = 1f;

        score = 0;
        nextPowerUpScore = 50;
        canSpawnPowerUp = true;

        // reset timer every round
        gameTime = startGameTime;

        UpdateScore(0);

        if (restartButton != null) restartButton.gameObject.SetActive(false);
        SafeSetActive(titleScreen, false, "titleScreen");
        SafeSetTMPActive(timerText, true, "timerText");
        SafeSetTMPActive(scoreText, true, "scoreText");
        SafeSetTMPActive(gameOverText, false, "gameOverText");
        SafeSetTMPActive(highScoreText, false, "highScoreText");
        SafeSetActive(pauseMenu, false, "pauseMenu");
        SafeSetActive(settingsMenu, false, "settingsMenu");

        if (crosshairTexture != null)
        {
            Cursor.SetCursor(
                crosshairTexture,
                new Vector2(crosshairTexture.width / 2f, crosshairTexture.height / 2f),
                CursorMode.Auto
            );
        }

        StartCoroutine(SpawnTarget());
        StartCoroutine(GameTimer());
    }

    public void GameOver()
    {
        isGameActive = false;
        isPaused = false;

        // Reset time scale in case slow-motion or pause was active
        Time.timeScale = 1f;

        if (restartButton != null) restartButton.gameObject.SetActive(true);
        SafeSetTMPActive(gameOverText, true, "gameOverText");
        SafeSetActive(pauseMenu, false, "pauseMenu");
        SafeSetActive(settingsMenu, false, "settingsMenu");

        // Save high score
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore;
            highScoreText.gameObject.SetActive(true);
        }

        // Cleanup power-up if one is active
        if (activePowerUp != null)
        {
            Destroy(activePowerUp);
            activePowerUp = null;
        }
        canSpawnPowerUp = true;
    }

    IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);

            Vector3 spawnPosition = GetValidSpawnPosition();
            int index = Random.Range(0, targets.Count);
            Instantiate(targets[index], spawnPosition, Quaternion.identity);

            if (score >= nextPowerUpScore && canSpawnPowerUp && activePowerUp == null && powerUpPrefab != null)
            {
                SpawnPowerUp();
                nextPowerUpScore += 50;
            }
        }
    }

    private Vector3 GetValidSpawnPosition()
    {
        Vector3 newSpawnPosition;
        bool positionValid;
        int maxAttempts = 10;
        int attempts = 0;

        do
        {
            positionValid = true;
            newSpawnPosition = new Vector3(Random.Range(-4f, 4f), -2f, 0f);

            foreach (Vector3 recentPosition in recentSpawnPositions)
            {
                if (Vector3.Distance(newSpawnPosition, recentPosition) < minSpawnDistance)
                {
                    positionValid = false;
                    break;
                }
            }

            attempts++;
            if (attempts >= maxAttempts) break;
        }
        while (!positionValid);

        recentSpawnPositions.Add(newSpawnPosition);
        if (recentSpawnPositions.Count > 5) recentSpawnPositions.RemoveAt(0);

        return newSpawnPosition;
    }

    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    private void SpawnPowerUp()
    {
        if (powerUpPrefab == null) return;

        Vector3 spawnPosition = GetPowerUpSpawnPosition();

        activePowerUp = Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
        activePowerUp.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        canSpawnPowerUp = false;
        StartCoroutine(RemovePowerUpAfterTime());
    }

    private Vector3 GetPowerUpSpawnPosition()
    {
        Vector3 spawnPosition;
        bool positionValid;
        int maxAttempts = 10;
        int attempts = 0;

        // If gunImage isn't assigned, use a safe default
        float minY = (gunImage != null) ? gunImage.position.y + 1.5f : 0.5f;
        float maxY = 4f;

        do
        {
            positionValid = true;
            spawnPosition = new Vector3(Random.Range(-4f, 4f), Random.Range(minY, maxY), 0f);

            foreach (Vector3 recentPosition in recentSpawnPositions)
            {
                if (Vector3.Distance(spawnPosition, recentPosition) < minSpawnDistance)
                {
                    positionValid = false;
                    break;
                }
            }

            attempts++;
            if (attempts >= maxAttempts) break;
        }
        while (!positionValid);

        return spawnPosition;
    }

    IEnumerator RemovePowerUpAfterTime()
    {
        yield return new WaitForSeconds(powerUpDuration);

        if (activePowerUp != null)
        {
            Destroy(activePowerUp);
            activePowerUp = null;
        }

        canSpawnPowerUp = true;
    }

    public void ActivatePowerUp()
    {
        if (activePowerUp != null)
        {
            Destroy(activePowerUp);
            activePowerUp = null;
            StartCoroutine(SlowTime());
        }
    }

    IEnumerator SlowTime()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(powerUpDuration);
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator GameTimer()
    {
        // Ensure timerText exists before we try to write to it
        while (gameTime > 0f && isGameActive)
        {
            gameTime -= 1f;

            if (timerText != null)
                timerText.text = "Time: " + Mathf.CeilToInt(gameTime);

            yield return new WaitForSeconds(1f);
        }

        if (isGameActive) GameOver();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenu != null) pauseMenu.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenu != null) pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(false);
    }

    public void AdjustMusicVolume(float volume)
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = volume;
        }
    }

    public void PlayButtonSound()
    {
        if (buttonSound != null && backgroundMusic != null)
        {
            backgroundMusic.PlayOneShot(buttonSound);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ---------- Helpers ----------
    private void SafeSetActive(GameObject obj, bool value, string fieldName)
    {
        if (obj != null) obj.SetActive(value);
        else Debug.LogWarning($"{fieldName} is not assigned in the Inspector (GameManager).");
    }

    private void SafeSetTMPActive(TextMeshProUGUI tmp, bool value, string fieldName)
    {
        if (tmp != null) tmp.gameObject.SetActive(value);
        else Debug.LogWarning($"{fieldName} is not assigned in the Inspector (GameManager).");
    }
}