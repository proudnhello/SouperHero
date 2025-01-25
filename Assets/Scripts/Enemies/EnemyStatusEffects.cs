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
    }

    public enum StatusEffect
    {
        Stun,
        Slow,
        Burn,
        Freeze,
        Poison
    }

    public List<StatusEffect> statusEffects = new();

    public void AddStatusEffect(StatusEffect statusEffect)
    {
        statusEffects.Add(statusEffect);

        Debug.Log("Adding" + statusEffect.ToString() + "...");

        if (statusEffect == StatusEffect.Slow && !HasStatusEffect(StatusEffect.Slow))
        {
            Debug.Log("Calling" + statusEffect.ToString() + "coroutine...");
            SlowCoroutine();
        }
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        statusEffects.Remove(statusEffect);
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

        // reset enemy speed
        enemy.moveSpeed = oldSpeed;

        // remove slow status effect
        RemoveStatusEffect(StatusEffect.Slow);
    }
}