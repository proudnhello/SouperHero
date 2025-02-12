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

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
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
    }

    // Function to remove an Ability Ingredient
    public void RemoveAbilityIngredient(AbilityIngredient ingredient)
    {
        potAbilityIngredients.Remove(ingredient);
    }

    // Function to add a Flavor Ingredient
    public void AddFlavorIngredient(FlavorIngredient ingredient)
    {
        potFlavorIngredients.Add(ingredient);
    }

    // Function to remove a Flavor Ingredient
    public void RemoveFlavorIngredient(FlavorIngredient ingredient)
    {
        potFlavorIngredients.Remove(ingredient);
    }

    //public GameObject CookingSlots;
    //public void CookIngredients()
    //{

    //    // Initialize Ability Ingredient List
    //    List<AbilityIngredient> potAbilityIngredients = new();

    //    // Initialize Flavor Ingredient List
    //    List<FlavorIngredient> potFlavorIngredients = new();

    //    // Get the transform of the cooking slots
    //    Transform cookingSlotsTransform = CookingSlots.transform;

    //    foreach (Transform child in cookingSlotsTransform)
    //    {
    //        Transform item = child.Find("Item(Clone)");
    //        Ingredient ingredient = item.GetComponent<Ingredient>();
    //        PlayerManager.instance.RemoveFromInventory(ingredient);
    //    }

    //}

}
