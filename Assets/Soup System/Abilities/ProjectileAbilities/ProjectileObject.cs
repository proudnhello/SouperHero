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
        gameObject.SetActive(true);
        persistenceTime = 0;

        transform.position = spawnPoint;
        rb.velocity = dir * stats.speed;
        transform.localScale = new Vector3(stats.size, stats.size, stats.size);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        bool isEnemy = CollisionLayers.Singleton.InEnemyLayer(collider.gameObject);
        if (isEnemy)
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();

            // Apply the infliction to the enemy
            entity.ApplyInfliction(inflictions, gameObject.transform);

            // Deactivate the projectile after hitting an enemy
            gameObject.SetActive(false);
        }
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
