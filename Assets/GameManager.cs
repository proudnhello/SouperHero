using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
        if (Input.GetKeyDown(pauseKey) && pauseScreen != null 
            && !CookingManager.Singleton.IsCooking() //Don't pause when cooking
            && SceneManager.GetActiveScene().buildIndex != 0) //Don't pause if in main menu
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
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }
    }

    void PauseGame() {
        Time.timeScale = 0;
        CursorManager.Singleton.cursorObject.SetActive(false);
        pauseScreen.SetActive(true);
        PlayerEntityManager.Singleton.input.Disable();

        //InputManager.playerInput.SwitchCurrentActionMap("UI");
    }

    void ResumeGame() {
        Time.timeScale = 1;
        CursorManager.Singleton.cursorObject.SetActive(true);
        pauseScreen.SetActive(false);
        PlayerEntityManager.Singleton.input.Enable();

        //InputManager.playerInput.SwitchCurrentActionMap("Player");
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

    // Goes to Win Scene
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
