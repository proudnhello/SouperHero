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
        var selectedSoup = PlayerInventory.Singleton.GetSelectedSoup();
        //Check if another slot was clicked previously, if not then set selected soup to the clicked index
        if (selectedSoup == -1)
        {
            PlayerInventory.Singleton.SetSelectedSoup(soupIndex);
        }
        else //Swap soups in Player Inventory and UI. Reset the index
        {
            //Check to make sure at least one index isn't null
            if (PlayerInventory.Singleton.GetSpoons()[soupIndex] != null
            || PlayerInventory.Singleton.GetSpoons()[selectedSoup] != null)
            {
                PlayerInventory.Singleton.SwapSoups(soupIndex, selectedSoup);
                //SoupUI.Singleton.SwapSoups(soupIndex, selectedSoup);
                int[] indices = { soupIndex, selectedSoup };
                SoupUI.Singleton.SwapSoups(indices);
            }
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
