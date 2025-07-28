using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BowlCookingSlot : MonoBehaviour, ICursorInteractable, ITooltipSource
{
    [SerializeField] Image SlotOutline;
    [SerializeField] Image SlotContent;
    [SerializeField] Image EmptySlotIcon;
    [SerializeField] float HoverTimeToDisplay;
    [SerializeField] float UnhoverTimeToHide;

    internal int soupSlotReference = -1;
    internal SoupBase soupBaseReference = null;
    public void EnterCookingScreen()
    {
        if (soupBaseReference != null) SoupInventoryUI.Singleton.SoupBio.TryDisplayHoverBio(soupBaseReference);

    }
    public void ExitCookingScreen()
    {
        
    }

    public void MouseDownOn()
    {
        if (soupBaseReference != null)
        {
            if (!CookingScreen.Singleton.IsCooking) return;

            SoupInventoryUI.Singleton.ClickOnSlot(-1);
            CursorManager.Singleton.PickupBowl(soupBaseReference);
            RenderSlot(false);
        }
    }

    public void MouseUpOn()
    {
        if (CursorManager.Singleton.currentBowlReference != null &&
            CursorManager.Singleton.currentBowlReference != (ISoupBowl)soupBaseReference)
        {
            SoupInventoryUI.Singleton.ReleaseOnSlot(-1);
            SoupInventoryUI.Singleton.SoupBio.ReleaseDrag();
            CursorManager.Singleton.DropBowl();
        }
    }

    public void Tap()
    {
        if (CursorManager.Singleton.currentBowlReference != null &&
            CursorManager.Singleton.currentBowlReference == (ISoupBowl)soupBaseReference)
        {
            SoupInventoryUI.Singleton.SoupBio.TryHideHoverBio(soupBaseReference);
            SoupInventoryUI.Singleton.ReturnBowlFromCookingSlot(soupSlotReference);
            CursorManager.Singleton.DropBowl();
        }
    }

    public void ReturnItemHereFromCursor()
    {
        SoupInventoryUI.Singleton.SoupBio.ReleaseDrag();
        RenderSlot(soupBaseReference != null);
    }

    public void RenderSlot(bool displayBowl)
    {
        if (soupBaseReference != null && displayBowl)
        {
            SlotOutline.gameObject.SetActive(false);
            SlotContent.gameObject.SetActive(true);
            SlotContent.sprite = soupBaseReference.baseSprite;
            EmptySlotIcon.gameObject.SetActive(false);
        }
        else
        {
            SlotOutline.gameObject.SetActive(true);
            EmptySlotIcon.gameObject.SetActive(true);
            SlotContent.gameObject.SetActive(false);
        }
    }

    public void AddBowlFromSlot(int slot)
    {
        soupSlotReference = slot;
        soupBaseReference = (SoupBase)PlayerInventory.Singleton.GetBowl(slot);
        RenderSlot(true);
    }

    public void RemoveBowl()
    {
        soupBaseReference = null;
        soupSlotReference = -1;
        RenderSlot(false);
    }

    public void OnHoverEnter()
    {
        if (soupBaseReference != null)
        {
            Debug.Log("hover over bowl");
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
            if (soupBaseReference != null) SoupInventoryUI.Singleton.SoupBio.TryDisplayHoverBio(soupBaseReference);
        }
        else
        {
            yield return new WaitForSeconds(UnhoverTimeToHide);
            if (soupBaseReference != null) SoupInventoryUI.Singleton.SoupBio.TryHideHoverBio(soupBaseReference);
        }
    }

    public void OnHoverExit()
    {
        if (soupBaseReference != null)
        {
            if (IHoverTimer != null) StopCoroutine(IHoverTimer);
            StartCoroutine(IHoverTimer = HoverTimer(false));
        }
    }
}
