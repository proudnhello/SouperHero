using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using StatusEffectInstance = EntityInflictionEffectHandler.StatusEffectInstance;
using Infliction = SoupSpoon.SpoonInfliction;
using UnityEngine.AI;
using Unity.VisualScripting.FullSerializer;

public class Inflictions
{
    #region INFLICTION PARAMETERS
    static float BURN_INTERVAL = 2f;
    static float BURN_INTERVAL_DEVIATION = .25f;
    static float FREEZE_TIME_DEVIATION = .25f;
    static float KNOCKBACK_MULTIPLIER = 150f;
    #endregion

    public static void Health(Infliction infliction, Entity entity)
    {
        entity.ModifyHealth(Mathf.CeilToInt(infliction.add * infliction.mult));
    }

    public static IEnumerator Damage(StatusEffectInstance instance)
    {
        instance.entity.ModifyHealth(-Mathf.CeilToInt(instance.amount));
        instance.entity.StartCoroutine(instance.entity.entityRenderer.TakeDamageAnimation());
        yield return new WaitForSeconds(instance.entity.GetInvincibility());
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }

    public static IEnumerator Burn(StatusEffectInstance instance)
    {
        instance.intervals = Mathf.CeilToInt(instance.duration / BURN_INTERVAL);
        while(instance.intervals > 0)
        {
            instance.intervals--;
            instance.entity.ModifyHealth(-Mathf.CeilToInt(instance.amount));
            yield return new WaitForSeconds(BURN_INTERVAL + Random.Range(-BURN_INTERVAL_DEVIATION, BURN_INTERVAL_DEVIATION));
        }
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }

    public static void WorsenBurn(StatusEffectInstance instance, Infliction newInfliction)
    {
        instance.duration = instance.duration > newInfliction.InflictionFlavor.statusEffectDuration ? instance.duration : newInfliction.InflictionFlavor.statusEffectDuration;
        instance.amount = instance.amount > newInfliction.InflictionFlavor.amount ? instance.amount : newInfliction.InflictionFlavor.amount;
        instance.intervals = Mathf.CeilToInt(instance.duration / BURN_INTERVAL);
    }

    public static IEnumerator Freeze(StatusEffectInstance instance)
    {
        instance.entity.SetMoveSpeed(instance.amount);
        yield return new WaitForSeconds(instance.duration + Random.Range(-FREEZE_TIME_DEVIATION, FREEZE_TIME_DEVIATION));
        instance.entity.ResetMoveSpeed();
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }

    public static IEnumerator WorsenFreeze(StatusEffectInstance instance, Infliction newInfliction)
    {
        instance.duration = instance.duration > newInfliction.InflictionFlavor.statusEffectDuration ? instance.duration : newInfliction.InflictionFlavor.statusEffectDuration;
        instance.amount = instance.amount > newInfliction.InflictionFlavor.amount ? instance.amount : newInfliction.InflictionFlavor.amount;
        instance.entity.SetMoveSpeed(instance.amount);
        yield return new WaitForSeconds(instance.duration + Random.Range(-FREEZE_TIME_DEVIATION, FREEZE_TIME_DEVIATION));
        instance.entity.ResetMoveSpeed();
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }


    public static IEnumerator Knockback(StatusEffectInstance instance, Rigidbody2D target, Transform source)
    {
        Debug.Log("adding " + instance.amount + " knockback to " + instance.entity.gameObject.name);
        NavMeshAgent agent = target.GetComponent<NavMeshAgent>();
        if (agent)
        {
            agent.updatePosition = false;
        }
        target.velocity = Vector3.zero;
        Vector3 direction = (target.transform.position - source.transform.position).normalized;
        target.AddForce(direction * instance.amount * KNOCKBACK_MULTIPLIER, ForceMode2D.Impulse);
        yield return new WaitForSeconds(instance.entity.GetInvincibility());
        instance.entity.inflictionHandler.EndStatusEffect(instance);
        // If the agent is dead, don't bother making it move again
        if (agent && !target.GetComponent<Entity>().IsDead())
        {
            agent.nextPosition = target.transform.position;
            agent.updatePosition = true;
        }
    }
}