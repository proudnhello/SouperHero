using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using StatusEffectInstance = EntityInflictionEffectHandler.StatusEffectInstance;
using Infliction = SoupSpoon.SpoonInfliction;

public class Inflictions
{
    #region INFLICTION PARAMETERS
    static float burnInterval = 2f;
    static float burnIntervalDeviation = .25f;
    static float freezeTimeDeviation = .25f;
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
        instance.intervals = Mathf.CeilToInt(instance.duration / burnInterval);
        while(instance.intervals > 0)
        {
            instance.intervals--;
            instance.entity.ModifyHealth(-Mathf.CeilToInt(instance.amount));
            yield return new WaitForSeconds(burnInterval + Random.Range(-burnIntervalDeviation, burnIntervalDeviation));
        }
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }

    public static void WorsenBurn(StatusEffectInstance instance, Infliction newInfliction)
    {
        instance.duration = instance.duration > newInfliction.InflictionFlavor.statusEffectDuration ? instance.duration : newInfliction.InflictionFlavor.statusEffectDuration;
        instance.amount = instance.amount > newInfliction.InflictionFlavor.amount ? instance.amount : newInfliction.InflictionFlavor.amount;
        instance.intervals = Mathf.CeilToInt(instance.duration / burnInterval);
    }

    public static IEnumerator Freeze(StatusEffectInstance instance)
    {
        instance.entity.SetMoveSpeed(instance.amount);
        yield return new WaitForSeconds(instance.duration + Random.Range(-freezeTimeDeviation, freezeTimeDeviation));
        instance.entity.ResetMoveSpeed();
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }

    public static IEnumerator WorsenFreeze(StatusEffectInstance instance, Infliction newInfliction)
    {
        instance.duration = instance.duration > newInfliction.InflictionFlavor.statusEffectDuration ? instance.duration : newInfliction.InflictionFlavor.statusEffectDuration;
        instance.amount = instance.amount > newInfliction.InflictionFlavor.amount ? instance.amount : newInfliction.InflictionFlavor.amount;
        instance.entity.SetMoveSpeed(instance.amount);
        yield return new WaitForSeconds(instance.duration + Random.Range(-freezeTimeDeviation, freezeTimeDeviation));
        instance.entity.ResetMoveSpeed();
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }


    public static IEnumerator Knockback(StatusEffectInstance instance, Rigidbody2D target, Transform source)
    {
        Debug.Log("adding " + instance.amount + " knockback to " + instance.entity.gameObject.name);
        Vector3 direction = (target.transform.position - source.transform.position).normalized;
        target.AddForce(direction * instance.amount, ForceMode2D.Impulse);
        yield return new WaitForSeconds(instance.entity.GetInvincibility());
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }
}