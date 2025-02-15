using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script for all collectable ingredients in the environment
//Based off of the Chest.cs script
public class Collectable : Interactable
{
    [Header("Collectable")]
    [SerializeField] private Ingredient ingredient;
    private bool collected = false;
    private Vector2 playerPosition;
    private float collectionSpeed = 6f;
    Collider2D _collider;

    public void Spawn(Vector2 spawnPoint)
    {
        type = this.name;
        interactablePrompt.SetActive(false);  //Disable interactable prompt
        _collider = GetComponent<Collider2D>();
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
        PlayerInventory.Singleton.CollectIngredient(ingredient);
        SetInteractable(false);  //Cannot interact multiple times
        SetInteractablePrompt(false);  //Remove prompt
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
        _collider.enabled = false;
        playerPosition = PlayerEntityManager.Singleton.gameObject.transform.position;
        transform.position = Vector2.MoveTowards(transform.position, playerPosition, collectionSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, playerPosition) < 0.01f)
        {
            Destroy(gameObject);
        }
    }
}
