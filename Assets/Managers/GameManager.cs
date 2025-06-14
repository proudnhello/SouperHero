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

    private float loadingProgressBuffer = 0;

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
    public void NewGame()
    {
        IsCurrentRunFromSave = false;
        SaveManager.Singleton.ResetGameState(); // Delete previous save

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        StartCoroutine(LoadScene());
    }

    public void LoadSave()
    {
        IsCurrentRunFromSave = true;

        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene() // taken from https://fmod.com/docs/2.02/unity/examples-async-loading.html
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(1);
        GameObject loadingBar = GameObject.Find("/LoadingCanvas/LoadingScreen/LoadingBar");

        //Display loading bar
        StartCoroutine(LoadingBar(async, loadingBar));

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


        // Allow the scene to be activated. This means that any OnActivated() or Start()
        // methods will be guaranteed that all FMOD Studio loading will be completed and
        // there will be no delay in starting events
        async.allowSceneActivation = true;

        // Keep yielding the co-routine until scene loading and activation is done.
        yield return new WaitUntil(() => async.isDone);
    }

    IEnumerator LoadingBar(AsyncOperation async, GameObject loadingBar)
    {
        float value = 0f;
        //while (!async.isDone && loadingBar)
        while (value < 1f && loadingBar)
        {
            float loadingProgressVal = async.progress;
            value = Mathf.Clamp01(((loadingProgressVal/0.9f) / 2f) + ((loadingProgressBuffer/0.9f) / 2f));
            loadingBar.GetComponent<Slider>().value = value;
            loadingProgressBuffer += 0.1f;
            yield return null;
        }
    }

    public static event Action OnStartRun;
    public void StartRun()
    {
        MetricsTracker.Singleton.StartRun();
        OnStartRun?.Invoke();
    }

    // Goes to Win or Death Scene
    public static event Action OnEndRun;
    public void EndRun(bool successfulRun)
    {
        MetricsTracker.Singleton.EndRun(successfulRun);
        Cursor.visible = true;
        OnEndRun?.Invoke();

        if (successfulRun) SceneManager.LoadScene(3);
        else SceneManager.LoadScene(2);
    }

}
