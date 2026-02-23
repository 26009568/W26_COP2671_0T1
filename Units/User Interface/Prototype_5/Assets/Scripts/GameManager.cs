using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public bool isGameActive;
    public List<GameObject> targets;
    
    private float spawnRate = 1.0f;
    private int score;
    private float gameTime = 60f; // Game duration in seconds
    private int highScore = 0;
    
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI highScoreText;
    
    public Button restartButton;
    public Button resumeButton;
    public Button quitButton;
    public GameObject titleScreen;
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject pressEnterText;
    public GameObject difficultyButtonsPanel;
    public AudioSource backgroundMusic;
    public Slider musicSlider;
    
    private bool isPaused = false;
    public AudioClip buttonSound;
    
    public GameObject powerUpPrefab;
    private GameObject activePowerUp;
    private float powerUpDuration = 5f;
    private bool canSpawnPowerUp = true;
    private int nextPowerUpScore = 50;
    private List<Vector3> recentSpawnPositions = new List<Vector3>();
    private float minSpawnDistance = 1.5f;
    public Texture2D crosshairTexture;
    public RectTransform gunImage;
    

    void Start()
    {
        pressEnterText.SetActive(true);
        difficultyButtonsPanel.SetActive(false);
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        timerText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        highScoreText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);

        if (backgroundMusic != null)
        {
            backgroundMusic.volume = 1.0f;
            backgroundMusic.Play();
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = 1.0f;
            backgroundMusic.volume = 1.0f; 
            musicSlider.onValueChanged.AddListener(AdjustMusicVolume);
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }
    
   void Update()
{
    if (!hasPressedEnter && Input.GetKeyDown(KeyCode.Return))
    {
        hasPressedEnter = true;
        pressEnterText.SetActive(false);
        difficultyButtonsPanel.SetActive(true);
    }

    if (!isGameActive) return; // Only process input when the game is active

    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (settingsMenu.activeSelf)
        {
            CloseSettings();
        }
        else
        {
            TogglePause();
        }
    }

    // Move gun image horizontally based on mouse position
    Vector3 mousePosition = Input.mousePosition;
    gunImage.position = new Vector3(mousePosition.x, gunImage.position.y, gunImage.position.z);
}

    public void GameOver()
    {
    restartButton.gameObject.SetActive(true);
    gameOverText.gameObject.SetActive(true);
    isGameActive = false;
    pauseMenu.SetActive(false);

    // Reset time scale in case slow-motion was active
    Time.timeScale = 1f;

    if (score > highScore)
    {
        highScore = score;
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    highScoreText.text = "High Score: " + highScore;
    highScoreText.gameObject.SetActive(true);
    }
    IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);
            
            Vector3 spawnPosition = GetValidSpawnPosition();
            int index = Random.Range(0, targets.Count);
            Instantiate(targets[index], spawnPosition, Quaternion.identity);
            
            if (score >= nextPowerUpScore && canSpawnPowerUp && activePowerUp == null)
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
    int maxAttempts = 10; // Prevent infinite loops
    int attempts = 0;

    do
    {
        positionValid = true;
        newSpawnPosition = new Vector3(Random.Range(-4f, 4f), -2, 0);

        foreach (Vector3 recentPosition in recentSpawnPositions)
        {
            if (Vector3.Distance(newSpawnPosition, recentPosition) < minSpawnDistance)
            {
                positionValid = false;
                break;
            }
        }

        attempts++;
        if (attempts >= maxAttempts)
        {
            break; // Prevent infinite loop if no valid position is found
        }
    }
    while (!positionValid);

    recentSpawnPositions.Add(newSpawnPosition);
    if (recentSpawnPositions.Count > 5)
    {
        recentSpawnPositions.RemoveAt(0);
    }

    return newSpawnPosition;
}

    
    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;
    }
    
    
    private void SpawnPowerUp()
{
    Vector3 spawnPosition = GetPowerUpSpawnPosition(); // New method for better positioning

    activePowerUp = Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
    activePowerUp.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    canSpawnPowerUp = false;

    StartCoroutine(RemovePowerUpAfterTime());
}
private Vector3 GetPowerUpSpawnPosition()
{
    Vector3 spawnPosition;
    bool positionValid;
    int maxAttempts = 10; // Prevent infinite loop
    int attempts = 0;

    float minY = gunImage.position.y + 1.5f; // Ensure power-ups spawn above the gun
    float maxY = 4f; // Adjust this based on your game area

    do
    {
        positionValid = true;
        spawnPosition = new Vector3(Random.Range(-4f, 4f), Random.Range(minY, maxY), 0);

        foreach (Vector3 recentPosition in recentSpawnPositions)
        {
            if (Vector3.Distance(spawnPosition, recentPosition) < minSpawnDistance)
            {
                positionValid = false;
                break;
            }
        }

        attempts++;
        if (attempts >= maxAttempts)
        {
            break; // Stop if no valid position is found after multiple attempts
        }
    }
    while (!positionValid);

    return spawnPosition;
}

    public void RestartGame()
    {   
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void StartGame(int difficulty)
    {
        spawnRate /= difficulty;
        isGameActive = true;
        
        StartCoroutine(SpawnTarget());
        StartCoroutine(GameTimer());
        
        score = 0;
        nextPowerUpScore = 50;
        UpdateScore(0);
        
        titleScreen.gameObject.SetActive(false);
        timerText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
        Cursor.SetCursor(crosshairTexture, new Vector2(crosshairTexture.width / 2, crosshairTexture.height / 2), CursorMode.Auto);
    }
    
    IEnumerator GameTimer()
    {
        while (gameTime > 0 && isGameActive)
        {
            gameTime--;
            timerText.text = "Time: " + gameTime;
            yield return new WaitForSeconds(1f);
        }
        GameOver();
    }
    
    IEnumerator RemovePowerUpAfterTime()
    {
        yield return new WaitForSeconds(powerUpDuration);
        if (activePowerUp != null)
        {
            Destroy(activePowerUp);
        }
        canSpawnPowerUp = true;
    }
    public void ActivatePowerUp()
    {
        if (activePowerUp != null)
        {
            Destroy(activePowerUp);
            StartCoroutine(SlowTime());
        }
    }
    
    IEnumerator SlowTime()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(powerUpDuration);
        Time.timeScale = 1f;
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    
    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
    }
    
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
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
}
