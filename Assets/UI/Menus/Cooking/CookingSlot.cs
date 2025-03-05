using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

        dropHelper(true, CursorManager.Singleton.cookingCursor.currentCollectableReference, null);

        // Get the Ingredient Type
        Debug.Log("Ingredient Drop Detected!");
        if (!basketDrop && !worldDrop)
        {
            if (draggableItem.resetParent())
            {
                // Get the Ingredient Type
                Debug.Log("Ingredient Drop Detected! " + this.gameObject.name);
                CookingManager.Singleton.AddIngredient(draggableItem.gameObject.transform.parent.gameObject.GetComponent<Collectable>());
            }
        } else if (worldDrop)
        {
            CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().raycastTarget = true;

            CursorManager.Singleton.cookingCursor.currentCollectableReference.gameObject.transform.SetParent(null);
            CursorManager.Singleton.cookingCursor.currentCollectableReference.gameObject.transform.localScale = Vector3.one;
            CursorManager.Singleton.cookingCursor.currentCollectableReference.Spawn(PlayerEntityManager.Singleton.gameObject.transform.position);
            CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableObj.gameObject.SetActive(true);
            CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.gameObject.SetActive(false);

            PlayerInventory.Singleton.RemoveIngredientCollectable(CursorManager.Singleton.cookingCursor.currentCollectableReference);
            CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableObj.SetInteractable(true);
            CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableObj.SetHighlighted(true);

            CursorManager.Singleton.cookingCursor.removeCursorImage();
            CookingManager.Singleton.disableWorldDrop();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if ((ingredientReference != null) && (faceImage.sprite != null))
        {
            CursorManager.Singleton.cookingCursor.switchCursorImageTo(ingredientReference, faceImage);
            CookingManager.Singleton.cookingSlot = this;
            CookingManager.Singleton.enableWorldDrop();
        }
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

            if (dropTarget.worldDrop)
            {
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().raycastTarget = true;

                CursorManager.Singleton.cookingCursor.currentCollectableReference.gameObject.transform.SetParent(null);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.gameObject.transform.localScale = Vector3.one;
                CursorManager.Singleton.cookingCursor.currentCollectableReference.Spawn(PlayerEntityManager.Singleton.gameObject.transform.position);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableObj.gameObject.SetActive(true);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.gameObject.SetActive(false);

                PlayerInventory.Singleton.RemoveIngredientCollectable(CursorManager.Singleton.cookingCursor.currentCollectableReference);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableObj.SetInteractable(true);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableObj.SetHighlighted(true);

            }
            else if (dropTarget.basketDrop)
            {
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().raycastTarget = true;
                draggableItem.updateParent();
                BasketUI.Singleton.AddIngredient(CursorManager.Singleton.cookingCursor.currentCollectableReference, false);
            }
            else
            {
                if (draggableItem.resetParent())
                {
                    // Get the Ingredient Type
                    Debug.Log("Ingredient Drop Detected! " + this.gameObject.name);
                    CookingManager.Singleton.AddIngredient(draggableItem.gameObject.transform.parent.gameObject.GetComponent<Collectable>());
                }
            }
        }
        CursorManager.Singleton.cookingCursor.removeCursorImage();
        CookingManager.Singleton.disableWorldDrop();
    }
}
