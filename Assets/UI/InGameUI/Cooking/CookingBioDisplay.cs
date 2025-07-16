using System.Collections.Generic;
using UnityEngine;

public class CookingBioDisplay : MonoBehaviour
{
    CookingScreen cs;
    public Sprite[] tickMarkSprites;
    [SerializeField] CookingFlavorIcon[] FlavorIcons;
    public void Init(CookingScreen cs)
    {
        this.cs = cs;
        foreach (var icon in FlavorIcons) 
        {
            icon.Init();
        }
    }

    public void DisplayBowl(SoupBase bowl)
    {
        ClearIngredients();
    }

    public void ClearDisplay()
    {
        ClearIngredients();
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
    }
}