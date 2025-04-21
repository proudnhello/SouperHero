using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Security;
using TMPro;

public class CookingSlot : MonoBehaviour, IDropHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool basketDrop = false;
    public bool worldDrop = false;
    public Image faceImage;
    public TMP_Text usesText;

    internal Collectable ingredientReference;

    // Slightly modifying OnDrop From the base class
    public new void OnDrop(PointerEventData eventData)
    {
        if (CursorManager.Singleton.cookingCursor.currentCollectableReference == null)
        {
            CursorManager.Singleton.cookingCursor.removeCursorImage();
            return;
        }

        DraggableItem draggableItem = CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<DraggableItem>();

        if (draggableItem.worldDropped)
        {
            return;
        }

        draggableItem.parentAfterDrag = transform;

        // SET PREVIOUS SLOT TO NULL
        draggableItem.previousParent.TryGetComponent<CookingSlot>(out CookingSlot previousSlot);
        if (previousSlot != null && ingredientReference == null && this != CookingManager.Singleton.currentCookingSlot && !previousSlot.basketDrop && !previousSlot.worldDrop)
        {
            previousSlot.ingredientReference = null;
            previousSlot.faceImage.sprite = null;
            previousSlot.usesText.text = "";
            CookingManager.Singleton.RemoveIngredient(CursorManager.Singleton.cookingCursor.currentCollectableReference);
       
            CookingSlot previousParent = CookingManager.Singleton.currentCookingSlot;
            if (previousParent != null)
            {
                CookingManager.Singleton.CookingSlotSetTransparent(previousSlot);
            }
        }

        if (ingredientReference == null && this != CookingManager.Singleton.currentCookingSlot && !draggableItem.worldDropped)
        {
            draggableItem.image.color = new Color(1.0f, 1.0f, 1.0f, DraggableItem.alphaOnPickup);
            draggableItem.image.raycastTarget = false;

            CookingManager.Singleton.CookingSlotSetOpaque(this);

            ingredientReference = CursorManager.Singleton.cookingCursor.currentCollectableReference;
            updateIngredientImage(draggableItem.image);

            CookingManager.Singleton.AddIngredient(draggableItem.gameObject.transform.parent.gameObject.GetComponent<Collectable>());

            if (ingredientReference.ingredient.GetType() == typeof(AbilityIngredient)) usesText.text = ((AbilityIngredient)ingredientReference.ingredient).uses.ToString();

            draggableItem.previousParent = transform;
            draggableItem.isDragging = false;
        }
        
        draggableItem.droppedOn = true;

        CookingManager.Singleton.currentCookingSlot = null;
        CursorManager.Singleton.cookingCursor.removeCursorImage();
        Encyclopedia.Singleton.Hide();
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
            Encyclopedia.Singleton.PullUpEntry(ingredientReference.ingredient);
        }
    }

    public void updateIngredientImage(Image newImage)
    {
        faceImage.sprite = newImage.sprite;
    }


    // This is called when you stopclicking on top of a cooking slot
    public void OnPointerUp(PointerEventData eventData)
    {
        if (CursorManager.Singleton.cookingCursor.currentCollectableReference != null)
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
                if (dropTarget != null)
                {
                    dropTarget.OnDrop(eventData);
                    break;
                }
            }

            if (dropTarget == null)
            {
                Vector2 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Bounds b = CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Collider2D>().bounds;

                // spawn in UI space
                Image draggableImage = CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<DraggableItem>().image;
                draggableImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

                if (CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<DraggableItem>().previousParent.TryGetComponent<CookingSlot>(out CookingSlot previousSlot))
                {
                    CookingManager.Singleton.RemoveIngredient(previousSlot.ingredientReference);
                    previousSlot.ingredientReference = null;
                    previousSlot.faceImage.sprite = null;
                    previousSlot.usesText.text = "";
                    CookingManager.Singleton.CookingSlotSetTransparent(previousSlot);
                    previousSlot = null;    
                }

                CookingManager.Singleton.currentCookingSlot = null;
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<DraggableItem>().previousParent = CursorManager.Singleton.cookingCursor.currentCollectableReference.gameObject.transform;

                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.gameObject.transform.position = mPos;

                CursorManager.Singleton.cookingCursor.removeCursorImage();

                if (BasketUI.Singleton.basketChange.bounds.Intersects(new Bounds(new Vector3(mPos.x, mPos.y, draggableImage.gameObject.transform.root.position.z), b.size)))
                {
                    draggableImage.raycastTarget = true;
                }
            }
        }
    }
}
