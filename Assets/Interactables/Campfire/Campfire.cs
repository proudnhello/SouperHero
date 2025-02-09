using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Campfire : Interactable
{
    [Header("Campfire")]
    [SerializeField] private GameObject cookingPot; 

    // Start is called before the first frame update
    void Start()
    {
        type = this.name;
        SetInteractable(true);
        SetInteractablePrompt(false);
        cookingPot.SetActive(false);
    }

    public override void Interact()
    {
        if (CanInteract())
        {
            cookingPot.SetActive(true);
            Debug.Log("Cooking at campfire");
        }
    }
}
