using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // Required for loading and restarting scenes
using UnityEngine.InputSystem; // Required to read the ESC key

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI Panels")]
    public GameObject pausePanel;

    [Header("First Selected Button (For Controllers)")]
    public GameObject resumeButton;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu"; // Type your actual Main Menu scene name here in the Inspector!

    private bool isPaused = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Make sure the menu is hidden when the game starts
        pausePanel.SetActive(false);
    }

    void Update()
    {
        // Check if a keyboard is plugged in, then check if ESC was pressed this exact frame
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);

        // Freezes the game's physics, animations, and timers
        Time.timeScale = 0f;

        // CRITICAL FOR CONTROLLERS: Tell the EventSystem to highlight the Resume button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(resumeButton);
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        // Unfreezes the game
        Time.timeScale = 1f;
    }

    // ---> NEW: Restarts the current level <---
    public void RestartGame()
    {
        // ALWAYS unfreeze time before loading a scene!
        Time.timeScale = 1f;

        // This dynamically finds whatever scene you are currently playing and reloads it
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ---> NEW: Loads the Main Menu <---
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;

        // Loads the scene name you typed into the Inspector
        SceneManager.LoadScene(mainMenuSceneName);
    }
}