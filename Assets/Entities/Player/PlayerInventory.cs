using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Singleton { get; private set; }
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

    private void Start()
    {
        PlayerEntityManager.Singleton.input.Player.UseSpoon.started += UseSpoon;
        PlayerEntityManager.Singleton.input.Player.CycleSpoon.started += CycleSpoons;
    }

    public void CollectIngredient(Ingredient ingredient)
    {
        ingredientsHeld.Add(ingredient);
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
    }

    void UseSpoon(InputAction.CallbackContext ctx)
    {
        SoupSpoon spoon = spoons[currentSpoon];
        spoon.UseSpoon();
        if (spoon.uses == 0)
        {
            spoons.RemoveAt(currentSpoon);
            currentSpoon--;
            currentSpoon = currentSpoon < 0 ? spoons.Count - 1 : currentSpoon;
        } 
    }
}