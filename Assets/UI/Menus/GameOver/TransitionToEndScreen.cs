using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TransitionToEndScreen : MonoBehaviour
{
    [SerializeField] GameObject NormalPostProcessing;
    [SerializeField] GameObject EndPostProcessing;
    [SerializeField] GameObject[] ObjsToTurnOff;
    [SerializeField] TMP_Text[] GameOverTexts;

    [SerializeField] float TIME_TIL_BOOK_SLIDE = 1f;

    [SerializeField] RectTransform Book;
    [SerializeField] Vector2 BookLocs;
    [SerializeField] float BOOK_SLIDE_TIME = 1.35f;
    [SerializeField] float START_TAB_SLIDE = .4f;
    [SerializeField] SettingBookTab[] tabs;
    [SerializeField] float TIME_BETWEEN_TABS;

    private void Start()
    {
        GameManager.OnEndRun += OnEndGame;
        Book.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        GameManager.OnEndRun -= OnEndGame;
    }

    void OnEndGame(bool success)
    {
        if (!success)
        {
            NormalPostProcessing.SetActive(false);
            EndPostProcessing.SetActive(true);
        }
        
        foreach (var obj in ObjsToTurnOff) obj.SetActive(false);

        foreach (var text in GameOverTexts) text.text = success ? LocalizationManager.GetLocalizedString("Win Screen") : LocalizationManager.GetLocalizedString("Game Over");

        StartCoroutine(EndAnimation());
    }

    IEnumerator EndAnimation()
    {
        yield return new WaitForSeconds(TIME_TIL_BOOK_SLIDE);

        Book.gameObject.SetActive(true);
        Book.anchoredPosition = new Vector2(BookLocs.x, Book.localPosition.y);

        StartCoroutine(CameraMover.Singleton.EndRunMoveAnim(BOOK_SLIDE_TIME));

        var s = DOTween.Sequence();
        s.Append(Book.DOAnchorPos(new Vector2(BookLocs.y, Book.localPosition.y), BOOK_SLIDE_TIME).SetEase(Ease.OutQuad));
        s = DOTween.Sequence();
        s.AppendInterval(START_TAB_SLIDE);
        foreach (var tab in tabs) {
            s.AppendCallback(() => tab.RevealTab());
            s.AppendInterval(TIME_BETWEEN_TABS);
        }
        s.OnComplete(() => { });
    }
}
