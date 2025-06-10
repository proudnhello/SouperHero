using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MainMenuAnims : MonoBehaviour
{
    //public static MainMenuAnims Singleton { get; private set; }

    public GameObject camera;
    public GameObject blackFade;
    public GameObject titleText;
    public GameObject book;
    public RectTransform firstButton;
    public RectTransform secondButton;
    public RectTransform thirdButton;

    public RectTransform fourthButton;
    public RectTransform fifthButton;
    public RectTransform sixthButton;

    public RectTransform tutorialButton;
    public RectTransform cosmeticsButton;

    public GameObject cosmeticsSet;

    public GameObject secondaryButtonSet;
    public GameObject primaryButtonSet;

    public Animator playerAnimator;

    [SerializeField] GameObject exitPanel;

    [Header("Start Sequence")]

    private bool isLoading = false;

    [Header("Menus")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Loading Bar")]
    [SerializeField] public Slider loadingSlider;

    [Header("Words")]
    [SerializeField] private GameObject find;
    [SerializeField] private GameObject the;
    [SerializeField] private GameObject exit;

    private Sequence s;
    private Sequence moveInSecondarySequence;
    private Sequence moveOutSecondarySequence;

    private bool isDone = false;

    // Start is called before the first frame update
    void Start()
    {
        s = DOTween.Sequence();

        s.Append(titleText.transform.DOLocalMoveY(0, 1.65f).SetEase(Ease.OutBounce));
        s.Append(blackFade.GetComponent<Image>().DOColor(new Color(0, 0, 0, 0), 2f).SetEase(Ease.InQuad));
        s.Append(book.transform.DOLocalMoveX(0, 1.35f).SetEase(Ease.OutQuad));
        s.Append(firstButton.DOAnchorPosX(42, 0.65f).SetEase(Ease.OutQuad));
        s.Append(secondButton.DOAnchorPosX(42, 0.65f).SetEase(Ease.OutQuad));
        s.Append(thirdButton.DOAnchorPosX(42, 0.65f).SetEase(Ease.OutQuad));
        s.OnComplete(() => { isDone = true; });

        moveInSecondarySequence = DOTween.Sequence();

        moveInSecondarySequence.Append(firstButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.InQuad));
        moveInSecondarySequence.Append(secondButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.InQuad));
        moveInSecondarySequence.Append(thirdButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.InQuad));

        moveInSecondarySequence.Append(fourthButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.InQuad));
        moveInSecondarySequence.Append(fifthButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));
        moveInSecondarySequence.Append(sixthButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));


        moveInSecondarySequence.SetAutoKill(false);
        moveInSecondarySequence.Pause();

        moveOutSecondarySequence = DOTween.Sequence();

        moveOutSecondarySequence.Append(fourthButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.OutQuad));
        moveOutSecondarySequence.Append(fifthButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.OutQuad));
        moveOutSecondarySequence.Append(sixthButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.OutQuad));

        moveOutSecondarySequence.Append(firstButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));
        moveOutSecondarySequence.Append(secondButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));
        moveOutSecondarySequence.Append(thirdButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));

        moveOutSecondarySequence.SetAutoKill(false);
        moveOutSecondarySequence.Pause();

        AudioManager.Singleton._MusicHandler.ChangeState(MusicHandler.MusicState.MAINMENU);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            playerAnimator.SetTrigger("Left Click");
            if (!isDone)
            {
                s.Kill();
                titleText.transform.localPosition = new Vector3(titleText.transform.localPosition.x, 0, titleText.transform.localPosition.z);
                blackFade.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                book.transform.localPosition = new Vector3(0, book.transform.localPosition.y, book.transform.localPosition.z);
                firstButton.anchoredPosition = new Vector2(42, firstButton.anchoredPosition.y);
                secondButton.anchoredPosition = new Vector2(42, secondButton.anchoredPosition.y);
                thirdButton.anchoredPosition = new Vector2(42, thirdButton.anchoredPosition.y);
                isDone = true;
            }
        }
    }

    public void ResetTrigger()
    {
        playerAnimator.ResetTrigger("Left Click");
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

        LoadScreen(false);
    }

    public void ContinueFromLoad()
    {
        if (isLoading) return;
        isLoading = true;

        LoadScreen(true);
    }

    //If there is save data (continueGame is true), load the save. If not, load a new game
    private void LoadScreen(bool continueGame)
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        AudioManager.Singleton._MusicHandler.ChangeState(MusicHandler.MusicState.LOADING);

        Sequence loadSequence = DOTween.Sequence();
        loadSequence.Append(exit.transform.DOLocalMoveY(-200, 0.25f));
        loadSequence.Append(the.transform.DOLocalMoveY(0, 0.25f));
        loadSequence.Append(find.transform.DOLocalMoveY(200, 0.25f));
        loadSequence.AppendInterval(2f);
        loadSequence.OnComplete(() =>
        {
            if (continueGame) GameManager.Singleton.LoadSave();
            else GameManager.Singleton.NewGame();
            isLoading = false;
        });
    }

    public void BackButton()
    {
        StartCoroutine("backCoroutine");
    }
    public void EnterOptionsScreen()
    {
        SceneManager.LoadScene(4);
    }

    public void ShowExitConfirmation()
    {
        exitPanel.SetActive(true);
    }

    public void ConfirmedExit()
    {
        Application.Quit();  // Quits the game in a build
    }

    public void ReturnFromExit()
    {
        exitPanel.SetActive(false);
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene(5);
    }

    public void GoToCosmetics()
    {
        if (isLoading)
        {
            return;
        }

        isLoading = true;
        Sequence cosmeticsInSequence = DOTween.Sequence();

        cosmeticsInSequence.Append(primaryButtonSet.transform.DOLocalMoveX(1500, 0.25f).SetEase(Ease.InQuad));
        cosmeticsInSequence.Append(secondaryButtonSet.transform.DOLocalMoveX(1500, 0.25f).SetEase(Ease.InQuad));
        cosmeticsInSequence.Append(book.transform.DOLocalMoveX(950, 0.5f).SetEase(Ease.InQuad));
        cosmeticsInSequence.Append(cosmeticsButton.transform.DOLocalMoveY(-650, 0.25f).SetEase(Ease.InQuad));
        cosmeticsInSequence.Append(tutorialButton.transform.DOLocalMoveY(-650, 0.25f).SetEase(Ease.InQuad));
        cosmeticsInSequence.Append(titleText.transform.DOLocalMoveY(550, 0.25f).SetEase(Ease.InQuad));

        cosmeticsInSequence.Append(camera.transform.DOLocalMoveX(-8, 0.5f).SetEase(Ease.OutQuad));

        cosmeticsInSequence.Append(cosmeticsSet.transform.DOLocalMoveX(0, 0.5f).SetEase(Ease.OutQuad));

        cosmeticsInSequence.OnComplete(() =>
        {
            isLoading = false;
        });
    }

    public void GetOutOfCosmetics()
    {
        if (isLoading)
        {
            return;
        }

        Sequence cosmeticsOutSequence = DOTween.Sequence();

        cosmeticsOutSequence.Append(cosmeticsSet.transform.DOLocalMoveX(-1800, 0.5f).SetEase(Ease.InQuad));

        cosmeticsOutSequence.Append(camera.transform.DOLocalMoveX(0, 0.5f).SetEase(Ease.OutQuad));

        cosmeticsOutSequence.Append(titleText.transform.DOLocalMoveY(0, 0.25f).SetEase(Ease.OutQuad));
        cosmeticsOutSequence.Append(tutorialButton.transform.DOLocalMoveY(-449, 0.25f).SetEase(Ease.OutQuad));
        cosmeticsOutSequence.Append(cosmeticsButton.transform.DOLocalMoveY(-449, 0.25f).SetEase(Ease.OutQuad));
        cosmeticsOutSequence.Append(book.transform.DOLocalMoveX(0, 0.5f).SetEase(Ease.OutQuad));
        cosmeticsOutSequence.Append(secondaryButtonSet.transform.DOLocalMoveX(549, 0.25f).SetEase(Ease.OutQuad));
        cosmeticsOutSequence.Append(primaryButtonSet.transform.DOLocalMoveX(549, 0.25f).SetEase(Ease.OutQuad));

        cosmeticsOutSequence.OnComplete(() =>
        {
            isLoading = false;
        });
    }
}
