using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Programed with the help of Github Copilot

public class PitHazard : MonoBehaviour
{
    [SerializeField] float coyoteTime = 0.2f; // Time entities can stay in the air before falling
    [SerializeField] Transform playerRespawnPoint;
    Dictionary<Entity, bool> inCollider = new Dictionary<Entity, bool>();

    // Based on https://discussions.unity.com/t/how-to-check-if-one-box-collider-2d-completely-overlaps-another-box-collider-2d/828436
    private void OnTriggerStay2D(Collider2D collision)
    {
        Collider2D pitCollider = GetComponent<Collider2D>();
        Collider2D entityCollider = collision;
        Bounds pitBounds = pitCollider.bounds;
        Bounds entityBounds = entityCollider.bounds;
        Entity entity = collision.GetComponent<Entity>();
        bool fullyInside =
            entityBounds.min.x >= pitBounds.min.x &&
            entityBounds.max.x <= pitBounds.max.x &&
            entityBounds.min.y >= pitBounds.min.y &&
            entityBounds.max.y <= pitBounds.max.y;
        bool val;
        if (fullyInside && entity != null && (!inCollider.TryGetValue(entity, out val) || !val))
        {
            print("Entity Bounds: " + entityBounds.min + " to " + entityBounds.max + " Pit Bounds: " + pitBounds.min + " to " + pitBounds.max);
            inCollider[entity] = true;
            StartCoroutine(Fall(entity));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Entity entity = other.GetComponent<Entity>();
        if (entity != null)
        {
            inCollider[entity] = false;
        }
    }

    private IEnumerator Fall(Entity entity)
    {
        // Give players a little extra leeway 
        if(entity.GetType() == typeof(PlayerEntityManager))
        {
            yield return new WaitForSeconds(coyoteTime);
        }
        if (inCollider[entity])
        {
            inCollider[entity] = false;
            entity.Fall(playerRespawnPoint);
        }
    }
}
