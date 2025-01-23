using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public GameObject player;


    [Header("Keybinds")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode soupKey = KeyCode.Mouse1;
    public KeyCode drinkey = KeyCode.Space;

    [Header("Attack")]
    [SerializeField] private LayerMask enemies;
    [SerializeField] private int playerDamage = 10;
    [SerializeField] private float attackSpeed = 3;

    [Header("Movement")]
    [SerializeField] float speed = 10.0f;

    [Header("Abilities")]
    [SerializeField] private List<AbilityAbstractClass> abilities;

    [Header("Soup")]
    [SerializeField] private AbilityLookup lookup;
    [SerializeField] private int maxPotSize = 100;
    List<(string, int)> pot = new List<(string, int)>();
    private int potFullness = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public int GetDamage()
    {
        return instance.playerDamage;
    }

    public void SetDamage(int newDamage)
    {
        instance.playerDamage = newDamage;
    }

    public float GetSpeed()
    {
        return instance.speed;
    }

    public void SetSpeed(float newSpeed)
    {
        instance.speed = newSpeed;
    }

    public float getAttackSpeed(){
        return attackSpeed;
    }

    public void setAttackSpeed(float newAttackSpeed){
        attackSpeed = newAttackSpeed;
    }

    public List<AbilityAbstractClass> GetAbilities()
    {
        return instance.abilities;
    }

    public LayerMask GetEnemies()
    {
        return instance.enemies;
    }

    // Add soup to the pot. If the pot is full, the soup will be wasted.
    public void AddToPot((string, int) soupVal)
    {
        print("soupVal" + soupVal.Item2);

        if (potFullness+soupVal.Item2 >= maxPotSize)
        {
            soupVal.Item2 = maxPotSize - potFullness;
        }
        if (soupVal.Item2 == 0)
        {
            return;
        }
        potFullness += soupVal.Item2;
        for (int i = 0; i < pot.Count; i++)
        {
            if (pot[i].Item1 == soupVal.Item1)
            {
                int newSoupVal = pot[i].Item2 + soupVal.Item2;
                pot[i] = (soupVal.Item1, newSoupVal);
                return;
            }
        }
        pot.Add(soupVal);
    }

    // Drink the soup in the pot and activate the abilities that correspond to the soup.
    public void Drink()
    {
        foreach((string, int) soup in pot)
        {
            print(soup.Item1 + " " + soup.Item2);
        }
        // You can't drink soup if the pot is empty
        if (pot.Count == 0)
        {
            return;
        }

        // First end all active abilities
        foreach (AbilityAbstractClass ability in abilities.ToList())
        {
            ability.End();
        }

        // Then drink the soup
        List<AbilityAbstractClass> drankAbilities = lookup.Drink(pot);
        print(drankAbilities);

        abilities.Clear();
        pot.Clear();
        foreach (AbilityAbstractClass ability in drankAbilities)
        {
            abilities.Add(ability);
        }
    }

    public void RemoveAbility(AbilityAbstractClass ability)
    {
        abilities.Remove(ability);
    }
}
