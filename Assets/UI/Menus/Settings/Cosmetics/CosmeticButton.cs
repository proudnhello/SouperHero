using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CosmeticButton : MonoBehaviour, IPointerClickHandler
{
    public AchievementData achievementUnlock;
    public CosmeticData cosmeticUnlock;
    public UnityEvent onClickEvent;
    [SerializeField] Image Button;
    [SerializeField] Color SelectedColor;
    [SerializeField] Color UnselectedColor;
    [SerializeField] Color LockedColor;
    [SerializeField] Image SkinImage;
    [SerializeField] Material Locked;

    public void Init()
    {
        SkinImage.material = cosmeticUnlock.Material;
        DeselectButton();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickEvent?.Invoke();
    }

    public void SetAsLocked()
    {
        SkinImage.material = Locked;
        Button.color = LockedColor;
    }

    public void DeselectButton()
    {
        Button.color = UnselectedColor;
    }

    public void SelectCosmetic()
    {
        Button.color = SelectedColor;
    }
}
