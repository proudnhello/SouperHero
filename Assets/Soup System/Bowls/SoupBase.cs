using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FlavorIngredient;

[CreateAssetMenu(fileName = "New Item", menuName = "Base/New Base")]

public class SoupBase : ScriptableObject
{
    public string baseName;
    public Sprite baseSprite;

    public float cooldown;
    public int maxFlavorIngredients;
    public int maxAbilityIngredients;
    public int maxWildcardIngredients;
    public int maxIngredients;

    // Possibly could have some elegant solution with ingredients or something
    // Can't be bothered, just cope with two lists
    public List<InflictionFlavor> inherentInflictionFlavors;
    public List<BuffFlavor> inherentBuffFlavors;
}
