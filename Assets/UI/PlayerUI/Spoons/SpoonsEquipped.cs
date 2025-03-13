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
    private int prevSpoon = -1;

    private Vector2 normalSize = new Vector2(82, 50);
    private Vector2 selectedSize = new Vector2(123, 75);

    private void Start()
    {
        PlayerInventory.ChangedSpoon += ChangeSpoon;
        PlayerInventory.AddSpoon += AddSpoon;
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
    }

    void AddSpoon(int spoon)
    {
        imageComponents[spoon].enabled = true;
    }
}
