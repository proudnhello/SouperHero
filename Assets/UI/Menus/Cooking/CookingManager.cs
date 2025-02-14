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


    public void CookTheSoup()
    {

        if (cookingIngredients == null || cookingIngredients.Count == 0)
        {
            Debug.Log("FillSpoon: Ability list is empty!");
            return;
        }

        // CHANGE THIS TO WHATEVER FUNCTION COOKS THE SOUP
        PlayerInventory.Singleton.CookSoup(cookingIngredients);

        cookingIngredients.Clear();

        foreach (Transform slot in CookingContent)
        {
            foreach (Transform item in slot)
            {
                Destroy(item.gameObject);
            }
        }
    }

}
