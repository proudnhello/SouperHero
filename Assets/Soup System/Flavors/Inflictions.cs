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
        entity.AddHealth(Mathf.CeilToInt(infliction.add * infliction.mult));
    }

    public static void Damage(Infliction infliction, Entity entity)
    {
        entity.TakeDamage(Mathf.CeilToInt(infliction.add * infliction.mult));
    }

    public static IEnumerator Burn(StatusEffectInstance instance)
    {
        instance.intervals = Mathf.CeilToInt(instance.duration / burnInterval);
        while(instance.intervals > 0)
        {
            instance.intervals--;
            instance.entity.TakeDamage(Mathf.CeilToInt(instance.amount));
            yield return new WaitForSeconds(burnInterval + Random.Range(-burnIntervalDeviation, burnIntervalDeviation));
        }
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }

    public static void WorsenBurn(StatusEffectInstance instance, Infliction newInfliction)
    {
        instance.duration = instance.duration > newInfliction.InflictionFlavor.duration ? instance.duration : newInfliction.InflictionFlavor.duration;
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
        instance.duration = instance.duration > newInfliction.InflictionFlavor.duration ? instance.duration : newInfliction.InflictionFlavor.duration;
        instance.amount = instance.amount > newInfliction.InflictionFlavor.amount ? instance.amount : newInfliction.InflictionFlavor.amount;
        instance.entity.SetMoveSpeed(instance.amount);
        yield return new WaitForSeconds(instance.duration + Random.Range(-freezeTimeDeviation, freezeTimeDeviation));
        instance.entity.ResetMoveSpeed();
        instance.entity.inflictionHandler.EndStatusEffect(instance);
    }
}