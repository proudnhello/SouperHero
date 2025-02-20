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

    [SerializeField] List<Ingredient> defaultSpoonIngredients;
    internal List<Ingredient> ingredientsHeld;
    List<SoupSpoon> spoons;

    int currentSpoon = 0;

    void Awake()
    {
        if (Singleton == null) Singleton = this;
        spoons = new()
        {
            new SoupSpoon(defaultSpoonIngredients, true)
        };
        ingredientsHeld = new();
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

    public void CollectIngredient(Collectable collectable)
    {
        Debug.Log($"Collected Ingredient {collectable.ingredient}");
        ingredientsHeld.Add(collectable.ingredient);
        BasketUI.Singleton.AddIngredient(collectable.collectableUI.gameObject);
    }

    public void RemoveIngredient(Ingredient ingredient)
    {
        ingredientsHeld.Remove(ingredient);
    }

    public bool CookSoup(List<Ingredient> ingredients)
    {
        if (spoons.Count == maxSpoons) return false;

        spoons.Add(new SoupSpoon(ingredients));
        currentSpoon = spoons.Count - 1;
        ChangedSpoon?.Invoke(currentSpoon);
        foreach (var ingredient in ingredients)
        {
            RemoveIngredient(ingredient);
        }

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