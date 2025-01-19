using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public GameObject player;


    [Header("Keybinds")]
    private KeyCode attackKey = KeyCode.Mouse0;

    [Header("Attack")]
    [SerializeField] private LayerMask enemies;
    [SerializeField] private int playerDamage = 10;
    [SerializeField] private GameObject attackSpeed;

    [Header("Movement")]
    [SerializeField] float speed = 10.0f;

    [Header("Abilities")]
    [SerializeField] private AbilityAbstractClass[] abilities;

    [Header("Soup")]
    [SerializeField] private int maxPotSize = 0;
    List<(string, int)> pot;
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

    public KeyCode GetAttackKey()
    {
        return instance.attackKey;
    }

    public AbilityAbstractClass[] GetAbilities()
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
    }
}
