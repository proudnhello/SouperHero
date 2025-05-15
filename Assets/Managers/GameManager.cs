using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
/*
#if UNITY_EDITOR
#else
#endif
*/

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
    public void NewGame()
    {
        IsCurrentRunFromSave = false;
        // Delete previous save
        SaveManager.Singleton.ResetGameState();
        
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        StartCoroutine(LoadScene());

        IEnumerator LoadScene() // taken from https://fmod.com/docs/2.02/unity/examples-async-loading.html
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(1);

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
    }

    public void LoadSave()
    {
        IsCurrentRunFromSave = true;

        StartCoroutine(LoadScene());

        IEnumerator LoadScene() // taken from https://fmod.com/docs/2.02/unity/examples-async-loading.html
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(1);

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
    }

    public void StartRun()
    {
        MetricsTracker.Singleton.StartRun();
    }

    // Goes to Win or Death Scene
    public void EndRun(bool successfulRun)
    {
        MetricsTracker.Singleton.EndRun(successfulRun);
        Cursor.visible = true;

        if (successfulRun) SceneManager.LoadScene(3);
        else SceneManager.LoadScene(2);
    }

}
