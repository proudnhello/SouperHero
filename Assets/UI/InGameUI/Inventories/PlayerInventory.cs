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

    [Serializable]
    public class InventoryData
    {
        public List<int> collectablesHeld;
        public List<Vector2> collectableLocations;
        public List<float> collectableRotations;
        public SerializedSoup[] soupsHeld;

        public InventoryData(PlayerInventory newInventory = null)
        {
            if (newInventory != null)
            {
                collectablesHeld = new();
                collectableLocations = new();
                collectableRotations = new();
                soupsHeld = new SerializedSoup[newInventory.maxSoups];
                for (int i = 0; i < soupsHeld.Length; i++) soupsHeld[i] = new(0);
            }
        }
    }

    [Serializable]
    public class SerializedSoup
    {
        public int[] ingredients; // null if just soup base
        public int soupbase;
        public int uses;

        public SerializedSoup() { }
        public SerializedSoup(int empty)
        {
            this.ingredients = null;
            this.soupbase = -1;
            this.uses = -1;
        }

        public SerializedSoup(SoupBase soupBase)
        {
            soupbase = soupBase.uuid;
        }

        public SerializedSoup(FinishedSoup finishedSoup)
        {
            this.uses = finishedSoup.uses;
            this.soupbase = finishedSoup.soupBase.uuid;
            this.ingredients = new int[finishedSoup.ingredientList.Count];
            int count = 0;
            foreach (var ing in finishedSoup.ingredientList) this.ingredients[count] = ing.uuid;
        }

        public ISoupBowl ConvertToSoup(PlayerInventory _inventory)
        {
            Debug.Log("SOUP BASE: " + soupbase);
            if (soupbase == -1) return null;
            else if (ingredients == null) return _inventory.RetrieveSoupBaseByUUID[soupbase];

            SoupBase soup = _inventory.RetrieveSoupBaseByUUID[soupbase];

            List<Ingredient> ing = new();
            foreach (var inting in ingredients)
            {
                if (_inventory.RetrieveCollectableByUUID.TryGetValue(inting, out Collectable value))
                {
                    ing.Add(value.ingredient);
                }
                else
                {
                    bool hasFound = false;
                    foreach (var i in _inventory.defaultSoupIngredients)
                    {
                        if (i.uuid == inting)
                        {
                            hasFound = true;
                            ing.Add(i);
                            break;
                        }
                    }
                    if (hasFound) break;
                    foreach (var i in _inventory.emptyBowlIngredients)
                    {
                        if (i.uuid == inting)
                        {
                            ing.Add(i);
                            break;
                        }
                    }
                }
            }
            FinishedSoup finishedSoup = new(ing, soup);
            finishedSoup.uses = this.uses;
            return finishedSoup;
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

    public SoupBase[] otherStartingBases;



    [Header("Soup Inventory")]
    private int maxSoups = 10;
    internal int maxEquippedSoups = 4;
    internal InventoryData data;

    internal int selectedEquippedSoup = 0;

    Dictionary<int, Collectable> RetrieveCollectableByUUID = new();
    static readonly string FlavorCollectablePath = "Ingredients/Flavors/Collectables/";
    static readonly string AbilityCollectablePath = "Ingredients/Abilities/Collectables/";
    Dictionary<int, SoupBase> RetrieveSoupBaseByUUID = new();
    static readonly string SoupBasePath = "Bowls/";

    // local inventory
    internal List<Collectable> collectablesHeld;
    internal ISoupBowl[] soupsHeld;

    void Awake()
    {
        if (Singleton == null) Singleton = this;
        emptyBowlAttack = new FinishedSoup(emptyBowlIngredients, emptyBowlBase);

        Collectable[] flavors = Resources.LoadAll<Collectable>(FlavorCollectablePath);
        if (flavors != null) foreach (var collectable in flavors) RetrieveCollectableByUUID.Add(collectable.ingredient.uuid, collectable);
        Collectable[] abilities = Resources.LoadAll<Collectable>(AbilityCollectablePath);
        if (abilities != null) foreach (var collectable in abilities) RetrieveCollectableByUUID.Add(collectable.ingredient.uuid, collectable);

        // i think i have to make anothehr dictionary just for ingredients since theres no collectable of the default soup ingredient

        SoupBase[] bases = Resources.LoadAll<SoupBase>(SoupBasePath);
        if (bases != null) foreach (var soup in bases) RetrieveSoupBaseByUUID.Add(soup.uuid, soup);
    }

    void Start()
    {
        collectablesHeld = new();
        soupsHeld = new ISoupBowl[maxSoups];

        data = SaveManager.Singleton.LoadInventoryData(this);
        if (data == null) // empty inventory, give default soup attack and bases
        {
            data = new(this);
            soupsHeld[0] = new FinishedSoup(defaultSoupIngredients, defaultSoupBase);
            int count = 1;
            foreach (var soup in otherStartingBases)
            {
                soupsHeld[count] = soup;
                count++;
            }
        } 
        else // populate inventories with existing soups and ingredients
        {
            for (int i = 0; i < data.soupsHeld.Length; i++)
            {
                Debug.Log(data.soupsHeld[i].ingredients);
            }
            for (int i = 0; i < data.soupsHeld.Length; i++)
            {
                soupsHeld[i] = data.soupsHeld[i].ConvertToSoup(this);
            }
            for (int i = 0; i < data.collectablesHeld.Count; i++)
            {
                collectablesHeld.Add(Instantiate(RetrieveCollectableByUUID[data.collectablesHeld[i]].gameObject).GetComponent<Collectable>());
                collectablesHeld[i].SpawnInUI(data.collectableLocations[i], data.collectableRotations[i]);
            }
        }



        SoupInventoryUI.Singleton.InitializeSlots(soupsHeld);
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
        soupsHeld[slot] = bowl;
        SoupInventoryUI.Singleton.AddSoupInSlot(bowl, slot);
    }

    void BowlIsEmptied(int slot)
    {
        soupsHeld[slot] = ((FinishedSoup)soupsHeld[slot]).soupBase;
        SoupInventoryUI.Singleton.AddSoupInSlot(soupsHeld[slot], slot);
    }

    public void BowlIsCooked(int slot, FinishedSoup finishedSoup)
    {
        soupsHeld[slot] = finishedSoup;
        SoupInventoryUI.Singleton.AddSoupInSlot(soupsHeld[slot], slot);      
    }

    public void SwapTwoSlots(int slot1, int slot2)
    {
        (soupsHeld[slot1], soupsHeld[slot2]) = (soupsHeld[slot2], soupsHeld[slot1]);
    }

    public ISoupBowl GetBowl(int slot) => soupsHeld[slot];
    public ISoupBowl GetCurrentBowl() => soupsHeld[selectedEquippedSoup];

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

    // Select bowl when scrolling with the scroll wheel
    void CycleCurrentBowl(int direction) 
    {
        if (SoupInventoryUI.Singleton.IsOpen) return;

        void Shift()
        {
            if (direction > 0)
            {
                selectedEquippedSoup = (selectedEquippedSoup + 1) % maxEquippedSoups;
            }
            else if (direction < 0)
            {
                selectedEquippedSoup = (selectedEquippedSoup - 1) < 0 ? maxEquippedSoups - 1 : selectedEquippedSoup - 1;
            }
        }

        int currSoup = selectedEquippedSoup;
        for (int i = 0; i < maxEquippedSoups; i++)
        {
            Shift();
            if (soupsHeld[selectedEquippedSoup] is FinishedSoup) break;
        }
        // if looped through and could not find a finished soup... just shift one over
        if (soupsHeld[selectedEquippedSoup] is not FinishedSoup)
        {
            Shift();
        }
        if (currSoup == selectedEquippedSoup) return;

        ChangedEquippedSoup?.Invoke();
    }

    // Select bowl when choosing a bowl with keys 1-4
    void ChooseCurrentBowl(int bowl)
    {
        if (SoupInventoryUI.Singleton.IsOpen) return;
        if (selectedEquippedSoup == bowl) return; //If current spoon is already selected, return
        selectedEquippedSoup = bowl;
        ChangedEquippedSoup?.Invoke();
    }

    int FindNextAvailableSlot()
    {
        for (int i = 0; i < maxSoups; i++)
        {
            if (soupsHeld[i] is not SoupBase && soupsHeld[i] is not FinishedSoup) return i;
        }
        return -1;
    }

    void FindNextUsableSoup()
    {
        for(int i = 0; i < maxEquippedSoups; i++)
        {
            if (soupsHeld[i] is FinishedSoup)
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
        ISoupBowl bowl = soupsHeld[selectedEquippedSoup];

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
        ISoupBowl bowl = soupsHeld[selectedEquippedSoup];

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
        CycleCurrentBowl(1);
    }

    void CycleSpoonRight(InputAction.CallbackContext ctx)
    {
        CycleCurrentBowl(-1);
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

    public void SaveInventory()
    {
        for (int i = 0; i < soupsHeld.Length; i++)
        {
            if (soupsHeld[i] is FinishedSoup finishedSoup) data.soupsHeld[i] = new(finishedSoup);
            else if (soupsHeld[i] is SoupBase soupBase) data.soupsHeld[i] = new(soupBase);
            else data.soupsHeld[i] = new(0);

        }

        data.collectableLocations.Clear();
        data.collectablesHeld.Clear();
        data.collectableRotations.Clear();
        for (int i = 0; i < collectablesHeld.Count; i++)
        {
            data.collectablesHeld.Add(collectablesHeld[i].ingredient.uuid);
            data.collectableLocations.Add(collectablesHeld[i].collectableUI.transform.localPosition);
            data.collectableRotations.Add(collectablesHeld[i].collectableUI.transform.localRotation.eulerAngles.z);
        }
        SaveManager.Singleton.SaveInventory(data);
    }

}