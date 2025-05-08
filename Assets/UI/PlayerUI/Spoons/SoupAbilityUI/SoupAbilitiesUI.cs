using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using static SoupSpoon;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class SoupAbilitiesUI : MonoBehaviour
{

    internal List<Image> soupAbilityIcons = new();
    //internal List<TextMeshProUGUI> usesTexts = new();

    // Start is called before the first frame update
    void Start()
    {
        // Fillout Lists
        foreach (Transform child in transform)
        {
            soupAbilityIcons.Add(child.GetComponent<Image>());
            //usesTexts.Add(child.GetChild(0).GetComponent<TextMeshProUGUI>());
        }

        // Subscribe to event
        //PlayerInventory.ChangedSpoon += UpdateUsesText;
        PlayerInventory.ChangedSpoon += UpdateIcons;

        // Update On Start
        //UpdateUsesText(PlayerInventory.Singleton.GetCurrentSpoon());
        UpdateIcons(PlayerInventory.Singleton.GetCurrentSpoon());
    }

    private void OnDisable()
    {
        // Subscribe from event
        //PlayerInventory.ChangedSpoon -= UpdateUsesText;
        PlayerInventory.ChangedSpoon -= UpdateIcons;
    }

    /*
    void UpdateUsesText(int spoonIdx)
    {
        SoupSpoon currentSpoon = PlayerInventory.Singleton.GetSpoons()[spoonIdx];

        // set the uses text for each ability
        int abilityIdx = 0;
        foreach (SpoonAbility spoonAbility in currentSpoon.spoonAbilities)
        {
            if (spoonAbility.uses == -1)
            {
                usesTexts[abilityIdx].text = "∞";
            }
            else
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
    */

    void UpdateIcons(int spoonIdx)
    {
        SoupSpoon currentSpoon = PlayerInventory.Singleton.GetSpoons()[spoonIdx];

        // Set the icon for each current ability
        int abilityIdx = 0;
        foreach (SpoonAbility spoonAbility in currentSpoon.spoonAbilities)
        {
            if (currentSpoon.uses == 0)
            {
                soupAbilityIcons[abilityIdx].gameObject.SetActive(false);
            }
            else
            {
                soupAbilityIcons[abilityIdx].gameObject.SetActive(true);
                //Note: This if/else is temporary until all icons are finalized.
                    //Once finished, only use iconUI.
                if (spoonAbility.iconUI) 
                {
                    soupAbilityIcons[abilityIdx].sprite = spoonAbility.iconUI;
                } else
                {
                    soupAbilityIcons[abilityIdx].sprite = spoonAbility.icon;
                }
            }


            Color tempColor = soupAbilityIcons[abilityIdx].color;
            tempColor.a = 1;
            soupAbilityIcons[abilityIdx].color = tempColor;

            abilityIdx++;
        }

        // set the rest of the icons to none
        for (int i = abilityIdx; i < CookingManager.Singleton.cookingSlots.Count; i++)
        {
            soupAbilityIcons[i].sprite = null;

            Color tempColor = soupAbilityIcons[i].color;
            tempColor.a = 0;
            soupAbilityIcons[i].color = tempColor;
        }
    }
}
