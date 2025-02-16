using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectableObject : Interactable
{
    [Header("Collectable")]
    [SerializeField] private Ingredient ingredient;
    private Vector2 playerPosition;
    private float collectionSpeed = 6f;
    Collider2D _collider;
    private bool collected = false;
    // Start is called before the first frame update

    public void Drop(Vector2 dropPoint)
    {
        transform.position = dropPoint;
        type = this.name;
        interactablePrompt.SetActive(false);
        _collider = GetComponent<Collider2D>();
    }

    public override void Interact()
    {
        PlayerInventory.Singleton.CollectIngredient(ingredient);
        SetInteractable(false);  //Cannot interact multiple times
        SetInteractablePrompt(false);  //Remove prompt
        _collider.enabled = false;
        StartCoroutine (CollectionAnimation());
    }

    private IEnumerator CollectionAnimation()
    {
        while (Vector2.Distance(transform.position, playerPosition) > 0.01f)
        {
            playerPosition = PlayerEntityManager.Singleton.gameObject.transform.position;
            transform.position = Vector2.MoveTowards(transform.position, playerPosition, collectionSpeed * Time.deltaTime);
            yield return null;
        }
        this.transform.parent.GetComponent<Collectable>().Collect();
    }
}
