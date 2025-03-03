using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//TODO: Don't pause game while cooking

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuration")]
    private static bool isPaused = false;
    public GameObject pauseScreen;

    [Header("Keybinds")]
    public KeyCode pauseKey = KeyCode.Escape;


    void Update()
    {
        if (Input.GetKeyDown(pauseKey) && pauseScreen != null)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        pauseScreen.SetActive(false);
    }

    void PauseGame() {
        Debug.Log("**GAME PAUSED**");
        Time.timeScale = 0;
        pauseScreen.SetActive(true);
    }

    void ResumeGame() {
        Debug.Log("**GAME RESUMED**");
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
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
}
