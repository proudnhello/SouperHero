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
    }

    // Function to remove an Ability Ingredient
    public void RemoveIngredient(Ingredient ingredient)
    {
        cookingIngredients.Remove(ingredient);
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

        // Destroy the objects that were cooked
        foreach (Transform slot in CookingContent)
        {
            foreach (Transform item in slot)
            {
                Destroy(item.gameObject);
            }
        }
    }

}
