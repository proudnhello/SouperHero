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


    public GameObject Spawn(Vector2 spawnPoint, Vector2 dir, AbilityStats stats, List<Infliction> inflictions)
    {
        this.stats = stats;
        this.inflictions = inflictions;
        gameObject.SetActive(true);
        persistenceTime = 0;

        transform.position = spawnPoint;
        rb.velocity = dir * stats.speed;
        transform.localScale = new Vector3(stats.size, stats.size, stats.size);
        return gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == PlayerInputAndAttackManager.Singleton.collisionLayer.value)
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();
            entity.TakeDamage(Mathf.CeilToInt(stats.damage), this.gameObject, stats.knockback);
            entity.ApplyInfliction(inflictions);
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
