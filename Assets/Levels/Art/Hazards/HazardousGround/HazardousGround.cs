using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardousGround : Hazard
{
    [SerializeField] List<FlavorIngredient.InflictionFlavor> appliedInflictions;
    List<FinishedSoup.SoupInfliction> inflictionType;
    [SerializeField] float mult = 1f;

    [SerializeField] float leaveTimer = 0f;

    // If true, the entity will be able to attack while on the ground. False will prevent attacking
    [SerializeField] bool canAttack = true;
    protected override void Start()
    {
        base.Start();
        inflictionType = new List<FinishedSoup.SoupInfliction>();
        foreach (FlavorIngredient.InflictionFlavor infliction in appliedInflictions)
        {
            FinishedSoup.SoupInfliction spoonInfliction = new FinishedSoup.SoupInfliction(infliction);
            spoonInfliction.add = infliction.amount;
            spoonInfliction.mult = mult;
            inflictionType.Add(spoonInfliction);
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
                if (!entity.HasInfliction(inflictionType[0]))
                {
                    entity.ApplyInfliction(inflictionType, transform, true);
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
            foreach (FinishedSoup.SoupInfliction infliction in inflictionType)
            {
                FinishedSoup.SoupInfliction copy = new FinishedSoup.SoupInfliction(infliction);
                copy.InflictionFlavor.statusEffectDuration = leaveTimer;
                entity.ApplyInfliction(new List<FinishedSoup.SoupInfliction> { infliction }, transform, true);
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
