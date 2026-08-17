using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] public Slider loadingSlider;
    [Header("Words")]
    [SerializeField] private GameObject find;
    [SerializeField] private GameObject the;
    [SerializeField] private GameObject exit;
    [SerializeField] TMP_Text LoadText;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.UnloadSceneAsync(0);
        AudioManager.Singleton._MusicHandler.ChangeState(MusicHandler.MusicState.LOADING);
        LoadText.text = "";
        Sequence loadSequence = DOTween.Sequence();
        loadSequence.Append(exit.transform.DOLocalMoveY(-200, 0.25f));
        loadSequence.Append(the.transform.DOLocalMoveY(0, 0.25f));
        loadSequence.Append(find.transform.DOLocalMoveY(200, 0.25f));
        loadSequence.AppendInterval(2f);
        loadSequence.OnComplete(() =>
        {
            StartCoroutine(LoadGameScene());
        });
    }

    IEnumerator LoadGameScene()
    {
        LoadText.text = "Loading in map...";

        AsyncOperation async = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
        async.allowSceneActivation = false;

        float value = 0f;
        float loadingProgressBuffer = 0;
        while (value < 1f)
        {
            float loadingProgressVal = async.progress;
            value = Mathf.Clamp01(((loadingProgressVal / 0.9f) / 2f) + ((loadingProgressBuffer / 0.9f) / 2f));
            loadingSlider.value = value;
            loadingProgressBuffer += 0.1f;
            yield return null;
        }

        async.allowSceneActivation = true;
        yield return new WaitUntil(() => async.isDone);

        Scene targetScene = SceneManager.GetSceneByBuildIndex(2);
        SceneManager.SetActiveScene(targetScene);

        yield return new WaitUntil(() => RoomGenerator2.Instance != null);
        LoadText.text = "Generating map...";

        yield return new WaitUntil(() => RoomGenerator2.Instance.chunkSpawner != null);
        LoadText.text = "Spawning rooms...";

        value = 0f;
        while (value < 1f)
        {
            float loadingProgressVal = RoomGenerator2.Instance.chunkSpawner.roomsSpawned / RoomGenerator2.Instance.chunkSpawner.totalRooms;
            value = Mathf.Clamp01(loadingProgressVal);
            loadingSlider.value = value;
            yield return null;
        }

        SceneManager.UnloadSceneAsync(1);
    }
}
