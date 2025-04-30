using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardousGround : MonoBehaviour
{
    [SerializeField] List<FlavorIngredient.InflictionFlavor> appliedInflictions;
    List<SoupSpoon.SpoonInfliction> inflictionType;
    [SerializeField] float mult = 1f;

    [SerializeField] float leaveTimer = 0f;

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
            foreach (SoupSpoon.SpoonInfliction infliction in inflictionType)
            {
                SoupSpoon.SpoonInfliction copy = new SoupSpoon.SpoonInfliction(infliction);
                copy.InflictionFlavor.statusEffectDuration = leaveTimer;
                entity.ApplyInfliction(new List<SoupSpoon.SpoonInfliction> { infliction }, transform, true);
            }
        }
    }
}
