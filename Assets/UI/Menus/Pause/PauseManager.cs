using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    internal static bool isPaused = false;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] KeyCode pauseKey = KeyCode.Escape;

    private void Awake()
    {
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }
        isPaused = false;

        // Hook up Pause Button from input map to OnPauseButton()
    }

    // ############ DELETE THIS BELOW AFTER HOOKING IT UP
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
    // ########################

    void OnPauseButton()
    {
        if (pauseScreen != null && !CookingManager.Singleton.IsCooking()) //Don't pause when cooking ---- Why????
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
        CursorManager.Singleton.cursorObject.SetActive(false);
        pauseScreen.SetActive(true);
        PlayerEntityManager.Singleton.input.Disable();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        CursorManager.Singleton.cursorObject.SetActive(true);
        pauseScreen.SetActive(false);
        PlayerEntityManager.Singleton.input.Enable();
    }
}
