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
    
    [HideInInspector] public int selectedPotSpoon = 1;

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;
    }

    // Initialize Ability Ingredient List
    public List<AbilityIngredient> potAbilityIngredients = new();

    // Initialize Flavor Ingredient List
    public List<FlavorIngredient> potFlavorIngredients = new();

    // Function to add an Ability Ingredient
    public void AddAbilityIngredient(AbilityIngredient ingredient)
    {
        potAbilityIngredients.Add(ingredient);
        DisplayPotStats();
    }

    // Function to remove an Ability Ingredient
    public void RemoveAbilityIngredient(AbilityIngredient ingredient)
    {
        potAbilityIngredients.Remove(ingredient);
        DisplayPotStats();
    }

    // Function to add a Flavor Ingredient
    public void AddFlavorIngredient(FlavorIngredient ingredient)
    {
        potFlavorIngredients.Add(ingredient);
        DisplayPotStats();
    }

    // Function to remove a Flavor Ingredient
    public void RemoveFlavorIngredient(FlavorIngredient ingredient)
    {
        potFlavorIngredients.Remove(ingredient);
        DisplayPotStats();
    }
    
    public void ClearPotIngredients()
    {
        potFlavorIngredients.Clear();
        potAbilityIngredients.Clear();
    }

    public void SetSelectedPotSpoon(int newSpoonNum)
    {
        selectedPotSpoon = newSpoonNum;
        Debug.Log($"You just changed selectedPotSpoon to [{selectedPotSpoon}]");
    }

    public void CookTheSoup()
    {

        if (potAbilityIngredients == null || potAbilityIngredients.Count == 0)
        {
            Debug.Log("FillSpoon: Ability list is empty!");
            return;
        }

        // CHANGE THIS TO WHATEVER FUNCTION COOKS THE SOUP
        PlayerManager.Singleton.FillPot(potFlavorIngredients, potAbilityIngredients, selectedPotSpoon);

        // Slot Debug
        Debug.Log($"You just cooked at {selectedPotSpoon} index");

        foreach (var ingredient in potFlavorIngredients)
        {
            PlayerManager.Singleton.RemoveFromInventory(ingredient);
        }

        foreach (var ingredient in potAbilityIngredients)
        {
            PlayerManager.Singleton.RemoveFromInventory(ingredient);
        }

        ClearPotIngredients();

        foreach (Transform slot in CookingContent)
        {
            foreach (Transform item in slot)
            {
                Destroy(item.gameObject);
            }
        }
    }

    public void DisplayPotStats(){
        string stats = PlayerManager.Singleton.GetSoupStats(potFlavorIngredients, potAbilityIngredients);
        Debug.Log(stats);
    }

}
