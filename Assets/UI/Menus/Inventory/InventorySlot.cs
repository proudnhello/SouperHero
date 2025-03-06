using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public GameObject inventorySlot;
    public bool basketDrop = false;
    public bool worldDrop = false;

    public Collectable ingredientReference;
    public Image faceImage;

    public void dropHelper(bool fromBasket, Collectable inventoryObject = null, PointerEventData eventData = null)
    {
        GameObject dropped;
        if(fromBasket)
        {
            dropped = inventoryObject.gameObject.transform.GetChild(1).gameObject;
        } else
        {
            dropped = eventData.pointerDrag;
        }

        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();

        if (draggableItem == null || draggableItem.previousParent == null)
        {
            Debug.Log("No Draggable Item Found!");
            return;
        }

        // check if the previous parent was a cooking slot: if so, remove it from pot ingredients
        CookingSlot cook = draggableItem.previousParent.GetComponent<CookingSlot>();
        if (cook != null && !cook.basketDrop && !cook.worldDrop)
        {
            cook.ingredientReference = null;
            cook.faceImage.sprite = null;
            // REMOVE INGREDIENT
            Debug.Log($"Parent after drag is a cooking slot: {cook.gameObject}");
            CookingManager.Singleton.RemoveIngredient(inventoryObject);
        }

        // set the parent of the dropped object to this object
        draggableItem.parentAfterDrag = transform;

        // resize the dropped object to this object
        //draggableItem.transform.localScale = inventorySlot.transform.localScale;
    }

    public void OnDrop(PointerEventData eventData)
    {
        //dropHelper(false, null, eventData);
    }

    public void updateIngredientImage(Image newImage)
    {
        faceImage.sprite = newImage.sprite;
    }
}
