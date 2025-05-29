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

    internal int soupInventoryIndex = -1;
    internal SoupBase soupBaseReference;

    public void MouseDownOn()
    {
        if (soupBaseReference == null)
        {
            int slot = SoupInventoryUI.Singleton.SelectBowlCookingSlot();
            if (slot >= 0)
            {
                soupBaseReference = (SoupBase)PlayerInventory.Singleton.GetBowl(slot);
                soupInventoryIndex = slot;
                SlotOutline.gameObject.SetActive(false);
                SlotContent.gameObject.SetActive(true);
                SlotContent.sprite = soupBaseReference.baseSprite;
            }
        }
        else
        {
            SoupInventoryUI.Singleton.DeselectBowlCookingSlot(soupInventoryIndex);
            RemoveBowl();
        }
    }

    public void RemoveBowl()
    {
        soupBaseReference = null;
        soupInventoryIndex = -1;
        SlotOutline.gameObject.SetActive(true);
        usesText.text = "";
        SlotContent.gameObject.SetActive(false);
    }

}
