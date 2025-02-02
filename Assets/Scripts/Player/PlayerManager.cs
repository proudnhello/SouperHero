using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public GameObject player;

    [Header("Keybinds")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode altAttackKey = KeyCode.V;
    public KeyCode soupKey = KeyCode.Mouse1;
    public KeyCode altSoupKey = KeyCode.F;
    public KeyCode drinkey = KeyCode.Space;
    [Header("Attack")]
    [SerializeField] private LayerMask enemies;
    [SerializeField] private int playerDamage = 10;
    public int GetDamage()
    {
        return instance.playerDamage;
    }

    public void SetDamage(int newDamage)
    {
        instance.playerDamage = newDamage;
    }

    [SerializeField] private float attackSpeed = 3;
    public float GetAttackSpeed()
    {
        return attackSpeed;
    }

    public void SetAttackSpeed(float newAttackSpeed)
    {
        attackSpeed = newAttackSpeed;
    }

    [SerializeField] private float attackDelay = 0;
    public float getAttackDelay()
    {
        return attackDelay;
    }

    public void setAttackDelay(float newAttackDelay)
    {
        attackDelay = newAttackDelay;
    }

    [SerializeField] private float attackRadius = 1.0f;
    public float GetAttackRadius()
    {
        return instance.attackRadius;
    }
    public float SetAttackRadius(float newRadius)
    {
        instance.attackRadius = newRadius;
        return instance.attackRadius;
    }

    [Header("Movement")]
    [SerializeField] float speed = 10.0f;
    public float GetSpeed()
    {
        return instance.speed;
    }

    public void SetSpeed(float newSpeed)
    {
        instance.speed = newSpeed;
    }

    [Header("Abilities")]
    [SerializeField] private List<AbilityAbstractClass> abilities;

    [Header("Soup")]
    [SerializeField] private AbilityLookup lookup;
    [SerializeField] private int maxPotSize = 5;
    [SerializeField] private int numberofPots = 3;
    [SerializeField] private int defaultSoupUsage = 3;
    private List<Ingredient> inventory = new List<Ingredient>();
    public int GetNumberOfPots()
    {
        return numberofPots;
    }

    List<Pot> pots = new List<Pot>();
    List<int> potFullnesses = new List<int>();

    public class Pot
    {
        public List<(string, int)> soup;
        public int fullness;
        public int uses;
        public int maxUsage;
    }

    [Serializable]
    public struct Ingredient
    {
        public string name;
        public List<string> flavors;
    }

    public void PrintIngredient(Ingredient i)
    {
        print(i.name + ", with flavors: " + String.Join(" ", i.flavors.ToArray()));
    }

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] public int health;
    private int shieldAmount = 0;
    public int getShieldAmount()
    {
        return shieldAmount;
    }

    public void setShieldAmount(int newShieldAmount)
    {
        shieldAmount = newShieldAmount;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        health = maxHealth;
        for (int i = 0; i < numberofPots; i++)
        {
            Pot pot = new Pot();
            pot.soup = new List<(string, int)>();
            pot.fullness = 0;
            pot.maxUsage = defaultSoupUsage;
            pot.uses = defaultSoupUsage;
            pots.Add(pot);
        }
    }

    public List<AbilityAbstractClass> GetAbilities()
    {
        return instance.abilities;
    }

    public LayerMask GetEnemies()
    {
        return instance.enemies;
    }

    // Add an ingredient to the player's inventory
    public void AddToInventory(Ingredient ingredient)
    {
        inventory.Add(ingredient);
        PrintIngredient(ingredient);
    }

    public static event Action<List<(string, int)>> SoupifyEnemy;

    // Convert a list of ingredients into a pot of soup, controlled by the potNumber
    public void CreatePot(List<Ingredient> ingedientValue, int potNumber)
    {
        Pot pot = pots[potNumber];
        pot.soup.Clear();
        print("Making Pot ;)");
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
    }

    public static event Action DrinkPot;

    // Drink the soup in the pot and activate the abilities that correspond to the soup.
    public void Drink(int potNumber)
    {
        // TESTING - fetch the first three ingredients in the inventory and create a pot with them
        if (inventory.Count < 3)
        {
            CreatePot(inventory, potNumber);
        }
        else
        {
            CreatePot(inventory.GetRange(0, 3), potNumber);
        }

        Pot pot = pots[potNumber];
        foreach((string, int) soup in pot.soup)
        {
            print(soup.Item1 + " " + soup.Item2);
        }
        // You can't drink soup if the pot is empty
        if (pot.soup.Count == 0)
        {
            return;
        }

        // First end all active abilities
        foreach (AbilityAbstractClass ability in abilities.ToList())
        {
            ability.End();
        }

        // Then drink the soup
        List<AbilityAbstractClass> drankAbilities = lookup.Drink(pot.soup);
        foreach(AbilityAbstractClass ability in drankAbilities){
            print(ability._abilityName);
        }

        pot.uses--;
        print("Remaining Uses " + pot.uses);

        // If there are no uses left, empty the pot and reset the usage
        if (pot.uses <= 0)
        {
            print("Empty Pot, emptying");
            abilities.Clear();
            pot.soup.Clear();
            pot.fullness = 0;
            pot.uses = pot.maxUsage;
        }
        foreach (AbilityAbstractClass ability in drankAbilities)
        {
            abilities.Add(ability);
        }
        DrinkPot?.Invoke();
    }

    public void RemoveAbility(AbilityAbstractClass ability)
    {
        abilities.Remove(ability);
    }

    public void SetHealth(int newHealth)
    {
        instance.health = (int)newHealth;
    }

    public int GetHealth()
    {
        return instance.health;
    }

    public int GetMaxHealth()
    {
        return instance.maxHealth;
    }

    public void Heal(int healAmount)
    {
        instance.health += healAmount;
        Debug.Log("Healing");
        if (instance.health > maxHealth)
        {
            instance.health = maxHealth;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (shieldAmount > 0)
        {
            instance.shieldAmount -= damageAmount;
            if(instance.shieldAmount <= 0)
            {
                instance.shieldAmount = 0;
            }
        }
        else
        {
            instance.health -= damageAmount;
        }
        Debug.Log("Taking damage");
        if (instance.health <= 0)
        {
            instance.health = 0;
            // Game over
            Debug.Log("Game Over womp womp");
        }
    }

}
