using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CookingSlot : InventorySlot, IDropHandler
{
    // Slightly modifying OnDrop From the base class
    public new void OnDrop(PointerEventData eventData)
    {
        // Call old drop
        base.OnDrop(eventData);

        // Get The Object Dropped On This One
        GameObject dropped = eventData.pointerDrag;
        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();

        if (draggableItem == null)
        {
            Debug.Log("No Draggable Item Found!");
            return;
        }   

        // Get the Ingredient Type

        Debug.Log("Ingredient Drop Detected!");
        CookingManager.Singleton.AddIngredient(draggableItem.ingredient);
    }
}
