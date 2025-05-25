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

    private Vector3 selectedSize = new Vector3(1.2f, 1.2f, 1.2f);
    private Vector3 originalSize = new Vector3(0.8f, 0.8f, 0.8f);

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
            imageComponents[prevSpoon].rectTransform.localScale = originalSize;
            SetAlpha(prevSpoon, 0.3f);
        }

        //Highlight current spoon
        prevSpoon = spoon;
        SetAlpha(spoon, 1);
        imageComponents[spoon].rectTransform.localScale = selectedSize;

        SoupUI.Singleton.SetUsesText(spoon);
    }

    void AddSpoon(int spoon)
    {
        Debug.Log("ADD SPOON");
        transform.GetChild(spoon).GetChild(0).gameObject.SetActive(true);
    }

    void RemoveSpoon(int spoon)
    {
        Debug.Log("REMOVE SPOON");
        transform.GetChild(spoon).GetChild(0).gameObject.SetActive(false);
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
