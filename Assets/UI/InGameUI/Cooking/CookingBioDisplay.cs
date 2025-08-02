using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CookingBioDisplay : MonoBehaviour
{
    CookingScreen cs;
    public Sprite[] tickMarkSprites;
    [SerializeField] FlavorBioTicks[] FlavorIcons;
    [SerializeField] StatTooltip CooldownStat;
    [SerializeField] StatTooltip UsesStat;
    [SerializeField] TMP_Text PlaceBowlText;
    [SerializeField] string PlaceBowlKey;
    public void Init(CookingScreen cs)
    {
        this.cs = cs;
        foreach (var icon in FlavorIcons) 
        {
            icon.Init();
        }
        PlaceBowlText.text = LocalizationManager.GetLocalizedString(PlaceBowlKey);
    }

    public void DisplayBowl(SoupBase bowl)
    {
        CooldownStat.gameObject.SetActive(true);
        CooldownStat.SetStat(bowl.cooldown, Color.white);
        UsesStat.SetStat(0, Color.white);
        UsesStat.SetColliderSize(1);
        DisplayIngredients(new());
        PlaceBowlText.gameObject.SetActive(false);
    }

    public void ClearDisplay()
    {
        ClearIngredients();
        CooldownStat.gameObject.SetActive(false);
        UsesStat.Clear();
        PlaceBowlText.gameObject.SetActive(true);
    }

    public void ClearIngredients()
    {
        foreach (var icon in FlavorIcons) icon.Clear();
    }

    public void DisplayIngredients(List<Ingredient> cookedIngredients)
    {
        ClearIngredients();

        FinishedSoup bowl = new(cookedIngredients, cs.BowlCookingSlot.soupBaseReference);
        int iconUsed = 0;
        foreach (var buff in bowl.soupBuffStats.Values)
        {
            FlavorIcons[iconUsed].Set(buff);
            iconUsed++;
        }
        foreach (var inf in bowl.soupInflictionStats.Values)
        {
            FlavorIcons[iconUsed].Set(inf);
            iconUsed++;
        }

        Color cooldownColor = bowl.cooldown < bowl.soupBase.cooldown ? BioDatabase.Singleton.BuffFlavorIcons[FlavorIngredient.BuffFlavor.BuffType.SWEET_Speed].COLOR : Color.white;
        CooldownStat.SetStat(bowl.cooldown, cooldownColor);
        
        UsesStat.SetStat(bowl.uses, Color.white);
        int places = 1;
        int d = bowl.uses;
        while (d > 9) { d %= 10; places++; }
        UsesStat.SetColliderSize(places);

        foreach (var slot in cs.IngredientCookingSlots)
        {
            if (slot.HasAbilityIngredient())
            {
                slot.SetAbilityStat(bowl.soupAbilities[((AbilityIngredient)slot.ingredientReference.ingredient).abilityType]);
            }
            else
            {
                slot.HideAbilityStat();
            }
        }

    }
}