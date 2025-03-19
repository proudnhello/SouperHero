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

    public Animator playerAnimator;

    private Sequence s;

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
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            playerAnimator.SetTrigger("Left Click");
        }
    }

    public void ResetTrigger()
    {
        playerAnimator.ResetTrigger("Left Click");
    }
}
