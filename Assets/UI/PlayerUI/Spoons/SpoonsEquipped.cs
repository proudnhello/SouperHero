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
   // private Vector2 selectedSize = new Vector2(123, 75);

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
            imageComponents[prevSpoon].rectTransform.localScale = new Vector3(.66f, .66f, .66f);
            SetAlpha(prevSpoon, 0.3f);
        }

        //Highlight current spoon
        prevSpoon = spoon;
        SetAlpha(spoon, 1);

        imageComponents[spoon].rectTransform.localScale = new Vector3(1f, 1f, 1f);
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
        transform.GetChild(spoon).gameObject.SetActive(false);
    }

    void SetUsesText(int spoon)
    {
        SoupSpoon soupSpoon = PlayerInventory.Singleton.GetSpoons()[spoon]; //Get current spoon

        if (soupSpoon.uses != -1)
        {
            usesTextComponents[spoon].text = soupSpoon.uses.ToString();
        } else
        {
            usesTextComponents[spoon].text = "∞";
        }
    }

    void SetAlpha(int spoon, float alphaAmount) //Set alpha value of spoons (more or less transparent)
    {
        Color imageColor = imageComponents[spoon].color;
        Color textColor = usesTextComponents[spoon].color;

        //Set alpha of soup image
        imageColor.a = alphaAmount;
        imageComponents[spoon].color = imageColor;

        //Set alpha of uses text
        textColor.a = alphaAmount;
        usesTextComponents[spoon].color = textColor;
    }
}
