using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BowlCookingSlot : MonoBehaviour, ICursorInteractable
{
    [SerializeField] Image SlotOutline;
    [SerializeField] Image SlotContent;
    [SerializeField] Image EmptySlotIcon;

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
            CursorManager.Singleton.DropBowl();
        }
    }

    public void Tap()
    {
        if (CursorManager.Singleton.currentBowlReference != null &&
            CursorManager.Singleton.currentBowlReference == (ISoupBowl)soupBaseReference)
        {
            SoupInventoryUI.Singleton.ReturnBowlFromCookingSlot(soupSlotReference);
            SoupInventoryUI.Singleton.SoupBio.CloseBio();
            CursorManager.Singleton.DropBowl();
        }
    }

    public void ReturnItemHereFromCursor()
    {
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
}
