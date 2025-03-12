using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FlavorIngredient;

[CreateAssetMenu(fileName = "New Item", menuName = "Ingredient/Create New Ability Ingredient")]
public class AbilityIngredient : Ingredient
{
    public string AbilityDescription;

    [Header("Ability Info")]
    public AbilityAbstractClass abilityType;
    public AbilityStats baseStats;
    public List<InflictionFlavor> inherentInflictionFlavors;
    public int uses;
}