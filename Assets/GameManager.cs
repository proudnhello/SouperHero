using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

        //playerInput = GetComponent<PlayerInput>(); //This is returning null
        //Debug.Log("INPUT:" + playerInput);
    }

    void PauseGame() {
        Time.timeScale = 0;
        pauseScreen.SetActive(true);
        //playerInput.Disable();
        InputManager.playerInput.SwitchCurrentActionMap("UI");
    }

    void ResumeGame() {
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        //playerInput.Enable();
        InputManager.playerInput.SwitchCurrentActionMap("Player");
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
