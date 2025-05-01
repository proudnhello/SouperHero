using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardousGround : MonoBehaviour
{
    [SerializeField] List<FlavorIngredient.InflictionFlavor> appliedInflictions;
    List<SoupSpoon.SpoonInfliction> inflictionType;
    [SerializeField] float mult = 1f;

    [SerializeField] float leaveTimer = 0f;

    // If true, the entity will be able to attack while on the ground. False will prevent attacking
    [SerializeField] bool canAttack = true;
    private void Start()
    {
        inflictionType = new List<SoupSpoon.SpoonInfliction>();
        foreach (FlavorIngredient.InflictionFlavor infliction in appliedInflictions)
        {
            SoupSpoon.SpoonInfliction spoonInfliction = new SoupSpoon.SpoonInfliction(infliction);
            spoonInfliction.add = infliction.amount;
            spoonInfliction.mult = mult;
            inflictionType.Add(spoonInfliction);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity != null && !entity.flying)
        {
            if (!canAttack)
            {
                entity.AddCantAttack();
            }
            EffectedAnimationStart(entity);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity != null && !entity.flying)
        {
            if (!entity.HasInfliction(inflictionType[0]))
            {
                entity.ApplyInfliction(inflictionType, transform, true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity != null && !entity.flying)
        {
            if (!canAttack)
            {
                entity.RemoveCantAttack();
            }
            EffectedAnimationEnd(entity);
            foreach (SoupSpoon.SpoonInfliction infliction in inflictionType)
            {
                SoupSpoon.SpoonInfliction copy = new SoupSpoon.SpoonInfliction(infliction);
                copy.InflictionFlavor.statusEffectDuration = leaveTimer;
                entity.ApplyInfliction(new List<SoupSpoon.SpoonInfliction> { infliction }, transform, true);
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
