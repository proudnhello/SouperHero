using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Infliction = SoupSpoon.SpoonInfliction;

public class Explosion : MonoBehaviour
{
    public float timeActive = 2f;
    public float radius = 6f;
    public float radiusToCollider = .5f;
    List<Entity> entitiesAffected = new();

    public void Explode(int damage = 10)
    {
        StartCoroutine(DestroyTimer());
        transform.localScale = new Vector3(radius, radius, radius);
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, radius * radiusToCollider, CollisionLayers.Singleton.GetEntityLayer());
        string s = "";
        foreach (var col in cols)
        {
            s += col.name + " ";
            try
            {
                Entity entity = col.GetComponent<Entity>();
                if (!entitiesAffected.Contains(entity))
                {
                    entitiesAffected.Add(entity);
                    entity.DealDamage(damage);
                }
            }
            catch
            {

            }
        }
    }

    public void Explode(List<Infliction> inflictions, float size = 6)
    {
        StartCoroutine(DestroyTimer());
        transform.localScale = new Vector3(size, size, size);
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, size * radiusToCollider, CollisionLayers.Singleton.GetEntityLayer());
        string s = "";
        foreach (var col in cols)
        {
            s += col.name + " ";
            try
            {
                Entity entity = col.GetComponent<Entity>();
                if (!entitiesAffected.Contains(entity))
                {
                    entitiesAffected.Add(entity);
                    entity.ApplyInfliction(inflictions, transform);
                }
            }
            catch
            {

            }
        }
    }

    IEnumerator DestroyTimer(){
        yield return new WaitForSeconds(timeActive);
        Destroy(gameObject);
    }
}