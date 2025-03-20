using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;
using static SoupSpoon;

public class SpoonsEquipped : MonoBehaviour
{
    [SerializeField] List<Image> imageComponents; //Image components of each spoon (no need to call GetComponent)
    [SerializeField] List<Sprite> spoonSprites; //Spoon sprites

    [SerializeField] TMP_Text[] usesTextComponents;
    private int prevSpoon = -1;

    private Vector2 normalSize = new Vector2(82, 50);
    private Vector2 selectedSize = new Vector2(123, 75);

    private void Start()
    {
        PlayerInventory.ChangedSpoon += ChangeSpoon;
        PlayerInventory.AddSpoon += AddSpoon;
        PlayerInventory.RemoveSpoon += RemoveSpoon;
        ChangeSpoon(0); //Start with default spoon
    }

    private void OnDisable()
    {
        PlayerInventory.ChangedSpoon -= ChangeSpoon;
        PlayerInventory.AddSpoon -= AddSpoon;
        PlayerInventory.RemoveSpoon -= RemoveSpoon;
    }

    void ChangeSpoon(int spoon)
    {
        if (prevSpoon >= 0) //Revert changes on previous spoon, except at game start
        {
            imageComponents[prevSpoon].rectTransform.sizeDelta = normalSize; //Reset to normal size
        }

        //Highlight current spoon
        prevSpoon = spoon;

        Debug.Log($"INDEXING INTO THE SPOON INDEX ${spoon}");
        imageComponents[spoon].rectTransform.sizeDelta = selectedSize; //Increase size
        SetUsesText(spoon);
    }

    //Enable spoon image when cooked
    void AddSpoon(int spoon)
    {
        transform.GetChild(spoon).gameObject.SetActive(true);
    }

    //Disable last spoon image when uses run out
    void RemoveSpoon(int spoon)
    {
        int spoonsLength = PlayerInventory.Singleton.GetSpoons().Count;
        transform.GetChild(spoonsLength).gameObject.SetActive(false); //Remove last spoon

        spoonSprites.RemoveAt(spoon);
        spoonSprites.Insert(3, imageComponents[spoon].sprite); //3 is last index

        UpdateSpoonImageComponents();

        for (var i = 0; i < spoonsLength; i++) //Update new uses
        {
            SetUsesText(i);
        }
    }

    void SetUsesText(int spoon)
    {

        int maxUses = 0;
        foreach (SpoonAbility ability in PlayerInventory.Singleton.GetSpoons()[spoon].spoonAbilities)
        {
            // check if we have an infinite use ingredient
            if (ability.uses == -1)
            {
                maxUses = -1;
                break;
            }

            if (ability.uses > maxUses)
            {
                maxUses = ability.uses;
            }
        }

        if (maxUses != -1)
        {
            usesTextComponents[spoon].text = maxUses.ToString();
        } else
        {
            usesTextComponents[spoon].text = "∞";
        }
    }

    void UpdateSpoonImageComponents()
    {
        for (var i = 0; i < 4; i++) {
            imageComponents[i].sprite = spoonSprites[i];
        }
    }
}
