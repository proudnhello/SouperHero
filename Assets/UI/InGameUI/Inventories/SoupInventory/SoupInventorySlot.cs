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
        SlotContent.color = new Color(1, 1, 1, .25f);
        SlotContent.transform.localScale = Vector3.one;
    }

    void RenderSlot()
    {
        if (bowlHeld is FinishedSoup finishedSoup)
        {
            SlotContent.enabled = true;
            SlotContent.sprite = finishedSoup.soupBase.finishedSprite;
        }
        else if (bowlHeld is SoupBase soupBase)
        {
            SlotContent.enabled = true;
            SlotContent.sprite = soupBase.baseSprite;
        }
        else
        {
            SlotContent.enabled = false;
        }
    }

    public void EnterCookingScreen()
    {
        SlotContent.color = Color.white;
        SlotContent.transform.localScale = Vector3.one;
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
        SlotContent.color = new Color(1, 1, 1, .25f);
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
        usesText.text = ((FinishedSoup)bowlHeld).uses.ToString();
    }

    public void MouseDownOn()
    {
        if (bowlHeld is not SoupBase && bowlHeld is not FinishedSoup)
        {
            SoupInventoryUI.Singleton.SetSelectedSoup(slotIndex);
        }
    }
}
