using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoupSelection : MonoBehaviour
{
    public SoupSpoon selectedSoup = null;
    
    //PlayerInventory.Singleton.GetSpoons()[spoon];

    public void OnSingleClick()
    {
        Debug.Log("Clicked one time!");
        //TODO: Check if selectedSoup is null

        //If null: Assign selectedSoup to selected slot
        //Allow for soup to be cooked

        //Else: swap soups in selectedSoup and the clicked slot
        //Re-assign selectedSoup to null

    }

    public void OnHoverEnter()
    {
        //Debug.Log("Hovered in!");
    }

    public void OnHoverExit()
    {
        //Debug.Log("Hovered out!");
    }
}
