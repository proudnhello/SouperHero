using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Dictionary containing an ability name and its corresponding color for UI
// ability names will be the strings determined in the CreateAssetMenu for each ability
// colors will be hex values for the UI to use 
public class AbilityColorLookup : MonoBehaviour
{
    // Start is called before the first frame update
    public Dictionary <string, string> abilityColorLookup;

    // get reference to soup manager/all created abilities
    void Start()
    {

        // Once soup manager is implemented, abilities will be pulled from there dynamically,
        // for now they are hardcoded 

        // Psuedocode: 
        // foreach (ability in soupManager.abilities)
        // {
        //     abilityColorLookup.Add(ability.name, ability.color);
        // }
        // ability colors might just be defined in this file ? confirm later 

        abilityColorLookup = new Dictionary<string, string>(); 
        abilityColorLookup.Add("Damage Up", "#3b00a1");
        abilityColorLookup.Add("Fireball", "e31300");
    }

    public string GetColor(string abilityName)
    {
        // returns the hex color value for the given ability
        return abilityColorLookup[abilityName];
    }
}
