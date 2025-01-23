using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/LookupTable")]

public class AbilityLookup : ScriptableObject
{
    // Lookup table for abilities. Running this may get slow if there are a lot of entries, I may convert this into a dictionary on load
    // The reason it has to be like this is, in order for us to be able to use the instances of abilities created in the editor, we must be able to edit list in the editor
    // Dictionaries are not editable in the editor. The only things that are editable are arrays and lists of serializable objects (which are primatives and structs of primatives)
    // This will get massily unwieldy to edit if we have a lot of abilities/enemy types, but I still think it's the best way
    [Serializable]
    public struct AbilityLookupEntry
    {
        public string enemyType;
        public int minSoupValue;
        public int maxSoupValue;
        public AbilityAbstractClass ability;
    }

    public AbilityLookupEntry[] lookup;

    public List<AbilityAbstractClass> Drink(List<(string, int)> pot)
    {
        List<AbilityAbstractClass> abilities = new List<AbilityAbstractClass>();

        foreach (AbilityLookupEntry entry in lookup)
        {
            foreach ((string, int) soup in pot)
            {
                string name = soup.Item1;
                int value = soup.Item2;
                if (name == entry.enemyType && value > entry.minSoupValue && value <= entry.maxSoupValue)
                {
                    Debug.Log("BRIAN TEST" + name);
                    AbilityAbstractClass ability = Instantiate(entry.ability);
                    ability.Initialize(value);
                    abilities.Add(ability);
                }
            }
        }

        return abilities;
    }
}
