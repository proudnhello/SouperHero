using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Ingredient/Create New Ability Ingredient")]
public class AbilityIngredient : Ingredient
{
    public AbilityAbstractClass ability;
    public int uses;
}