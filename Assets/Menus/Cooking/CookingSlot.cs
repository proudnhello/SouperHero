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
        string ingredientType = draggableItem.ingredientType;

        Debug.Log("Ingredient Drop Detected!");

        if (ingredientType == "Ability")
        {
            Debug.Log("Ingredient Drop Ability");
            CookingManager.Singleton.AddAbilityIngredient(draggableItem.abilityIngredient);
        } else if (ingredientType == "Flavor")
        {
            Debug.Log("Ingredient Drop Flavor");
            CookingManager.Singleton.AddFlavorIngredient(draggableItem.flavorIngredient);
        } else
        {
            Debug.LogError($"Invalid Ingredient Type ({ingredientType})");
        }

    }
}
