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

    public void SpawnIngredient(Collectable collectable, Vector2 spawnPoint, float rotation)
    {
        collectable.collectableUI.transform.SetParent(this.transform, false);
        collectable.collectableUI.transform.localPosition = spawnPoint;
        collectable.collectableUI.transform.localRotation = Quaternion.Euler(0, 0, rotation);
        basketCollectables.Add(collectable);
    }

    public void AddIngredient(Collectable collectable, bool needsAdd)
    {
        //TODO: Set parent of collectable to pot
        collectable.collectableUI.transform.SetParent(this.transform, false);
        collectable.collectableUI.transform.position = new Vector2(SpawnPoint.position.x, SpawnPoint.position.y + offsetYSpawn);
        collectable.collectableUI.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
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
                    collectable.collectableUI.transform.SetParent(collectable.transform);
                    Destroy(collectable.gameObject);
                }
                break;
            }
        }

    }
}
