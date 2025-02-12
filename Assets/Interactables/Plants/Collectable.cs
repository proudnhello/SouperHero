using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ingredient = FlavorIngredient;

//Script for all collectable ingredients in the environment
//Based off of the Chest.cs script
public class Collectable : Interactable
{
    [Header("Collectable")]
    [SerializeField] private Ingredient ingredient;
    private bool collected = false;
    private Vector2 playerPosition;
    private float collectionSpeed = 6f;

    private void Start()
    {
        type = this.name;
        interactablePrompt.SetActive(false);  //Disable interactable prompt
    }

    public override void Interact()
    {
        if (CanInteract())
        {
            Collect();
            collected = true;
        }
    }

    private void Collect()
    {
        PlayerManager.instance.AddToInventory(ingredient);
        SetInteractable(false);  //Cannot interact multiple times
        SetInteractablePrompt(false);  //Remove prompt

        Debug.Log("Foraged " + type);
    }

    private void FixedUpdate()
    {
        if (collected)
        {
            CollectionAnimation();
        }
    }

    private void CollectionAnimation()
    {
        this.GetComponent<Collider2D>().enabled = false;
        playerPosition = GameObject.FindGameObjectsWithTag("Player")[0].transform.position;
        this.transform.position = Vector2.MoveTowards(transform.position, playerPosition, collectionSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, playerPosition) < 0.01f)
        {
            Destroy(this.gameObject);
        }
    }
}
