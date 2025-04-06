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
        public float invincibility;
    }
    [Serializable]
    public struct CurrentStats
    {
        public int health;
        public float moveSpeed;
    }

    // ~~~ VARIABLES ~~~
    [SerializeField] BaseStats baseStats;
    [SerializeField] CurrentStats currentStats;
    internal EntityInflictionEffectHandler inflictionHandler;
    internal EntityRenderer entityRenderer;
    internal Rigidbody2D _rigidbody;

    [SerializeField] GameObject hitmarker;

    public void InitEntity()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        inflictionHandler = new(this);
        ResetStats();
    }

    public void ResetStats()
    {
        currentStats.health = baseStats.maxHealth;
        currentStats.moveSpeed = baseStats.baseMoveSpeed;
    }

    public virtual void ApplyInfliction(List<Infliction> spoonInflictions, Transform source)
    {
        inflictionHandler.ApplyInflictions(spoonInflictions, source);
    }

    // Displays hitmarkers
    public void DisplayHitmarker(Color color, string text)
    {
        GameObject hitmarkerInstance = Instantiate(hitmarker, transform.position, Quaternion.identity);
        hitmarkerInstance.GetComponentInChildren<TextMeshPro>().text = text;
        hitmarkerInstance.GetComponentInChildren<TextMeshPro>().color = color;
    }

    public BaseStats GetBaseStats()
    {
        return baseStats;
    }

    public int GetHealth()
    {
        return currentStats.health;
    }

    // Directly edit the health of the entity, will not trigger damage effects
    public virtual void ModifyHealth(int amount)
    {
        currentStats.health += amount;
        currentStats.health = Mathf.Clamp(currentStats.health, 0, baseStats.maxHealth);
    }

    // Deal damage to the entity. Use this to trigger damage effects
    public virtual void DealDamage(int damage)
    {
        inflictionHandler.DealDamage(damage);
    }

    // Directly set the health of the entity, will not trigger damage effects
    public virtual void SetHealth(int health)
    {
        currentStats.health = Mathf.Clamp(currentStats.health, 0, baseStats.maxHealth);
    }

    public bool IsDead()
    {
        return currentStats.health <= 0;
    }

    public float GetMoveSpeed()
    {
        return currentStats.moveSpeed;
    }
    public virtual void SetMoveSpeed(float newSpeed)
    {
        print("Changing speed to " + newSpeed);
        currentStats.moveSpeed = newSpeed;
    }

    public virtual void ResetMoveSpeed()
    {
        currentStats.moveSpeed = baseStats.baseMoveSpeed;
    }

    public float GetInvincibility()
    {
        return baseStats.invincibility;
    }

}
