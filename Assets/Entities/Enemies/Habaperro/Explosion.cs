using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float timeActive = 2f;
    public float radius = 6f;
    List<Entity> entitiesAffected = new();
    void Start()
    {
        StartCoroutine(timer());
        transform.localScale = new Vector3(radius, radius, radius);
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, radius);
        Debug.DrawLine(transform.position, transform.position + new Vector3(radius, radius, radius), Color.green, 10);
        foreach(var col in cols)
        {
            if (CollisionLayers.Singleton.InEntityLayer(col.gameObject))
            {
                try
                {
                    Entity entity = col.GetComponent<Entity>();
                    if (!entitiesAffected.Contains(entity))
                    {
                        entitiesAffected.Add(entity);
                        entity.ModifyHealth(-10);
                    }
                }
                catch
                {
                    return;
                }
            }
        }
    }

    IEnumerator timer(){
        yield return new WaitForSeconds(timeActive);
        Destroy(gameObject);
    }
}