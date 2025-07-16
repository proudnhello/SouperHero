// portions of this file were generated using GitHub Copilot
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InflictionStat = FinishedSoup.SoupInflictionStat;
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
        public Entity entity;
        public IEnumerator StatusMethod;
        public int intervals;
        public InflictionType type;
        public bool triggerEnd = false;

        public StatusEffectInstance(Entity entity, InflictionStat infliction)
        {
            this.entity = entity;
            amount = infliction.Amount;
            type = infliction.InflictionType;
        }

        public StatusEffectInstance(Entity entity, float amount, InflictionType type)
        {
            this.entity = entity;
            this.amount = amount;
            this.type = type;
        }

        public void StartStatusEffect(IEnumerator method)
        {
            if (entity.isActiveAndEnabled)
            {
                entity.StartCoroutine(StatusMethod = method);
            }
        }

        public void WorsenStatusEffect(InflictionStat infliction)
        {
            switch (infliction.InflictionType)
            {
                case InflictionType.SPICY_Burn:
                    Inflictions.WorsenBurn(this, infliction);
                    break;
                case InflictionType.FROSTY_Freeze:
                    Inflictions.WorsenFreeze(this, infliction);
                    break;
            }
        }

        public void End()
        {
            if (entity.isActiveAndEnabled)
            {
                triggerEnd = true;
            }
        }
    }

    public bool HasInfliction(InflictionType infliction)
    {
        return activeStatuses.ContainsKey(infliction);
    }

    public void ApplyInflictions(List<InflictionStat> spoonInflictions, Transform source)
    {
        float damage = 0;
        foreach (var infliction in spoonInflictions)
        {
            if (HasInfliction(infliction.InflictionType)) 
                activeStatuses[infliction.InflictionType].WorsenStatusEffect(infliction);
            else
            {
                if (infliction.InflictionType == InflictionType.SPICY_Burn && !entity.IsInvincible())
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Burn(instance));
                }
                else if (infliction.InflictionType == InflictionType._Health)
                {
                    Inflictions.Health(infliction, entity);
                }
                else if (infliction.InflictionType == InflictionType.SPIKY_Damage && !entity.IsInvincible())
                {
                    damage += infliction.Amount;
                } 
                else if (infliction.InflictionType == InflictionType.SLIMY_Knockback && !entity.IsInvincible())
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Knockback(instance, entity._rigidbody, source));
                }
                else if(infliction.InflictionType == InflictionType.VAMPIRISM_LifeSteal && !entity.IsInvincible())
                {
                    damage += infliction.Amount;
                    Inflictions.Vampirism(infliction, entity, source);
                }
                else if(infliction.InflictionType == InflictionType.FROSTY_Freeze && !entity.IsInvincible())
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Freeze(instance));
                }
                else if (infliction.InflictionType == InflictionType._Water)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Water(instance));
                }
            }
        }

        if (damage > 0)
        {
            DealDamage(damage);
        }
    }

    public void DealDamage(float dmg)
    {
        if (!HasInfliction(InflictionType.SPIKY_Damage))
        {
            StatusEffectInstance instance = new(entity, dmg, InflictionType.SPIKY_Damage);
            activeStatuses.Add(InflictionType.SPIKY_Damage, instance);
            instance.StartStatusEffect(Inflictions.Damage(instance));
        }
    }

    public Dictionary<InflictionType, StatusEffectInstance> GetActiveStatuses()
    {
        return activeStatuses;
    }

    public void EndStatusEffect(StatusEffectInstance instance)
    {
        if (HasInfliction(instance.type))
        {
            activeStatuses[instance.type].End();
            activeStatuses.Remove(instance.type);
        }
    }
    public void EndStatusEffect(InflictionType type)
    {
        if (HasInfliction(type))
        {
            activeStatuses[type].End();
            activeStatuses.Remove(type);
        }
    }
}