using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusEffects
{
    private EnemyBaseClass enemy;

    public EnemyStatusEffects(EnemyBaseClass enemy)
    {
        this.enemy = enemy;
        CreateStatusEffects();
    }
    private Dictionary<StatusEffect, IEnumerator> effectCoroutines;
    
    public enum StatusEffect
    {
        Stun,
        Slow,
        Burn,
        Freeze,
        Poison
    }

    private void CreateStatusEffects()
    {
        effectCoroutines = new Dictionary<StatusEffect, IEnumerator>
        {
            { StatusEffect.Stun, StunCoroutine() },
            { StatusEffect.Slow, SlowCoroutine() },
            { StatusEffect.Burn, BurnCoroutine() },
            { StatusEffect.Freeze, FreezeCoroutine() },
            { StatusEffect.Poison, PoisonCoroutine() },
        };
        // Each status Effect should have a parameter to multiply the numbers
    }  

    public List<StatusEffect> statusEffects = new();
    public void AddStatusEffect(StatusEffect statusEffect)
    {
        // Check what the status effect is and if it has been added before adding
        if (effectCoroutines.ContainsKey(statusEffect) && !HasStatusEffect(statusEffect))
        {
            Debug.Log("Adding " + statusEffect + "...");
            statusEffects.Add(statusEffect);

            Debug.Log("Calling " + statusEffect + " coroutine...");
            enemy.ModifyEffect(statusEffect.ToString());
            enemy.StartCoroutine(effectCoroutines[statusEffect]);
        }
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        statusEffects.Remove(statusEffect);
        enemy.ModifyEffect("");
    }
    public bool HasStatusEffect(StatusEffect statusEffect)
    {
        return statusEffects.Contains(statusEffect);
    }

    // Different status effects have different coroutines
    public IEnumerator SlowCoroutine()
    {
        Debug.Log("Enemy " + enemy.name + " Old Movespeed:" + enemy.moveSpeed);
        // lower enemy speed by 10
        float oldSpeed = enemy.moveSpeed;
        enemy.moveSpeed = enemy.moveSpeed - 10f;

        // if enemy speed is less than 1, set it to 1
        if (enemy.moveSpeed < 1)
        {
            enemy.moveSpeed = 1;
        }

        Debug.Log("Enemy " + enemy.name + " New Movespeed:" + enemy.moveSpeed);

        // wait for 10 seconds
        yield return new WaitForSeconds(10);
        // reset enemy speed and color
        enemy.moveSpeed = oldSpeed;

        // remove slow status effect
        RemoveStatusEffect(StatusEffect.Slow);
    }

    public IEnumerator BurnCoroutine()
    {
        // TODO: complete burn status
        // damage enemy for 10 seconds maybe for 5 health each
        float duration = 5f;
        float interval = 1f; 
        int dmgPerInterval = 5;
        while (duration > 0f) {
            enemy.TakeDamageNoSource(Mathf.Max(dmgPerInterval, 0));
            yield return new WaitForSeconds(interval);
            duration-= interval;
            dmgPerInterval--;
        }
        RemoveStatusEffect(StatusEffect.Burn);
    }

    public IEnumerator StunCoroutine()
    {
        // stun enemy for 5 seconds
        float oldSpeed = enemy.moveSpeed;
        enemy.moveSpeed = 0f;
        yield return new WaitForSeconds(3);
        enemy.moveSpeed = oldSpeed;
        RemoveStatusEffect(StatusEffect.Stun);

    }

    public IEnumerator FreezeCoroutine()
    {
        // TODO: make freeze different from stun
        // freeze enemy for 10 seconds
        float oldSpeed = enemy.moveSpeed;
        enemy.moveSpeed = 0f;
        yield return new WaitForSeconds(10);
        enemy.moveSpeed = oldSpeed;
        RemoveStatusEffect(StatusEffect.Freeze);

    }

    public IEnumerator PoisonCoroutine()
    {   
        // slows and depletes health for 10 seconds
        float oldSpeed = enemy.moveSpeed;
        enemy.moveSpeed = enemy.moveSpeed - 10f;

        // if enemy speed is less than 1, set it to 1
        if (enemy.moveSpeed < 1)
        {
            enemy.moveSpeed = 1;
        }

        float duration = 5f;
        float interval = 1f; 
        int dmgPerInterval = 2;
        while (duration > 0f) {
            // while active
            enemy.TakeDamageNoSource(Mathf.Max(dmgPerInterval, 0));
            yield return new WaitForSeconds(interval);
            duration-= interval;
            dmgPerInterval--;
        }

        yield return new WaitForSeconds(10);
        enemy.moveSpeed = oldSpeed;
        RemoveStatusEffect(StatusEffect.Poison);

    }
}