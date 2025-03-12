using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float timeActive = 2f;
    public float radius = 6f;
    public float radiusToCollider = .5f;
    public int damage = 10;
    List<Entity> entitiesAffected = new();

    void Start()
    {
        StartCoroutine(timer());
        transform.localScale = new Vector3(radius, radius, radius);
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, radius * radiusToCollider, CollisionLayers.Singleton.GetEntityLayer());
        foreach (var col in cols)
        {
            try
            {
                Entity entity = col.GetComponent<Entity>();
                if (!entitiesAffected.Contains(entity))
                {
                    entitiesAffected.Add(entity);
                    entity.ModifyHealth(-damage);
                }
            }
            catch
            {

            }
        }
    }

    private void Update()
    {
        
    }

    IEnumerator timer(){
        yield return new WaitForSeconds(timeActive);
        Destroy(gameObject);
    }
}