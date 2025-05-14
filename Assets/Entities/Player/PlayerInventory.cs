// portions of this file were generated using GitHub Copilot
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using TMPro;
using static SoupSpoon;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Singleton { get; private set; }
    public static event Action UsedSpoon;
    public static event Action<int> ChangedSpoon;
    public static event Action<int> AddSpoon;
    public static event Action<int> RemoveSpoon;
    public int maxSpoons = 4;

    public List<Ingredient> defaultSpoonIngredients;

    [SerializeField]
    internal List<Ingredient> ingredientsHeld;

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
            new SoupSpoon(defaultSpoonIngredients)
        };
        collectablesHeld = new();
        // scroll = new InputActions();
        // scroll.Player.Enable();
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
        // PlayerEntityManager.Singleton.input.Player.UseSpoon.started += UseSpoon;
        PlayerKeybinds.Singleton.attack.action.started += UseSpoon;
        PlayerKeybinds.Singleton.cycleSpoonLeft.action.started += CycleSpoonLeft;
        PlayerKeybinds.Singleton.cycleSpoonRight.action.started += CycleSpoonRight;
        // PlayerEntityManager.Singleton.input.Player.CycleSpoon.started += CycleSpoons;
        // PlayerKeybinds.Singleton.cycleSpoon.action.started += CycleSpoons;
        PlayerKeybinds.Singleton.soup_1.action.started += Soup1;
        PlayerKeybinds.Singleton.soup_2.action.started += Soup2;
        PlayerKeybinds.Singleton.soup_3.action.started += Soup3;
        PlayerKeybinds.Singleton.soup_4.action.started += Soup4;
    }

    private void OnDisable()
    {
        // PlayerEntityManager.Singleton.input.Player.UseSpoon.started -= UseSpoon;
        PlayerKeybinds.Singleton.attack.action.started -= UseSpoon;
        PlayerKeybinds.Singleton.cycleSpoonLeft.action.started -= CycleSpoonLeft;
        PlayerKeybinds.Singleton.cycleSpoonRight.action.started -= CycleSpoonRight;
        // PlayerEntityManager.Singleton.input.Player.CycleSpoon.started -= CycleSpoons;
        // PlayerKeybinds.Singleton.cycleSpoon.action.started -= CycleSpoons;
        PlayerKeybinds.Singleton.soup_1.action.started -= Soup1;
        PlayerKeybinds.Singleton.soup_2.action.started -= Soup2;
        PlayerKeybinds.Singleton.soup_3.action.started -= Soup3;
        PlayerKeybinds.Singleton.soup_4.action.started -= Soup4;
    }

    public void CollectIngredientCollectable(Collectable collectable)
    {
        collectablesHeld.Add(collectable);
        MetricsTracker.Singleton.RecordIngredientCollected();
        BasketUI.Singleton.AddIngredient(collectable, true);
    }


    // Removes ingredient from the player inventory
    // By default it removes the first insance of an ingredient if there are multiple
    // set reverse to true to remove the last instance of the ingredient
    // (The collider under the basket calls it in reverse, the cook button calls it forward)
    public void RemoveIngredientCollectable(Collectable collectable, bool needsDestroy)
    {
        collectablesHeld.Remove(collectable);
        BasketUI.Singleton.RemoveIngredient(collectable, needsDestroy);
    }

    public bool CookSoup(List<Ingredient> ingredients)
    {
        if (spoons.Count == maxSpoons) return false;

        spoons.Add(new SoupSpoon(ingredients));
        currentSpoon = spoons.Count - 1;
        AddSpoon?.Invoke(currentSpoon);
        ChangedSpoon?.Invoke(currentSpoon);

        MetricsTracker.Singleton.RecordSoupsCooked();

        return true;
    }

    // CYCLING SPOON
    void CycleSpoon(int direction)
    {
        // int direction = cycleSpoon.action.Scroll.ReadValue<Vector2>().normalized.y;
        if (spoons.Count <= 1) return;

        if (direction == -1 && currentSpoon > 0)
        {
            currentSpoon--;
            // currentSpoon = currentSpoon < 0 ? spoons.Count - 1 : currentSpoon;
        }
        else if(direction == 1 && currentSpoon < 3) 
        {
            currentSpoon++;
            // currentSpoon = currentSpoon >= spoons.Count ? currentSpoon = 0 : currentSpoon;
        } 
        // else
        // {
        //     //TODO: Add check for count
        //     currentSpoon = (int)ctx.ReadValue<float>() - 1 >= spoons.Count ? currentSpoon = currentSpoon : (int)ctx.ReadValue<float>() - 1;
        // }
        ChangedSpoon?.Invoke(currentSpoon);
    }

    void CycleSpoonLeft(InputAction.CallbackContext ctx)
    {
        CycleSpoon(-1);
    }

    void CycleSpoonRight(InputAction.CallbackContext ctx)
    {
        CycleSpoon(1);
    }

    void Soup1(InputAction.CallbackContext ctx)
    {
        currentSpoon = 0;
        ChangedSpoon?.Invoke(currentSpoon);
    }

    void Soup2(InputAction.CallbackContext ctx)
    {
        if(spoons.Count > 1)
        {
            currentSpoon = 1;
        }
        ChangedSpoon?.Invoke(currentSpoon);
    }

    void Soup3(InputAction.CallbackContext ctx)
    {
        if(spoons.Count > 2)
        {
            currentSpoon = 2;
        }
        ChangedSpoon?.Invoke(currentSpoon);
    }

    void Soup4(InputAction.CallbackContext ctx)
    {
        if(spoons.Count > 3)
        {
            currentSpoon = 3;
        }
        ChangedSpoon?.Invoke(currentSpoon);
    }

    // THE ATTACK FUNCTION
    void UseSpoon(InputAction.CallbackContext ctx)
    {
        // Don't Use Spoon if In Cooking Screen or if the player can't attack
        if (CookingManager.Singleton.IsCooking() || !PlayerEntityManager.Singleton.CanAttack())
        {
            return;
        }

        // Index into current spoon
        SoupSpoon spoon = spoons[currentSpoon];
        
        // See if spoon is on cooldown
        bool notOnCD = spoon.UseSpoon();

        if (!notOnCD)
        {
            return;
        }

        // Invoke That You are using a spoon
        // Audio and Animation are currently subscribed
        UsedSpoon?.Invoke();

        // check if any of the abilities have uses left
        bool noUsesLeft = true;

        if (spoon.GetUses() > 0 || spoon.GetUses() == -1)
        {
            noUsesLeft = false;
        }


        // remove spoon if no uses left
        if (noUsesLeft)
        {
            spoons.RemoveAt(currentSpoon);
            RemoveSpoon?.Invoke(currentSpoon);
            currentSpoon--;
            currentSpoon = currentSpoon < 0 ? spoons.Count - 1 : currentSpoon;
        }

        // Invoke the changed spoon event to indicate it has changed
        ChangedSpoon?.Invoke(currentSpoon);
    }
}