using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpoonsEquipped : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    private void Start()
    {
        PlayerInventory.ChangedSpoon += ChangeSpoon;
    }

    void ChangeSpoon(int spoon)
    {
        text.text = "Current Spoon = " + spoon;
    }
}
