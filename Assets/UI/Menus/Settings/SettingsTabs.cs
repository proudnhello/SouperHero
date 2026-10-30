using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SettingsTabs : MonoBehaviour
{
    public CanvasGroup[] UI_List;
    public SettingBookTab[] tabs;
    
    public GameObject settingsMenu;

    [SerializeField] float delayBeforeTabs;
    [SerializeField] float delayBeforeFadingInContent;
    [SerializeField] float fadeTime;
    [SerializeField] float TIME_BETWEEN_TAB_REVEAL;
    [SerializeField] float BookFlipTime;
    [SerializeField] Animator BookFlip;
    Tween[] fadeTweens;

    private void Awake()
    {
        for (int c = 0; c < UI_List.Length; c++) UI_List[c].alpha = 0;
        fadeTweens = new Tween[UI_List.Length];
        foreach (var c in UI_List) { c.alpha = 0; c.blocksRaycasts = false; }
    }

    int currentlyShowing = -1;
    Sequence fade;
    Sequence tabsSeq;
    public void InstantOpenSettings()
    {
        foreach (var c in UI_List)
        {
            c.alpha = 0;
            c.blocksRaycasts = false;
        }
        int showing = currentlyShowing == -1 ? 0 : currentlyShowing;
        for (int t = 0; t < tabs.Length; t++)
        {
            if (t == showing) tabs[t].KeepSelected(true);
            else tabs[t].InstantUnhover();
        }
        tabs[showing].KeepSelected(true);
        currentlyShowing = -1;
        queuedTab = -1;
        fade.Kill();
        ShowUI(showing);
    }

    public void OpenSettings()
    {
        tabsSeq = DOTween.Sequence().SetUpdate(true);
        int t = 0;
        float time = delayBeforeTabs;
        foreach (var tab in tabs)
        {
            bool matches = t == currentlyShowing;
            tabsSeq.InsertCallback(time, () => tab.RevealTab(matches));
            t++;
            time += TIME_BETWEEN_TAB_REVEAL;
        }
        tabsSeq.Play();

        foreach (var tab in tabs) tab.DisableButton(true);
        currentlyShowing = currentlyShowing == -1 ? 0 : currentlyShowing;
        fade = DOTween.Sequence().SetUpdate(true);
        fadeTweens[currentlyShowing] = UI_List[currentlyShowing].DOFade(1, fadeTime).SetEase(Ease.OutSine);
        fade.Insert(delayBeforeFadingInContent, fadeTweens[currentlyShowing]);
        fade.OnComplete(() => {
            UI_List[currentlyShowing].blocksRaycasts = true;
            foreach (var tab in tabs) 
                tab.DisableButton(false); });
    }

    int queuedTab = -1;
    public void ShowUI(int index)
    {
        if (queuedTab == index) return;

        queuedTab = index;
        if (currentlyShowing >= 0)
        {
            float elapsed = fadeTweens[currentlyShowing] == null ? 0 :
                !fadeTweens[currentlyShowing].IsActive() ? fadeTime : fade.Elapsed();
            fade.Kill();
            fade = DOTween.Sequence().SetUpdate(true);
            fadeTweens[currentlyShowing] = UI_List[currentlyShowing].DOFade(0, elapsed).SetEase(Ease.OutSine).SetUpdate(true);
            UI_List[currentlyShowing].blocksRaycasts = false;
            fade.Append(fadeTweens[currentlyShowing]);
            fade.OnComplete(() => ShowNext());
        }
        else
        {
            ShowNext();
        }


        void ShowNext()
        {
            fade = DOTween.Sequence().SetUpdate(true);
            foreach (var tab in tabs) tab.DisableButton(true);
            float fadeDelay = currentlyShowing != index ? BookFlipTime : 0f;
            string flipType = currentlyShowing < index ? "FlipForward" : "FlipBackward";
            if (currentlyShowing != index) fade.InsertCallback(0, () => BookFlip.SetTrigger(flipType));
            currentlyShowing = index;
            fadeTweens[currentlyShowing] = UI_List[currentlyShowing].DOFade(1, fadeTime).SetEase(Ease.OutSine);
            fade.InsertCallback(fadeDelay, () => { foreach (var tab in tabs) tab.DisableButton(false); });
            fade.Insert(fadeDelay, fadeTweens[currentlyShowing]);
            fade.OnComplete(() => UI_List[currentlyShowing].blocksRaycasts = true);
        }  
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void CloseSettings()
    {
        tabsSeq.Kill();
        tabsSeq = DOTween.Sequence();
        foreach (var tab in tabs)
        {
            tabsSeq.AppendCallback(() => tab.HideTab());
            tabsSeq.AppendInterval(TIME_BETWEEN_TAB_REVEAL);
        }
        tabsSeq.Play();


        float elapsed = fadeTweens[currentlyShowing] == null ? fadeTime :
            !fadeTweens[currentlyShowing].IsActive() ? fadeTime : fade.Elapsed();
        fade.Kill();
        fade = DOTween.Sequence();
        fadeTweens[currentlyShowing] = UI_List[currentlyShowing].DOFade(0, elapsed).SetEase(Ease.OutSine);
        UI_List[currentlyShowing].blocksRaycasts = false;
        fade.Append(fadeTweens[currentlyShowing]);
        fade.Play();
    }
}
