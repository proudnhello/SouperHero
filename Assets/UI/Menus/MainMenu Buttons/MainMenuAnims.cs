using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Who had the bright idea to put this script on the PLAYER in the main menu???? Why?
public class MainMenuAnims : MonoBehaviour
{
    //public static MainMenuAnims Singleton { get; private set; }

    public GameObject _camera;
    public Image blackFadeWorld;
    public Image blackFadeOverlay;
    public GameObject titleText;
    public GameObject book;

    [SerializeField] float bookMoveInPosition = 876f;


    [Header("Button Fade In Anim")]
    [SerializeField] float delayAfterBookSlideToFade = 1f;
    [SerializeField] Vector2 fadeInPoses;
    [SerializeField] float fadeInTime = 0.65f;
    [SerializeField] float timeBetweenFadeIns = 0.3f;
    [SerializeField] float delayForQuitTab = 0.15f;

    public RectTransform[] bookPageOne;
    public BookButtonContainer bookPageOneContainer;
    [SerializeField] SettingBookTab bookPageOneTab;
    public RectTransform[] bookPageTwo;
    public BookButtonContainer bookPageTwoContainer;
    public BookButtonScript continueButton;

    [Header("Page Flip Anim")]
    [SerializeField] float pageFlipForwardDelay;
    [SerializeField] float pageFlipForwardAnimTime;
    [SerializeField] Animator BookFlip;
    [SerializeField] float fadeInTimeOnFlip = 0.65f;
    [SerializeField] float timeBetweenFadeInsOnFlip = 0.3f;
    [SerializeField] float pageFlipBackwardDelay;
    [SerializeField] float pageFlipBackwardAnimTime;

    [Header("Settings Move Anim")]
    [SerializeField] GameObject settingsMenu;
    [SerializeField] float bookSlideSettingsTime = 0.75f;
    [SerializeField] float settingsBookPos = 0;
    [SerializeField] float settingsCameraPos = 0;
    [SerializeField] SettingsTabs settingsTabs;
    [SerializeField] float delayAfterLeavingSettingsContents = 0.80f;
    [SerializeField] float delayFlipBookOnExitOptions = 0.2f;

    public Animator playerAnimator;

    [SerializeField] MainMenuSceneTransition sceneTransitionStarter;

    private bool isLoading = false;
    [SerializeField] private GraphicRaycaster mainMenu;



    private Sequence s;
    private Sequence moveInSecondarySequence;
    private Sequence moveOutSecondarySequence;

    private bool isDone = false;
    private void Awake()
    {
        settingsMenu.SetActive(false);
    }
    void Start()
    {
        bookPageOneContainer.Disable();
        bookPageTwoContainer.Disable();

        s = DOTween.Sequence();

        blackFadeWorld.color = Color.white;
        blackFadeOverlay.color = new Color(1, 1, 1, 0);
        s.Append(titleText.transform.DOLocalMoveY(0, 1.65f).SetEase(Ease.OutBounce));
        s.Append(blackFadeWorld.DOColor(new Color(1, 1, 1, 0), 2f).SetEase(Ease.InQuad));
        s.Append(book.transform.DOLocalMoveX(bookMoveInPosition, 1.35f).SetEase(Ease.OutQuad));

        for (int i = 0; i < bookPageOne.Length; i++)
        {
            var but = bookPageOne[i];
            var canv = but.GetComponent<CanvasGroup>();
            but.anchoredPosition = new Vector2(but.anchoredPosition.x, fadeInPoses.x);
            canv.alpha = 0;
            s.Join(but.DOAnchorPosY(fadeInPoses.y, fadeInTime).SetEase(Ease.OutBack).SetDelay(i == 0 ? delayAfterBookSlideToFade : timeBetweenFadeIns));
            s.Join(canv.DOFade(1, fadeInTime).SetEase(Ease.OutSine).SetDelay(i == bookPageOne.Length-1 ? delayForQuitTab : 0));
        }

        s.JoinCallback(() => bookPageOneTab.RevealTab());
        s.OnComplete(() => { isDone = true; bookPageOneContainer.Enable(); });

        BuildFlipSequences();

        AudioManager.Singleton._MusicHandler.ChangeState(MusicHandler.MusicState.MAINMENU);
    }

    public void BuildFlipSequences()
    {
        moveInSecondarySequence = DOTween.Sequence();
        float t = 0;
        moveInSecondarySequence.InsertCallback(0, () => bookPageOneContainer.Disable());
        for (int i = 0; i < bookPageOne.Length; i++)
        {
            var but = bookPageOne[i];
            var canv = but.GetComponent<CanvasGroup>();
            moveInSecondarySequence.Insert(t, but.DOAnchorPosY(fadeInPoses.x, fadeInTimeOnFlip).SetEase(Ease.InBack));
            moveInSecondarySequence.Insert(t, canv.DOFade(0, fadeInTimeOnFlip).SetEase(Ease.InSine));
            if (i < bookPageOne.Length - 1) t += timeBetweenFadeIns;
        }
        t += pageFlipForwardDelay;
        moveInSecondarySequence.InsertCallback(t, () => BookFlip.SetTrigger("FlipForward"));
        t += pageFlipForwardAnimTime;
        for (int i = 0; i < bookPageTwo.Length; i++)
        {
            var but = bookPageTwo[i];
            var canv = but.GetComponent<CanvasGroup>();
            but.anchoredPosition = new Vector2(but.anchoredPosition.x, fadeInPoses.x);
            canv.alpha = 0;
            moveInSecondarySequence.Insert(t, but.DOAnchorPosY(fadeInPoses.y, fadeInTimeOnFlip).SetEase(Ease.OutBack));
            moveInSecondarySequence.Insert(t, canv.DOFade(1, fadeInTimeOnFlip).SetEase(Ease.OutSine));
            t += timeBetweenFadeInsOnFlip;
        }
        moveInSecondarySequence.OnComplete(() => bookPageTwoContainer.Enable());
        moveInSecondarySequence.SetAutoKill(false);
        moveInSecondarySequence.Pause();


        moveOutSecondarySequence = DOTween.Sequence();
        t = 0;
        moveOutSecondarySequence.InsertCallback(0, () => bookPageTwoContainer.Disable());
        for (int i = 0; i < bookPageTwo.Length; i++)
        {
            var but = bookPageTwo[i];
            var canv = but.GetComponent<CanvasGroup>();
            moveOutSecondarySequence.Insert(t, but.DOAnchorPosY(fadeInPoses.x, fadeInTimeOnFlip).SetEase(Ease.InBack));
            moveOutSecondarySequence.Insert(t, canv.DOFade(0, fadeInTimeOnFlip).SetEase(Ease.InSine));
            if (i < bookPageTwo.Length - 1) t += timeBetweenFadeIns;
        }
        t += pageFlipBackwardDelay;
        moveOutSecondarySequence.InsertCallback(t, () => BookFlip.SetTrigger("FlipBackward"));
        t += pageFlipBackwardAnimTime;
        for (int i = 0; i < bookPageOne.Length; i++)
        {
            var but = bookPageOne[i];
            var canv = but.GetComponent<CanvasGroup>();
            moveOutSecondarySequence.Insert(t, but.DOAnchorPosY(fadeInPoses.y, fadeInTimeOnFlip).SetEase(Ease.OutBack));
            moveOutSecondarySequence.Insert(t, canv.DOFade(1, fadeInTimeOnFlip).SetEase(Ease.OutSine));
            t += timeBetweenFadeInsOnFlip;
        }
        moveOutSecondarySequence.OnComplete(() => bookPageOneContainer.Enable());
        moveOutSecondarySequence.SetAutoKill(false);
        moveOutSecondarySequence.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //playerAnimator.SetTrigger("Left Click");
            if (!isDone)
            {
                //s.Kill();
                //titleText.transform.localPosition = new Vector3(titleText.transform.localPosition.x, 0, titleText.transform.localPosition.z);
                //blackFade.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                //book.transform.localPosition = new Vector3(0, book.transform.localPosition.y, book.transform.localPosition.z);
                ////firstButton.anchoredPosition = new Vector2(42, firstButton.anchoredPosition.y);
                ////secondButton.anchoredPosition = new Vector2(42, secondButton.anchoredPosition.y);
                ////tutorialButton.anchoredPosition = new Vector2(42, tutorialButton.anchoredPosition.y);
                ////thirdButton.anchoredPosition = new Vector2(42, thirdButton.anchoredPosition.y);
                //isDone = true;
            }
        }
    }



    public IEnumerator startCoroutine()
    {
        if (moveOutSecondarySequence.IsPlaying())
        {
            yield return moveOutSecondarySequence.WaitForCompletion();
        }
        if (!moveInSecondarySequence.IsPlaying())
        {
            moveInSecondarySequence.Restart();
        }
    }

    public void StartButton()
    {
        Debug.Log($"State Path: {SaveManager.Singleton.HasSavedRun()}");
        if (!SaveManager.Singleton.HasSavedRun()) continueButton.Disable();
        StartCoroutine("startCoroutine");
    }

    public IEnumerator backCoroutine()
    {
        if (moveInSecondarySequence.IsPlaying())
        {
            yield return moveInSecondarySequence.WaitForCompletion();
        }
        if (!moveOutSecondarySequence.IsPlaying())
        {
            moveOutSecondarySequence.Restart();
        }
    }

    public void NewGame()
    {
        if (isLoading) return;
        isLoading = true;
        mainMenu.enabled = false;
        GameManager.Singleton.NewGame(sceneTransitionStarter);
    }

    public void ContinueFromLoad()
    {
        if (isLoading) return;
        isLoading = true;

        mainMenu.enabled = false;
        GameManager.Singleton.LoadSave(sceneTransitionStarter);
    }

    public void BackButton()
    {
        StartCoroutine("backCoroutine");
    }
    public void EnterOptionsScreen()
    {
        Sequence sweep = DOTween.Sequence();
        settingsMenu.SetActive(true);
        float t = 0;
        bookPageOneContainer.Disable();
        for (int i = 0; i < bookPageOne.Length; i++)
        {
            var but = bookPageOne[i];
            var canv = but.GetComponent<CanvasGroup>();
            sweep.Insert(t, but.DOAnchorPosY(fadeInPoses.x, fadeInTimeOnFlip).SetEase(Ease.InBack));
            sweep.Insert(t, canv.DOFade(0, fadeInTimeOnFlip).SetEase(Ease.InSine));
            if (i < bookPageOne.Length - 1) t += timeBetweenFadeIns;
        }
        t += pageFlipForwardDelay;
        sweep.InsertCallback(t, () => BookFlip.SetTrigger("FlipForwardPlus"));
        sweep.Insert(t, blackFadeOverlay.DOColor(new Color(1, 1, 1, .75f), bookSlideSettingsTime).SetEase(Ease.OutQuad));
        sweep.Insert(t, _camera.transform.DOLocalMoveX(settingsCameraPos, bookSlideSettingsTime).SetEase(Ease.InOutQuad));
        sweep.Insert(t, book.transform.DOLocalMoveX(settingsBookPos, bookSlideSettingsTime).SetEase(Ease.InOutQuad));
        sweep.InsertCallback(t, () => bookPageOneTab.HideTab());
        sweep.InsertCallback(t, () => settingsTabs.OpenSettings());
        sweep.Play();
    }

    public void ExitOptionsScreen()
    {
        Sequence sweep = DOTween.Sequence();
        settingsTabs.CloseSettings();
        bookPageOneTab.RevealTab();

        float t = 0;
        sweep.Insert(t, blackFadeOverlay.DOColor(new Color(1, 1, 1, 0), bookSlideSettingsTime).SetEase(Ease.OutQuad));
        sweep.Insert(t, _camera.transform.DOLocalMoveX(0, bookSlideSettingsTime).SetEase(Ease.InOutQuad));
        sweep.Insert(t, book.transform.DOLocalMoveX(bookMoveInPosition, bookSlideSettingsTime).SetEase(Ease.InOutQuad));
        t += delayFlipBookOnExitOptions;
        sweep.InsertCallback(t, () => BookFlip.SetTrigger("FlipBackwardPlus"));
        t += delayAfterLeavingSettingsContents;

        for (int i = 0; i < bookPageOne.Length; i++)
        {
            var but = bookPageOne[i];
            var canv = but.GetComponent<CanvasGroup>();
            sweep.Insert(t, but.DOAnchorPosY(fadeInPoses.y, fadeInTime).SetEase(Ease.OutBack));
            sweep.Insert(t, canv.DOFade(1, fadeInTime).SetEase(Ease.OutSine));
            if (i < bookPageOne.Length - 1) t += timeBetweenFadeIns;
        }
        sweep.OnComplete(() => bookPageOneContainer.Enable());
        sweep.Play();
    }

    public void StartTutorial()
    {
        if (isLoading) return;
        isLoading = true;
        mainMenu.enabled = false;
        GameManager.Singleton.LoadTutorial(sceneTransitionStarter);
    }
}
