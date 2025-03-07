using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Security;

public class CookingSlot : InventorySlot, IDropHandler, IPointerDownHandler, IPointerUpHandler
{
    // Slightly modifying OnDrop From the base class
    public new void OnDrop(PointerEventData eventData)
    {
        if (CursorManager.Singleton.cookingCursor.currentCollectableReference == null)
        {
            CursorManager.Singleton.cookingCursor.removeCursorImage();
            CookingManager.Singleton.disableWorldDrop();
            return;
        }

        DraggableItem draggableItem = CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<DraggableItem>();

        draggableItem.parentAfterDrag = transform;

        // SET PREVIOUS SLOT TO NULL
        CookingSlot previousSlot = draggableItem.previousParent.GetComponent<CookingSlot>();
        if (previousSlot != null && ingredientReference == null && this != CookingManager.Singleton.currentCookingSlot && !previousSlot.basketDrop && !previousSlot.worldDrop)
        {
            previousSlot.ingredientReference = null;
            previousSlot.faceImage.sprite = null;
            CookingManager.Singleton.RemoveIngredient(CursorManager.Singleton.cookingCursor.currentCollectableReference);
       
            CookingSlot previousParent = CookingManager.Singleton.currentCookingSlot;
            if (previousParent != null)
            {
                CookingManager.Singleton.CookingSlotSetTransparent(previousSlot);
            }
            Debug.Log("MadeNULL!");
        }

        // SET ACTUAL COOKING SLOT
        if (basketDrop)
        {
            basketDropNonsense(CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>());
        } else if (worldDrop)
        {
            worldDropNonsense(draggableItem);
        } else
        {
            if (ingredientReference == null && this != CookingManager.Singleton.currentCookingSlot)
            {
                //Debug.Log("Set!");

                draggableItem.image.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                draggableItem.image.raycastTarget = false;

                CookingManager.Singleton.CookingSlotSetOpaque(this);

                ingredientReference = CursorManager.Singleton.cookingCursor.currentCollectableReference;
                updateIngredientImage(draggableItem.image);

                CookingManager.Singleton.AddIngredient(draggableItem.gameObject.transform.parent.gameObject.GetComponent<Collectable>());

                draggableItem.previousParent = transform;
                draggableItem.isDragging = false;
            } else
            {
                //Debug.Log((ingredientReference == null) + ", " + (this != CookingManager.Singleton.currentCookingSlot));
            }
        }

        CookingManager.Singleton.currentCookingSlot = null;
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
            Encyclopedia.Singleton.PullUpEntry(ingredientReference.ingredient);
        }
    }

    private void basketDropNonsense(Image image)
    {
        image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        image.raycastTarget = true;
        BasketUI.Singleton.AddIngredient(CursorManager.Singleton.cookingCursor.currentCollectableReference, false);
    }

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

        d.previousParent = d.gameObject.transform.parent;
        d.parentAfterDrag = null;
    }


    // This is called when you stopclicking on top of a cooking slot
    public void OnPointerUp(PointerEventData eventData)
    {
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
            if(dropTarget != null)
            {
                dropTarget.OnDrop(eventData);
                break;
            }
        }

        if(dropTarget == null)
        {
            CursorManager.Singleton.cookingCursor.removeCursorImage();
            CookingManager.Singleton.disableWorldDrop();
        }
    }
}
