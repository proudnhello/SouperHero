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
    public struct EnemyLookupEntry
    {
        public string enemyType;
        public List<AbilityLookupEntry> abilities;
    }

    [Serializable]
    public struct AbilityLookupEntry
    {
        public int minSoupValue;
        public int maxSoupValue;
        public AbilityAbstractClass ability;
        public Color color;
    }

    public EnemyLookupEntry[] lookup;

    public List<AbilityAbstractClass> Drink(List<(string, int)> pot)
    {
        List<AbilityAbstractClass> abilities = new List<AbilityAbstractClass>();

        foreach ((string, int) soup in pot)
        {
            bool foundEnemy = false;
            foreach (EnemyLookupEntry entry in lookup)
            {
                if(soup.Item1 != entry.enemyType)
                {
                    continue;
                }
                foundEnemy = true;
                foreach (AbilityLookupEntry abilityEntry in entry.abilities)
                {
                    if (soup.Item2 > abilityEntry.minSoupValue && soup.Item2 <= abilityEntry.maxSoupValue)
                    {
                        AbilityAbstractClass ability = Instantiate(abilityEntry.ability);
                        ability.Initialize(soup.Item2);
                        abilities.Add(ability);
                    }
                }
            }
            if (!foundEnemy)
            {
                Debug.LogError($"Enemy type '{soup.Item1}' not found in ability lookup table");
            }
        }

        return abilities;
    }

    public Color GetAbilityColor(AbilityAbstractClass ability)
{
    foreach (var enemyEntry in lookup)
    {
        foreach (var abilityEntry in enemyEntry.abilities)
        {
            if (abilityEntry.ability.GetType() == ability.GetType()) // Match by type
            {
                return abilityEntry.color;
            }
        }
    }
    return Color.white;
}
}
