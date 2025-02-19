using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public void Start()
    {
        string text = ingredient.name + "\n";
        if(interactablePromptText != null)
        {
            if (ingredient.GetType() == typeof(AbilityIngredient))
            {
                AbilityIngredient ability = (AbilityIngredient)ingredient;
                text += ability.ability._abilityName;
            }else if (ingredient.GetType() == typeof(FlavorIngredient))
            {
                FlavorIngredient stat = (FlavorIngredient)ingredient;
                foreach (var flavor in stat.buffFlavors)
                {
                    text += flavor.buffType.ToString() + "\n";
                }
                foreach (var flavor in stat.inflictionFlavors)
                {
                    text += flavor.inflictionType.ToString() + "\n";
                }
            }
            interactablePromptText.text = text;
        }
        _collider = GetComponent<Collider2D>();
        type = this.name;
        interactablePrompt.SetActive(false);
    }

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
