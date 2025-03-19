using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MainMenuAnims : MonoBehaviour
{
    public GameObject blackFade;
    public GameObject titleText;
    public GameObject book;
    public RectTransform firstButton;
    public RectTransform secondButton;
    public RectTransform thirdButton;
    public RectTransform fourthButton;

    public RectTransform fifthButton;
    public RectTransform sixthButton;
    public RectTransform seventhButton;

    public GameObject secondaryButtonSet;
    public GameObject primaryButtonSet;

    public Animator playerAnimator;

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
        s.Append(fourthButton.DOAnchorPosX(42, 0.65f).SetEase(Ease.OutQuad));
        s.OnComplete(() => { isDone = true; });

        moveInSecondarySequence = DOTween.Sequence();

        moveInSecondarySequence.Append(firstButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.InQuad));
        moveInSecondarySequence.Append(secondButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.InQuad));
        moveInSecondarySequence.Append(thirdButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.InQuad));
        moveInSecondarySequence.Append(fourthButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.InQuad));

        moveInSecondarySequence.Append(fifthButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));
        moveInSecondarySequence.Append(sixthButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));
        moveInSecondarySequence.Append(seventhButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));

        moveInSecondarySequence.SetAutoKill(false);
        moveInSecondarySequence.Pause();

        moveOutSecondarySequence = DOTween.Sequence();

        moveOutSecondarySequence.Append(fifthButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.OutQuad));
        moveOutSecondarySequence.Append(sixthButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.OutQuad));
        moveOutSecondarySequence.Append(seventhButton.DOAnchorPosX(194, 0.25f).SetEase(Ease.OutQuad));

        moveOutSecondarySequence.Append(firstButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));
        moveOutSecondarySequence.Append(secondButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));
        moveOutSecondarySequence.Append(thirdButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));
        moveOutSecondarySequence.Append(fourthButton.DOAnchorPosX(42, 0.25f).SetEase(Ease.OutQuad));

        moveOutSecondarySequence.SetAutoKill(false);
        moveOutSecondarySequence.Pause();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            playerAnimator.SetTrigger("Left Click");
            if(!isDone)
            {
                s.Kill();
                titleText.transform.localPosition = new Vector3(titleText.transform.localPosition.x, 0, titleText.transform.localPosition.z);
                blackFade.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                book.transform.localPosition = new Vector3(0, book.transform.localPosition.y, book.transform.localPosition.z);
                firstButton.anchoredPosition = new Vector2(42, firstButton.anchoredPosition.y);
                secondButton.anchoredPosition = new Vector2(42, secondButton.anchoredPosition.y);
                thirdButton.anchoredPosition = new Vector2(42, thirdButton.anchoredPosition.y);
                fourthButton.anchoredPosition = new Vector2(42, fourthButton.anchoredPosition.y);
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

    public void BackButton()
    {
        StartCoroutine("backCoroutine");
    }
}
