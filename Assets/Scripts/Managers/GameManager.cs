using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance 
    { 
        get 
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
            }
            return _instance;
        }
    }

    [Header("Events")]
    public UnityEvent<string> onSceneLoadStarted = new UnityEvent<string>();
    public UnityEvent<string> onSceneLoadFinished = new UnityEvent<string>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void LoadMainMenu()
    {
        LoadScene("MainMenu");
    }

    public void LoadLevel(string sceneName)
    {
        LoadScene(sceneName);
    }

    private void LoadScene(string sceneName)
    {
        Debug.Log($"[GameManager] Starting load of scene: {sceneName}");
        onSceneLoadStarted?.Invoke(sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void LevelComplete()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[GameManager] Level Complete: {currentSceneName}");

        // Attempt to find the next level based on the "LevelN" naming convention
        if (currentSceneName.StartsWith("Level"))
        {
            string numberPart = currentSceneName.Substring(5); // Skip "Level" prefix
            if (int.TryParse(numberPart, out int currentLevelNumber))
            {
                int nextLevelNumber = currentLevelNumber + 1;
                string nextLevelName = "Level" + nextLevelNumber;

                // Check if the next level exists in the build settings
                if (Application.CanStreamedLevelBeLoaded(nextLevelName))
                {
                    Debug.Log($"[GameManager] Loading next level: {nextLevelName}");
                    LoadLevel(nextLevelName);
                    return;
                }
            }
        }

        // If no next level is found, or naming convention is not met, return to main menu
        Debug.Log("[GameManager] No next level found or naming convention not met. Returning to Main Menu.");
        LoadMainMenu();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] Finished loading scene: {scene.name}");
        onSceneLoadFinished?.Invoke(scene.name);
    }
}
