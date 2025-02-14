using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{
    [Header("Chest")]
    [SerializeField] private FlavorIngredient ingredient;

    private void Start()
    {
        // set the type of the interactable
        //type = "Chest";
        type = this.name;
        // disable the interactable prompt
        interactablePrompt.SetActive(false);
    }

    public override void Interact()
    { 
        if(CanInteract())
        {
            OpenChest();
        }
    }

    private void OpenChest(){

        if (ingredient == null)
        {
            Debug.LogError("OpenChest: ingredient is null! Make sure to check its defined in the inspector!");
            return;
        }

        // add the ingredient to the player's inventory
        PlayerInventory.Singleton.CollectIngredient(ingredient);
        // set the interactable to false so the chest can't be opened multiple times
        SetInteractable(false);
        // remove the interactable prompt
        SetInteractablePrompt(false);
        // set chest to a different color so we know it's open
        GetComponent<SpriteRenderer>().color = Color.gray;

        Debug.Log("Opened chest");
    }
}
