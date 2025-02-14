using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionSelectionButton : MonoBehaviour
{
    // Button 1 is 0 index, Button 2 is 1 index, etc.
    public int slot;
    
    public void SetCurrentSpoon()
    {
        //CookingManager.Singleton.SetSelectedPotSpoon(slot);
    }
}
