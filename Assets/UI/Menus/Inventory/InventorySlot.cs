using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public GameObject inventorySlot;
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            GameObject dropped = eventData.pointerDrag;
            DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();

            if (draggableItem == null || draggableItem.parentAfterDrag == null)
            {
                Debug.Log("No Draggable Item Found!");
                return;
            }

            // check if the previous parent was a cooking slot: if so, remove it from pot ingredients
            if(draggableItem.parentAfterDrag.GetComponent<CookingSlot>() != null)
            {
                CookingManager.Singleton.RemoveIngredient(draggableItem.ingredient);
            }

            // set the parent of the dropped object to this object
            draggableItem.parentAfterDrag = transform;

            // resize the dropped object to this object
            draggableItem.transform.localScale = inventorySlot.transform.lossyScale;

        }
    }
}
