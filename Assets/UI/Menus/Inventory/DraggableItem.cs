using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using static SoupSpoon;
using static FlavorIngredient;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public GameObject draggableItem;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public string ingredientType;

    [Header("Do Not Edit, Ingredient is Set In CookingUI's Enable()")]
    public Ingredient ingredient = null;
    public void OnBeginDrag(PointerEventData eventData)
    {
        // save the original parent
        parentAfterDrag = transform.parent;

        // bring the ingredient to the front of the scene while dragging
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        // set raycast off so that when you drop on the slot
        // the drop system doesn't think you dropped it on itself
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // map item position to mouse position
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // set the parent to the parent after drag
        transform.SetParent(parentAfterDrag);

        // return raycast to true
        image.raycastTarget = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log($"Mouse entered UI element {ingredient.ingredientName}!");

        CookingManager.Singleton.DisplayItemStats();
        GameObject itemStatsScreen = CookingManager.Singleton.itemStatsScreen;

        Transform background = itemStatsScreen.transform.Find("Background");
        Transform header = background.transform.Find("Header");
        Transform body = background.transform.Find("Body");

        // move the screen
        itemStatsScreen.transform.SetParent(this.transform);
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 actualSize = new Vector2(rt.rect.width * rt.lossyScale.x, rt.rect.height * rt.lossyScale.y);
        itemStatsScreen.transform.position = new Vector2(this.transform.position.x, this.transform.position.y) + new Vector2(actualSize.x / 2, -actualSize.y / 2);
        //Debug.Log(itemStatsScreen.transform.position);
        //Debug.Log(rt.rect.size);
        //Debug.Log(this.transform.position);

        // bring to the front
        itemStatsScreen.transform.SetParent(transform.root);
        itemStatsScreen.transform.SetAsLastSibling();


        // set text
        TextMeshProUGUI headerText = header.GetComponent<TextMeshProUGUI>();
        headerText.text = ingredient.ingredientName;  

        TextMeshProUGUI bodyText = body.GetComponent<TextMeshProUGUI>();

        if (ingredient.GetType() == typeof(AbilityIngredient))
        {
            AbilityIngredient abilityIngredient = ingredient as AbilityIngredient;
            bodyText.text = $"<color=purple>Ability Ingredient</color>\nType: {abilityIngredient.ability._abilityName}\n\n";

            foreach (InflictionFlavor inflictionFlavor in abilityIngredient.inherentInflictionFlavors)
            {
                switch (inflictionFlavor.inflictionType)
                {
                    case FlavorIngredient.InflictionFlavor.InflictionType.SPICY_Burn:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            bodyText.text += "Spicy (Burn):" + " Add " + inflictionFlavor.amount + "\n";
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Spicy (Burn):" + " Mult " + inflictionFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.FROSTY_Freeze:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            bodyText.text += "Frosty (Freeze):" + " Add " + inflictionFlavor.amount + "\n";
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Frosty (Freeze):" + " Mult " + inflictionFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.HEARTY_Health:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            bodyText.text += "Hearty (Healing):" + " Add " + inflictionFlavor.amount + "\n";
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Hearty (Healing):" + " Mult " + inflictionFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            bodyText.text += "Spiky (Damage):" + " Add " + inflictionFlavor.amount + "\n";
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Spiky (Damage):" + " Mult " + inflictionFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.GREASY_Knockback:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            bodyText.text += "Greasy (Knockback):" + " Add " + inflictionFlavor.amount + "\n";
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Greasy (Knockback):" + " Mult " + inflictionFlavor.amount + "\n";
                        }
                        break;
                }
            }

            bodyText.text += "Sour (Duration): " + abilityIngredient.baseStats.duration + "\n";
            bodyText.text += "Bitter (Size): " + abilityIngredient.baseStats.size + "\n";
            bodyText.text += "Salty (Critical Strike): " + abilityIngredient.baseStats.crit + "\n";
            bodyText.text += "Sweet (Speed): " + abilityIngredient.baseStats.speed + "\n";
            bodyText.text += "Cooldown: " + abilityIngredient.baseStats.cooldown + "\n";

        } else if (ingredient.GetType() == typeof(FlavorIngredient))
        {
            FlavorIngredient flavorIngredient = ingredient as FlavorIngredient;
            bodyText.text = "<color=yellow>Flavor Ingredient</color>\n\n";

            foreach (InflictionFlavor inflictionFlavor in flavorIngredient.inflictionFlavors)
            {
                switch (inflictionFlavor.inflictionType)
                {
                    case FlavorIngredient.InflictionFlavor.InflictionType.SPICY_Burn:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            bodyText.text += "Spicy (Burn):" + " Add " + inflictionFlavor.amount + "\n";
                        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Spicy (Burn):" + " Mult " + inflictionFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.FROSTY_Freeze:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            bodyText.text += "Frosty (Freeze):" + " Add " + inflictionFlavor.amount + "\n";
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Frosty (Freeze):" + " Mult " + inflictionFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.HEARTY_Health:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            bodyText.text += "Hearty (Healing):" + " Add " + inflictionFlavor.amount + "\n";
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Hearty (Healing):" + " Mult " + inflictionFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            bodyText.text += "Spiky (Damage):" + " Add " + inflictionFlavor.amount + "\n";
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Spiky (Damage):" + " Mult " + inflictionFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.GREASY_Knockback:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            bodyText.text += "Greasy (Knockback):" + " Add " + inflictionFlavor.amount + "\n";
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Greasy (Knockback):" + " Mult " + inflictionFlavor.amount + "\n";
                        }
                        break;
                }
            }

            foreach (BuffFlavor buffFlavor in flavorIngredient.buffFlavors)
            {
                switch (buffFlavor.buffType)
                {
                    case FlavorIngredient.BuffFlavor.BuffType.BITTER_Size:
                        if (buffFlavor.operation == BuffFlavor.Operation.Add)
                        {
                            bodyText.text += "Bitter (Size):" + " Add " + buffFlavor.amount + "\n";
                        }
                        else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Bitter (Size):" + " Mult " + buffFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.BuffFlavor.BuffType.SALTY_CriticalStrike:
                        if (buffFlavor.operation == BuffFlavor.Operation.Add)
                        {
                            bodyText.text += "Salty (Critical Strike):" + " Add " + buffFlavor.amount + "\n";
                        }
                        else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Salty (Critical Strike):" + " Mult " + buffFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.BuffFlavor.BuffType.SOUR_Duration:
                        if (buffFlavor.operation == BuffFlavor.Operation.Add)
                        {
                            bodyText.text += "Sour (Duration):" + " Add " + buffFlavor.amount + "\n";
                        }
                        else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Sour (Duration):" + " Mult " + buffFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.BuffFlavor.BuffType.UMAMI_Vampirism:
                        if (buffFlavor.operation == BuffFlavor.Operation.Add)
                        {
                            bodyText.text += "Cooldown:" + " Add " + buffFlavor.amount + "\n";
                        }
                        else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Cooldown:" + " Mult " + buffFlavor.amount + "\n";
                        }
                        break;
                    case FlavorIngredient.BuffFlavor.BuffType.SWEET_Speed:
                        if (buffFlavor.operation == BuffFlavor.Operation.Add)
                        {
                            bodyText.text += "Sweet (Speed):" + " Add " + buffFlavor.amount + "\n";
                        }
                        else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
                        {
                            bodyText.text += "Sweet (Speed):" + " Mult " + buffFlavor.amount + "\n";
                        }
                        break;
                }
            }
        } else
        {
            Debug.LogError("Invalid Ingredient Type");
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log($"Mouse exited UI element {ingredient.ingredientName}!");
        CookingManager.Singleton.HideItemStats();

        GameObject itemStatsScreen = CookingManager.Singleton.itemStatsScreen;
        GameObject CookingCanvas = CookingManager.Singleton.CookingCanvas;
        itemStatsScreen.transform.SetParent(CookingManager.Singleton.CookingCanvas.transform);

    }

    public void OnDestroy()
    {
        Transform itemStatsScreenTrasnform = this.transform.Find("IngredientStats");
        if (itemStatsScreenTrasnform != null)
        {
            CookingManager.Singleton.HideItemStats();
            GameObject CookingCanvas = CookingManager.Singleton.CookingCanvas;
            itemStatsScreenTrasnform.SetParent(CookingManager.Singleton.CookingCanvas.transform);
        } 
    }

}
