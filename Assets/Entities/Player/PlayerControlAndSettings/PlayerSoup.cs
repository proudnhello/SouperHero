using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static PlayerManager;
using StatusEffect = EntityStatusEffects.StatusEffect;
using AbilityStats = AbilityAbstractClass.AbilityStats;

public class PlayerSoup : MonoBehaviour
{
    private List<FlavorIngredient> flavorInventory = new List<FlavorIngredient>();
    private List<AbilityIngredient> abilityInventory = new List<AbilityIngredient>();
    private int numberofPots = 3;
    // This is set in player manager, so I don't want people being confused seeing it in the editor
    [NonSerialized]
    public AbilityLookup lookup;
    public int GetNumberOfPots()
    {
        return numberofPots;
    }

    public class Spoon
    {
        public List<AbilityAbstractClass> abilities;
        public List<StatusEffect> statusEffects;
        public AbilityStats stats;
        public int uses;
        public int maxUsage;
        public bool empty;

        public Spoon()
        {
            statusEffects = new List<StatusEffect>();
            abilities = new List<AbilityAbstractClass>();
            stats = AbilityAbstractClass.NewAbilityStats();
            uses = -1;
            maxUsage = -1;
            empty = true;
        }

        public void Refill()
        {
            uses = maxUsage;
        }

        public void MakeEmpty()
        {
            statusEffects.Clear();
            abilities.Clear();
            uses = maxUsage;
            empty = true;
        }

        public bool IsEmpty(){
            return empty;
        }
    }

    //[Serializable]
    //public struct FlavorIngredient
    //{
    //    public string name;
    //    public List<string> flavors;
    //}

    //[Serializable]
    //public struct AbilityIngredient
    //{
    //    public string name;
    //    public AbilityAbstractClass ability;
    //    public int uses;
    //}

    // Add an ingredient to the player's inventory
    public void AddToInventory(FlavorIngredient ingredient)
    {
        flavorInventory.Add(ingredient);
        PrintIngredient(ingredient);
    }
    
    public void AddToInventory(AbilityIngredient ingredient)
    {
        abilityInventory.Add(ingredient);
    }

    //public static event Action<List<(string, int)>> SoupifyEnemy;

    // Convert a list of ingredients into a spoon that can be used
    public Spoon FillSpoon(List<FlavorIngredient> flavor, List<AbilityIngredient> ability, Spoon spoon)
    {
        if (ability == null || ability.Count == 0)
        {
            Debug.LogError("FillSpoon: Ability list is empty!");
        }
        
        spoon.MakeEmpty();
        spoon.empty = false;
        spoon.maxUsage = 0;
        List<(string, int)> pot = new List<(string, int)>();
        List<AbilityAbstractClass> added = new List<AbilityAbstractClass>();

        // First, compile the flavors in a format the lookuptable will use
        foreach (FlavorIngredient ingredient in flavor)
        {
            PrintIngredient(ingredient);
            foreach (string f in ingredient.flavors)
            {
                bool found = false;
                for (int i = 0; i < pot.Count; i++)
                {
                    if (pot[i].Item1 == f)
                    {
                        pot[i] = (f, pot[i].Item2 + 1);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    pot.Add((f, 1));
                }
            }
        }

        // Then, lookup status effects and abilities
        (List<StatusEffect>, AbilityStats) temp = lookup.GetStatusEffects(pot);
        spoon.statusEffects = temp.Item1;
        spoon.stats = temp.Item2;

        // Then, add abilities to the spoon, applying the ability buffs as we go
        foreach (AbilityIngredient a in ability)
        {
            spoon.maxUsage += a.uses;
            // Because all of these ingredients ***should*** contain the same instance of the ability, we can check if we've already added it like this
            // in b4 i make a duplicate of an instance for some fukin reason then get confused why it's not working
            if (!added.Contains(a.ability))
            {
                AbilityAbstractClass newAbility = Instantiate(a.ability);
                newAbility.SetStats(spoon.stats);
                newAbility.SetStatusEffects(spoon.statusEffects);
                spoon.abilities.Add(newAbility);
                added.Add(a.ability);
            }
        }

        spoon.uses = spoon.maxUsage;
        return spoon;
    }

    public static event Action DrinkPot;

    public void PrintIngredient(FlavorIngredient i)
    {
        print(i.name + ", with flavors: " + String.Join(" ", i.flavors.ToArray()));
    }
}
