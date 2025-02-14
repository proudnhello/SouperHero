using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FlavorIngredient;

[CreateAssetMenu(fileName = "New Item", menuName = "Ingredient/Create New Ability Ingredient")]
public class AbilityIngredient : Ingredient
{
    [Header("Ability Info")]
    public AbilityAbstractClass ability;
    public AbilityStats baseStats;
    public List<InflictionFlavor> inherentInflictionFlavors;
    public int uses;
}