using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System.Collections;

public class SettingBookTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
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
    [SerializeField] Button button;

    private void Awake()
    {
        if (isQuitButton) text.text = LocalizationManager.GetLocalizedString("Exit Button");
    }

    private void OnEnable()
    {
        text.gameObject.SetActive(false);
        if (StartRevealed)
        {
            tab.anchoredPosition = new Vector2(UnselectedPosX, tab.localPosition.y);
            button.enabled = true;
        }
        else
        {
            tab.anchoredPosition = new Vector2(HiddenPosX, tab.localPosition.y);
            button.enabled = false;
        }
    }


    bool isSelected;
    bool isHovering;
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (!button.interactable) return;
        HoverTab();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (!isSelected)
        {
            UnhoverTab();
        }
    }

    Sequence tabPosSequence;
    public void RevealTab(bool startSelected = false)
    {
        tabPosSequence?.Kill();

        isSelected = startSelected;
        tab.anchoredPosition = new Vector2(HiddenPosX, tab.localPosition.y);
        tabPosSequence = DOTween.Sequence();
        tabPosSequence.Append(tab.DOAnchorPos(new Vector2(isSelected ? SelectedPosX : UnselectedPosX, tab.localPosition.y), RevealAnimTime).SetEase(Ease.OutQuad)).SetUpdate(true);
        tabPosSequence.OnComplete(() => { button.enabled = true; });
        text.gameObject.SetActive(isSelected);
    }

    public void HideTab()
    {
        tabPosSequence?.Kill();
        tabPosSequence = DOTween.Sequence();
        button.enabled = false;
        tabPosSequence.Append(tab.DOAnchorPos(new Vector2(HiddenPosX, tab.localPosition.y), RevealAnimTime).SetEase(Ease.OutQuad)).SetUpdate(true);
        tabPosSequence.Play();
    }
    public void HoverTab()
    {
        tabPosSequence?.Kill();

        button.enabled = true;
        text.gameObject.SetActive(true);
        tabPosSequence = DOTween.Sequence();
        tabPosSequence.Append(tab.DOAnchorPos(new Vector2(SelectedPosX, tab.localPosition.y), SelectAnimTime).SetEase(Ease.OutQuad)).SetUpdate(true);
        tabPosSequence.Play();
    }

    public void UnhoverTab()
    {
        tabPosSequence?.Kill();

        text.gameObject.SetActive(false);
        tabPosSequence = DOTween.Sequence();
        tabPosSequence.Append(tab.DOAnchorPos(new Vector2(UnselectedPosX, tab.localPosition.y), SelectAnimTime).SetEase(Ease.OutQuad)).SetUpdate(true);
        quitPhase = 0;
        if (isQuitButton) text.text = LocalizationManager.GetLocalizedString("Exit Button");
        tabPosSequence.Play();
    }

    public void InstantUnhover()
    {
        tabPosSequence?.Kill();
        text.gameObject.SetActive(false);
        tab.localPosition = new Vector2(UnselectedPosX, tab.localPosition.y);
        isSelected = false;
        isHovering = false;
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

    public void DisableButton(bool disable)
    {
        button.interactable = !disable;
        if (button.interactable && isHovering) HoverTab();
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
