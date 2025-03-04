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

        collectableObj.Init(this);
        collectableUI.Init(this);
        collectableObj.Drop(spawnPoint);
    }

    public void Collect()
    {
        collectableObj.gameObject.SetActive(false);
        collectableUI.gameObject.SetActive(true);
        PlayerInventory.Singleton.CollectIngredient(this);
    }
}