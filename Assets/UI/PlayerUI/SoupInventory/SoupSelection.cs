using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class SoupSelection : MonoBehaviour
{
    public void OnSingleClick()
    {
        var soupIndex = this.transform.GetSiblingIndex(); //Get index of clicked slot
        //Check if another slot was clicked previously, if not then set selected soup to the clicked index
        if (PlayerInventory.Singleton.GetSelectedSoup() == -1)
        {
            PlayerInventory.Singleton.SetSelectedSoup(soupIndex);
        }
        else //Swap soups in spoons (Player Inventory) and UI. Reset the index
        {
            PlayerInventory.Singleton.SwapSoups(soupIndex, PlayerInventory.Singleton.GetSelectedSoup());
            SoupUI.Singleton.SwapSoups(soupIndex, PlayerInventory.Singleton.GetSelectedSoup());
            PlayerInventory.Singleton.SetSelectedSoup(-1);
        }
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
