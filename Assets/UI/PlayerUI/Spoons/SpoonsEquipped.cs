using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;

public class SpoonsEquipped : MonoBehaviour
{
    [SerializeField] Image[] imageComponents;
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
            imageComponents[prevSpoon].color = Color.white;
            imageComponents[prevSpoon].rectTransform.sizeDelta = normalSize;
        }

        //Highlight current spoon
        prevSpoon = spoon;
        imageComponents[spoon].color = new Color(252f/255f, 173f/255f, 3f/255f, 1.0f);
        imageComponents[spoon].rectTransform.sizeDelta = selectedSize;
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

        transform.GetChild(spoonsLength).gameObject.SetActive(false);
        for(var i = 0; i < spoonsLength; i++) //Update new uses
        {
            SetUsesText(i);
        }
    }

    void SetUsesText(int spoon)
    {
        int uses = PlayerInventory.Singleton.GetSpoons()[spoon].uses;
        if (uses != -1)
        {
            usesTextComponents[spoon].text = uses.ToString();
        } else
        {
            usesTextComponents[spoon].text = "∞";
        }
    }
}
