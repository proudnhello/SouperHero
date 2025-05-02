using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Programed with the help of Github Copilot

public class PitHazard : Hazard
{
    [SerializeField] float coyoteTime = 0.2f; // Time entities can stay in the air before falling
    [SerializeField] Transform playerRespawnPoint;
    
    public override void AddEntity(Entity entity)
    {
        if (effectedEntities.Contains(entity))
        {
            return;
        }
        StartCoroutine(Fall(entity));
        base.AddEntity(entity);
    }

    private IEnumerator Fall(Entity entity)
    {
        // Give players a little extra leeway 
        if(entity.GetType() == typeof(PlayerEntityManager))
        {
            yield return new WaitForSeconds(coyoteTime);
        }
        if (entity.CheckBounds(this))
        {
            entity.Fall(playerRespawnPoint);
            base.RemoveEntity(entity);
        }
    }
}
