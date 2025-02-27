using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;

public class EntityInflictionEffectHandler
{

    Entity entity;
    Dictionary<InflictionType, StatusEffectInstance> activeStatuses;
    public EntityInflictionEffectHandler(Entity entity)
    {
        this.entity = entity;
        activeStatuses = new();
    }

    public class StatusEffectInstance
    {
        public float amount;
        public float duration;
        public Entity entity;
        public IEnumerator StatusMethod;
        public int intervals;
        public InflictionType type;

        public StatusEffectInstance(Entity entity, Infliction infliction)
        {
            this.entity = entity;
            amount = infliction.add * infliction.mult;
            duration = infliction.InflictionFlavor.statusEffectDuration;
            type = infliction.InflictionFlavor.inflictionType;
        }

        public StatusEffectInstance(Entity entity, int amount, InflictionType type)
        {
            this.entity = entity;
            this.amount = amount;
            this.type = type;
        }

        public void StartStatusEffect(IEnumerator method)
        {
            entity.StartCoroutine(StatusMethod = method);
        }

        public void WorsenStatusEffect(Infliction infliction)
        {
            switch (infliction.InflictionFlavor.inflictionType)
            {
                case InflictionType.SPICY_Burn:
                    Inflictions.WorsenBurn(this, infliction);
                    break;
                case InflictionType.FROSTY_Freeze:
                    if (StatusMethod != null) entity.StopCoroutine(StatusMethod);
                    entity.StartCoroutine(StatusMethod = Inflictions.WorsenFreeze(this, infliction));
                    break;
            }
            
        }
    }

    public void ApplyInflictions(List<Infliction> spoonInflictions, Transform source)
    {
        foreach (var infliction in spoonInflictions)
        {
            Debug.Log("applying infliction " + infliction.InflictionFlavor.inflictionType);
            if (activeStatuses.ContainsKey(infliction.InflictionFlavor.inflictionType)) 
                activeStatuses[infliction.InflictionFlavor.inflictionType].WorsenStatusEffect(infliction);
            else
            {
                if (infliction.InflictionFlavor.inflictionType == InflictionType.SPICY_Burn)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Burn(instance));
                }
                else if (infliction.InflictionFlavor.inflictionType == InflictionType.FROSTY_Freeze)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Burn(instance));
                }
                else if (infliction.InflictionFlavor.inflictionType == InflictionType.HEARTY_Health)
                {
                    Inflictions.Health(infliction, entity);
                }
                else if (infliction.InflictionFlavor.inflictionType == InflictionType.SPIKY_Damage)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Damage(instance));
                } 
                else if (infliction.InflictionFlavor.inflictionType == InflictionType.GREASY_Knockback)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Knockback(instance, entity._rigidbody, source));
                }
            }
        }                   
    }

    public void DealDamage(int dmg)
    {
        if (!activeStatuses.ContainsKey(InflictionType.SPIKY_Damage))
        {
            StatusEffectInstance instance = new(entity, dmg, InflictionType.SPIKY_Damage);
            activeStatuses.Add(InflictionType.SPIKY_Damage, instance);
            instance.StartStatusEffect(Inflictions.Damage(instance));
        }

    }

    public bool IsAfflicted(InflictionType inflictionType)
    {
        return activeStatuses.ContainsKey(inflictionType);
    }

    public Dictionary<InflictionType, StatusEffectInstance> GetActiveStatuses()
    {
        return activeStatuses;
    }

    public void EndStatusEffect(StatusEffectInstance instance)
    {
        if (activeStatuses.ContainsKey(instance.type)) activeStatuses.Remove(instance.type);
    }
}