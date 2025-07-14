// portions of this file were generated using GitHub Copilot
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = FinishedSoup.SoupInfliction;
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

        public StatusEffectInstance(Entity entity, Infliction infliction)
        {
            this.entity = entity;
            amount = infliction.amount;
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
            if (entity.isActiveAndEnabled)
            {
                entity.StartCoroutine(StatusMethod = method);
            }
        }

        public void WorsenStatusEffect(Infliction infliction)
        {
            switch (infliction.InflictionFlavor.inflictionType)
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

    public bool HasInfliction(Infliction infliction)
    {
        if (activeStatuses.ContainsKey(infliction.InflictionFlavor.inflictionType))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ApplyInflictions(List<Infliction> spoonInflictions, Transform source, bool quiet = false)
    {
        foreach (var infliction in spoonInflictions)
        {
            Color hitmarkerColor = FlavorIngredient.GetFlavorHitmarkerColor(infliction.InflictionFlavor.inflictionType);
            string hitmarkerText = FlavorIngredient.GetFlavorHitmarker(infliction.InflictionFlavor.inflictionType);
            hitmarkerText ??= "";
            if (activeStatuses.ContainsKey(infliction.InflictionFlavor.inflictionType)) 
                activeStatuses[infliction.InflictionFlavor.inflictionType].WorsenStatusEffect(infliction);
            else
            {
                if (infliction.InflictionFlavor.inflictionType == InflictionType.SPICY_Burn)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    // Handle hitmarkers in the damage coroutine
                    hitmarkerText = "+" + infliction.amount + " " + hitmarkerText;
                    instance.StartStatusEffect(Inflictions.Burn(instance));
                }
                else if (infliction.InflictionFlavor.inflictionType == InflictionType._Health)
                {
                    Inflictions.Health(infliction, entity);
                    hitmarkerText = "+" + infliction.amount + " " + hitmarkerText;
                }
                else if (infliction.InflictionFlavor.inflictionType == InflictionType.SPIKY_Damage)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Damage(instance));
                    hitmarkerText = "-" + infliction.amount + " " + hitmarkerText;
                } 
                else if (infliction.InflictionFlavor.inflictionType == InflictionType.SLIMY_Knockback)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Knockback(instance, entity._rigidbody, source));
                    hitmarkerText = "+" + infliction.amount + " " + hitmarkerText;
                }else if(infliction.InflictionFlavor.inflictionType == InflictionType.VAMPIRISM_LifeSteal)
                {
                    Inflictions.Vampirism(infliction, entity, source);
                }else if(infliction.InflictionFlavor.inflictionType == InflictionType.FROSTY_Freeze)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Freeze(instance));
                    hitmarkerText = "+" + infliction.amount + " " + hitmarkerText;
                }
                else if (infliction.InflictionFlavor.inflictionType == InflictionType._Water)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
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