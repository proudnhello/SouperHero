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

    private void Start()
    {
        PlayerInventory.ChangedSpoon += ChangeSpoon;
        ChangeSpoon(0);
    }

    void ChangeSpoon(int spoon)
    {
        if (prevSpoon >= 0)
        {
            imageComponents[prevSpoon].color = Color.white; //Change current spoon's color
        }
        prevSpoon = spoon;
        imageComponents[spoon].color = new Color(252f/255f, 173f/255f, 3f/255f, 1.0f); //Change current spoon's color
    }
}
