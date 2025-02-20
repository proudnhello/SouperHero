using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Lo: This class contains the contents of the player's pot
public class BasketUI : MonoBehaviour
{

    public static BasketUI Singleton;
    [SerializeField] Transform SpawnPoint;
    [SerializeField] float offsetYSpawn = 85;
    [SerializeField] Vector2 dropForceRange = new Vector2(90, 110);

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
        //TODO: Set parent of leek to pot
        collectable.transform.SetParent(this.transform, false);
        collectable.collectableUI.transform.position = new Vector2(SpawnPoint.position.x, SpawnPoint.position.y + offsetYSpawn);
        collectable.collectableUI.rb.velocity = new Vector2(0, -Random.Range(dropForceRange.x, dropForceRange.y));
    }

    public void RemoveIngredient()
    {

    }
}
