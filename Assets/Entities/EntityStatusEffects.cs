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
            intensity = intensity
        };
    }

    public StatusEffect Duplicate(StatusEffect effect)
    {
        return new StatusEffect
        {
            statusType = effect.statusType,
            duration = effect.duration,
            intensity = effect.intensity,
            operation = effect.operation
        };
    }

    private delegate void StatusDelegate(StatusEffect effect);
    private void CreateStatusEffects()
    {
        print("making status effects");
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
        // Take all of the status effects and merge them into one per type
        // Then apply the status effect to the entity
        foreach (StatusType status in effectFunctions.Keys)
        {
            if(!HasStatusEffect(status))
            {
                continue;
            }
            StatusEffect effect = new StatusEffect();
            effect.statusType = status;
            effect.intensity = 0;
            effect.operation = Operation.Add;

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

            // Finally, apply the status effect
            effectFunctions[status](effect);
        }
    }

    // Different status effects have different coroutines
    public void Slow(StatusEffect effect)
    {
        entity.SetMoveSpeed(entity.GetBaseMoveSpeed() - effect.intensity);
    }

    public void Burn(StatusEffect effect)
    {
        print("burning " + effect.intensity);
        entity.TakeDamage(effect.intensity);
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