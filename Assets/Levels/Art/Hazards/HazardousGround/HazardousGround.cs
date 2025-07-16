using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardousGround : Hazard
{
    [SerializeField] List<FlavorIngredient.InflictionFlavor> HazardInflictions;
    List<FinishedSoup.SoupInflictionStat> inflictions;


    // If true, the entity will be able to attack while on the ground. False will prevent attacking
    [SerializeField] bool canAttack = true;
    protected override void Start()
    {
        base.Start();
        inflictions = new List<FinishedSoup.SoupInflictionStat>();
        foreach (FlavorIngredient.InflictionFlavor infliction in HazardInflictions)
        {
            FinishedSoup.SoupInflictionStat spoonInfliction = new FinishedSoup.SoupInflictionStat(infliction.inflictionType);
            spoonInfliction.Add(infliction.amount);
            inflictions.Add(spoonInfliction);
        }
    }

    public override void AddEntity(Entity entity)
    {
        if (entity != null && !entity.flying)
        {
            base.AddEntity(entity);
            if (!canAttack)
            {
                entity.AddCantAttack();
            }
            EffectedAnimationStart(entity);
        }
    }


    private void Update()
    {
        foreach (Entity entity in effectedEntities)
        {
            if (entity != null && !entity.flying)
            {
                if (!entity.HasInfliction(inflictions[0].InflictionType))
                {
                    entity.ApplyInfliction(inflictions, transform);
                }
            }
        }
    }

    public override void RemoveEntity(Entity entity)
    {
        if (entity != null && effectedEntities != null && effectedEntities.Contains(entity))
        {
            base.RemoveEntity(entity);
            if (!canAttack)
            {
                entity.RemoveCantAttack();
            }
            EffectedAnimationEnd(entity);
            if (entity.HasInfliction(inflictions[0].InflictionType))
            {
                entity.inflictionHandler.EndStatusEffect(inflictions[0].InflictionType);
            }
        }
    }

    // By default, these are empty. They can be overridden in the child classes to add animations
    protected virtual void EffectedAnimationStart(Entity entity)
    {
        return;
    }

    protected virtual void EffectedAnimationEnd(Entity entity)
    {
        return;
    }
}
