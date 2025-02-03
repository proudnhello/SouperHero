using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuration")]
    private bool isPaused = false;
    public GameObject pauseScreen;

    [Header("Keybinds")]
    public KeyCode pauseKey = KeyCode.Escape;


    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            isPaused = !isPaused;
            Debug.Log("Escape pressed");
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
}
