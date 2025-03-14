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
        imageComponents[prevSpoon].rectTransform.sizeDelta = selectedSize;
        usesTextComponents[spoon].text = PlayerInventory.Singleton.GetSpoons()[spoon].uses.ToString();
    }

    //Enable spoon image when cooked
    void AddSpoon(int spoon)
    {
        this.gameObject.transform.GetChild(spoon).gameObject.SetActive(true);
        //imageComponents[spoon].enabled = true;
    }

    //Disable spoon image when uses run out
    void RemoveSpoon(int spoon)
    {
        this.gameObject.transform.GetChild(spoon).gameObject.SetActive(false);
        //imageComponents[spoon].enabled = false;
    }
}
