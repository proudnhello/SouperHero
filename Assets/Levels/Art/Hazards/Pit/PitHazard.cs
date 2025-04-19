using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Programed with the help of Github Copilot

public class PitHazard : MonoBehaviour
{
    [SerializeField] float coyoteTime = 0.2f; // Time entities can stay in the air before falling
    [SerializeField] Transform playerRespawnPoint;
    Dictionary<Entity, bool> inCollider = new Dictionary<Entity, bool>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity != null)
        {
            print(entity.name + " entered pit hazard collider");
            inCollider[entity] = true;
            StartCoroutine(Fall(entity));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity != null)
        {
            print(entity.name + " exited pit hazard collider");
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
            print(entity.name + " fell into pit hazard");
            entity.Fall(playerRespawnPoint);
        }
        else
        {
            print(entity.name + " did not fall into pit hazard");
        }
    }
}
