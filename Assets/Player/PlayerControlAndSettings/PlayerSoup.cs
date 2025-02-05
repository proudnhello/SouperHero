using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static PlayerManager;

public class PlayerSoup : MonoBehaviour
{
    private List<Ingredient> inventory = new List<Ingredient>();
    private int numberofPots = 3;
    // This is set in player manager, so I don't want people being confused seeing it in the editor
    [NonSerialized]
    public AbilityLookup lookup;
    public int GetNumberOfPots()
    {
        return numberofPots;
    }

    public class Pot
    {
        public List<(string, int)> soup;
        public int uses;
        public int maxUsage;

        public Pot(int usage)
        {
            soup = new List<(string, int)>();
            uses = usage;
            maxUsage = uses;
        }

        public void Refill()
        {
            uses = maxUsage;
        }

        public void Empty()
        {
            soup.Clear();
            uses = maxUsage;
        }
    }

    [Serializable]
    public struct Ingredient
    {
        public string name;
        public List<string> flavors;
    }

    // Add an ingredient to the player's inventory
    public void AddToInventory(Ingredient ingredient)
    {
        inventory.Add(ingredient);
        PrintIngredient(ingredient);
    }

    //public static event Action<List<(string, int)>> SoupifyEnemy;

    // Convert a list of ingredients into a pot of soup, controlled by the potNumber
    public Pot FillPot(List<Ingredient> ingedientValue, Pot pot)
    {
        pot.Empty();
        foreach (Ingredient ingredient in ingedientValue)
        {
            PrintIngredient(ingredient);
            foreach (string flavor in ingredient.flavors)
            {
                bool found = false;
                for (int i = 0; i < pot.soup.Count; i++)
                {
                    if (pot.soup[i].Item1 == flavor)
                    {
                        pot.soup[i] = (flavor, pot.soup[i].Item2 + 1);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    pot.soup.Add((flavor, 1));
                }
            }
        }
        return pot;
    }

    public static event Action DrinkPot;

    // Drink the soup in the pot and activate the abilities that correspond to the soup.
    public List<AbilityAbstractClass> Drink(Pot pot)
    {
        List<AbilityAbstractClass> abilities = new List<AbilityAbstractClass>();
        // TESTING - fetch the first three ingredients in the inventory and create a pot with them

        foreach ((string, int) soup in pot.soup)
        {
            print(soup.Item1 + " " + soup.Item2);
        }
        // You can't drink soup if the pot is empty
        if (pot.soup.Count == 0)
        {
            return null;
        }

        // Then drink the soup
        List<AbilityAbstractClass> drankAbilities = lookup.Drink(pot.soup);
        foreach (AbilityAbstractClass ability in drankAbilities)
        {
            print(ability._abilityName);
        }

        pot.uses--;
        print("Remaining Uses " + pot.uses);

        // If there are no uses left, empty the pot and reset the usage
        if (pot.uses <= 0)
        {
            pot.Empty();
        }
        foreach (AbilityAbstractClass ability in drankAbilities)
        {
            abilities.Add(ability);
        }

        return abilities;
    }

    public void PrintIngredient(Ingredient i)
    {
        print(i.name + ", with flavors: " + String.Join(" ", i.flavors.ToArray()));
    }
}
