/*
 * An old version of this file was modified with the help of LLMs: 
 * https://github.com/djlouie/project-soup-chat-logs/blob/main/logs/log10.md
 */

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class SoupInventorySlot : MonoBehaviour, ICursorInteractable, ITooltipSource
{
    [SerializeField] TMP_Text usesText;
    [SerializeField] Image SlotContent;
    [SerializeField] Sprite EmptySoupSlotSprite;
    [SerializeField] float HoverTimeToDisplay;
    [SerializeField] float UnhoverTimeToHide;
    internal ISoupBowl bowlHeld;
    bool HasBowl { get => bowlHeld is FinishedSoup || bowlHeld is SoupBase; }
    int slotIndex;
    bool isSelected = false;

    public void Init(int index, ISoupBowl bowl)
    {
        slotIndex = index;
        bowlHeld = bowl;
        RenderSlotContents();
    }
    public void SetSoup(ISoupBowl bowl)
    {
        bowlHeld = bowl;
        RenderSlotContents();
    }

    public void EquipSlot()
    {
        SlotContent.color = Color.white;
        SlotContent.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        SoupInventoryUI.Singleton.EnableFlavorParticles(bowlHeld, this.gameObject);
        RenderSlotContents();
    }

    public void UnequipSlot()
    {
        SlotContent.color = new Color(.5f, .5f, .5f, .8f);
        SlotContent.transform.localScale = new Vector3(.6f, .6f, .6f);
        SoupInventoryUI.Singleton.DisableFlavorParticles(this.gameObject);
        RenderSlotContents();
    }

    public void SelectSlot()
    {
        SlotContent.transform.localScale = new Vector3(.8f, .8f, .8f);
        isSelected = true;
    }

    public void DeselectSlot()
    {
        SlotContent.transform.localScale = Vector3.one;
        isSelected = false;
    }


    void RenderSlotContents()
    {
        SlotContent.enabled = true;
        SlotContent.rectTransform.sizeDelta = new Vector2(123, 75);
        usesText.text = "";
        if (bowlHeld is FinishedSoup finishedSoup)
        {
            SlotContent.sprite = finishedSoup.soupBase.finishedSprite;
            if (finishedSoup.uses < 0) usesText.text = "∞";
            else usesText.text = finishedSoup.uses.ToString();
        }
        else if (bowlHeld is SoupBase soupBase)
        {
            SlotContent.sprite = soupBase.baseSprite;
        }
        else
        {
            SlotContent.rectTransform.sizeDelta = new Vector2(82, 50);
            SlotContent.sprite = EmptySoupSlotSprite;
            SlotContent.enabled = SoupInventoryUI.Singleton.IsOpen;
        }
    }

    public void EnterInventoryScreen()
    {
        SlotContent.color = Color.white;
        if (CookingScreen.Singleton.BowlCookingSlot.soupSlotReference == slotIndex) SelectSlot();
        else DeselectSlot();
        RenderSlotContents();
    }

    public void UpdateUseCount()
    {
        if (bowlHeld is FinishedSoup finishedSoup)
        {
            if (finishedSoup.uses < 0) usesText.text = "∞";
            else usesText.text = finishedSoup.uses.ToString();
        }
    }

    public void MouseDownOn() // select slot
    {
        if (HasBowl)
        {
            if (SoupInventoryUI.Singleton.IsOpen)
            {
                if (isSelected) // return from bowl slot to here
                {
                    SoupInventoryUI.Singleton.ReturnBowlFromCookingSlot(slotIndex);
                }
                else
                {
                    SoupInventoryUI.Singleton.ClickOnSlot(slotIndex);
                    CursorManager.Singleton.PickupBowl(bowlHeld);
                    SelectSlot();
                    SoupInventoryUI.Singleton.SoupBio.DragBowl(bowlHeld);
                }
            }
        }
    }

    public void ReturnItemHereFromCursor()
    {
        SoupInventoryUI.Singleton.SoupBio.ReleaseDrag();
        if (bowlHeld != null) SoupInventoryUI.Singleton.SoupBio.TryHideHoverBio(bowlHeld);
        DeselectSlot();
    }


    public void MouseUpOn()
    {
        if (CursorManager.Singleton.currentBowlReference != null)
        {
            if (CursorManager.Singleton.currentBowlReference != bowlHeld)
            {
                if (SoupInventoryUI.Singleton.ReleaseOnSlot(slotIndex))
                {
                    CursorManager.Singleton.DropBowl();
                }
            }
            SoupInventoryUI.Singleton.SoupBio.ReleaseDrag();
        }
    }
    public void Tap()
    {
        if (CursorManager.Singleton.currentBowlReference != null && CursorManager.Singleton.currentBowlReference == bowlHeld) 
        {
            if (CookingScreen.Singleton.AtCookingScreen && bowlHeld is SoupBase) // add directly to available cooking slot
            {
                SoupInventoryUI.Singleton.TapSoupSlot(slotIndex);
                CursorManager.Singleton.DropBowl();
            }
            SoupInventoryUI.Singleton.SoupBio.ReleaseDrag();    
        }
    }

    public void OnHoverEnter()
    {
        if (HasBowl)
        {
            if (IHoverTimer != null) StopCoroutine(IHoverTimer);
            StartCoroutine(IHoverTimer = HoverTimer(true));
        }
    }

    IEnumerator IHoverTimer;
    IEnumerator HoverTimer(bool enter)
    {
        if (enter)
        {
            yield return new WaitForSeconds(HoverTimeToDisplay);
            SoupInventoryUI.Singleton.SoupBio.TryDisplayHoverBio(bowlHeld);
        }
        else
        {
            yield return new WaitForSeconds(UnhoverTimeToHide);
            SoupInventoryUI.Singleton.SoupBio.TryHideHoverBio(bowlHeld);
        }
    }

    public void OnHoverExit()
    {
        if (HasBowl)
        {
            if (IHoverTimer != null) StopCoroutine(IHoverTimer);
            StartCoroutine(IHoverTimer = HoverTimer(false));
        }
    }
}
