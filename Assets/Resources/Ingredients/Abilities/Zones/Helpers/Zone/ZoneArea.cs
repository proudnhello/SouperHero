// portions of this file were generated using GitHub Copilot
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

public class ZoneArea : MonoBehaviour
{
    public List<Infliction> inflictions;
    public List<GameObject> gameObjectsEffected;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (CollisionLayers.Singleton.InEnemyLayer(collider.gameObject) || collider.gameObject.CompareTag("Player"))
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();

            if (entity != null)
            {
                // Apply the infliction to the enemy
                entity.ApplyInfliction(inflictions, gameObject.transform);
            }
         
        }
        else if (CollisionLayers.Singleton.InDestroyableLayer(collider.gameObject))
        {
            collider.gameObject.GetComponent<Destroyables>().RemoveDestroyable();
        }
    }

}
