using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PotContentUI : MonoBehaviour
{
    TMP_Text text;
    private void Start()
    {
        text = GetComponent<TMP_Text>();
        PlayerManager.SoupifyEnemy += OnSoupifyEnemy;
        PlayerManager.DrinkPot += () => text.text = "";
    }

    void OnSoupifyEnemy(List<(string, int)> pot)
    {
        string display = "";
        foreach (var type in pot)
        {
            display += type.Item1 + " " + type.Item2 + "\n";
        }
        text.text = display;
    }
}
