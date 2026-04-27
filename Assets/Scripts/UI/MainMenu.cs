using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the main menu UI interactions, specifically starting the game and quitting.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Reference to the button that starts the first level.")]
    [SerializeField] private Button startGameButton;

    [Tooltip("Reference to the button that exits the application.")]
    [SerializeField] private Button quitGameButton;

    private void OnEnable()
    {
        // Subscribe to button click events
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(HandleStartGame);
        }

        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(HandleQuitGame);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks or errors when object is disabled
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveListener(HandleStartGame);
        }

        if (quitGameButton != null)
        {
            quitGameButton.onClick.RemoveListener(HandleQuitGame);
        }
    }

    /// <summary>
    /// Loads the first level defined in the GameManager's level list.
    /// </summary>
    private void HandleStartGame()
    {
        if (GameManager.Instance != null)
        {
            string firstLevel = "Level1";
            if (Application.CanStreamedLevelBeLoaded(firstLevel))
            {
                Debug.Log("[MainMenu] Start Game button clicked. Loading first level: " + firstLevel);
                GameManager.Instance.LoadLevel(firstLevel);
            }
            else
            {
                Debug.LogError($"[MainMenu] Cannot start game: {firstLevel} not found in build settings! Ensure your first level is named 'Level1' and added to the build settings.");
            }
        }
        else
        {
            Debug.LogError("[MainMenu] GameManager instance not found in the scene!");
        }
    }

    /// <summary>
    /// Closes the application.
    /// </summary>
    private void HandleQuitGame()
    {
        Debug.Log("[MainMenu] Quit Game button clicked. Closing application...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
