using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{
    [Header("Chest")]
    //[SerializeField] private FlavorIngredient ingredient;
    public Collectable item;
    [SerializeField] Vector3 spawnOffset;

    bool isOpen;
    public override void Interact()
    { 
        if(CanInteract() && !isOpen)
        {
            isOpen = true;

            Instantiate(item.gameObject, transform.position, Quaternion.identity).GetComponent<Collectable>().Spawn(transform.position + spawnOffset); //Spawn collectable when chest is opened

            // set the interactable to false so the chest can't be opened multiple times
            SetInteractable(false);
            // remove the interactable prompt
            SetHighlighted(false);
            // set chest to a different color so we know it's open
            interactableSpriteRenderer.color = Color.gray;
        }
    }
}
