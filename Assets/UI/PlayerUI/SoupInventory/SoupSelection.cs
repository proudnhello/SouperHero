using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class SoupSelection : MonoBehaviour
{
    public static SoupSelection Singleton { get; private set; }

    //private int selectedIndex = -1;

    public void OnSingleClick()
    {
        Debug.Log("Clicked one time!");

        //TODO: Check if selectedSoup is null

        //If null: Assign selectedSoup to selected slot
        //Allow for soup to be cooked

        //Else: swap soups in selectedSoup and the clicked slot
        //Re-assign selectedSoup to null

        var soupIndex = this.transform.GetSiblingIndex();
        PlayerInventory.Singleton.SetSelectedSoup(soupIndex);
    }

    /*
    public int GetSelectedIndex() {
        return selectedIndex;
    }
    */

    public void OnHoverEnter()
    {
        //Debug.Log("Hovered in!");
    }

    public void OnHoverExit()
    {
        //Debug.Log("Hovered out!");
    }
}
