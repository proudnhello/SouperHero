using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Ingredient/Create New Ability Ingredient")]
public class AbilityIngredient : ScriptableObject
{
    public AbilityAbstractClass ability;
    public int uses;
    public int id;
    public string ingredientName;
    public int value;
    public Sprite icon;
}