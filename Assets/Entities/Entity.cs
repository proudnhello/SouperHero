using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static EntityInflictionEffectHandler;
using Unity.VisualScripting;
using System;
using Infliction = SoupSpoon.SpoonInfliction;

public class Entity : MonoBehaviour
{
    // ~~~ DEFINITIONS ~~~
    [Serializable]
    public struct BaseStats
    {
        public int maxHealth;
        public float baseMoveSpeed;
    }
    public struct CurrentStats
    {
        public int health;
        public float moveSpeed;
    }

    // ~~~ VARIABLES ~~~
    [SerializeField] BaseStats baseStats;
    public float invincibility = 1.0f;   // time the player flashes red when taking damage
    CurrentStats currentStats;
    internal EntityInflictionEffectHandler inflictionHandler;
    public void InitEntity()
    {
        inflictionHandler = new(this);
        ResetStats();
    }

    public void ResetStats()
    {
        currentStats.health = baseStats.maxHealth;
        currentStats.moveSpeed = baseStats.baseMoveSpeed;
    }

    public void ApplyInfliction(List<Infliction> spoonInflictions)
    {
        inflictionHandler.ApplyInflictions(spoonInflictions);
    }

    public int GetHealth()
    {
        return currentStats.health;
    }
    public virtual void AddHealth(int amount)
    {
        currentStats.health += amount;
        currentStats.health = Mathf.Clamp(currentStats.health, 0, baseStats.maxHealth);
    }

    public bool IsDead()
    {
        return currentStats.health <= 0;
    }

    public virtual void TakeDamage(int amount)
    {
        currentStats.health -= amount;
        currentStats.health = Mathf.Clamp(currentStats.health, 0, baseStats.maxHealth);
    }
    public virtual void TakeDamage(int amount, GameObject source, float knockback)
    {
        currentStats.health -= amount;
        currentStats.health = Mathf.Clamp(currentStats.health, 0, baseStats.maxHealth);
    }


    public float GetMoveSpeed()
    {
        return currentStats.moveSpeed;
    }
    public void SetMoveSpeed(float newSpeed)
    {
        currentStats.moveSpeed = newSpeed;
    }

    public void ResetMoveSpeed()
    {
        currentStats.moveSpeed = baseStats.baseMoveSpeed;
    }

}
