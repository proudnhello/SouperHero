using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;
using Pot = PlayerSoup.Pot;
using Ingredient = PlayerSoup.Ingredient;

public class PlayerManager : MonoBehaviour
{
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        health = maxHealth;
        for (int i = 0; i < numberofPots; i++)
        {
            Pot pot = new Pot(defaultSoupUsage);
            pots.Add(pot);
        }
        soup = player.GetComponent<PlayerSoup>();
        soup.lookup = lookup;
    }

    public static PlayerManager instance;
    public GameObject player;
    public PlayerSoup soup;

    [Header("Keybinds")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode altAttackKey = KeyCode.V;
    public KeyCode soupKey = KeyCode.Mouse1;
    public KeyCode altSoupKey = KeyCode.F;
    public KeyCode drinkey = KeyCode.Space;
    [Header("Attack")]
    [SerializeField] private LayerMask enemies;
    [SerializeField] private int playerDamage = 10;

    private bool dead = false;

    public int GetDamage()
    {
        return instance.playerDamage;
    }

    public bool IsDead()
    {
        return instance.dead;
    }

    public bool IsAlive()
    {
        return !instance.dead;
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
    //[SerializeField] private int maxPotSize = 5;
    [SerializeField] private int numberofPots = 3;
    [SerializeField] private int defaultSoupUsage = 3;
    private List<Ingredient> inventory = new List<Ingredient>();
    public int GetNumberOfPots()
    {
        return numberofPots;
    }

    List<Pot> pots = new List<Pot>();
    List<int> potFullnesses = new List<int>();

    public void PrintIngredient(Ingredient i)
    {
        print(i.name + ", with flavors: " + String.Join(" ", i.flavors.ToArray()));
    }

    // Convert a list of ingredients into a pot of soup, controlled by the potNumber
    public void FillPot(List<Ingredient> ingedientValue, int potNumber)
    {
        soup.FillPot(ingedientValue, pots[potNumber]);
    }

    public static event Action DrinkPot;

    // Drink the soup in the pot and activate the abilities that correspond to the soup.
    public void Drink(int potNumber)
    {
        // TESTING - fetch the first three ingredients in the inventory and create a pot with them
        if (inventory.Count < 3)
        {
            FillPot(inventory, potNumber);
            inventory.Clear();
        }
        else
        {
            FillPot(inventory.GetRange(0, 3), potNumber);
            for (int i = 0; i < 3; i++)
            {
                inventory.RemoveAt(0);
            }
        }

        // First end all active abilities
        foreach (AbilityAbstractClass ability in abilities.ToList())
        {
            ability.End();
        }
        abilities.Clear();

        abilities = soup.Drink(pots[potNumber]);
        if(abilities == null)
        {
            abilities = new List<AbilityAbstractClass>();
        }
    }

    public void RemoveAbility(AbilityAbstractClass ability)
    {
        abilities.Remove(ability);
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

    //public static event Action<List<(string, int)>> SoupifyEnemy;

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

    public void TakeDamage(int damageAmount, GameObject source)
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
            player.GetComponent<PlayerHealth>().TakeDamage(damageAmount, source);
        }
        if (instance.health <= 0)
        {
            instance.health = 0;
            instance.dead = true;
            // Game over
            Debug.Log("Game Over womp womp");
        }
    }

    // Thoughtlessly reduces health, will not cause iframes or animation. You should probably use TakeDamage
    public void ReduceHealth(int damage)
    {
        instance.health -= damage;
        if (instance.health <= 0)
        {
            instance.health = 0;
            instance.dead = true;
            // Game over
            Debug.Log("Game Over womp womp");
        }
    }
}
