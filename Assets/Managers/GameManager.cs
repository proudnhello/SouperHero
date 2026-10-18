/*
 * An old version of this file was modified with the help of LLMs: 
 * https://github.com/djlouie/project-soup-chat-logs/blob/main/logs/log16.md
 */

using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton { get; private set; }


    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        } 
        else
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    internal bool IsCurrentRunFromSave;
    internal int SceneToLoad = 2;
    public void NewGame(MainMenuSceneTransition trans)
    {
        IsCurrentRunFromSave = false;
        SaveManager.Singleton.ResetGameState(); // Delete previous save

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
        SceneToLoad = 2;
        StartCoroutine(LoadLoadingScene(trans));
    }

    public void LoadSave(MainMenuSceneTransition trans)
    {
        IsCurrentRunFromSave = true;
        SceneToLoad = 2;
        StartCoroutine(LoadLoadingScene(trans));
    }

    public void LoadTutorial(MainMenuSceneTransition trans)
    {
        SceneToLoad = 6;
        StartCoroutine(LoadLoadingScene(trans));
    }

    IEnumerator LoadLoadingScene(MainMenuSceneTransition trans) // taken from https://fmod.com/docs/2.02/unity/examples-async-loading.html
    {
        trans.FadeToBlack();

        AsyncOperation async = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);

        //Display loading bar

        // Don't let the scene start until all Studio Banks have finished loading
        async.allowSceneActivation = false;

        // Iterate all the Studio Banks and start them loading in the background
        // including the audio sample data
        foreach (var bank in AudioManager.Singleton.FMOD_Banks)
        {
            FMODUnity.RuntimeManager.LoadBank(bank, true);
        }

        // Keep yielding the co-routine until all the bank loading is done
        // (for platforms with asynchronous bank loading)
        yield return new WaitUntil(() => FMODUnity.RuntimeManager.HaveAllBanksLoaded);


        // Keep yielding the co-routine until all the sample data loading is done
        yield return new WaitUntil(() => !FMODUnity.RuntimeManager.AnySampleDataLoading());

        yield return new WaitUntil(() => !trans.inProgress);

        // Allow the scene to be activated. This means that any OnActivated() or Start()
        // methods will be guaranteed that all FMOD Studio loading will be completed and
        // there will be no delay in starting events
        async.allowSceneActivation = true;

        // Keep yielding the co-routine until scene loading and activation is done.
        yield return new WaitUntil(() => async.isDone);
    }


    public static event Action OnStartRun;
    internal bool GameRunning;
    internal bool GameReady;
    internal bool TutorialRunning;

    public void StartRun(bool isTutorial = false)
    {
        OnStartRun?.Invoke();
        GameReady = true;
        TutorialRunning = isTutorial;
    }

    // Goes to Win or Death Scene
    public static event Action OnEndRun;
    public void EndRun(bool successfulRun)
    {
        if (!TutorialRunning) MetricsTracker.Singleton.EndRun(successfulRun);
        Cursor.visible = true;
        OnEndRun?.Invoke();
        GameRunning = false;
        GameReady = false;

        if (TutorialRunning) { TutorialRunning = false; SceneManager.LoadScene(0);}
        else if (successfulRun) SceneManager.LoadScene(4);
        else SceneManager.LoadScene(3);
    }

}
