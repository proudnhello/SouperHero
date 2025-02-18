using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Playables;
using UnityEngine;


// Gets Items In the Cooking Slots and Call FillPot
public class CookingManager : MonoBehaviour
{
    public static CookingManager Singleton { get; private set; }

    public Transform CookingContent;
    public TMP_Text BuffText;
    public TMP_Text InflictionText;
    public TMP_Text AbilitiesText;
    public GameObject CookingCanvas;
    private SoupSpoon statSpoon;
    
    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;
    }

    // Initialize Ingredient List
    public List<Ingredient> cookingIngredients = new();


    // Function to add an Ability Ingredient
    public void AddIngredient(Ingredient ingredient)
    {
        cookingIngredients.Add(ingredient);
        UpdateStatsText();
    }

    // Function to remove an Ability Ingredient
    public void RemoveIngredient(Ingredient ingredient)
    {
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

    // Call this to cook the soup
    public void CookTheSoup()
    {
        // Don't cook if there is no ability ingredient, return early
        if (!HasAbilityIngredient())
        {
            Debug.Log("FillSpoon: Ability list is empty! Can't cook without an ability ingredient");
            return;
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
        BuffText.text = "Buff Flavors:\n";
        InflictionText.text = "Infliction Flavors:\n";
        AbilitiesText.text = "Abilities:\n";
        statSpoon = new SoupSpoon(cookingIngredients);
        float totalDuration = 0;
        float totalSize = 0;
        float totalCrit = 0;
        float totalSpeed = 0;
        float totalCooldown = 0;

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

        foreach(var spoonInfliction in statSpoon.spoonInflictions){
            // get the name of each infliction
            InflictionText.text += spoonInfliction.InflictionFlavor.inflictionType + "\n";
        }

        BuffText.text += "Sour (Duration): " + totalDuration + "\n";
        BuffText.text += "Bitter (Size): " + totalSize + "\n";
        BuffText.text += "Salty (Critical Strike): " + totalCrit + "\n";
        BuffText.text += "Sweet (Speed): " + totalSpeed + "\n";
        BuffText.text += "Cooldown: " + totalCooldown + "\n";
    }

}
