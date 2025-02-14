using UnityEngine;

[CreateAssetMenu(fileName = "CollisionLayers", menuName = "Helper/CollisionLayers")]
public class CollisionLayers : ScriptableObject
{
    public static CollisionLayers Singleton { get; private set; }
    public LayerMask collisionLayer;
    public LayerMask interactableLayer;
    public LayerMask enemyLayer;

    private void OnEnable()
    {
        Singleton = this;
    }

    public bool InEnemyLayer(GameObject source)
    {
        return (enemyLayer.value & (1 << source.layer)) != 0;
    }

    public bool InCollisionLayer(GameObject source)
    {
        return (collisionLayer.value & (1 << source.layer)) != 0;
    }

    public bool InInteractableLayer(GameObject source)
    {
        return (interactableLayer.value & (1 << source.layer)) != 0;
    }

    public int GetInteractableLayer()
    {
        return LayerMask.NameToLayer("Interactable");
    }
}

