using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FlavorIngredient;

[CreateAssetMenu(fileName = "New Item", menuName = "Base/New Base")]

public class SoupBase : ScriptableObject, ISoupBowl
{
    public string baseName;
    public Sprite baseSprite;
    public Sprite finishedSprite;

    public float cooldown;
    public int maxFlavorIngredients;
    public int maxAbilityIngredients;
    public int maxWildcardIngredients;
    public int maxIngredients;

    public List<InflictionFlavor> inherentInflictionFlavors;
    public List<BuffFlavor> inherentBuffFlavors;
}
