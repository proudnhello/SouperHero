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
            }
        }
    }

    public void ResetTrigger()
    {
        playerAnimator.ResetTrigger("Left Click");
    }
}
