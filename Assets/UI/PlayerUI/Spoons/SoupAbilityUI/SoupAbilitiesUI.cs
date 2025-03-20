using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using static SoupSpoon;

public class SoupAbilitiesUI : MonoBehaviour
{

    public List<Image> children;
    public List<TextMeshProUGUI> usesTexts;

    // Start is called before the first frame update
    void Start()
    {
        // Fillout Lists
        foreach(Transform child in transform)
        {
            children.Add(child.GetComponent<Image>());
            usesTexts.Add(child.GetChild(0).GetComponent<TextMeshProUGUI>());
        }

        // Subscribe to event
        PlayerInventory.ChangedSpoon += UpdateUsesText;
    }

    void UpdateUsesText(int spoonIdx)
    {
        SoupSpoon currentSpoon = PlayerInventory.Singleton.GetSpoons()[spoonIdx];

        // set the uses text for each ability
        int abilityIdx = 0;
        foreach (SpoonAbility spoonAbility in currentSpoon.spoonAbilities)
        {
            if(spoonAbility.uses == -1)
            {
                usesTexts[abilityIdx].text = "∞";
            } else
            {
                usesTexts[abilityIdx].text = spoonAbility.uses.ToString();
            }
            
            abilityIdx++;
        }

        // set the rest of the texts to none
        for (int i = abilityIdx; i < CookingManager.Singleton.cookingSlots.Count; i++)
        {
            usesTexts[i].text = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
