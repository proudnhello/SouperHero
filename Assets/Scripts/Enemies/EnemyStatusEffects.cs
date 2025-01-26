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


        // Check what the status effect is and if it has been added before adding

        Debug.Log("Slow?" + (statusEffect == StatusEffect.Slow));
        Debug.Log("Does Not Have Slow?" + (!HasStatusEffect(StatusEffect.Slow)));

        if (statusEffect == StatusEffect.Slow && !HasStatusEffect(StatusEffect.Slow))
        {
            Debug.Log("Adding" + statusEffect.ToString() + "...");
            // Add Status Effect
            statusEffects.Add(statusEffect);
            Debug.Log("Calling" + statusEffect.ToString() + "coroutine...");
            // Call Coroutine Associated with the status effect (Slow)
            enemy.StartCoroutine(SlowCoroutine());
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

        //// Store original color and set to light blue
        //Color originalColor = enemy.sprite.color;
        //enemy.sprite.color = new Color(0.7f, 0.7f, 1.0f);

        // wait for 10 seconds
        yield return new WaitForSeconds(10);

        // reset enemy speed and color
        enemy.moveSpeed = oldSpeed;
        //enemy.sprite.color = originalColor;

        // remove slow status effect
        RemoveStatusEffect(StatusEffect.Slow);
    }
}