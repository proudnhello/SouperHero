using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using static UnityEngine.InputManagerEntry;

public class ASyncLoader : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Loading Bar")]
    [SerializeField] private Slider loadingSlider;

    [Header("Words")]
    [SerializeField] private GameObject find;
    [SerializeField] private GameObject the;
    [SerializeField] private GameObject exit;

    public void LoadLevelButton(int level)
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        Sequence loadSequence = DOTween.Sequence();
        loadSequence.Append(exit.transform.DOLocalMoveY(-200, 0.25f));
        loadSequence.Append(the.transform.DOLocalMoveY(0, 0.25f));
        loadSequence.Append(find.transform.DOLocalMoveY(200, 0.25f));
        loadSequence.AppendInterval(2f);
        loadSequence.OnComplete(() =>
        {
            Debug.Log("MOVE COMPLETED");
            StartCoroutine(LoadLevelASync(level));
        });
    }

    IEnumerator LoadLevelASync(int levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        while(!loadOperation.isDone)
        {
            float progressVal = Mathf.Clamp01(loadOperation.progress/0.9f);
            loadingSlider.value = progressVal;
            yield return null;
        }
    }
}