using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatusEffects : MonoBehaviour
{
    private Entity entity;
    private float interval = 0.5f;
    private float counter = 0f;

    public void LinkToEntity(Entity entity)
    {
        CreateStatusEffects();
        this.entity = entity;
    }
    private Dictionary<StatusType, StatusDelegate> effectFunctions;

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
    public enum Operation
    {
        Add,
        Multiply
    }
    [SerializeField]
    [Serializable]
    public struct StatusEffect
    {
        public StatusType statusType;
        public Operation operation;
        public float duration;
        public int intensity;
    }

    public static StatusEffect CreateStatusEffect(StatusType statusType, float duration, Operation op, int intensity)
    {
        return new StatusEffect
        {
            statusType = statusType,
            duration = duration,
            intensity = intensity,
            operation = op,
        };
    }

    public StatusEffect Duplicate(StatusEffect effect)
    {
        return new StatusEffect
        {
            statusType = effect.statusType,
            duration = effect.duration,
            intensity = effect.intensity,
            operation = effect.operation,
        };
    }

    private delegate void StatusDelegate(StatusEffect effect);
    private void CreateStatusEffects()
    {
        effectFunctions = new Dictionary<StatusType, StatusDelegate>
        {
            { StatusType.Stun, Stun },
            { StatusType.Slow, Slow },
            { StatusType.Burn, Burn },
            { StatusType.Freeze, Freeze },
            { StatusType.Poison, Poison },
        };
        // Each status Effect should have a parameter to multiply the numbers
    }  

    public List<StatusEffect> addStatusEffects = new();
    public List<StatusEffect> multStatusEffects = new();
    public void AddStatusEffect(StatusEffect statusEffect)
    {
        if(statusEffect.operation == Operation.Add)
        {
            addStatusEffects.Add(statusEffect);
        }
        else
        {
            multStatusEffects.Add(statusEffect);
        }
    }

    // Removes all status effects of a certain type
    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        for (int i = 0; i < addStatusEffects.Count; i++)
        {
            if (addStatusEffects[i].statusType == statusEffect.statusType)
            {
                addStatusEffects.RemoveAt(i);
            }
        }

        for (int i = 0; i < multStatusEffects.Count; i++)
        {
            if (multStatusEffects[i].statusType == statusEffect.statusType)
            {
                multStatusEffects.RemoveAt(i);
            }
        }
    }
    public bool HasStatusEffect(StatusType statusEffect)
    {
        return addStatusEffects.Exists(effect => effect.statusType == statusEffect) || multStatusEffects.Exists(effect => effect.statusType == statusEffect);
    }

    public void Update()
    {
        // Apply status effects every interval
        counter += Time.deltaTime;
        if(counter < interval)
        {
            return;
        }
        counter = 0f;
        // Call the status effect functions
        foreach (StatusType status in effectFunctions.Keys)
        {
            StatusEffect effect = new StatusEffect();
            effect.statusType = status;
            effectFunctions[status](effect);
        }
    }

    // Determines the intensity of the status effect
    // If there's a base value of the stat being modified (ie, moveSpeed), pass it in as baseValue, otherwise pass in 0
    public int CalculateIntensity(List<StatusEffect> effects, StatusEffect effect, int baseValue)
    {
        StatusType status = effect.statusType;
        effect.operation = Operation.Add;
        effect.intensity = baseValue;

        // First, merge add all of the status effects of the same type (reducing their interval as we go
        for (int i = 0;i < addStatusEffects.Count;i++)
        {
            if (addStatusEffects[i].statusType == status)
            {
                StatusEffect addEffect = addStatusEffects[i];
                effect.intensity += addStatusEffects[i].intensity;
                addEffect.duration -= interval;
                addStatusEffects[i] = addEffect;
                if(addEffect.duration <= 0)
                {
                    addStatusEffects.RemoveAt(i);
                    i--;
                }
            }
        }

        // Then, multiply all of the status effects of the same type
        for(int i = 0; i < multStatusEffects.Count; i++)
        {
            if (multStatusEffects[i].statusType == status)
            {
                StatusEffect multEffect = multStatusEffects[i];
                effect.intensity *= multStatusEffects[i].intensity;
                multEffect.duration -= interval;
                multStatusEffects[i] = multEffect;
                if(multEffect.duration <= 0)
                {
                    multStatusEffects.RemoveAt(i);
                    i--;
                }
            }
        }
        return effect.intensity;
    }

    // Different status effects have different coroutines
    public void Slow(StatusEffect effect)
    {
        int intensity = CalculateIntensity(addStatusEffects, effect, 0);
        entity.SetMoveSpeed(entity.GetBaseMoveSpeed() - intensity);
    }

    public void Burn(StatusEffect effect)
    {
        int intensity = CalculateIntensity(addStatusEffects, effect, 0);
        if(intensity <= 0)
        {
            return;
        }
        entity.TakeDamage(intensity);
    }

    public void Stun(StatusEffect effect)
    {

    }

    public void Freeze(StatusEffect effect)
    {
        // TODO: make freeze different from stun
        // freeze enemy for 10 seconds
        /*float oldSpeed = entity.GetMoveSpeed();
        entity.SetMoveSpeed(0f);
        yield return new WaitForSeconds(10);
        entity.SetMoveSpeed(oldSpeed);
        RemoveStatusEffect(effect);*/

    }

    public void Poison(StatusEffect effect)
    {   
        /*// slows and depletes health for 10 seconds
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
        RemoveStatusEffect(effect);*/

    }
}