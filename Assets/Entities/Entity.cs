using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using StatusType = EntityStatusEffects.StatusType;
using StatusEffect = EntityStatusEffects.StatusEffect;
using static EntityStatusEffects;
using Unity.VisualScripting;

public class Entity : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected int health;
    [SerializeField] protected int maxHealth;
    public int GetHealth()
    {
        return health;
    }
    public void SetHealth(int newHealth)
    {
        health = newHealth;
    }
    public void ChangeHealth(int amount)
    {
        health += amount;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        if (health <= 0)
        {
            health = 0;
        }
    }

    public virtual void TakeDamage(int amount)
    {
        health -= amount;
    }
    public virtual void TakeDamage(int amount, GameObject source)
    {
        health -= amount;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
    }
    public void ChangeMaxHealth(int amount)
    {
        maxHealth += amount;
        ChangeHealth(amount);
    }

    [Header("Movement")]
    [SerializeField] protected float baseMoveSpeed;
    protected float moveSpeed;
    public float GetBaseMoveSpeed() { 
        return baseMoveSpeed;
    }
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    [Header("Attacks")]
    [SerializeField] protected float baseAttackDamage;
    protected int attackDamage;
    public float GetBaseAttackDamage()
    {
        return baseAttackDamage;
    }
    public int GetDamage()
    {
        return attackDamage;
    }
    public void SetDamage(int newDamage)
    {
        attackDamage = newDamage;
    }

    [Header("Status Effects")]
    [SerializeField] List<StatusEffect> statusEffects = new List<StatusEffect>();
    // initialize enemy status effect class
    internal EntityStatusEffects statusEffect;
    [SerializeField] TMP_Text statusText;
    public void InitializeStats()
    {
        InitializeStatusEffects();
        attackDamage = (int)baseAttackDamage;
        moveSpeed = baseMoveSpeed;
        health = maxHealth;
    }
    public void InitializeStatusEffects()
    {
        statusEffect = this.AddComponent<EntityStatusEffects>();
        statusEffect.LinkToEntity(this);
        for (int i = 0; i < statusEffects.Count; i++)
        {
            statusEffect.AddStatusEffect(statusEffects[i]);
        }
    }
    public void AddStatusEffects(List<StatusEffect> effects)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            statusEffect.AddStatusEffect(effects[i]);
            // statusText.text += effects[i].statusType.ToString() + "\n";
        }
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        statusEffect.AddStatusEffect(effect);
        // statusText.text += effect.statusType.ToString() + "\n";
    }

    public void RemoveStatusText()
    {
        // Search for the name of the status effect in the status text and remove it
        for (int i = 0; i < statusEffects.Count; i++)
        {
            statusText.text = statusText.text.Replace(statusEffects[i].statusType.ToString() + "\n", "");
        }
    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        statusEffect.RemoveStatusEffect(effect);
    }

}
