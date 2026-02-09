using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    internal static bool isPaused = false;
    [SerializeField] GameObject pauseScreen;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject minimap;
    [SerializeField] GameObject playerMinimapIcon;

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
        isPaused = true;

        disableMiniMap();
        increasePlayerMinimapIcon();
        
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        PlayerEntityManager.Singleton.input.Enable();
        isPaused = false;

        enableMiniMap();
        decreasePlayerMinimapIcon();
        
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

    public void SettingsMenu()
    {
        Time.timeScale = 0;
        settingsMenu.SetActive(true);
    }

    public void enableMiniMap()
    {
        minimap.SetActive(true);
    }

   

    public void disableMiniMap()
    {
        minimap.SetActive(false);
    }

    public void increasePlayerMinimapIcon()
    {
        playerMinimapIcon.transform.localScale = new Vector3(7f, 7f, 1f);
    }

    public void decreasePlayerMinimapIcon()
    {
        playerMinimapIcon.transform.localScale = new Vector3(2f, 2f, 1f);

    }

   



}
