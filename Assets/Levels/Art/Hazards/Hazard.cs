using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Hazard : MonoBehaviour
{
    protected List<Entity> effectedEntities;

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
        effectedEntities.Remove(entity);
    }
}
