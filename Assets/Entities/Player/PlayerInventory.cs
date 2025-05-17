// portions of this file were generated using GitHub Copilot
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using TMPro;
using static SoupSpoon;
using System.Linq.Expressions;
using static UnityEditor.Progress;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Singleton { get; private set; }
    public static event Action UsedSpoon;
    public static event Action<int> ChangedSpoon;
    public static event Action<int> AddSpoon;
    public static event Action<int> RemoveSpoon;
    public int maxSpoons = 4;

    public bool playerHolding = false;
    public Throwable objectHolding = null;

    public List<Ingredient> defaultSpoonIngredients;
    public SoupBase defaultSoupBase;

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
            new SoupSpoon(defaultSpoonIngredients, defaultSoupBase)
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

    // This is used to cook soup w/o a base. It's here while the soup UI is being worked on and the base hook is missing
    public bool OLD_AND_BAD__AND_DUMB_STUPID_COOK_SOUP_TO_BE_REMOVED(List<Ingredient> ingredients)
    {
        return CookSoup(ingredients, defaultSoupBase);
    }

    public bool CookSoup(List<Ingredient> ingredients, SoupBase b)
    {
        if (spoons.Count == maxSpoons) return false;

        spoons.Add(new SoupSpoon(ingredients, b));
        currentSpoon = spoons.Count - 1;
        AddSpoon?.Invoke(currentSpoon);
        ChangedSpoon?.Invoke(currentSpoon);

        MetricsTracker.Singleton.RecordSoupsCooked();

        return true;
    }

    void CycleSpoons(InputAction.CallbackContext ctx)
    {
        if (spoons.Count <= 1) return;

        if (ctx.ReadValue<float>() < 0)
        {
            currentSpoon--;
            currentSpoon = currentSpoon < 0 ? spoons.Count - 1 : currentSpoon;
        }
        else if(ctx.ReadValue<float>() > 4) //4 is the number of hotkeys
        {
            currentSpoon++;
            currentSpoon = currentSpoon >= spoons.Count ? currentSpoon = 0 : currentSpoon;
        } 
        else
        {
            //TODO: Add check for count
            currentSpoon = (int)ctx.ReadValue<float>() - 1 >= spoons.Count ? currentSpoon : (int)ctx.ReadValue<float>() - 1;
        }
        ChangedSpoon?.Invoke(currentSpoon);
    }

    void UseSpoon(InputAction.CallbackContext ctx)
    {
        // Don't Use Spoon if In Cooking Screen or if the player can't attack
        if (CookingManager.Singleton.IsCooking() || !PlayerEntityManager.Singleton.CanAttack())
        {
            return;
        }
        //handle thrwoing object
        else if (playerHolding)
        {
            playerHolding = false;
            Throw(objectHolding);
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



    void Throw(Throwable item)
    {
        float theta = PlayerEntityManager.Singleton.playerAttackPoint.rotation.eulerAngles.z + 90f;
        Vector2 direction = new Vector2(Mathf.Cos(theta * Mathf.Deg2Rad), Mathf.Sin(theta * Mathf.Deg2Rad));
        Vector2 playerPos = PlayerEntityManager.Singleton.GetPlayerPosition();

        item.ThrowItem(playerPos, direction);    
    }

}