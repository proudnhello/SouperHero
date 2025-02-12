using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CookingSlot : InventorySlot, IDropHandler
{
    public GameObject cookingSlot;
    // Slightly modifying OnDrop From the base class
    public new void OnDrop(PointerEventData eventData)
    {
        // Call old drop
        base.OnDrop(eventData);
    }
}
