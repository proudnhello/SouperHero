using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Infliction = SoupSpoon.SpoonInfliction;

public class ProjectileObject : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float SIZE_MULTIPLIER = .25f;
    [SerializeField] int BOUNCE_MAX = 3;

    List<Infliction> inflictions;
    bool canHitPlayer = false;
    AbilityStats stats;
    float persistenceTime;
    int bounceCounter = 0;

    public void Spawn(Vector2 spawnPoint, Vector2 dir, AbilityStats stats, List<Infliction> inflictions, Sprite[] frames, float animFPS)
    {
        this.stats = stats;
        persistenceTime = 0;

        transform.position = spawnPoint;
        transform.localScale = new Vector3(stats.size * SIZE_MULTIPLIER, stats.size * SIZE_MULTIPLIER, stats.size * SIZE_MULTIPLIER);
        transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        this.inflictions = inflictions;
        gameObject.SetActive(true);
        rb.velocity = dir * stats.speed;
        canHitPlayer = false;
        bounceCounter = 0;
        if (frames != null) StartCoroutine(HandleAnimation(frames, animFPS));
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (CollisionLayers.Singleton.InEnvironmentLayer(collider.gameObject) && collider.tag != "PitHazard")
        {
            canHitPlayer = true;
            BounceOff(collider);
        }
        else if (CollisionLayers.Singleton.InEnemyLayer(collider.gameObject) || (collider.gameObject.CompareTag("Player") && canHitPlayer))
        {
            Entity entity = collider.gameObject.GetComponent<Entity>();

            // check if entity is null for player projectile
            if (entity == null)
            {
                return;
            }

            // Apply the infliction to the enemy
            entity.ApplyInfliction(inflictions, gameObject.transform);
            canHitPlayer = false;
            gameObject.SetActive(false);
        }
        else if (CollisionLayers.Singleton.InDestroyableLayer(collider.gameObject))
        {
            collider.gameObject.GetComponent<Destroyables>().RemoveDestroyable();
            canHitPlayer = false;
            gameObject.SetActive(false);
        }
    }

    IEnumerator HandleAnimation(Sprite[] frames, float FPS)
    {
        int frame = 0;
        while (true)
        {
            spriteRenderer.sprite = frames[frame];
            yield return new WaitForSeconds(1f / FPS);
            frame = (frame + 1) % frames.Length;
        }
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

        Vector2 dir = rb.velocity.normalized;
        transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        bounceCounter++;
        if (bounceCounter >= BOUNCE_MAX) gameObject.SetActive(false);
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
