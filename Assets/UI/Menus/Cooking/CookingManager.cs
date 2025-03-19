using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static SoupSpoon;
using UnityEngine.Rendering.Universal;


// Gets Items In the Cooking Slots and Call FillPot
public class CookingManager : MonoBehaviour
{
    public static CookingManager Singleton { get; private set; }

    public Transform CookingContent;
    public TMP_Text BuffText;
    public TMP_Text InflictionText;
    public TMP_Text AbilitiesText;
    public TMP_Text UsesText;
    [SerializeField] private GameObject abilityIngWarning;
    public GameObject CookingCanvas;
    public GameObject itemStatsScreen;
    private SoupSpoon statSpoon;
    bool isCooking = false;
    [SerializeField] private GameObject campfireWarning;
    public GameObject worldDrop;
    public GameObject basketDrop;
    internal CookingSlot currentCookingSlot;

    public List<CookingSlot> cookingSlots;

    public static event Action CookSoup;

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;
    }

    //// Initialize Ingredient List
    //public List<Ingredient> cookingIngredients = new();

    // Initialize Ingredient List
    [SerializeField]
    public List<Collectable> cookingIngredients = new();

    [SerializeField] internal Campfire CurrentCampfire;

    public void EnterCooking(Campfire source)
    {
        CurrentCampfire = source;
        CursorManager.Singleton.ShowCursor();
        CursorManager.Singleton.ShowCookingCursor();
        ResetStatsText();
        CookingCanvas.SetActive(true);
        CookingCanvas.transform.position = source.GetCanvasPosition();
        isCooking = true;
        ClearCookingManagerSprites();
        PlayerEntityManager.Singleton.input.Player.Interact.started += ExitCooking;
        foreach(CookingSlot c in cookingSlots)
        {
            c.ingredientReference = null;
            c.faceImage.sprite = null;
            c.usesText.text = "";
        }
    }


    public void ExitCooking(InputAction.CallbackContext ctx = default)
    {
        if (CurrentCampfire != null)
        {
            CurrentCampfire.StopPrepping();
            CurrentCampfire = null;
            CursorManager.Singleton.HideCursor();

            if(CursorManager.Singleton.cookingCursor.currentCollectableReference != null)
            {
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().raycastTarget = true;

                CursorManager.Singleton.cookingCursor.removeCursorImage();
                CursorManager.Singleton.cursorObject.SetActive(true);
                Encyclopedia.Singleton.setInActive();
            }

            CursorManager.Singleton.HideCookingCursor();
            CookingCanvas.SetActive(false);
            ResetStatsText();
            isCooking = false;
            foreach(Collectable c in cookingIngredients)
            {
                c.collectableUI.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                c.collectableUI.GetComponent<Image>().raycastTarget = true;
                c.collectableUI.GetComponent<DraggableItem>().previousParent = c.transform;
            }
            cookingIngredients.Clear();

            Transform itemStatsScreenTransform = itemStatsScreen.transform;
            if (itemStatsScreenTransform != null)
            {
                HideItemStats();
                GameObject cCanvas = CookingCanvas;
                itemStatsScreenTransform.SetParent(cCanvas.transform);
            }

            PlayerEntityManager.Singleton.input.Player.Interact.started -= ExitCooking;
        }
    }
    
    // No Parameters For the Exit Button
    public void ExitCooking()
    {
        ExitCooking(default);
    }

    public bool IsCooking()
    {
        return isCooking;
    }


    //private void OnDisable()
    //{

    //    PlayerEntityManager.Singleton.input.Player.Interact.started -= ExitCooking;
    //}

    // Function to add an Ability Ingredient
    public void AddIngredient(Collectable ingredient)
    {

        //Debug.Log($"Added Ingredient: {ingredient}");
        cookingIngredients.Add(ingredient);
        UpdateStatsText();
    }

    // Function to remove an Ability Ingredient
    public void RemoveIngredient(Collectable ingredient)
    {
        //Debug.Log($"Removed Ingredient: {ingredient}");
        cookingIngredients.Remove(ingredient);
        UpdateStatsText();
    }

    // Check if there is an ability ingredient in the pot
    public bool HasAbilityIngredient()
    {
        // Don't cook if there are no ability ingredients
        foreach (Collectable ingredient in cookingIngredients)
        {
            //Debug.Log("Has Ability Ingredient: " + ingredient.ingredient.IngredientName);
            if (ingredient.ingredient.GetType() == typeof(AbilityIngredient))
            {
                return true;
            }
        }

        return false;
    }

    public void DisplayItemStats()
    {
        itemStatsScreen.SetActive(true);
    }

    public void HideItemStats()
    {
        itemStatsScreen.SetActive(false);
    }

    public void DisplayAbilityIngWarning()
    {
        abilityIngWarning.SetActive(true);
    }

    public void HideAbilityIngWarning()
    {
        abilityIngWarning.SetActive(false);
    }

    public void ShowCampfireWarning()
    {
        campfireWarning.SetActive(true);
    }

    public void HideCampfireWarning()
    {
        campfireWarning.SetActive(false);
    }

    // Call this to cook the soup
    public void CookTheSoup()
    {
        // Don't cook if there is no ability ingredient, return early
        if (!HasAbilityIngredient())
        {
            DisplayAbilityIngWarning();
            return;
        } else
        {
            HideAbilityIngWarning();
        }

        //if (!PlayerEntityManager.Singleton.HasCooked())
        //{
        //    ShowCampfireWarning();
        //    PlayerEntityManager.Singleton.SetCooked(true);
        //}

        // turn off interactable after cooking once
        CurrentCampfire.SetInteractable(false);
        CurrentCampfire.SetHighlighted(false);

        // Trigger Transition to Break Animation
        Animator campfireAnimator = CurrentCampfire.GetComponent<Animator>();

        // Turn Off Light
        // Get the Light 2D component from the child object
        Light2D light = CurrentCampfire.transform.Find("Light 2D").GetComponent<Light2D>();
        if (light != null)
        {
            light.enabled = false;
        }

        if (CurrentCampfire.gameObject == null)
        {
            Debug.Log("Campfire Animator not found!");
        }

        if (campfireAnimator == null)
        {
            Debug.Log("Campfire Animator not found!");
        }

        campfireAnimator.SetTrigger("Cooked The Soup");

        // Cook the soup with what is currently in the pot
        List<Ingredient> cookedIngredients = new();
        foreach (Collectable ingredient in cookingIngredients)
        {
            cookedIngredients.Add(ingredient.ingredient);
        }
        PlayerInventory.Singleton.CookSoup(cookedIngredients);

        // Remove From Player Inventory
        foreach (Collectable ingredient in cookingIngredients)
        {
            PlayerInventory.Singleton.RemoveIngredientCollectable(ingredient, true);
        }

        cookingIngredients.Clear();
        ResetStatsText();

        ClearCookingManagerSprites();

        CookSoup?.Invoke();
    }

    // Sets all the sprites in the cooking slot to null and 0 alpha
    public void ClearCookingManagerSprites()
    {
        foreach (Transform slot in CookingContent)
        {
            CookingSlot cookingSlot = slot.gameObject.GetComponent<CookingSlot>();
            cookingSlot.ingredientReference = null;
            cookingSlot.faceImage.sprite = null;

            Color tempColor = cookingSlot.faceImage.color;
            tempColor.a = 0;
            cookingSlot.faceImage.color = tempColor;
            cookingSlot.usesText.text = "";
        }
    }

    // Sets all the images in the cooking slot to 1 alpha
    // Only if the cooking sprite is not null (meaning an ingredient is in it)
    public void CookingManagerSpritesSetOpaque()
    {
        foreach (Transform slot in CookingContent)
        {
            foreach (Transform item in slot)
            {
                Image image = item.gameObject.GetComponent<Image>();
                if (image.sprite != null)
                {
                    Color tempColor = image.color;
                    tempColor.a = 1;
                    item.gameObject.GetComponent<Image>().color = tempColor;
                }
            }
        }
    }

    // Turn a specific cookingslot transparent
    public void CookingSlotSetTransparent(CookingSlot slot)
    {
        // index into first child slot bc should only be 1 child
        //slot.transform.GetChild(0).GetComponent<Image>().sprite = null;
        Image image = slot.transform.GetChild(0).GetComponent<Image>();
        Color tempColor = image.color;
        tempColor.a = 0;
        slot.transform.GetChild(0).GetComponent<Image>().color = tempColor;
    }

    // Turn a specific cookingslot opaque
    public void CookingSlotSetOpaque(CookingSlot slot)
    {
        Image image = slot.transform.GetChild(0).GetComponent<Image>();
        Color tempColor = image.color;
        tempColor.a = 1;
        slot.transform.GetChild(0).GetComponent<Image>().color = tempColor;
    }

    public void ResetStatsText()
    {
        BuffText.text = "Buff Flavors:\n";
        InflictionText.text = "Infliction Flavors:\n";
        AbilitiesText.text = "Abilities:\n";
    }



    public void UpdateStatsText()
    {
        // Clear the text except for the headers
        BuffText.text = "";
        InflictionText.text = "";
        AbilitiesText.text = "Abilities:\n";
        UsesText.text = "Uses: ";
        // Cook the soup with what is currently in the pot
        List<Ingredient> cookedIngredients = new();
        foreach (Collectable ingredient in cookingIngredients)
        {
            cookedIngredients.Add(ingredient.ingredient);
        }
        statSpoon = new SoupSpoon(cookedIngredients);
        float totalDuration = 0;
        float totalSize = 0;
        float totalCrit = 0;
        float totalSpeed = 0;
        float totalCooldown = 0;

        // Display Uses
        UsesText.text += statSpoon.uses;

        // Show The Abilities and Calculate Buff Stats
        foreach(var spoonAbility in statSpoon.spoonAbilities){
            // get the name of each ability
            AbilitiesText.text += spoonAbility.ability._abilityName + "\n";
            // get the stats of each ability
            totalDuration += spoonAbility.statsWithBuffs.duration;
            totalSize += spoonAbility.statsWithBuffs.size;
            totalCrit += spoonAbility.statsWithBuffs.crit;
            totalSpeed += spoonAbility.statsWithBuffs.speed;
            totalCooldown += spoonAbility.statsWithBuffs.cooldown;

        }

        float totalAddBurn = 0;
        float totalAddFreeze = 0;
        float totalAddHealing = 0;
        float totalAddDamage = 0;
        float totalAddKnockback = 0;
        float totalMultBurn = 0;
        float totalMultFreeze = 0;
        float totalMultHealing = 0;
        float totalMultDamage = 0;
        float totalMultKnockback = 0;
        foreach (var spoonInfliction in statSpoon.spoonInflictions){
            // get the name of each infliction

            switch(spoonInfliction.InflictionFlavor.inflictionType)
            {
                case FlavorIngredient.InflictionFlavor.InflictionType.SPICY_Burn:
                    totalAddBurn += spoonInfliction.add;
                    totalMultBurn += spoonInfliction.mult;
                    break;
                case FlavorIngredient.InflictionFlavor.InflictionType.FROSTY_Freeze:
                    totalAddFreeze += spoonInfliction.add;
                    totalMultFreeze += spoonInfliction.mult;
                    break;
                case FlavorIngredient.InflictionFlavor.InflictionType.HEARTY_Health:
                    totalAddHealing += spoonInfliction.add;
                    totalMultHealing += spoonInfliction.mult;
                    break;
                case FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage:
                    totalAddDamage += spoonInfliction.add;
                    totalMultDamage += spoonInfliction.mult;
                    break;
                case FlavorIngredient.InflictionFlavor.InflictionType.GREASY_Knockback:
                    totalAddKnockback += spoonInfliction.add;
                    totalMultKnockback += spoonInfliction.mult;
                    break;
            }
        }

        // update buff text
        if (totalDuration > 0)
        {
            string sourColor = ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.SOUR_Duration]);
            BuffText.text += $"<color=#{sourColor}>Sour (Duration):</color> " + totalDuration + "\n";
        }
        if (totalSize > 0)
        {
            string bittedColor = ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.BITTER_Size]);
            BuffText.text += $"<color=#{bittedColor}>Bitter (Size):</color> " + totalSize + "\n";
        }
        if (totalCrit > 0)
        {
            string saltyColor = ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.SALTY_Crit]);
            BuffText.text += $"<color=#{saltyColor}>Salty (Crit):</color> " + totalCrit + "\n";
        }
        if (totalSpeed > 0)
        {
            string sweetColor = ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.SWEET_Speed]);
            BuffText.text += $"<color=#{sweetColor}>Sweet (Speed):</color> " + totalSpeed + "\n";
        }
        BuffText.text += "<color=blue>Cooldown:</color> " + totalCooldown + "\n";

        // update infliction text
        string spicyColor = ColorUtility.ToHtmlStringRGB(FlavorIngredient.inflictionColorMapping[FlavorIngredient.InflictionFlavor.InflictionType.SPICY_Burn]);
        if (totalAddBurn > 0 || totalMultBurn > 0)
        {
            InflictionText.text += $"<color=#{spicyColor}>Spicy (Burn):</color>\n" + (totalAddBurn > 0 ? $"+{totalAddBurn} " : "") + (totalMultBurn != 1 ? $"x{totalMultBurn} " : "") + "\n";
        }
        
        string frostyColor = ColorUtility.ToHtmlStringRGB(FlavorIngredient.inflictionColorMapping[FlavorIngredient.InflictionFlavor.InflictionType.FROSTY_Freeze]);
        if (totalAddFreeze > 0 || totalMultFreeze > 0)
        {
            InflictionText.text += $"<color=#{frostyColor}>Frosty (Freeze):</color>\n" + (totalAddFreeze > 0 ? $"+{totalAddFreeze} " : "") + (totalMultFreeze != 1 ? $"x{totalMultFreeze} " : "") + "\n";
        }
        
        string heartyColor = ColorUtility.ToHtmlStringRGB(FlavorIngredient.inflictionColorMapping[FlavorIngredient.InflictionFlavor.InflictionType.HEARTY_Health]);
        if (totalAddHealing > 0 || totalMultHealing > 0)
        {
            InflictionText.text += $"<color=#{heartyColor}>Hearty (Healing):</color>\n" + (totalAddHealing > 0 ? $"+{totalAddHealing} " : "") + (totalMultHealing != 1 ? $"x{totalMultHealing} " : "") + "\n";
        }
        
        string spikyColor = ColorUtility.ToHtmlStringRGB(FlavorIngredient.inflictionColorMapping[FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage]);
        if (totalAddDamage > 0 || totalMultDamage > 0)
        {
            InflictionText.text += $"<color=#{spikyColor}>Spiky (Damage):</color>\n" + (totalAddDamage > 0 ? $"+{totalAddDamage} " : "") + (totalMultDamage != 1 ? $"x{totalMultDamage} " : "") + "\n";
        }
        
        string greasyColor = ColorUtility.ToHtmlStringRGB(FlavorIngredient.inflictionColorMapping[FlavorIngredient.InflictionFlavor.InflictionType.GREASY_Knockback]);
        if (totalAddKnockback > 0 || totalMultKnockback > 0)
        {
            InflictionText.text += $"<color=#{greasyColor}>Greasy (Knockback):</color>\n" + (totalAddKnockback > 0 ? $"+{totalAddKnockback} " : "") + (totalMultKnockback != 1 ? $"x{totalMultKnockback} " : "") + "\n";
        }
    }
}
