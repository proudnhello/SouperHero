using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BowlCookingSlot : MonoBehaviour, ICursorInteractable
{
    [SerializeField] TMP_Text usesText;
    [SerializeField] Image SlotOutline;
    [SerializeField] Image SlotContent;

    internal int soupSlotReference = -1;
    internal SoupBase soupBaseReference = null;

    private void Start()
    {
        RemoveBowl();
    }

    public void MouseDownOn()
    {
        if (soupBaseReference == null)
        {
            int slot = SoupInventoryUI.Singleton.AddBowlToCookingSlot();
            if (slot >= 0)
            {
                AddBowlFromSlot(slot);
                CookingScreen.Singleton.CheckIfSoupIsValid();
            }
        }
        else
        {
            int slot = SoupInventoryUI.Singleton.AddBowlToCookingSlot();
            if (slot == -2) return; // if bowl selected isn't a base, don't swap
            RemoveBowl();
            if (slot >= 0)
            {
                AddBowlFromSlot(slot);
            }
            CookingScreen.Singleton.CheckIfSoupIsValid();
        }
    }

    void AddBowlFromSlot(int slot)
    {
        soupBaseReference = (SoupBase)PlayerInventory.Singleton.GetBowl(slot);
        SlotOutline.gameObject.SetActive(false);
        SlotContent.gameObject.SetActive(true);
        SlotContent.sprite = soupBaseReference.baseSprite;
        soupSlotReference = slot;
    }

    public void RemoveBowl()
    {
        if (soupBaseReference != null) SoupInventoryUI.Singleton.RemoveBowlCookingSlot(soupSlotReference);
        soupBaseReference = null;
        soupSlotReference = -1;
        SlotOutline.gameObject.SetActive(true);
        usesText.text = "";
        SlotContent.gameObject.SetActive(false);
    }

}
