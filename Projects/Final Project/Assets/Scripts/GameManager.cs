using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the overall game flow: spawning targets, tracking score, handling power-ups, 
/// and controlling the UI (menus, timer, game over screen, etc.).
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public bool isGameActive;                  // Is the game currently active?
    private bool isPaused = false;             // Is the game currently paused?
    
    [Header("Target Management")]
    public List<GameObject> targets;           // List of available targets to spawn
    private float spawnRate = 1.0f;            // Rate at which targets spawn
    private float minSpawnDistance = 1.5f;     // Minimum distance between spawn positions
    private List<Vector3> recentSpawnPositions = new List<Vector3>(); // Recent spawn positions to avoid overlapping
    
    [Header("Scoring")]
    private int score;                         // Current game score
    private int nextPowerUpScore = 100;         // Score threshold for the next power-up
    
    [Header("Game Timer")]
    private float gameTime = 60f;              // Game duration in seconds
    
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject pressEnterText;                   // "Press Enter to Start" message
    public GameObject difficultyButtonsPanel;           // Container for Easy/Medium/Hard buttons
    private bool hasPressedEnter = false;               // Has the player already pressed Enter?
    public TextMeshProUGUI scoreboardTitleText;         // "Top Scores" label
    public TextMeshProUGUI scoreboardListText;          // List of top scores

    private List<int> scoreHistory = new List<int>();   // Runtime list of scores
    private const int maxScores = 5;                    // Top 5 scores tracked


    
    [Header("Menu Buttons")]
    public Button restartButton;
    public Button resumeButton;
    public Button quitButton;
    
    [Header("Screen/Canvas Elements")]
    public GameObject titleScreen;
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    
    [Header("Audio")]
    public AudioSource sfxSource;              // Sound effects
    public AudioSource backgroundMusic;        // Background music source
    public Slider musicSlider;                 // Slider to adjust music volume
    public AudioClip buttonSound;              // Sound to play when buttons are clicked    

    
    [Header("Power-Up")]
    public GameObject powerUpPrefab;           // The power-up object to spawn
    private GameObject activePowerUp;          // Reference to any currently active power-up
    private float powerUpDuration = 5f;        // Duration of the power-up effect
    private bool canSpawnPowerUp = true;       // Can we spawn a new power-up?
    
    [Header("Crosshair/Gun")]
    public Texture2D crosshairTexture;
    public RectTransform gunImage;             // UI image that moves with the mouse
    
    // -------------------------------------------------------------------
    //  UNITY LIFECYCLE METHODS
    // -------------------------------------------------------------------
    
    void Start()
    {
        // Show the "Press Enter" prompt and hide the difficulty buttons until Enter is pressed
        pressEnterText.SetActive(true);
        difficultyButtonsPanel.SetActive(false);

        // Ensure pause & settings menus are hidden initially
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        
        // Hide UI elements until the game is started
        timerText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        gunImage.gameObject.SetActive(false);


        // Set up background music
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = 1.0f;
            backgroundMusic.Play();
        }

        // Set up music slider
        if (musicSlider != null)
        {
            // Clear any existing listeners, set default volume to 1, then re-add listener
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = 1.0f;
            backgroundMusic.volume = 1.0f; 
            musicSlider.onValueChanged.AddListener(AdjustMusicVolume);
        }

        // Load previous scores
        scoreHistory.Clear();
        for (int i = 0; i < maxScores; i++)
        {
            if (PlayerPrefs.HasKey("Score_" + i))
            {
                scoreHistory.Add(PlayerPrefs.GetInt("Score_" + i));
            }
        }

        
    }
    
    void Update()
    {
        // Check for initial Enter press to begin showing the difficulty options
        if (!hasPressedEnter && Input.GetKeyDown(KeyCode.Return))
        {
            hasPressedEnter = true;
            // Hide the "Press Enter" text and show the difficulty buttons
            pressEnterText.SetActive(false);
            difficultyButtonsPanel.SetActive(true);
        }

        // Only process input if the game is active
        if (!isGameActive) return;

        // Check for Escape key (pause or close settings)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If we're in settings, close settings; otherwise toggle the pause menu
            if (settingsMenu.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                TogglePause();
            }
        }

        // Move the gun image horizontally based on the mouse's X position
        Vector3 mousePosition = Input.mousePosition;
        gunImage.position = new Vector3(mousePosition.x, gunImage.position.y, gunImage.position.z);
        
    }
    
    // -------------------------------------------------------------------
    //  GAME FLOW
    // -------------------------------------------------------------------
    
    /// <summary>
    /// Handles the end of the game, displaying the Game Over screen and resetting game state.
    /// </summary>
    public void GameOver()
    {
        // Show the restart button and "Game Over" text
        restartButton.gameObject.SetActive(true);
        
        // Deactivate the game and hide the pause menu
        isGameActive = false;
        pauseMenu.SetActive(false);

        // Reset time scale in case a power-up was slowing time
        Time.timeScale = 1f;

        // Add current score to score history and sort
        scoreHistory.Add(score);
        scoreHistory.Sort((a, b) => b.CompareTo(a)); // Sort descending

        // Keep only the top N scores
        if (scoreHistory.Count > maxScores)
        {
            scoreHistory = scoreHistory.GetRange(0, maxScores);
        }

        // Save to PlayerPrefs
        for (int i = 0; i < scoreHistory.Count; i++)
        {
            PlayerPrefs.SetInt("Score_" + i, scoreHistory[i]);
        }
        PlayerPrefs.Save();

        // Show scoreboard
        DisplayScoreboard();


    }
    /// <summary>
    /// Displays the top scores on the screen after the game ends. 
    /// Activates the scoreboard title and list UI, and populates the list with 
    /// up to the top 5 highest scores stored in memory (and persisted via PlayerPrefs).
    /// </summary>
    private void DisplayScoreboard()
    {
        // Activate scoreboard UI elements
        scoreboardTitleText.gameObject.SetActive(true);
        scoreboardListText.gameObject.SetActive(true);

        // Format the score list as numbered entries
        string scoreList = "";
        for (int i = 0; i < scoreHistory.Count; i++)
        {
            scoreList += (i + 1) + ". " + scoreHistory[i] + "\n";
        }

        // Update the UI text
        scoreboardListText.text = scoreList;
    }

    /// <summary>
    /// Begins spawning targets while the game is active, at intervals controlled by spawnRate.
    /// Also handles spawning power-ups based on the player's score.
    /// </summary>
    private IEnumerator SpawnTarget()
    {
        while (isGameActive)
        {
            // Wait for the specified spawn interval
            yield return new WaitForSeconds(spawnRate);
            
            // Spawn a target at a valid position
            Vector3 spawnPosition = GetValidSpawnPosition();
            int index = Random.Range(0, targets.Count);
            Instantiate(targets[index], spawnPosition, Quaternion.identity);
            
            // Check if we can spawn a power-up
            if (score >= nextPowerUpScore && canSpawnPowerUp && activePowerUp == null)
            {
                SpawnPowerUp();
                nextPowerUpScore += 100; // Increase threshold for next power-up
            }
        }
    }

    /// <summary>
    /// Ensures targets are not spawned too close together by checking recent positions.
    /// </summary>
    private Vector3 GetValidSpawnPosition()
    {
        Vector3 newSpawnPosition;
        bool positionValid;
        int maxAttempts = 10; // Avoid infinite loops
        int attempts = 0;

        do
        {
            positionValid = true;
            newSpawnPosition = new Vector3(Random.Range(-4f, 4f), -2, 0);

            // Compare distance with recently used positions
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
                // Stop if we can't find a valid position
                break;
            }
        }
        while (!positionValid);

        // Update our list of recent positions
        recentSpawnPositions.Add(newSpawnPosition);
        if (recentSpawnPositions.Count > 5)
        {
            recentSpawnPositions.RemoveAt(0);
        }

        return newSpawnPosition;
    }

    /// <summary>
    /// Updates the score and displays it on the scoreText UI.
    /// </summary>
    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
        scoreText.text = "Score: " + score;
    }

    // -------------------------------------------------------------------
    //  POWER-UPS
    // -------------------------------------------------------------------
    
    /// <summary>
    /// Spawns a power-up in a valid spawn position, and starts the timer for removal.
    /// </summary>
    private void SpawnPowerUp()
    {
        Vector3 spawnPosition = GetPowerUpSpawnPosition(); // Calculate a valid position
        
        activePowerUp = Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);
        activePowerUp.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        
        canSpawnPowerUp = false; // Disable further power-up spawns temporarily

        StartCoroutine(RemovePowerUpAfterTime());
    }

    /// <summary>
    /// Finds a suitable position for the power-up, avoiding recent spawn locations.
    /// </summary>
    private Vector3 GetPowerUpSpawnPosition()
    {
        Vector3 spawnPosition;
        bool positionValid;
        int maxAttempts = 10;
        int attempts = 0;

        do
        {
            positionValid = true;
            spawnPosition = new Vector3(Random.Range(-4f, 4f), Random.Range(-1f, 3f), 0);

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
                break;
            }
        }
        while (!positionValid);

        return spawnPosition;
    }

    /// <summary>
    /// Removes the power-up object after the specified duration, and allows the next one to spawn.
    /// </summary>
    private IEnumerator RemovePowerUpAfterTime()
    {
        yield return new WaitForSeconds(powerUpDuration);
        if (activePowerUp != null)
        {
            Destroy(activePowerUp);
        }
        canSpawnPowerUp = true;
    }

    /// <summary>
    /// When the player clicks the power-up, trigger its effect (slow down time).
    /// </summary>
    public void ActivatePowerUp()
    {
        if (activePowerUp != null)
        {
            Destroy(activePowerUp);
            StartCoroutine(SlowTime());
        }
    }

    /// <summary>
    /// Slows down time for the power-up duration, then returns it to normal.
    /// </summary>
    private IEnumerator SlowTime()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(powerUpDuration);
        Time.timeScale = 1f;
    }
    
    // -------------------------------------------------------------------
    //  GAME SETUP & RESTART
    // -------------------------------------------------------------------
    
    /// <summary>
    /// Restarts the current scene, effectively resetting the game.
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Starts the game with a given difficulty, which modifies the spawn rate of targets.
    /// </summary>
    public void StartGame(int difficulty)
    {
        // Adjust spawn rate based on difficulty
        spawnRate /= difficulty;
        
        // Mark the game as active and start relevant coroutines
        isGameActive = true;
        StartCoroutine(SpawnTarget());
        StartCoroutine(GameTimer());
        
        // Reset score and power-up threshold
        score = 0;
        nextPowerUpScore = 100;
        UpdateScore(0);
        
        // Hide the title screen and show the timer,score text,and gun image
        titleScreen.gameObject.SetActive(false);
        timerText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
        gunImage.gameObject.SetActive(true);

        // Set a crosshair cursor
        Cursor.SetCursor(crosshairTexture, new Vector2(crosshairTexture.width / 2, crosshairTexture.height / 2), CursorMode.Auto);
    }

    /// <summary>
    /// Controls the countdown timer for the game, then ends the game when time runs out.
    /// </summary>
    private IEnumerator GameTimer()
    {
        while (gameTime > 0 && isGameActive)
        {
            gameTime--;
            timerText.text = "Time: " + gameTime;
            yield return new WaitForSeconds(1f);
        }
        GameOver();
    }
    
    // -------------------------------------------------------------------
    //  MENU & SETTINGS
    // -------------------------------------------------------------------
    
    /// <summary>
    /// Toggles the pause menu on and off, and sets time scale accordingly
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0 : 1;
    }
    
    /// <summary>
    /// Resumes the game by hiding the pause menu and resetting time scale.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    
    /// <summary>
    /// Opens the settings menu.
    /// </summary>
    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
    }
    
    /// <summary>
    /// Closes the settings menu.
    /// </summary>
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
    }
    
    /// <summary>
    /// Adjusts the background music volume (tied to the UI slider).
    /// </summary>
    public void AdjustMusicVolume(float volume)
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.volume = volume;
        }
    }
    
    /// <summary>
    /// Plays a button click sound effect (if assigned).
    /// </summary>
    public void PlayButtonSound()
    {
        if (buttonSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(buttonSound);
        }
    }
    
    /// <summary>
    /// Quits the application. In the editor, this does nothing.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}