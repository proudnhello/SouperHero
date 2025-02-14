using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class CookingUI : MonoBehaviour
{
    public Transform InventoryContent;
    public Transform CookingContent;
    public GameObject InventorySlot;
    public GameObject DraggableItem;

    public void OnEnable()
    {

        // clean before use
        foreach (Transform item in InventoryContent)
        {
            Destroy(item.gameObject);
        }

        foreach (Transform slot in CookingContent)
        {
            foreach (Transform item in slot)
            {
                Destroy(item.gameObject);
            }
        }

        List<Ingredient> currentInventory = PlayerInventory.Singleton.ingredientsHeld;

        foreach (var ingredient in currentInventory)
        {
            // Instantiate the InventorySlot prefab into InventoryContent
            GameObject obj = Instantiate(InventorySlot, InventoryContent);

            if (obj == null)
            {
                Debug.LogError("Object instantiation failed.");
                return;
            }

            // Instantiate draggableItem as a child of the InventorySlot
            GameObject draggableInstance = Instantiate(DraggableItem, obj.transform);

            if (draggableInstance == null)
            {
                Debug.LogError("Draggable item instantiation failed.");
                return;
            }

            // Find ItemName and ItemIcon transforms once
            Transform itemIconTransform = obj.transform.Find("Item(Clone)");
            if (itemIconTransform == null)
            {
                Debug.LogError("Could not find ItemIcon in instantiated object.");
                return;
            }

            // Find the ItemName inside ItemIcon and ensure the components exist
            Transform itemNameTransform = itemIconTransform.Find("ItemName");
            if (itemNameTransform == null)
            {
                Debug.LogError("Could not find ItemName inside ItemIcon.");
                return;
            }

            // Get the components from the found transforms
            TextMeshProUGUI itemName = itemNameTransform.GetComponent<TextMeshProUGUI>();
            UnityEngine.UI.Image itemIcon = itemIconTransform.GetComponent<UnityEngine.UI.Image>();

            if (itemName == null)
            {
                Debug.LogError("itemName is null, couldn't find the TextMeshProUGUI component.");
                return;
            }

            if (itemIcon == null)
            {
                Debug.LogError("itemIcon is null, couldn't find the Image component.");
                return;
            }

            // Update the UI components with ingredient values
            itemName.text = ingredient.ingredientName;
            itemIcon.sprite = ingredient.icon;

            // Set the ingredient reference in the draggable item
            DraggableItem draggableItemComponent = draggableInstance.GetComponent<DraggableItem>();
            draggableItemComponent.ingredient = ingredient;

            // Change the Alpha
            Color currentColor = itemIcon.color;
            currentColor.a = 1f;
            itemIcon.color = currentColor;
        }
    }
}