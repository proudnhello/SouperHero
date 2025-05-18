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

    public bool playerHolding = false;
    public Throwable objectHolding = null;

    public List<Ingredient> defaultSpoonIngredients;
    public SoupBase defaultSoupBase;

    //[SerializeField]
    //internal List<Ingredient> ingredientsHeld;

    [SerializeField]
    internal List<Collectable> collectablesHeld;

    [SerializeField]
    SoupSpoon[] spoons;

    [Header("Soup Inventory")]
    private int maxSpoons = 10;
    private int maxSelectedSpoons = 4;
    public int currentSpoon = 0;
    private int selectedSlot = -1;


    void Awake()
    {
        if (Singleton == null) Singleton = this;

        spoons = new SoupSpoon[10];
        spoons[0] = new SoupSpoon(defaultSpoonIngredients, defaultSoupBase);
        for(int i = 1; i < maxSpoons - 1; i++) //Set the rest of spoons in array to null
        {
            spoons[i] = null;
        }

        collectablesHeld = new();
    }

    public SoupSpoon[] GetSpoons()
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
        if (selectedSlot < 0) return false;

        spoons[selectedSlot] = new SoupSpoon(ingredients, b);
        SoupUI.Singleton.AddSoupInSlot(selectedSlot);
        currentSpoon = 0;
        
        if(selectedSlot < 4)
        {
            currentSpoon = selectedSlot;
            AddSpoon?.Invoke(currentSpoon);
            ChangedSpoon?.Invoke(currentSpoon);
        }

        MetricsTracker.Singleton.RecordSoupsCooked();

        return true;
    }

    //Set variable for which soup slot was clicked
    public void SetSelectedSoup(int index)
    {
        selectedSlot = index;
    }

    //TODO: Fix!!
    void CycleSpoons(InputAction.CallbackContext ctx)
    {
        //if (spoons.Count <= 1) return;

        if (ctx.ReadValue<float>() < 0)
        {
            currentSpoon = FindNextAvalaibleIndex(currentSpoon, false);
        }
        else if(ctx.ReadValue<float>() > 4) //4 is the number of hotkeys
        {
            currentSpoon = FindNextAvalaibleIndex(currentSpoon, true);
        } 
        else
        {
            //TODO: Add check for count
            currentSpoon = (int)ctx.ReadValue<float>() - 1 >= maxSelectedSpoons ? currentSpoon : (int)ctx.ReadValue<float>() - 1;
        }
        Debug.Log("Next spoon at index: " + currentSpoon);
        ChangedSpoon?.Invoke(currentSpoon);
    }

    //Iterate to find next avaliable spoon and return the index
    //If increment bool is false: decriment, if true: increment

    //Fix: When at index 0, cannot scroll down!
    int FindNextAvalaibleIndex(int curr, bool increment)
    {
        if (increment) {
            for (int i = curr + 1; i < maxSelectedSpoons; i++)
            {
                if(spoons[i] != null) { return i; }
            }
        } else {
            if(curr == 0) { curr = maxSelectedSpoons - 1; }
            for (int i = curr - 1 ; i > 0; i--)
            {
                if (spoons[i] != null) { return i; }
            }
        }
        return 0;
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
            spoons[currentSpoon] = null;
            RemoveSpoon?.Invoke(currentSpoon);
            currentSpoon = FindNextAvalaibleIndex(currentSpoon, false);
            //currentSpoon = currentSpoon < 0 ? maxSelectedSpoons - 1 : currentSpoon;
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