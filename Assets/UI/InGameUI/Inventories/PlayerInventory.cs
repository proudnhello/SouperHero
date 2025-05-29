// portions of this file were generated using GitHub Copilot
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using TMPro;

//TODO: Don't allow empty/null soups to be swapped
//FIX: Cannot cook in anything in slots 6-9 (index out of range)
public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Singleton { get; private set; }

    public class InventoryData
    {
        public  List<Collectable> collectablesHeld;
        public ISoupBowl[] soupsHeld;
        public int soupsHeldCount;

        public InventoryData(PlayerInventory newInventory = null)
        {
            if (newInventory != null)
            {
                collectablesHeld = new();
                soupsHeld = new ISoupBowl[newInventory.maxSoups];
                soupsHeld[0] = new FinishedSoup(newInventory.defaultSoupIngredients, newInventory.defaultSoupBase);
                soupsHeldCount = 1;
            }
        }
    }

    public static event Action UsedSoupAttack;
    public static event Action ChangedEquippedSoup;

    internal bool playerHolding = false;
    internal Throwable objectHolding = null;

    public List<Ingredient> defaultSoupIngredients;
    public SoupBase defaultSoupBase;

    public SoupBase emptyBowlBase;
    public List<Ingredient> emptyBowlIngredients;
    FinishedSoup emptyBowlAttack;


    [Header("Soup Inventory")]
    private int maxSoups = 10;
    internal int maxEquippedSoups = 4;
    internal InventoryData data;

    internal int selectedEquippedSoup = 0;


    void Awake()
    {
        if (Singleton == null) Singleton = this;
        emptyBowlAttack = new FinishedSoup(emptyBowlIngredients, emptyBowlBase);
    }

    void Start()
    {
        data = SaveManager.Singleton.LoadInventoryData(this);
        SoupInventoryUI.Singleton.InitializeSlots(data.soupsHeld);
        ChangedEquippedSoup?.Invoke();
        PlayerKeybinds.Singleton.useSpoon.action.started += UseSoupAttack;
        PlayerKeybinds.Singleton.drinkSoup.action.started += DrinkSoup;
        PlayerKeybinds.Singleton.inventory.action.started += Inventory;
        PlayerKeybinds.Singleton.cycleSpoonLeft.action.started += CycleSpoonLeft;
        PlayerKeybinds.Singleton.cycleSpoonRight.action.started += CycleSpoonRight;
        PlayerKeybinds.Singleton.bowl1.action.started += Bowl1;
        PlayerKeybinds.Singleton.bowl2.action.started += Bowl2;
        PlayerKeybinds.Singleton.bowl3.action.started += Bowl3;
        PlayerKeybinds.Singleton.bowl4.action.started += Bowl4;
    }
    private void OnDisable()
    {
        PlayerKeybinds.Singleton.useSpoon.action.started -= UseSoupAttack;
        PlayerKeybinds.Singleton.drinkSoup.action.started -= DrinkSoup;
        PlayerKeybinds.Singleton.inventory.action.started -= Inventory;
        PlayerKeybinds.Singleton.cycleSpoonLeft.action.started -= CycleSpoonLeft;
        PlayerKeybinds.Singleton.cycleSpoonRight.action.started -= CycleSpoonRight;
        PlayerKeybinds.Singleton.bowl1.action.started -= Bowl1;
        PlayerKeybinds.Singleton.bowl2.action.started -= Bowl2;
        PlayerKeybinds.Singleton.bowl3.action.started -= Bowl3;
        PlayerKeybinds.Singleton.bowl4.action.started -= Bowl4;
    }

    public void AddBowlToInventory(ISoupBowl bowl)
    {
        int slot = FindNextAvailableSlot();
        data.soupsHeld[slot] = bowl;
        data.soupsHeldCount++;
        SoupInventoryUI.Singleton.AddSoupInSlot(bowl, slot);
    }

    void BowlIsEmptied(int slot)
    {
        data.soupsHeld[slot] = ((FinishedSoup)data.soupsHeld[slot]).soupBase;
        SoupInventoryUI.Singleton.AddSoupInSlot(data.soupsHeld[slot], slot);
    }

    public void BowlIsCooked(int slot, FinishedSoup soup)
    {
        data.soupsHeld[slot] = soup;
        SoupInventoryUI.Singleton.AddSoupInSlot(data.soupsHeld[slot], slot);
    }

    public void SwapTwoSlots(int slot1, int slot2)
    {
        (data.soupsHeld[slot1], data.soupsHeld[slot2]) = (data.soupsHeld[slot2], data.soupsHeld[slot1]);
    }

    public ISoupBowl GetBowl(int slot) => data.soupsHeld[slot];
    public ISoupBowl GetCurrentBowl() => data.soupsHeld[selectedEquippedSoup];

    public void CollectIngredientCollectable(Collectable collectable)
    {
        data.collectablesHeld.Add(collectable);
        MetricsTracker.Singleton.RecordIngredientCollected();
        BasketUI.Singleton.AddIngredient(collectable, true);
    }

    // Removes ingredient from the player inventory
    // By default it removes the first insance of an ingredient if there are multiple
    // set reverse to true to remove the last instance of the ingredient
    // (The collider under the basket calls it in reverse, the cook button calls it forward)
    public void RemoveIngredientCollectable(Collectable collectable, bool needsDestroy)
    {
        data.collectablesHeld.Remove(collectable);
        BasketUI.Singleton.RemoveIngredient(collectable, needsDestroy);
    }

    // Select bowl when scrolling with the scroll wheel
    void CycleCurrentBowl(int direction) 
    {
        if (direction > 0)
        {
            selectedEquippedSoup = (selectedEquippedSoup + 1) % maxEquippedSoups;
        }
        else if (direction < 0)
        {
            selectedEquippedSoup = (selectedEquippedSoup - 1) < 0 ? maxEquippedSoups - 1 : selectedEquippedSoup - 1;
        }
        ChangedEquippedSoup?.Invoke();
    }

    // Select bowl when choosing a bowl with keys 1-4
    void ChooseCurrentBowl(int bowl)
    {
        if (selectedEquippedSoup == bowl) return; //If current spoon is already selected, return
        selectedEquippedSoup = bowl;
        ChangedEquippedSoup?.Invoke();
    }

    //Iterate to find next avaliable spoon and return the index
    //If increment bool is false: decriment, if true: increment
    //Lo: I know this is stupid, but everything is stupid.
    int FindNextAvailableSlot()
    {
        for (int i = 0; i < maxSoups; i++)
        {
            if (data.soupsHeld[i] is not SoupBase && data.soupsHeld[i] is not FinishedSoup) return i;
        }
        return -1;
    }

    void FindNextUsableSoup()
    {
        for(int i = 0; i < maxEquippedSoups; i++)
        {
            if (data.soupsHeld[i] is FinishedSoup)
            {
                selectedEquippedSoup = i;
                ChangedEquippedSoup?.Invoke();
            }
        }
    }
    

    void UseSoupAttack(InputAction.CallbackContext ctx)
    {
        // Don't Use Spoon if In Cooking Screen or if the player can't attack
        if (CookingScreen.Singleton.IsCooking || !PlayerEntityManager.Singleton.CanAttack())
        {
            return;
        }
        //handle throwing object
        else if (playerHolding)
        {
            playerHolding = false;
            Throw(objectHolding);
            return;
        }

        // Index into current spoon
        ISoupBowl bowl = data.soupsHeld[selectedEquippedSoup];

        if (bowl is FinishedSoup finishedSoup)
        {
            if (!finishedSoup.UseSoupAttack()) return;
            // remove spoon if no uses left
            if (finishedSoup.uses == 0)
            {
                BowlIsEmptied(selectedEquippedSoup);
                FindNextUsableSoup();
            }
        }
        else
        {
            if (!emptyBowlAttack.UseSoupAttack()) return;
        }

        UsedSoupAttack?.Invoke();
    }

    void DrinkSoup(InputAction.CallbackContext ctx)
    {
        if (CookingScreen.Singleton.IsCooking) return;

        // Index into current spoon
        ISoupBowl bowl = data.soupsHeld[selectedEquippedSoup];

        if (bowl is FinishedSoup finishedSoup)
        {
            if (finishedSoup.GetUses() < 5) return;
            finishedSoup.DrinkSoup();
            if (finishedSoup.uses <= 0)
            {
                BowlIsEmptied(selectedEquippedSoup);
                FindNextUsableSoup();
            }
        }

        UsedSoupAttack?.Invoke();
    }

    public void Inventory(InputAction.CallbackContext ctx)
    {
        SoupInventoryUI.Singleton.ToggleInventory();
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