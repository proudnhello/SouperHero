using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

public class ProjectileObject : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;

    AbilityStats stats;
    List<Infliction> inflictions;
    float persistenceTime;


    public void Spawn(Vector2 spawnPoint, Vector2 dir, AbilityStats stats, List<Infliction> inflictions)
    {
        this.stats = stats;
        this.inflictions = inflictions;
        persistenceTime = 0;

        transform.position = spawnPoint;
        transform.localScale = new Vector3(stats.size, stats.size, stats.size);
        gameObject.SetActive(true);
        rb.velocity = dir * stats.speed;

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (CollisionLayers.Singleton.InEnemyLayer(collider.gameObject))
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();

            // Apply the infliction to the enemy
            entity.ApplyInfliction(inflictions, gameObject.transform);
        }
        else if (CollisionLayers.Singleton.InDestroyableLayer(collider.gameObject))
        {
            collider.gameObject.GetComponent<Destroyables>().RemoveDestroyable();
        }

        // Reflect the projectile instead of deactivating it
        BounceOff(collider);
    }

    private void BounceOff(Collider2D collider)
    {
        if (rb == null) return;

        // Find the closest point on the collider
        Vector2 collisionPoint = collider.ClosestPoint(transform.position);

        // Approximate the collision normal
        Vector2 normal = ((Vector2)transform.position - collisionPoint).normalized;

        // Reflect the velocity along the normal
        rb.velocity = Vector2.Reflect(rb.velocity, normal);
    }



    private void FixedUpdate()
    {
        if (persistenceTime < stats.duration)
        {
            persistenceTime += Time.fixedDeltaTime;
        } 
        else
        {
            gameObject.SetActive(false);
        } 
    }
}
