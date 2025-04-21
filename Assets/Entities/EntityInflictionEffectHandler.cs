// portions of this file were generated using GitHub Copilot
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
            amount = infliction.amount;
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
            }
            
        }
    }

    public void ApplyInflictions(List<Infliction> spoonInflictions, Transform source)
    {
        foreach (var infliction in spoonInflictions)
        {
            Color hitmarkerColor = FlavorIngredient.inflictionColorMapping[infliction.InflictionFlavor.inflictionType];
            string hitmarkerText = FlavorIngredient.GetFlavorHitmarker(infliction.InflictionFlavor.inflictionType);
            if(hitmarkerColor == null) hitmarkerColor = Color.white;
            if(hitmarkerText == null) hitmarkerText = "DEFAULT HITMARKER TEXT";
            //Debug.Log("applying infliction " + infliction.InflictionFlavor.inflictionType);
            if (activeStatuses.ContainsKey(infliction.InflictionFlavor.inflictionType)) 
                activeStatuses[infliction.InflictionFlavor.inflictionType].WorsenStatusEffect(infliction);
            else
            {
                if (infliction.InflictionFlavor.inflictionType == InflictionType.SPICY_Burn)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    // Handle hitmarkers in the damage coroutine
                    hitmarkerText = "";
                    instance.StartStatusEffect(Inflictions.Burn(instance));
                }
                else if (infliction.InflictionFlavor.inflictionType == InflictionType.HEARTY_Health)
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
                else if (infliction.InflictionFlavor.inflictionType == InflictionType.GREASY_Knockback)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    instance.StartStatusEffect(Inflictions.Knockback(instance, entity._rigidbody, source));
                    hitmarkerText = "+" + infliction.amount + " " + hitmarkerText;
                }else if(infliction.InflictionFlavor.inflictionType == InflictionType.UMAMI_Vampirism)
                {
                    Inflictions.Vampirism(infliction, entity, source);
                    // Display nothing, as it'll appear as healing the player and damage to the enemy
                    hitmarkerText = "";
                }else if(infliction.InflictionFlavor.inflictionType == InflictionType.FROSTY_Freeze)
                {
                    StatusEffectInstance instance = new(entity, infliction);
                    activeStatuses.Add(infliction.InflictionFlavor.inflictionType, instance);
                    Debug.Log("Freezing " + entity.gameObject.name);
                    instance.StartStatusEffect(Inflictions.Freeze(instance));
                }
            }
            entity.DisplayHitmarker(hitmarkerColor, hitmarkerText);
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
        if (activeStatuses.ContainsKey(instance.type)) activeStatuses.Remove(instance.type);
    }
}