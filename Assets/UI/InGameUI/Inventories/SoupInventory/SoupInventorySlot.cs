using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class SoupInventorySlot : MonoBehaviour, ICursorInteractable
{
    [SerializeField] TMP_Text usesText;
    [SerializeField] Image SlotContent;
    [SerializeField] Sprite EmptySoupSlotSprite;
    internal ISoupBowl bowlHeld;
    int slotIndex;

    public void Init(int index, ISoupBowl bowl)
    {
        slotIndex = index;
        bowlHeld = bowl;
        RenderSlot();
    }

    public void EquipSlot()
    {
        SlotContent.color = Color.white;
        SlotContent.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }

    public void UnequipSlot()
    {
        SlotContent.color = new Color(.5f, .5f, .5f, .8f);
        SlotContent.transform.localScale = new Vector3(.6f, .6f, .6f);
    }

    void RenderSlot()
    {
        SlotContent.enabled = true;
        SlotContent.rectTransform.sizeDelta = new Vector2(123, 75);
        SlotContent.color = Color.white;
        SlotContent.transform.localScale = Vector3.one;
        usesText.text = "";
        if (bowlHeld is FinishedSoup finishedSoup)
        {
            SlotContent.sprite = finishedSoup.soupBase.finishedSprite;
            if (finishedSoup.uses < 0) usesText.text = "∞";
            else usesText.text = finishedSoup.uses.ToString();
        }
        else if (bowlHeld is SoupBase soupBase)
        {
            if (CookingScreen.Singleton.BowlCookingSlot.soupSlotReference == slotIndex) AddBowlToCookingSlot();
            else RemoveBowlFromCookingSlot();
            SlotContent.sprite = soupBase.baseSprite;
        }
        else
        {
            SlotContent.rectTransform.sizeDelta = new Vector2(75, 75);
            SlotContent.sprite = EmptySoupSlotSprite;
            SlotContent.enabled = SoupInventoryUI.Singleton.IsOpen;
        }
    }

    public void EnterInventoryScreen()
    {

        RenderSlot();
    }

    public void ExitInventoryScreen()
    {
        RenderSlot();
    }

    public void SelectSlot()
    {
        SlotContent.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }

    public void DeselectSlot()
    {
        SlotContent.transform.localScale = Vector3.one;
    }

    public void AddBowlToCookingSlot() 
    {
        SlotContent.color = new Color(.7f, .7f, .7f, .4f);
    }

    public void RemoveBowlFromCookingSlot()
    {
        SlotContent.color = Color.white;
    }

    public void SetSoup(ISoupBowl bowl)
    {
        bowlHeld = bowl;
        RenderSlot();
    }

    public void UpdateUseCount()
    {
        if (bowlHeld is FinishedSoup finishedSoup)
        {
            if (finishedSoup.uses < 0) usesText.text = "∞";
            else usesText.text = finishedSoup.uses.ToString();
        }
    }

    public void MouseDownOn()
    {
        if (!SoupInventoryUI.Singleton.IsOpen) return;
        SoupInventoryUI.Singleton.SetSelectedSoup(slotIndex);
    }
}
