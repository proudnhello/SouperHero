using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Ingredient/Create New Flavor Ingredient")]
public class FlavorIngredient : ScriptableObject
{
    public List<string> flavors;
    public int id;
    public string ingredientName;
    public int value;
    public Sprite icon;

}
