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

    [Header("Movement")]
    [SerializeField] float speed = 10.0f;

    [Header("Abilities")]
    [SerializeField] private AbilityAbstractClass[] abilities; 
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
}
