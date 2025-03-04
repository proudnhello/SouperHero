using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

public class ProjectileArea : MonoBehaviour
{
    public List<Infliction> inflictions;
    public ProjectileObject projectileObject;
    public bool canHitPlayer = false;

    // To protect the player from being hit as they shoot the projectile, a delay is added for delay seconds before the player can be hit
    public IEnumerator PlayerDelay(float delay)
    {
        canHitPlayer = false;
        yield return new WaitForSeconds(delay);
        canHitPlayer = true;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (CollisionLayers.Singleton.InEnemyLayer(collider.gameObject) || (collider.gameObject.CompareTag("Player") && canHitPlayer))
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();

            // Apply the infliction to the enemy
            entity.ApplyInfliction(inflictions, gameObject.transform);
            StopCoroutine("PlayerDelay");
            canHitPlayer = false;
            projectileObject.gameObject.SetActive(false);
        }
        else if (CollisionLayers.Singleton.InDestroyableLayer(collider.gameObject))
        {
            collider.gameObject.GetComponent<Destroyables>().RemoveDestroyable();
            StopCoroutine("PlayerDelay");
            canHitPlayer = false;
            projectileObject.gameObject.SetActive(false);
        }
    }

}
