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
    [SerializeField] Vector2 dropForceRange = new Vector2(90, 110);

    [SerializeField] List<Collectable> basketCollectables = new List<Collectable>();

    [SerializeField] GameObject deathBox;

    private void Awake()
    {
        if(Singleton != null)
        {
            Destroy(this);
            return;
        }
        Singleton = this;
    }

    public void AddIngredient(Collectable collectable)
    {
        //TODO: Set parent of collectable to pot
        collectable.transform.SetParent(this.transform, false);
        collectable.collectableUI.transform.position = new Vector2(SpawnPoint.position.x, SpawnPoint.position.y + offsetYSpawn);
        collectable.collectableUI.rb.velocity = new Vector2(0, -Random.Range(dropForceRange.x, dropForceRange.y));

        // Add collectable to list
        basketCollectables.Add(collectable);
    }

    public void RemoveIngredient(Ingredient ingredient, bool reverse = false)
    {

        if (!reverse)
        {
            foreach (Collectable collectable in basketCollectables)
            {
                if (Object.Equals(collectable.ingredient, ingredient))
                {
                    basketCollectables.Remove(collectable);
                    Destroy(collectable.gameObject);
                    break;
                }
            }
        }
        else
        {
            foreach (Collectable collectable in basketCollectables.AsEnumerable().Reverse())
            {
                if (Object.Equals(collectable.ingredient, ingredient))
                {
                    basketCollectables.Remove(collectable);
                    Destroy(collectable.gameObject);
                    break;
                }
            }
        }

    }
}
