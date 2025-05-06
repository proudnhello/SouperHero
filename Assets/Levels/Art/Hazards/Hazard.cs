using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hazard : MonoBehaviour
{
    protected List<Entity> effectedEntities = null;

    protected virtual void Start()
    {
        effectedEntities = new List<Entity>();
    }

    public virtual void AddEntity(Entity entity)
    {
        effectedEntities.Add(entity);
    }
    public virtual void RemoveEntity(Entity entity)
    {
        if (entity != null && effectedEntities != null && effectedEntities.Contains(entity))
        {
            effectedEntities.Remove(entity);
        }
    }
}
