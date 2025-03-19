using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
/*
#if UNITY_EDITOR
#else
#endif
*/

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuration")]
    public static bool isPaused = false;
    public GameObject pauseScreen;
    [SerializeField] GameObject exitPanel;

    [FMODUnity.BankRef]
    public List<string> FMOD_Banks = new List<string>();

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
        isPaused = false;
    }

    public void PauseGame() {
        isPaused = true;
        Time.timeScale = 0;
        CursorManager.Singleton.cursorObject.SetActive(false);
        pauseScreen.SetActive(true);
        PlayerEntityManager.Singleton.input.Disable();

        //InputManager.playerInput.SwitchCurrentActionMap("UI");
    }

    public void ResumeGame() {
        isPaused = false;
        Time.timeScale = 1;
        CursorManager.Singleton.cursorObject.SetActive(true);
        pauseScreen.SetActive(false);
        PlayerEntityManager.Singleton.input.Enable();

        //InputManager.playerInput.SwitchCurrentActionMap("Player");
    }

    public void NewGame()
    {
        // Delete previous save
        File.Delete(Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Stats.json"));
        File.Delete(Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Enemies.json"));

        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
        
        StartCoroutine(LoadScene());
        Time.timeScale = 1;

        IEnumerator LoadScene() // taken from https://fmod.com/docs/2.02/unity/examples-async-loading.html
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(1);

            // Don't let the scene start until all Studio Banks have finished loading
            async.allowSceneActivation = false;


            // Iterate all the Studio Banks and start them loading in the background
            // including the audio sample data
            foreach (var bank in FMOD_Banks)
            {
                FMODUnity.RuntimeManager.LoadBank(bank, true);
            }

            // Keep yielding the co-routine until all the bank loading is done
            // (for platforms with asynchronous bank loading)
            yield return new WaitUntil(() => FMODUnity.RuntimeManager.HaveAllBanksLoaded);


            // Keep yielding the co-routine until all the sample data loading is done
            yield return new WaitUntil(() => !FMODUnity.RuntimeManager.AnySampleDataLoading());


            // Allow the scene to be activated. This means that any OnActivated() or Start()
            // methods will be guaranteed that all FMOD Studio loading will be completed and
            // there will be no delay in starting events
            async.allowSceneActivation = true;

            // Keep yielding the co-routine until scene loading and activation is done.
            yield return new WaitUntil(() => async.isDone);
        }
    }

    public void LoadSave()
    {
        StartCoroutine(LoadScene());
        Time.timeScale = 1;

        IEnumerator LoadScene() // taken from https://fmod.com/docs/2.02/unity/examples-async-loading.html
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(1);

            // Don't let the scene start until all Studio Banks have finished loading
            async.allowSceneActivation = false;


            // Iterate all the Studio Banks and start them loading in the background
            // including the audio sample data
            foreach (var bank in FMOD_Banks)
            {
                FMODUnity.RuntimeManager.LoadBank(bank, true);
            }

            // Keep yielding the co-routine until all the bank loading is done
            // (for platforms with asynchronous bank loading)
            yield return new WaitUntil(() => FMODUnity.RuntimeManager.HaveAllBanksLoaded);


            // Keep yielding the co-routine until all the sample data loading is done
            yield return new WaitUntil(() => !FMODUnity.RuntimeManager.AnySampleDataLoading());


            // Allow the scene to be activated. This means that any OnActivated() or Start()
            // methods will be guaranteed that all FMOD Studio loading will be completed and
            // there will be no delay in starting events
            async.allowSceneActivation = true;

            // Keep yielding the co-routine until scene loading and activation is done.
            yield return new WaitUntil(() => async.isDone);
        }
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
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    // Restarts the Game
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }

    public void LanguageSelect(){
        SceneManager.LoadScene(4);
    }
    public void EnterControlsScreen()
    {
        SceneManager.LoadScene(5);
    }
    public void ShowExitConfirmation()
    {
        exitPanel.SetActive(true);
    }

    public void ConfirmedExit()
    {
        /*
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
        */
        Application.Quit();  // Quits the game in a build
    }

    public void ReturnFromExit()
    {
        exitPanel.SetActive(false);
    }
}
