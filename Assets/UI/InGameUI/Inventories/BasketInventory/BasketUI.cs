using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Lo: This class contains the contents of the player's pot
public class BasketUI : MonoBehaviour
{

    public static BasketUI Singleton;
    [SerializeField] Transform SpawnPoint;
    [SerializeField] float offsetYSpawn = 85;
    [SerializeField] Vector2 dropForceRangeX= new Vector2(0, 0);
    [SerializeField] Vector2 dropForceRangeY = new Vector2(2, 8);

    List<Collectable> basketCollectables = new List<Collectable>();
    private void Awake()
    {
        if(Singleton != null)
        {
            Destroy(this);
            return;
        }
        Singleton = this;
    }

    public void SpawnIngredient(Collectable collectable, Vector2 spawnPoint)
    {
        collectable.transform.SetParent(this.transform, false);
        collectable.collectableUI.transform.position = spawnPoint;
        basketCollectables.Add(collectable);
    }

    public void AddIngredient(Collectable collectable, bool needsAdd)
    {
        //TODO: Set parent of collectable to pot
        collectable.transform.SetParent(this.transform, false);
        collectable.collectableUI.transform.position = new Vector2(SpawnPoint.position.x, SpawnPoint.position.y + offsetYSpawn);
        collectable.collectableUI.rb.velocity = new Vector2(Random.Range(dropForceRangeX.x, dropForceRangeX.y), -Random.Range(dropForceRangeY.x, dropForceRangeY.y));

        // Add collectable to list
        if (needsAdd)
        {
            basketCollectables.Add(collectable);
        }
    }

    public void RemoveIngredient(Collectable ingredient, bool needsDestroy)
    {

        foreach (Collectable collectable in basketCollectables)
        {
            if (Object.Equals(collectable, ingredient))
            {
                basketCollectables.Remove(collectable);
                if (needsDestroy)
                {
                    Destroy(collectable.gameObject);
                }
                break;
            }
        }

    }
}
