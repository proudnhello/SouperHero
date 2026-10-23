using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class SettingBookTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] int spriteScalar = 5;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] RectTransform tab;
    [SerializeField] bool StartRevealed = true;
    [SerializeField] bool isQuitButton = false;

    [SerializeField] float RevealAnimTime;
    [SerializeField] float SelectAnimTime;

    [SerializeField] float HiddenPosX;
    [SerializeField] float UnselectedPosX;
    [SerializeField] float SelectedPosX;
    [SerializeField] float ExtendedPosX;

    private void Awake()
    {
        if (isQuitButton) text.text = LocalizationManager.GetLocalizedString("Exit Button");
        isRevealed = StartRevealed;
    }

    private void OnEnable()
    {
        text.gameObject.SetActive(false);
        if (StartRevealed)
        {
            tab.anchoredPosition = new Vector2(UnselectedPosX, tab.localPosition.y);
        }
        else tab.anchoredPosition = new Vector2(HiddenPosX, tab.localPosition.y);
    }


    bool isSelected;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isRevealed) return;
        HoverTab();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
        {
            UnhoverTab();
        }
    }

    Sequence tabPosSequence;
    bool isRevealed;
    public void RevealTab()
    {
        tabPosSequence?.Kill();

        tab.anchoredPosition = new Vector2(HiddenPosX, tab.localPosition.y);
        tabPosSequence = DOTween.Sequence();
        tabPosSequence.Append(tab.DOAnchorPos(new Vector2(UnselectedPosX, tab.localPosition.y), RevealAnimTime).SetEase(Ease.OutQuad)).SetUpdate(true);
        tabPosSequence.OnComplete(() => { isRevealed = true; });
    }
    public void HoverTab()
    {
        tabPosSequence?.Kill();

        text.gameObject.SetActive(true);
        tabPosSequence = DOTween.Sequence();
        tabPosSequence.Append(tab.DOAnchorPos(new Vector2(SelectedPosX, tab.localPosition.y), SelectAnimTime).SetEase(Ease.OutQuad)).SetUpdate(true);
        Debug.Log("hover " + gameObject.name);
    }

    public void UnhoverTab()
    {
        tabPosSequence?.Kill();

        text.gameObject.SetActive(false);
        tabPosSequence = DOTween.Sequence();
        tabPosSequence.Append(tab.DOAnchorPos(new Vector2(UnselectedPosX, tab.localPosition.y), SelectAnimTime).SetEase(Ease.OutQuad)).SetUpdate(true);
        quitPhase = 0;
        if (isQuitButton) text.text = LocalizationManager.GetLocalizedString("Exit Button");
    }

    public void KeepSelected(bool selection)
    {
        isSelected = selection;

        if (!isSelected)
        {
            UnhoverTab();
        } else
        {
            HoverTab();
        }
    }

    int quitPhase = 0;
    public void QuitTab()
    {
        if (quitPhase == 0)
        {
            text.text = LocalizationManager.GetLocalizedString("Exit Button 2");
            tabPosSequence = DOTween.Sequence();
            tabPosSequence.Append(tab.DOAnchorPos(new Vector2(ExtendedPosX, tab.localPosition.y), SelectAnimTime).SetEase(Ease.OutQuad));
        } 
        else
        {
            Application.Quit();
        }
        quitPhase++;
    }
}
