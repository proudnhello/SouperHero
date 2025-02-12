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

            // check if the previous parent was a cooking slot: if so, remove it from pot ingredients
            if(draggableItem.parentAfterDrag.GetComponent<CookingSlot>() != null)
            {
                if (draggableItem.ingredientType == "Ability")
                {
                    CookingManager.Singleton.RemoveAbilityIngredient(draggableItem.abilityIngredient);
                } else if (draggableItem.ingredientType == "Flavor")
                {
                    CookingManager.Singleton.RemoveFlavorIngredient(draggableItem.flavorIngredient);
                }
                else
                {
                    Debug.LogError($"Invalid Ingredient Type ({draggableItem.ingredientType})");
                }
            }

            // set the parent of the dropped object to this object
            draggableItem.parentAfterDrag = transform;

            // resize the dropped object to this object
            draggableItem.transform.localScale = inventorySlot.transform.localScale;

        }
    }
}
