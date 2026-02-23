using UnityEngine;

/// <summary>
/// Manages background switching when the game starts after a difficulty selection.
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the initial background (Background1).
    /// This will be disabled when the game starts.
    /// </summary>
    [SerializeField] private GameObject background1;

    /// <summary>
    /// Reference to the secondary background (Background2).
    /// This will be enabled when the game starts.
    /// </summary>
    [SerializeField] private GameObject background2;

    /// <summary>
    /// Called when the player selects a difficulty.
    /// It switches from Background1 to Background2.
    /// </summary>
    public void ChangeBackground()
    {
        // Ensure Background2 is not null before enabling it
        if (background2 != null)
        {
            background2.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Background2 is not assigned in the inspector.");
        }

        // Ensure Background1 is not null before disabling it
        if (background1 != null)
        {
            background1.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Background1 is not assigned in the inspector.");
        }
    }
}
