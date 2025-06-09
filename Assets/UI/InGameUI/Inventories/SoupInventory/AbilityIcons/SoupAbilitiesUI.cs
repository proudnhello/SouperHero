using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using static FinishedSoup;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class SoupAbilitiesUI : MonoBehaviour
{
    [SerializeField] List<Image> soupAbilityIcons;

    void Start()
    {
        PlayerInventory.ChangedEquippedSoup += UpdateIcons;
        PlayerInventory.UsedSoupAttack += UpdateIcons;
        foreach (var icon in soupAbilityIcons) icon.gameObject.SetActive(false);
    }
    
    private void OnDisable()
    {
        PlayerInventory.ChangedEquippedSoup -= UpdateIcons;
        PlayerInventory.UsedSoupAttack -= UpdateIcons;
    }

    void UpdateIcons()
    {
        ISoupBowl bowl = PlayerInventory.Singleton.GetCurrentBowl();
        foreach (var icon in soupAbilityIcons) icon.gameObject.SetActive(false);

        if (bowl is FinishedSoup soup)
        {
            for (int i = 0; i < soup.soupAbilities.Count; i++)
            {
                soupAbilityIcons[i].gameObject.SetActive(true);
                soupAbilityIcons[i].sprite = soup.soupAbilities[i].iconUI;
            }
        }
    }
}
