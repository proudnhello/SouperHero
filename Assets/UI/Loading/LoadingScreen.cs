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

    [SerializeField] CanvasGroup Alpha;
    [SerializeField] float ALPHA_FADE_ANIM_TIME;
    [SerializeField] RectTransform CircleWipe;
    [SerializeField] float START_WIPE_TIME;
    [SerializeField] float PERCENTAGE_TO_HOLD;
    [SerializeField] float HOLD_WIPE_TIME;
    [SerializeField] float FINISH_WIPE_TIME;
    [SerializeField] float WIPE_SCREEN_SIZE_MULT = 1.3953125f;
    [SerializeField] AnimationCurve wipeMotionCurve;

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

        LoadText.text = "Finalizing chunks...";

        value = 0f;
        while (value < 1f)
        {
            float loadingProgressVal = RoomGenerator2.Instance.chunkSpawner.chunksSpawned / RoomGenerator2.Instance.chunkSpawner.totalChunks;
            value = Mathf.Clamp01(loadingProgressVal);
            loadingSlider.value = value;
            yield return null;
        }

        yield return new WaitUntil(() => GameManager.Singleton.GameReady);

        float time = ALPHA_FADE_ANIM_TIME;
        while (time > 0)
        {
            time -= Time.deltaTime;
            Alpha.alpha = time / ALPHA_FADE_ANIM_TIME;
            yield return null;
        }
        Alpha.alpha = 0f;

        GameManager.Singleton.GameRunning = true;
        MetricsTracker.Singleton.StartRun();

        time = 0;
        CameraMover.Singleton.StartCoroutine(CameraMover.Singleton.ZoomAnimIntro(HOLD_WIPE_TIME + START_WIPE_TIME, FINISH_WIPE_TIME));

        while (time <= START_WIPE_TIME)
        {
            time += Time.deltaTime;
            var percentCompleted = Mathf.Clamp01(time / (START_WIPE_TIME));
            var scaledPercentaged = wipeMotionCurve.Evaluate(percentCompleted);
            float val = PERCENTAGE_TO_HOLD * scaledPercentaged * Screen.width * WIPE_SCREEN_SIZE_MULT;
            CircleWipe.sizeDelta = new Vector2(val, val);
            yield return null;
        }

        yield return new WaitForSeconds(HOLD_WIPE_TIME);

        time = 0;
        while (time <= FINISH_WIPE_TIME)
        {
            time += Time.deltaTime;
            var percentCompleted = Mathf.Clamp01(time / FINISH_WIPE_TIME);
            var scaledPercentaged = wipeMotionCurve.Evaluate(percentCompleted);
            float val = (1 - PERCENTAGE_TO_HOLD) * scaledPercentaged * Screen.width * WIPE_SCREEN_SIZE_MULT +
                 PERCENTAGE_TO_HOLD * Screen.width * WIPE_SCREEN_SIZE_MULT;
            CircleWipe.sizeDelta = new Vector2(val, val);
            yield return null;
        }
        CircleWipe.gameObject.SetActive(false);

        SceneManager.UnloadSceneAsync(1);
    }
}
