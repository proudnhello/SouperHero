using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpoonUses : MonoBehaviour
{

    [SerializeField] TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        PlayerInventory.ChangedSpoon += ChangeSpoon;
    }

    // Update is called once per frame
    void ChangeSpoon(int spoonNum)
    {
        SoupSpoon spoon = PlayerInventory.Singleton.GetSpoons()[spoonNum];
        if (spoon.uses >= 0)
        {
            text.text = "Uses = " + spoon.uses;
        } else if (spoon.uses == -1){
            text.text = "Uses = ∞";
        }
        else
        {
            Debug.LogError("Invalid Uses!");
        }
    }

}
