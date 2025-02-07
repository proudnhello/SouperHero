using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ingredient = PlayerSoup.Ingredient;

public class Chest : Interactable
{
    [Header("Chest")]
    [SerializeField] private Ingredient ingredient;

    private void Start()
    {
        // set the type of the interactable
        type = "Chest";
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
        // add the ingredient to the player's inventory
        PlayerManager.instance.AddToInventory(ingredient);
        // set the interactable to false so the chest can't be opened multiple times
        SetInteractable(false);
        // remove the interactable prompt
        SetInteractablePrompt(false);
        // set chest to a different color so we know it's open
        GetComponent<SpriteRenderer>().color = Color.gray;
    }
}
