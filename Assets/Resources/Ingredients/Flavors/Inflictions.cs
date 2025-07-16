// portions of this file were generated using GitHub Copilot
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using StatusEffectInstance = EntityInflictionEffectHandler.StatusEffectInstance;
using InflictionStat = FinishedSoup.SoupInflictionStat;
using UnityEngine.AI;
using Unity.VisualScripting.FullSerializer;
using System.Collections.Generic;

public class Inflictions
{
    #region INFLICTION PARAMETERS
    static float BURN_INTERVAL_DURATION = 1f;
    static int MAX_BURN_INTERVALS = 10;
    static float BURN_INTERVAL_DEVIATION = .25f;
    static float FREEZE_INTERVAL_DURATION = .25f;
    static float FREEZE_TIME_DEVIATION = .05f;
    static int MAX_FREEZE_INTERVALS = 10;
    static float KNOCKBACK_MULTIPLIER = 150f;
    #endregion

    public static void Health(InflictionStat infliction, Entity entity)
    {
        entity.DisplayHitmarker(FlavorIngredient.InflictionFlavor.InflictionType._Health, infliction.Amount);
        entity.ModifyHealth(Mathf.CeilToInt(infliction.Amount));
    }

    public static IEnumerator Damage(StatusEffectInstance instance)
    {
        instance.entity.ModifyHealth(-Mathf.CeilToInt(instance.amount));
        instance.entity.entityRenderer.TakeDamage();
        instance.entity.DisplayHitmarker(instance.type, instance.amount);
        yield return new WaitForSeconds(instance.entity.GetInvincibility());
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }

    public static IEnumerator Burn(StatusEffectInstance instance)
    {
        instance.intervals = Mathf.Clamp(Mathf.CeilToInt(instance.amount / BURN_INTERVAL_DURATION),0, MAX_BURN_INTERVALS);
        while(instance.intervals > 0)
        {
            instance.intervals--;
            instance.entity.ModifyHealth(-Mathf.CeilToInt(instance.amount));
            instance.entity.DisplayHitmarker(instance.type, instance.amount);
            instance.entity.entityRenderer.TakeDamage();
            yield return new WaitForSeconds(BURN_INTERVAL_DURATION + Random.Range(-BURN_INTERVAL_DEVIATION, BURN_INTERVAL_DEVIATION));
        }
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }

    public static void WorsenBurn(StatusEffectInstance instance, InflictionStat newInfliction)
    {
        instance.amount = instance.amount > newInfliction.Amount ? instance.amount : newInfliction.Amount;
        instance.intervals = Mathf.Clamp(Mathf.CeilToInt(instance.intervals + instance.amount / BURN_INTERVAL_DURATION), 0, MAX_BURN_INTERVALS);
    }

    public static IEnumerator Freeze(StatusEffectInstance instance)
    {
        instance.entity.SetMoveSpeed(10, 1 / instance.amount);
        instance.intervals = Mathf.CeilToInt(instance.amount);
        instance.entity.DisplayHitmarker(instance.type, instance.amount);
        do
        {
            instance.intervals--;
            yield return new WaitForSeconds(FREEZE_INTERVAL_DURATION + Random.Range(-FREEZE_TIME_DEVIATION, FREEZE_TIME_DEVIATION));
        } while (instance.intervals > 0);
        instance.entity.ResetMoveSpeed(10);
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }

    public static void WorsenFreeze(StatusEffectInstance instance, InflictionStat newInfliction)
    {
        instance.intervals = Mathf.Clamp(Mathf.CeilToInt(newInfliction.Amount+instance.intervals), 0, MAX_FREEZE_INTERVALS);
    }

    public static IEnumerator Water(StatusEffectInstance instance)
    {
        instance.entity.SetMoveSpeed(15123, 1 / instance.amount);

        yield return new WaitUntil(() => instance.triggerEnd);

        instance.entity.ResetMoveSpeed(15123);
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }

    public static IEnumerator Knockback(StatusEffectInstance instance, Rigidbody2D target, Transform source)
    {
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

    // Deal damage to the target and (try to) heal the source by applying the respective inflictions
    public static void Vampirism(InflictionStat instance, Entity target, Transform source)
    {
        //InflictionStat damage = new(FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage);
        //damage.CombineStats(instance);
        //target.ApplyInfliction(new() { damage}, source, true);

        if (!source.TryGetComponent(out Entity entity))
        {
            if (source.CompareTag("Player")) entity = PlayerEntityManager.Singleton;
        }

        Debug.Log(entity);

        if (entity != null)
        {
            InflictionStat heal = new(FlavorIngredient.InflictionFlavor.InflictionType._Health);
            heal.CombineStats(instance);
            // Only heal half the amount of damage (that was supposed to be dealt)
            heal.add /= 2;
            List<InflictionStat> heals = new() { heal };
            entity.ApplyInfliction(heals, source);
        }
    }
}