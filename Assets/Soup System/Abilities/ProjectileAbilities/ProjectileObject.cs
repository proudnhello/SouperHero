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
        Debug.Log($"Projectile hit: {collider.gameObject.name}");

        if (CollisionLayers.Singleton == null)
        {
            Debug.LogError("CollisionLayers.Singleton is null!");
            return;
        }

        bool isEnemy = CollisionLayers.Singleton.InEnemyLayer(collider.gameObject);
        Debug.Log($"Is enemy? {isEnemy}");

        if (isEnemy)
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();

            if (entity == null)
            {
                Debug.LogError($"Entity component is missing on {collider.gameObject.name}!");
                return;
            }

            Debug.Log($"Applying infliction to {collider.gameObject.name}");

            if (inflictions == null)
            {
                Debug.LogError("Inflictions list is null!");
                return;
            }

            // Apply the infliction to the enemy
            entity.ApplyInfliction(inflictions, gameObject.transform);
            Debug.Log("Infliction applied successfully");

            // Deactivate the projectile after hitting an enemy
            gameObject.SetActive(false);
            Debug.Log("Projectile deactivated");
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
