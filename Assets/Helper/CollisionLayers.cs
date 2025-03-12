using UnityEngine;

public class CollisionLayers : MonoBehaviour
{
    public static CollisionLayers Singleton { get; private set; }
    public LayerMask collisionLayer;
    public LayerMask interactableLayer;
    public LayerMask destroyableLayer;
    public LayerMask enemyLayer;
    public LayerMask environmentLayer;
    public LayerMask entityLayer;

    private void Awake()
    {
        Singleton = this;
    }

    public bool InEnemyLayer(GameObject source)
    {
        return (enemyLayer.value & (1 << source.layer)) != 0;
    }
    public bool InEntityLayer(GameObject source)
    {
        return (entityLayer.value & (1 << source.layer)) != 0;
    }
    public bool InCollisionLayer(GameObject source)
    {
        return (collisionLayer.value & (1 << source.layer)) != 0;
    }

    public bool InInteractableLayer(GameObject source)
    {
        return (interactableLayer.value & (1 << source.layer)) != 0;
    }

    public bool InDestroyableLayer(GameObject source)
    {
        return (destroyableLayer.value & (1 << source.layer)) != 0;
    }

    public bool InEnvironmentLayer(GameObject source)
    {
        return (environmentLayer.value & (1 << source.layer)) != 0;
    }

    public LayerMask GetCollisionLayer()
    {
        return collisionLayer;
    }
    public LayerMask GetEnemyLayer()
    {
        return enemyLayer;
    }

    public LayerMask GetDestroyableLayer()
    {
        return destroyableLayer;
    }

    public LayerMask GetEnvironmentLayer()
    {
        return environmentLayer;
    }

    public int GetInteractableLayer()
    {
        return LayerMask.NameToLayer("Interactable");
    }

    public int GetEntityLayer()
    {
        return entityLayer.value;
    }
}

