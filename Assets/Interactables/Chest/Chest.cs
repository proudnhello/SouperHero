using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{
    [Header("Chest")]
    //[SerializeField] private FlavorIngredient ingredient;
    [SerializeField] protected List<Collectable> items;

    public override void Interact()
    { 
        if(CanInteract())
        {
            Vector3 offset = new Vector3(0, -0.75f, 0);

            Collectable collectable = items[Random.Range(0, items.Count)];  // choose a random item from the list
            Instantiate(collectable.gameObject, transform.position, Quaternion.identity).GetComponent<Collectable>().Spawn(transform.position + offset); //Spawn collectable when chest is opened

            // set the interactable to false so the chest can't be opened multiple times
            SetInteractable(false);
            // remove the interactable prompt
            SetHighlighted(false);
            // set chest to a different color so we know it's open
            interactableSpriteRenderer.color = Color.gray;
        }
    }
}
