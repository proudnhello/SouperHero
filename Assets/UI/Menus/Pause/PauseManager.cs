using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    internal static bool isPaused = false;
    [SerializeField] GameObject pauseScreen;


    private void Awake()
    {
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }
        isPaused = false;

        // Hook up Pause Button from input map to OnPauseButton()
        PlayerKeybinds.Singleton.pause.action.started += Pause;
    }

    void OnDisable()
    {
        PlayerKeybinds.Singleton.pause.action.started -= Pause;
    }

    // ############ DELETE THIS BELOW AFTER HOOKING IT UP
    // void Update()
    // {
    //     if (Input.GetKeyDown(pauseKey) && pauseScreen != null
    //         && !CookingManager.Singleton.IsCooking) //Don't pause when cooking
    //     {
    //         isPaused = !isPaused;
    //         if (isPaused)
    //         {
    //             PauseGame();
    //         }
    //         else
    //         {
    //             ResumeGame();
    //         }
    //     }
    // }
    // ########################

    void Pause(InputAction.CallbackContext ctx)
    {
        OnPauseButton();
    }

    void OnPauseButton()
    {
        if (pauseScreen != null && !CookingScreen.Singleton.IsCooking) //Don't pause when cooking ---- Why????
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

    public void PauseGame()
    {
        Time.timeScale = 0;
        pauseScreen.SetActive(true);
        PlayerEntityManager.Singleton.input.Disable();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        PlayerEntityManager.Singleton.input.Enable();
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void RestartGame()
    {
        SaveManager.Singleton.ResetGameState();
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
}
