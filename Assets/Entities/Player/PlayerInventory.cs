using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Singleton { get; private set; }
    public static event Action UsedSpoon;
    public static event Action<int> ChangedSpoon;
    public int maxSpoons = 4;

    public List<Ingredient> defaultSpoonIngredients;

    //[SerializeField]
    //internal List<Ingredient> ingredientsHeld;

    [SerializeField]
    internal List<Collectable> collectablesHeld;

    [SerializeField]
    List<SoupSpoon> spoons;

    int currentSpoon = 0;

    void Awake()
    {
        if (Singleton == null) Singleton = this;
        spoons = new()
        {
            new SoupSpoon(defaultSpoonIngredients, true)
        };
        collectablesHeld = new();
    }

    public List<SoupSpoon> GetSpoons()
    {
        return spoons;
    }

    public int GetCurrentSpoon()
    {
        return currentSpoon;
    }

    private void Start()
    {
        PlayerEntityManager.Singleton.input.Player.UseSpoon.started += UseSpoon;
        PlayerEntityManager.Singleton.input.Player.CycleSpoon.started += CycleSpoons;
    }

    private void OnDisable()
    {
        PlayerEntityManager.Singleton.input.Player.UseSpoon.started -= UseSpoon;
        PlayerEntityManager.Singleton.input.Player.CycleSpoon.started -= CycleSpoons;
    }

    public void CollectIngredientCollectable(Collectable collectable)
    {
        Debug.Log($"Collected Ingredient {collectable.ingredient}");
        collectablesHeld.Add(collectable);
        BasketUI.Singleton.AddIngredient(collectable, true);
    }


    // Removes ingredient from the player inventory
    // By default it removes the first insance of an ingredient if there are multiple
    // set reverse to true to remove the last instance of the ingredient
    // (The collider under the basket calls it in reverse, the cook button calls it forward)
    public void RemoveIngredientCollectable(Collectable collectable)
    {
        Debug.Log("Remove Ingredient Called");
        collectablesHeld.Remove(collectable);
        BasketUI.Singleton.RemoveIngredient(collectable);
    }

    public bool CookSoup(List<Ingredient> ingredients)
    {
        if (spoons.Count == maxSpoons) return false;

        spoons.Add(new SoupSpoon(ingredients));
        currentSpoon = spoons.Count - 1;
        ChangedSpoon?.Invoke(currentSpoon);

        return true;
    }

    void CycleSpoons(InputAction.CallbackContext ctx)
    {
        if (spoons.Count <= 1) return;

        if (ctx.ReadValue<float>() > 0)
        {
            currentSpoon++;
            currentSpoon = currentSpoon >= spoons.Count ? currentSpoon = 0 : currentSpoon;
        }
        else
        {
            currentSpoon--;
            currentSpoon = currentSpoon < 0 ? spoons.Count - 1 : currentSpoon;
        }
        ChangedSpoon?.Invoke(currentSpoon);
    }

    void UseSpoon(InputAction.CallbackContext ctx)
    {

        if (CookingManager.Singleton.IsCooking())
        {
            return;
        }

        SoupSpoon spoon = spoons[currentSpoon];
        bool notOnCD = spoon.UseSpoon();

        if (!notOnCD)
        {
            return;
        }

        UsedSpoon?.Invoke();

        if (spoon.uses == 0)
        {
            spoons.RemoveAt(currentSpoon);
            currentSpoon--;
            currentSpoon = currentSpoon < 0 ? spoons.Count - 1 : currentSpoon;
        } 
    }

    public void UpdateSpooon()
    {
        ChangedSpoon?.Invoke(currentSpoon);
    }
}