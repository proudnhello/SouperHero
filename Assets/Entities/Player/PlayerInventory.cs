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
using System.Linq;

//TODO: Don't allow empty/null soups to be swapped
//FIX: Cannot cook in anything in slots 6-9 (index out of range)
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

        spoons = new SoupSpoon[maxSpoons];
        spoons[0] = new SoupSpoon(defaultSpoonIngredients, defaultSoupBase);
        for (int i = 1; i < maxSpoons - 1; i++) //Set the rest of spoons in array to null
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

    //Set variable for which soup slot was clicked
    public void SetSelectedSoup(int index)
    {
        selectedSlot = index;
    }

    //Get which soup slot was clicked
    public int GetSelectedSoup()
    {
        return selectedSlot;
    }

    private void Start()
    {
        PlayerKeybinds.Singleton.useSpoon.action.started += UseSpoon;
        PlayerKeybinds.Singleton.drinkSoup.action.started += DrinkSoup;
        PlayerKeybinds.Singleton.inventory.action.started += Inventory;
        PlayerKeybinds.Singleton.cycleSpoonLeft.action.started += CycleSpoonLeft;
        PlayerKeybinds.Singleton.cycleSpoonRight.action.started += CycleSpoonRight;
        PlayerKeybinds.Singleton.bowl1.action.started += Bowl1;
        PlayerKeybinds.Singleton.bowl2.action.started += Bowl2;
        PlayerKeybinds.Singleton.bowl3.action.started += Bowl3;
        PlayerKeybinds.Singleton.bowl4.action.started += Bowl4;
        // PlayerEntityManager.Singleton.input.Player.CycleSpoon.started += CycleSpoons;
    }

    private void OnDisable()
    {
        PlayerKeybinds.Singleton.useSpoon.action.started -= UseSpoon;
        PlayerKeybinds.Singleton.drinkSoup.action.started -= DrinkSoup;
        PlayerKeybinds.Singleton.inventory.action.started -= Inventory;
        PlayerKeybinds.Singleton.cycleSpoonLeft.action.started -= CycleSpoonLeft;
        PlayerKeybinds.Singleton.cycleSpoonRight.action.started -= CycleSpoonRight;
        PlayerKeybinds.Singleton.bowl1.action.started -= Bowl1;
        PlayerKeybinds.Singleton.bowl2.action.started -= Bowl2;
        PlayerKeybinds.Singleton.bowl3.action.started -= Bowl3;
        PlayerKeybinds.Singleton.bowl4.action.started -= Bowl4;
        // PlayerEntityManager.Singleton.input.Player.CycleSpoon.started -= CycleSpoons;
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
        if (selectedSlot < 0) return false; //Check to make sure valid slot is selected

        spoons[selectedSlot] = new SoupSpoon(ingredients, b);
        SoupUI.Singleton.AddSoupInSlot(selectedSlot);
        SoupUI.Singleton.SetUsesText(selectedSlot);
        //SoupUI.Singleton.SetImage(selectedSlot);
        currentSpoon = 0; //Reset to spoon in first slot
        
        if(selectedSlot < maxSelectedSpoons) //If soup is made in selected spot
        {
            currentSpoon = selectedSlot;
            AddSpoon?.Invoke(currentSpoon);
            ChangedSpoon?.Invoke(currentSpoon);
        }

        MetricsTracker.Singleton.RecordSoupsCooked();

        return true;
    }

    public void SwapSoups(int index1, int index2)
    {
        (spoons[index1], spoons[index2]) = (spoons[index2], spoons[index1]);
    }

    // Select bowl when scrolling with the scroll wheel
    void CycleCurrentBowl(int direction) 
    {
        if (direction < 0)
        {
            currentSpoon = FindNextAvalaibleIndex(currentSpoon, false);
        }
        else if (direction > 0)
        {
            currentSpoon = FindNextAvalaibleIndex(currentSpoon, true);
        }
        ChangedSpoon?.Invoke(currentSpoon);
    }

    // Select bowl when choosing a bowl with keys 1-4
    void ChooseCurrentBowl(int bowl)
    {
        if (currentSpoon == bowl) return; //If current spoon is already selected, return
        if (spoons[bowl] != null && spoons[bowl].spoonAbilities != null)
        {
            currentSpoon = bowl;
            ChangedSpoon?.Invoke(currentSpoon);
        }
    }

    //Iterate to find next avaliable spoon and return the index
    //If increment bool is false: decriment, if true: increment
    //Lo: I know this is stupid, but everything is stupid.
    public int FindNextAvalaibleIndex(int curr, bool increment)
    {
        if (increment) {
            for (int i = curr + 1; i < maxSelectedSpoons; i++)
            {
                if (spoons[i] != null && spoons[i].spoonAbilities != null) { return i; }
            }
        } else {
            if(curr == 0) { curr = maxSelectedSpoons; }
            for (int i = curr - 1 ; i > 0; i--)
            {
                if (spoons[i] != null && spoons[i].spoonAbilities != null) { return i; }
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
        }

        // Invoke the changed spoon event to indicate it has changed
        ChangedSpoon?.Invoke(currentSpoon);
    }

    void DrinkSoup(InputAction.CallbackContext ctx)
    {
        // Index into current spoon
        SoupSpoon spoon = spoons[currentSpoon];

        if (CookingManager.Singleton.IsCooking() || spoon.GetUses() < 5)
        {
            return;
        }

        spoon.DrinkSoup(gameObject.GetComponent<Entity>());

        bool noUsesLeft = true;

        if (spoon.GetUses() > 0)
        {
            noUsesLeft = false;
        }

        if (noUsesLeft)
        {
            spoons[currentSpoon] = null;
            RemoveSpoon?.Invoke(currentSpoon);
            currentSpoon = FindNextAvalaibleIndex(currentSpoon, false);
        }

        ChangedSpoon?.Invoke(currentSpoon);
    }

    public void Inventory(InputAction.CallbackContext ctx)
    {
        //CookingManager.Singleton.PlayerPressedInventory();
    }

    void CycleSpoonLeft(InputAction.CallbackContext ctx)
    {
        CycleCurrentBowl(-1);
    }

    void CycleSpoonRight(InputAction.CallbackContext ctx)
    {
        CycleCurrentBowl(1);
    }

    void Bowl1(InputAction.CallbackContext ctx)
    {
        ChooseCurrentBowl(0);
    }

    void Bowl2(InputAction.CallbackContext ctx)
    {
        ChooseCurrentBowl(1);
    }

    void Bowl3(InputAction.CallbackContext ctx)
    {
        ChooseCurrentBowl(2);
    }

    void Bowl4(InputAction.CallbackContext ctx)
    {
        ChooseCurrentBowl(3);
    }

    void Throw(Throwable item)
    {
        float theta = PlayerEntityManager.Singleton.playerAttackPoint.rotation.eulerAngles.z + 90f;
        Vector2 direction = new Vector2(Mathf.Cos(theta * Mathf.Deg2Rad), Mathf.Sin(theta * Mathf.Deg2Rad));
        Vector2 playerPos = PlayerEntityManager.Singleton.GetPlayerPosition();

        item.ThrowItem(playerPos, direction);    
    }

}