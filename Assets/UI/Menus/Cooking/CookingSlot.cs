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
        GameObject dropped = eventData.pointerDrag;
        if (!dropped.TryGetComponent<DraggableItem>(out var draggableItem))
        {
            Debug.Log("No Draggable Item Found!");
            return;
        }

        if(!ingredientReference) {
            dropHelper(true, CursorManager.Singleton.cookingCursor.currentCollectableReference, null);

            // Get the Ingredient Type
            Debug.Log("Ingredient Drop Detected!");
            if (!basketDrop && !worldDrop)
            {
                if (draggableItem.resetParent())
                {
                    // Get the Ingredient Type
                    Debug.Log("Ingredient Drop Detected! " + this.gameObject.name);
                    // set this cooking slot image alpha to 1
                    CookingManager.Singleton.CookingSlotSetOpaque(this);
                    // set previous slot image alpha to 0
                    CookingSlot previousCookingSlot = CookingManager.Singleton.currentCookingSlot;
                    if ( previousCookingSlot != null)
                    {
                        CookingManager.Singleton.CookingSlotSetTransparent(previousCookingSlot);
                        Debug.Log("PREVIOUS COOKING SLOT: " + previousCookingSlot.name);
                    }
                    CookingManager.Singleton.AddIngredient(draggableItem.gameObject.transform.parent.gameObject.GetComponent<Collectable>());
                }
            } else if (worldDrop)
            {
                worldDropNonsense(draggableItem);
            } else if (basketDrop)
            {
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().raycastTarget = true;

                // set previous slot image alpha to 0
                CookingManager.Singleton.CookingSlotSetTransparent(draggableItem.parentAfterDrag.gameObject.GetComponent<CookingSlot>());

                // Add ingredient to basket
                BasketUI.Singleton.AddIngredient(CursorManager.Singleton.cookingCursor.currentCollectableReference, false);
            }
            draggableItem.isDragging = false;
        }

        CursorManager.Singleton.cookingCursor.removeCursorImage();
        CookingManager.Singleton.disableWorldDrop();
    }

    // This is called when you click on a cooking slot
    // It sets the cursor image to the ingredient image
    // and enables world drop box
    public void OnPointerDown(PointerEventData eventData)
    {
        if ((ingredientReference != null) && (faceImage.sprite != null))
        {
            CursorManager.Singleton.cookingCursor.switchCursorImageTo(ingredientReference, faceImage);
            CookingManager.Singleton.currentCookingSlot = this;
            CookingManager.Singleton.enableWorldDrop();
        }
    }


    // Function that allows ingredients to drop into the world
    private void worldDropNonsense(DraggableItem d)
    {
        CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().raycastTarget = true;

        CursorManager.Singleton.cookingCursor.currentCollectableReference.gameObject.transform.SetParent(null);
        CursorManager.Singleton.cookingCursor.currentCollectableReference.gameObject.transform.localScale = Vector3.one;
        CursorManager.Singleton.cookingCursor.currentCollectableReference.Spawn(PlayerEntityManager.Singleton.gameObject.transform.position);
        CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableObj.gameObject.SetActive(true);
        CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.gameObject.SetActive(false);

        PlayerInventory.Singleton.RemoveIngredientCollectable(CursorManager.Singleton.cookingCursor.currentCollectableReference, false);
        CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableObj.SetInteractable(true);
        CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableObj.SetHighlighted(true);

        d.pseudoParent = d.gameObject.transform.parent;
        d.parentAfterDrag = null;
    }


    // This is called when you stopclicking on top of a cooking slot
    public void OnPointerUp(PointerEventData eventData)
    {
        if(CursorManager.Singleton.cookingCursor.currentCollectableReference == null)
        {
            CursorManager.Singleton.cookingCursor.removeCursorImage();
            CookingManager.Singleton.disableWorldDrop();
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
            if (dropTarget != null && dropTarget != CookingManager.Singleton.currentCookingSlot && !dropTarget.ingredientReference)
            {
                dropTarget.dropHelper(true, CursorManager.Singleton.cookingCursor.currentCollectableReference, null);
                break;
            }
        }

        GameObject dropped = CursorManager.Singleton.cookingCursor.currentCollectableReference.gameObject.transform.GetChild(1).gameObject;
        if (!dropped.TryGetComponent<DraggableItem>(out var draggableItem))
        {
            Debug.Log("No Draggable Item Found!");
            return;
        }

        if (dropTarget != null && dropTarget != CookingManager.Singleton.currentCookingSlot && !dropTarget.ingredientReference)
        {

            if (dropTarget.worldDrop)
            {
                worldDropNonsense(draggableItem);

                // set previous slot image alpha to 0
                CookingSlot previousCookingSlot = CookingManager.Singleton.currentCookingSlot;
                if (previousCookingSlot != null)
                {
                    CookingManager.Singleton.CookingSlotSetTransparent(previousCookingSlot);
                    Debug.Log("PREVIOUS COOKING SLOT: " + previousCookingSlot.name);
                }
            }
            else if (dropTarget.basketDrop)
            {
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().raycastTarget = true;
                draggableItem.updateParent();
                // set previous slot image alpha to 0
                CookingSlot previousCookingSlot = CookingManager.Singleton.currentCookingSlot;
                if (previousCookingSlot != null)
                {
                    CookingManager.Singleton.CookingSlotSetTransparent(previousCookingSlot);
                    Debug.Log("PREVIOUS COOKING SLOT: " + previousCookingSlot.name);
                }
                BasketUI.Singleton.AddIngredient(CursorManager.Singleton.cookingCursor.currentCollectableReference, false);
            }
            else
            {
                if (draggableItem.resetParent())
                {
                    // Get the Ingredient Type
                    Debug.Log("Ingredient Drop Detected! " + this.gameObject.name);
                    // set this cooking slot image alpha to 1
                    CookingManager.Singleton.CookingSlotSetOpaque(this);
                    // set previous slot image alpha to 0
                    CookingSlot previousCookingSlot = CookingManager.Singleton.currentCookingSlot;
                    if (previousCookingSlot != null)
                    {
                        CookingManager.Singleton.CookingSlotSetTransparent(previousCookingSlot);
                        Debug.Log("PREVIOUS COOKING SLOT: " + previousCookingSlot.name);
                    }
                    CookingManager.Singleton.AddIngredient(draggableItem.gameObject.transform.parent.gameObject.GetComponent<Collectable>());
                }
            }
        } else
        {
            if (dropTarget != null && dropTarget.worldDrop)
            {
                worldDropNonsense(draggableItem);
            }

            if(dropTarget != null && dropTarget.ingredientReference != null)
            {
                draggableItem.gameObject.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                draggableItem.gameObject.GetComponent<Image>().raycastTarget = true;
            }
        }
        CursorManager.Singleton.cookingCursor.removeCursorImage();
        CookingManager.Singleton.disableWorldDrop();
        draggableItem.isDragging = false;
    }
}
