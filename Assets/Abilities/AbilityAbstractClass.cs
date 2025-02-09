using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Base ability abstract class. Contains declarations for functions that abilities will implement
public abstract class AbilityAbstractClass : ScriptableObject
{
    [Header("Info")]
    [SerializeField] public string _abilityName;

    // IF YOU'RE ADDING A NEW STAT, UPDATE THE ABILITYSTATS STRUCT, THE ABILITYSTATS CONSTRUCTOR, AND THE INCREASESTAT FUNCTION
    // You will probably also need to update all of the abilities, but ah well sure, you knew that
    [Serializable] public struct AbilityStats
    {
        public int duration;
        public int cooldown;
        public int size;
        public int speed;
        public int damage;
    }

    public enum Stat
    {
        Duration, Cooldown, Size, Speed, Damage
    }

    [Header("Stats")]
    [SerializeField] protected AbilityStats stats = new AbilityStats { cooldown = 1, duration = 1, size = 1, speed = 1, damage = 1 };
    [SerializeField] protected List<EntityStatusEffects.StatusEffect> statusEffects;

    public static AbilityStats NewAbilityStats(int duration = 1, int cooldown = 1, int size = 1, int speed = 1, int damage = 1)
    {
        return new AbilityStats
        {
            duration = duration,
            cooldown = cooldown,
            size = size,
            speed = speed,
            damage = PlayerManager.instance.GetDamage()
        };
    }

    public static AbilityStats IncreaceStat(AbilityStats stats, AbilityLookup.BuffLookup buff)
    {
        if (buff.givesBuff != true)
        {
            return stats;
        }

        BuffFunction buffFunction;
        switch (buff.operation)
        {
            case EntityStatusEffects.Operation.Add:
                buffFunction = (a, b) => a + b;
                break;
            case EntityStatusEffects.Operation.Multiply:
                buffFunction = (a, b) => a * b;
                break;
            default:
                buffFunction = (a, b) => a + b;
                break;
        }

        switch (buff.stat)
        {
            case Stat.Cooldown:
                stats.cooldown = (int)buffFunction(stats.cooldown, buff.value);
                break;
            case Stat.Duration:
                stats.duration = (int)buffFunction(stats.duration, buff.value);
                break;
            case Stat.Size:
                stats.size = (int)buffFunction(stats.size, buff.value);
                break;
            case Stat.Speed:
                stats.speed = (int)buffFunction(stats.speed, buff.value);
                break;
            case Stat.Damage:
                stats.damage = (int)buffFunction(stats.damage, buff.value);
                break; 
            default:
                break;
        }

        return stats;
    }

    delegate float BuffFunction(float a, float b);

    
    public void SetStats(AbilityStats stats)
    {
        this.stats = stats;
    }

    public void SetStatusEffects(List<EntityStatusEffects.StatusEffect> spoonEffects)
    {
        this.statusEffects = new List<EntityStatusEffects.StatusEffect>(spoonEffects);
    }

    // Initialize and End are mostly for use by buffs, most abilities get triggered with a call to Active().
    public abstract void Initialize(int soupVal);
    public abstract void Active();
    public abstract void End();
}
