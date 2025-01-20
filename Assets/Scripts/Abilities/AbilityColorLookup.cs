using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AbilityColors/LookupTable")]
public class AbilityColorLookup : ScriptableObject
{
    [Serializable]

    public struct AbilityColorLookupEntry
    {
        public string color;                        // string containing a hex value
        public string enemyName;                    // enemy associated with the color
    }
    public AbilityColorLookupEntry[] lookup;
    private Dictionary<string, string> abilityColorLookup;
    
    public void CreateColorLookup()
    {
        abilityColorLookup = new Dictionary<string, string>();
        foreach (AbilityColorLookupEntry entry in lookup)
        {
            abilityColorLookup.Add(entry.enemyName, entry.color);
        }
    }   

    public string GetColor(string enemyName)
    {
        return abilityColorLookup[enemyName];
    }

    public void AddColor(string enemyName, string color)
    {
        abilityColorLookup.Add(enemyName, color);
    }

}
