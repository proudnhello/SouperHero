using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StatusEffect = EntityStatusEffects.StatusEffect;
using AbilityStats = AbilityAbstractClass.AbilityStats;

[CreateAssetMenu(menuName = "Abilities/LookupTable")]

public class AbilityLookup : ScriptableObject
{
    // Lookup table for abilities. Running this may get slow if there are a lot of entries, I may convert this into a dictionary on load
    // The reason it has to be like this is, in order for us to be able to use the instances of abilities created in the editor, we must be able to edit list in the editor
    // Dictionaries are not editable in the editor. The only things that are editable are arrays and lists of serializable objects (which are primatives and structs of primatives)
    // This will get massily unwieldy to edit if we have a lot of abilities/enemy types, but I still think it's the best way
    [Serializable]
    public struct FlavorLookup
    {
        public string flavorName;
        public List<FlavorLookupEntry> abilities;
    }

    [Serializable]
    public struct FlavorLookupEntry
    {
        public int minAppearences;
        public int maxAppearances;
        public StatusEffectLookup status;
        public BuffLookup buff;
        public Color color;
    }

    [Serializable]
    public struct StatusEffectLookup
    {
        public bool givesStatusEffect;
        public StatusEffect status;
    }

    [Serializable]
    public struct BuffLookup
    {
        public bool givesBuff;
        public AbilityAbstractClass.Stat stat;
        public float value;
        public EntityStatusEffects.Operation operation;
    }

    public FlavorLookup[] lookup;

    public (List<StatusEffect>, AbilityStats) GetStatusEffects(List<(string, int)> pot)
    {
        List<StatusEffect> statuses = new List<StatusEffect>();
        List <BuffLookup> buffs = new List<BuffLookup>();

        foreach ((string, int) soup in pot)
        {
            bool foundFlavor = false;
            foreach (FlavorLookup entry in lookup)
            {
                if(soup.Item1 != entry.flavorName)
                {
                    continue;
                }
                foundFlavor = true;
                foreach (FlavorLookupEntry abilityEntry in entry.abilities)
                {
                    // If the count is in min, inclusive, and max, exclusive
                    if (soup.Item2 >= abilityEntry.minAppearences && soup.Item2 < abilityEntry.maxAppearances)
                    {
                        if (abilityEntry.status.givesStatusEffect)
                        {
                            // Holy shit, status.status.statusType.ToSring(). That's vile
                            Debug.Log("adding status " + abilityEntry.status.status.statusType.ToString());
                            // Not that status.status is pretty either
                            statuses.Add(abilityEntry.status.status);
                        }
                        if (abilityEntry.buff.givesBuff)
                        {
                            buffs.Add(abilityEntry.buff);
                        }
                    }
                }
            }
            if (!foundFlavor)
            {
                Debug.LogError($"Enemy type '{soup.Item1}' not found in ability lookup table");
            }
        }

        // This sorts the buffs by their operation, so that the adds are all done first, then the multiplies
        // (Hopefully, my sort, enum.compareto, and lambda functions are rusty)
        buffs.Sort((a, b) => a.operation.CompareTo(b.operation));

        AbilityStats stats = AbilityAbstractClass.NewAbilityStats();

        foreach (BuffLookup buff in buffs)
        {
            stats = AbilityAbstractClass.IncreaceStat(stats, buff);
        }

        return (statuses, stats);
    }

    public Color GetAbilityColor(StatusEffect ability)
    {
        /*foreach (var enemyEntry in lookup)
        {
            foreach (var abilityEntry in enemyEntry.abilities)
            {
                if (abilityEntry.ability.GetType() == ability.GetType()) // Match by type
                {
                    return abilityEntry.color;
                }
            }
        }*/
        return Color.white;
    }
}
