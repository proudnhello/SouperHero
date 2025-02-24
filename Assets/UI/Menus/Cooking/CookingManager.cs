using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static SoupSpoon;


// Gets Items In the Cooking Slots and Call FillPot
public class CookingManager : MonoBehaviour
{
    public static CookingManager Singleton { get; private set; }

    public Transform CookingContent;
    public TMP_Text BuffText;
    public TMP_Text InflictionText;
    public TMP_Text AbilitiesText;
    public TMP_Text UsesText;
    [SerializeField] private TMP_Text WarningText;
    public GameObject CookingCanvas;
    public GameObject itemStatsScreen;
    private SoupSpoon statSpoon;
    
    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;
    }

    // Initialize Ingredient List
    public List<Ingredient> cookingIngredients = new();

    private Campfire CurrentCampfire;

    public void EnterCooking(Campfire source)
    {
        CurrentCampfire = source;
        CursorManager.Singleton.ShowCursor();
        ResetStatsText();
        CookingCanvas.SetActive(true);
        PlayerEntityManager.Singleton.input.Player.Interact.started += ExitCooking;

    }


    public void ExitCooking(InputAction.CallbackContext ctx = default)
    {
        if (CurrentCampfire != null)
        {
            CurrentCampfire.StopCooking();
            CurrentCampfire = null;
            CursorManager.Singleton.HideCursor();
            CookingCanvas.SetActive(false);
            ResetStatsText();
            PlayerEntityManager.Singleton.input.Player.Interact.started -= ExitCooking;
        }
    }
    
    // No Parameters For the Exit Button
    public void ExitCooking()
    {
        ExitCooking(default);
    }


    private void OnDisable()
    {
        PlayerEntityManager.Singleton.input.Player.Interact.started -= ExitCooking;
    }

    // Function to add an Ability Ingredient
    public void AddIngredient(Ingredient ingredient)
    {

        Debug.Log($"Added Ingredient: {ingredient}");
        cookingIngredients.Add(ingredient);
        UpdateStatsText();
    }

    // Function to remove an Ability Ingredient
    public void RemoveIngredient(Ingredient ingredient)
    {
        Debug.Log($"Removed Ingredient: {ingredient}");
        cookingIngredients.Remove(ingredient);
        UpdateStatsText();
    }

    // Check if there is an ability ingredient in the pot
    public bool HasAbilityIngredient()
    {
        // Don't cook if there are no ability ingredients
        foreach (Ingredient ingredient in cookingIngredients)
        {
            Debug.Log(ingredient.ingredientName);
            if (ingredient.GetType() == typeof(AbilityIngredient))
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

    public void DisplayWarning()
    {
        WarningText.gameObject.SetActive(true);
    }

    public void HideWarning()
    {
        WarningText.gameObject.SetActive(false);
    }

    // Call this to cook the soup
    public void CookTheSoup()
    {
        // Don't cook if there is no ability ingredient, return early
        if (!HasAbilityIngredient())
        {
            DisplayWarning();
            return;
        } else
        {
            HideWarning();
        }

        // Cook the soup with what is currently in the pot
        PlayerInventory.Singleton.CookSoup(cookingIngredients);

        cookingIngredients.Clear();
        ResetStatsText();

        // Destroy the objects that were cooked
        foreach (Transform slot in CookingContent)
        {
            foreach (Transform item in slot)
            {
                Destroy(item.gameObject);
            }
        }
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
        statSpoon = new SoupSpoon(cookingIngredients);
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
        BuffText.text += "<color=yellow>Sour (Duration):</color> " + totalDuration + "\n";
        BuffText.text += "<color=#00FF00>Bitter (Size):</color> " + totalSize + "\n";
        BuffText.text += "<color=orange>Salty (Crit):</color> " + totalCrit + "\n";
        BuffText.text += "<color=purple>Sweet (Speed):</color> " + totalSpeed + "\n";
        BuffText.text += "<color=blue>Cooldown:</color> " + totalCooldown + "\n";

        // update infliction text
        InflictionText.text += "<color=red>Spicy (Burn):</color>" + "\n" + "| Add "+ totalAddBurn + " | Mult " + totalMultBurn + " |\n";
        InflictionText.text += "<color=#00FFFF>Frosty (Freeze):</color>" + "\n" + "| Add " + totalAddFreeze + " | Mult " + totalMultFreeze + " |\n";
        InflictionText.text += "<color=green>Hearty (Healing):</color>" + "\n" + "| Add " + totalAddHealing + " | Mult " + totalMultHealing + " |\n";
        InflictionText.text += "<color=#FF00FF>Spiky (Damage):</color>" + "\n" + "| Add " + totalAddDamage + " | Mult " + totalMultDamage + " |\n";
        InflictionText.text += "<color=#8B4513>Greasy (Knockback):</color>" + "\n" + "| Add " + totalAddKnockback + " | Mult " + totalMultKnockback + " |\n";
    }

}
