using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : HazardousGround
{
    protected override void EffectedAnimationStart(Entity entity)
    {
        base.EffectedAnimationStart(entity);
        if (entity.submergeMask != null)
        {
            entity.submergeMask.enabled = true;
        }
    }

    protected override void EffectedAnimationEnd(Entity entity)
    {
        base.EffectedAnimationEnd(entity);
        if (entity.submergeMask != null)
        {
            entity.submergeMask.enabled = false;
        }
    }
}
