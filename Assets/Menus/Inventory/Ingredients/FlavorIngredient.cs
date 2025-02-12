using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Ingredient/Create New Flavor Ingredient")]
public class FlavorIngredient : Ingredient
{
    public List<string> flavors;
    [HideInInspector] public string ingredientType = "Flavor";
}
