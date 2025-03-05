using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CookingSlot : InventorySlot, IDropHandler, IPointerDownHandler, IPointerUpHandler
{
    // Slightly modifying OnDrop From the base class
    public new void OnDrop(PointerEventData eventData)
    {
        // Call old drop
        //base.OnDrop(eventData);

        // Get The Object Dropped On This One
        GameObject dropped = eventData.pointerDrag;
        if (!dropped.TryGetComponent<DraggableItem>(out var draggableItem))
        {
            Debug.Log("No Draggable Item Found!");
            return;
        }   

        // Get the Ingredient Type
        Debug.Log("Ingredient Drop Detected!");
        CookingManager.Singleton.AddIngredient(draggableItem.gameObject.transform.parent.gameObject.GetComponent<Collectable>().ingredient);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log((ingredientReference == null) + ", " + faceImage.sprite);
        if ((ingredientReference != null) && (faceImage.sprite != null))
        {
            CursorManager.Singleton.cookingCursor.switchCursorImageTo(ingredientReference, faceImage);
        }

        CookingManager.Singleton.cookingSlot = this;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(CursorManager.Singleton.cookingCursor.currentCollectableReference == null)
        {
            return;
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        CookingSlot dropTarget = null;

        foreach (RaycastResult result in results)
        {
            dropTarget = result.gameObject.GetComponent<CookingSlot>();
            if (dropTarget != null && dropTarget != CookingManager.Singleton.cookingSlot)
            {
                Debug.Log("Dropped on: " + dropTarget.name);
                dropTarget.dropHelper(true, CursorManager.Singleton.cookingCursor.currentCollectableReference, null);
                break;
            }
        }

        if (dropTarget != null && dropTarget != CookingManager.Singleton.cookingSlot)
        {
            GameObject dropped = CursorManager.Singleton.cookingCursor.currentCollectableReference.gameObject.transform.GetChild(1).gameObject;
            if (!dropped.TryGetComponent<DraggableItem>(out var draggableItem))
            {
                Debug.Log("No Draggable Item Found!");
                return;
            }

            if (draggableItem.resetParent())
            {
                // Get the Ingredient Type
                Debug.Log("Ingredient Drop Detected! " + this.gameObject.name);
                CookingManager.Singleton.AddIngredient(draggableItem.gameObject.transform.parent.gameObject.GetComponent<Collectable>().ingredient);
            }
        }
        CursorManager.Singleton.cookingCursor.removeCursorImage();
    }
}
