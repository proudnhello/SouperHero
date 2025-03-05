using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableDeathBox : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Called when a collision with another collider occurs
        if (collision.gameObject.GetComponent<CollectableUI>() != null)
        {
            CollectableUI ingredientUI = collision.gameObject.GetComponent<CollectableUI>();
            Collectable ingredientCollectable = ingredientUI.GetCollectable();
            PlayerInventory.Singleton.RemoveIngredientCollectable(ingredientCollectable);
        }
    }
}
