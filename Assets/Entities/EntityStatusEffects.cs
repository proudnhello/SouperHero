using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatusEffects : MonoBehaviour
{
    private Entity entity;

    public void LinkToEntity(Entity entity)
    {
        CreateStatusEffects();
        this.entity = entity;
    }
    private Dictionary<StatusType, string> effectCoroutines;

    [Serializable]
    public enum StatusType
    {
        Stun,
        Slow,
        Burn,
        Freeze,
        Poison
    }

    [Serializable]
    public struct StatusEffect
    {
        public StatusType statusType;
        public float duration;
        public float interval;
        public int intensity;
    }

    public static StatusEffect CreateStatusEffect(StatusType statusType, float duration, float interval, int intensity)
    {
        return new StatusEffect
        {
            statusType = statusType,
            duration = duration,
            interval = interval,
            intensity = intensity
        };
    }

    private void CreateStatusEffects()
    {
        print("making status effects");
        effectCoroutines = new Dictionary<StatusType, string>
        {
            { StatusType.Stun, "StunCoroutine" },
            { StatusType.Slow, "SlowCoroutine" },
            { StatusType.Burn, "BurnCoroutine" },
            { StatusType.Freeze, "FreezeCoroutine" },
            { StatusType.Poison, "PoisonCoroutine" },
        };
        // Each status Effect should have a parameter to multiply the numbers
    }  

    public List<StatusType> statusEffects = new();
    public void AddStatusEffect(StatusEffect statusEffect)
    {
        // Check what the status effect is and if it has been added before adding
        print(effectCoroutines);
        if (effectCoroutines.ContainsKey(statusEffect.statusType) && !HasStatusEffect(statusEffect.statusType))
        {
            Debug.Log("Adding " + statusEffect.statusType + "...");
            statusEffects.Add(statusEffect.statusType);

            Debug.Log("Calling " + effectCoroutines[statusEffect.statusType] + " coroutine...");
            StartCoroutine(effectCoroutines[statusEffect.statusType], statusEffect);
        }
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        statusEffects.Remove(statusEffect.statusType);
        StopCoroutine(effectCoroutines[statusEffect.statusType]);
    }
    public bool HasStatusEffect(StatusType statusEffect)
    {
        return statusEffects.Contains(statusEffect);
    }

    // Different status effects have different coroutines
    public IEnumerator SlowCoroutine(StatusEffect effect)
    {
        Debug.Log("Enemy " + entity.name + " Old Movespeed:" + entity.GetMoveSpeed());
        // lower enemy speed by 10
        float oldSpeed = entity.GetMoveSpeed();
        entity.SetMoveSpeed(entity.GetMoveSpeed() - 10f);

        // if enemy speed is less than 1, set it to 1
        if (entity.GetMoveSpeed() < 1)
        {
            entity.SetMoveSpeed(1);
        }

        Debug.Log("Enemy " + entity.name + " New Movespeed:" + entity.GetMoveSpeed());

        // wait for 10 seconds
        yield return new WaitForSeconds(10);
        // reset enemy speed and color
        entity.SetMoveSpeed(oldSpeed);

        // remove slow status effect
        RemoveStatusEffect(effect);
    }

    public IEnumerator BurnCoroutine(StatusEffect effect)
    {
        // TODO: complete burn status
        // damage enemy for 10 seconds maybe for 5 health each
        float duration = 5f;
        float interval = 1f; 
        int dmgPerInterval = 5;
        while (duration > 0f) {
            entity.TakeDamage(Mathf.Max(dmgPerInterval, 0));
            yield return new WaitForSeconds(interval);
            duration-= interval;
            dmgPerInterval--;
        }
        RemoveStatusEffect(effect);
    }

    public IEnumerator StunCoroutine(StatusEffect effect)
    {
        // stun enemy for 5 seconds
        float oldSpeed = entity.GetMoveSpeed();
        entity.SetMoveSpeed(0f);
        yield return new WaitForSeconds(3);
        entity.SetMoveSpeed(oldSpeed);
        RemoveStatusEffect(effect);

    }

    public IEnumerator FreezeCoroutine(StatusEffect effect)
    {
        // TODO: make freeze different from stun
        // freeze enemy for 10 seconds
        float oldSpeed = entity.GetMoveSpeed();
        entity.SetMoveSpeed(0f);
        yield return new WaitForSeconds(10);
        entity.SetMoveSpeed(oldSpeed);
        RemoveStatusEffect(effect);

    }

    public IEnumerator PoisonCoroutine(StatusEffect effect)
    {   
        // slows and depletes health for 10 seconds
        float oldSpeed = entity.GetMoveSpeed();
        entity.SetMoveSpeed(entity.GetMoveSpeed() - 10f);

        // if enemy speed is less than 1, set it to 1
        if (entity.GetMoveSpeed() < 1)
        {
            entity.SetMoveSpeed(1);
        }

        float duration = 5f;
        float interval = 1f; 
        int dmgPerInterval = 2;
        while (duration > 0f) {
            // while active
            entity.TakeDamage(Mathf.Max(dmgPerInterval, 0));
            yield return new WaitForSeconds(interval);
            duration-= interval;
            dmgPerInterval--;
        }

        yield return new WaitForSeconds(10);
        entity.SetMoveSpeed(oldSpeed);
        RemoveStatusEffect(effect);

    }
}