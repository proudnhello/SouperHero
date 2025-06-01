using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script for all collectable ingredients in the environment
//Based off of the Chest.cs script
public class Collectable : MonoBehaviour
{
    public Ingredient ingredient;
    public CollectableObject collectableObj;
    public CollectableUI collectableUI;
    internal string promptText;

    public void Spawn(Vector2 spawnPoint)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPoint, 0.01f);
        foreach (Collider2D collider in colliders)
        {
            // If the collectable were to be spawned in a pit hazard, don't spawn it
            if (collider.gameObject.CompareTag("PitHazard"))
            {
                return;
            }
        }

        GeneratePromptText();

        collectableObj.Init(this);
        collectableUI.Init(this);
        collectableObj.Drop(spawnPoint);
    }

    public void SpawnInUI(Vector2 spawnPoint, float rotation)
    {
        GeneratePromptText();
        collectableObj.Init(this);
        collectableUI.Init(this);
        collectableObj.gameObject.SetActive(false);
        collectableUI.gameObject.SetActive(true);
        collectableUI.PickUp();
        BasketUI.Singleton.SpawnIngredient(this, spawnPoint, rotation);
    }

    void GeneratePromptText()
    {
        promptText = ingredient.name + "\n";

        if (ingredient.GetType() == typeof(AbilityIngredient))
        {
            AbilityIngredient ability = (AbilityIngredient)ingredient;
            promptText += ability.abilityType._abilityName;
        }
        else if (ingredient.GetType() == typeof(FlavorIngredient))
        {
            FlavorIngredient stat = (FlavorIngredient)ingredient;
            foreach (var flavor in stat.buffFlavors)
            {
                promptText += flavor.buffType.ToString() + "\n";
            }
            foreach (var flavor in stat.inflictionFlavors)
            {
                promptText += flavor.inflictionType.ToString() + "\n";
            }
        }
    }

    public void Drop()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(PlayerEntityManager.Singleton.transform.position, 0.01f);
        foreach (Collider2D collider in colliders)
        {
            // If the collectable were to be spawned in a pit hazard, don't spawn it
            if (collider.gameObject.CompareTag("PitHazard"))
            {
                return;
            }
        }
        collectableUI.transform.SetParent(transform);
        collectableUI.gameObject.SetActive(false);
        collectableObj.gameObject.SetActive(true);
        collectableObj.SetInteractable(true);
        collectableObj.Drop(PlayerEntityManager.Singleton.gameObject.transform.position);
        PlayerInventory.Singleton.RemoveIngredientCollectable(this, false);
    }

    public void Collect()
    {
        collectableObj.gameObject.SetActive(false);
        collectableUI.gameObject.SetActive(true);
        collectableUI.PickUp();
        PlayerInventory.Singleton.CollectIngredientCollectable(this);
    }
}