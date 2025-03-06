using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
#else
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuration")]
    private bool isPaused = false;
    public GameObject pauseScreen;

    [Header("Keybinds")]
    public KeyCode pauseKey = KeyCode.Escape;

    [SerializeField] GameObject exitPanel;


    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            isPaused = !isPaused;
        }
        if (isPaused) {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void PauseGame() {
        // For possible view of inventory
        if(pauseScreen != null)
        {
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
        }
    }

    void ResumeGame() {
        if (pauseScreen != null)
        {
            Time.timeScale = 1;
            pauseScreen.SetActive(false);
        }
    }

    public void LoadGameLevel()
    {
        SceneManager.LoadScene(1);
    }

    // Goes to Death Scene
    public void DeathScreen()
    {
        SceneManager.LoadScene(2);
        Cursor.visible = true;
    }

    // Goes to Death Scene
    public void WinScreen()
    {
        SceneManager.LoadScene(3);
        Cursor.visible = true;
    }

    // Goes back to Main Menu
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    // Restarts the Game
    public void RestartGame()
    {
        SceneManager.LoadScene(1);
    }
    public void ShowExitConfirmation()
    {
        exitPanel.SetActive(true);
    }

    public void ConfirmedExit()
    {
        if (Application.isEditor)
        {
            // Code for Unity Editor
            Debug.Log("Exiting Game in Editor");
            EditorApplication.isPlaying = false;  // Stops Play Mode in the editor
        }
        else
        {
            // Code for a built application
            Debug.Log("Exiting Game in Build");
            Application.Quit();  // Quits the game in a build
        }
    }

    public void ReturnFromExit()
    {
        exitPanel.SetActive(false);
    }
}
