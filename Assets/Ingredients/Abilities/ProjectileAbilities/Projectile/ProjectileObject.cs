using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

public class ProjectileObject : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] ProjectileArea projectileArea;

    AbilityStats stats;
    List<Infliction> inflictions;
    float persistenceTime;
    float PLAYER_SAFTY_DELAY = 0.2f;

    public void Spawn(Vector2 spawnPoint, Vector2 dir, AbilityStats stats, List<Infliction> inflictions)
    {
        this.stats = stats;
        this.inflictions = inflictions;
        persistenceTime = 0;

        transform.position = spawnPoint;
        projectileArea.transform.localScale = new Vector3(stats.size, stats.size, stats.size);
        projectileArea.inflictions = inflictions;
        gameObject.SetActive(true);
        projectileArea.StartCoroutine(projectileArea.PlayerDelay(PLAYER_SAFTY_DELAY));
        rb.velocity = dir * stats.speed;

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (CollisionLayers.Singleton.InEnvironmentLayer(collider.gameObject))
        {
            BounceOff(collider);
        }
        print("ProjectileObject collided with " + collider.gameObject.name);
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
