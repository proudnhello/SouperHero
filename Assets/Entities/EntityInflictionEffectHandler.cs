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

        public StatusEffectInstance(Entity entity, int amount, InflictionType type)
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

    public bool HasInfliction(InflictionStat infliction)
    {
        if (activeStatuses.ContainsKey(infliction.InflictionType))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ApplyInflictions(List<InflictionStat> spoonInflictions, Transform source, bool quiet = false)
    {
        foreach (var infliction in spoonInflictions)
        {
            Color hitmarkerColor = FlavorIngredient.GetFlavorHitmarkerColor(infliction.InflictionType);
            string hitmarkerText = FlavorIngredient.GetFlavorHitmarker(infliction.InflictionType);
            hitmarkerText ??= "";
            if (activeStatuses.ContainsKey(infliction.InflictionType)) 
                activeStatuses[infliction.InflictionType].WorsenStatusEffect(infliction);
            else
            {
                if (infliction.InflictionType == InflictionType.SPICY_Burn)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionType, instance);
                    // Handle hitmarkers in the damage coroutine
                    hitmarkerText = "+" + infliction.Amount + " " + hitmarkerText;
                    instance.StartStatusEffect(Inflictions.Burn(instance));
                }
                else if (infliction.InflictionType == InflictionType._Health)
                {
                    Inflictions.Health(infliction, entity);
                    hitmarkerText = "+" + infliction.Amount + " " + hitmarkerText;
                }
                else if (infliction.InflictionType == InflictionType.SPIKY_Damage)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Damage(instance));
                    hitmarkerText = "-" + infliction.Amount + " " + hitmarkerText;
                } 
                else if (infliction.InflictionType == InflictionType.SLIMY_Knockback)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Knockback(instance, entity._rigidbody, source));
                    hitmarkerText = "+" + infliction.Amount + " " + hitmarkerText;
                }else if(infliction.InflictionType == InflictionType.VAMPIRISM_LifeSteal)
                {
                    Inflictions.Vampirism(infliction, entity, source);
                }else if(infliction.InflictionType == InflictionType.FROSTY_Freeze)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Freeze(instance));
                    hitmarkerText = "+" + infliction.Amount + " " + hitmarkerText;
                }
                else if (infliction.InflictionType == InflictionType._Water)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Water(instance));
                }
            }
            if (!quiet)
            {
                entity.DisplayHitmarker(hitmarkerColor, hitmarkerText);
            }
        }
    }

    public void DealDamage(int dmg)
    {
        if (!activeStatuses.ContainsKey(InflictionType.SPIKY_Damage))
        {
            Color hitmarkerColor = FlavorIngredient.inflictionColorMapping[InflictionType.SPIKY_Damage];
            string hitmarkerText = FlavorIngredient.GetFlavorHitmarker(InflictionType.SPIKY_Damage);
            StatusEffectInstance instance = new(entity, dmg, InflictionType.SPIKY_Damage);
            activeStatuses.Add(InflictionType.SPIKY_Damage, instance);
            instance.StartStatusEffect(Inflictions.Damage(instance));
            hitmarkerText = "-" + dmg + " " + hitmarkerText;
            entity.DisplayHitmarker(hitmarkerColor, hitmarkerText);
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
        if (activeStatuses.ContainsKey(instance.type))
        {
            activeStatuses[instance.type].End();
            activeStatuses.Remove(instance.type);
        }
    }
    public void EndStatusEffect(InflictionType type)
    {
        if (activeStatuses.ContainsKey(type))
        {
            activeStatuses[type].End();
            activeStatuses.Remove(type);
        }
    }
}