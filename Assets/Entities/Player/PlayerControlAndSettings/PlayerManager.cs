using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;
using Spoon = PlayerSoup.Spoon;
using static UnityEditor.Progress;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class PlayerManager : Entity
{
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        health = maxHealth;
        for (int i = 0; i < numberOfSpoons + 1; i++)
        {
            Spoon spoon = new Spoon();
            spoons.Add(spoon);
        }
        soup = player.GetComponent<PlayerSoup>();
        soup.lookup = lookup;
        InitializeStats();
    }

    public static PlayerManager instance;
    public GameObject player;
    public PlayerSoup soup;

    [Header("Keybinds")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode altAttackKey = KeyCode.V;
    public KeyCode soupKey = KeyCode.Mouse1;
    public KeyCode altSoupKey = KeyCode.F;
    public KeyCode drinkey = KeyCode.Space;
    public KeyCode interactionKey = KeyCode.E;
    [Header("Attack")]
    [SerializeField] private LayerMask enemies;

    private bool dead = false;

    public bool IsDead()
    {
        return instance.dead;
    }

    public bool IsAlive()
    {
        return !instance.dead;
    }

    [SerializeField] private float attackSpeed = 3;
    public float GetAttackSpeed()
    {
        return attackSpeed;
    }

    public void SetAttackSpeed(float newAttackSpeed)
    {
        attackSpeed = newAttackSpeed;
    }

    [SerializeField] private float attackDelay = 0;
    public float getAttackDelay()
    {
        return attackDelay;
    }

    public void setAttackDelay(float newAttackDelay)
    {
        attackDelay = newAttackDelay;
    }

    [SerializeField] private float attackRadius = 1.0f;
    public float GetAttackRadius()
    {
        return instance.attackRadius;
    }
    public float SetAttackRadius(float newRadius)
    {
        instance.attackRadius = newRadius;
        return instance.attackRadius;
    }

    public float GetSpeed()
    {
        return instance.moveSpeed;
    }

    public void SetSpeed(float newSpeed)
    {
        instance.moveSpeed = newSpeed;
    }

    [Header("Abilities")]
    // Add abilities for testing here
    [SerializeField] private List<AbilityAbstractClass> DEBUG_ONLY_ABILITIES;

    [Header("Soup")]
    [SerializeField] private AbilityLookup lookup;
    //[SerializeField] private int maxPotSize = 5;
    [SerializeField] public int numberOfSpoons = 4;
    private int currentSpoon = 0;

    [Header("Inventory")]
    public List<FlavorIngredient> flavorInventory = new List<FlavorIngredient>();
    public List<AbilityIngredient> abilityInventory = new List<AbilityIngredient>();
    public int GetNumberOfPots()
    {
        return numberOfSpoons;
    }

    List<Spoon> spoons = new List<Spoon>();

    public void PrintIngredient(FlavorIngredient i)
    {
        if (i == null)
        {
            Debug.LogError("PrintIngredient: FlavorIngredient is null!");
            return;
        }

        Debug.Log($"Ingredient: {i}");
        Debug.Log($"Ingredient Name: {i.ingredientName}");
        Debug.Log($"Ingredient Flavors: {i.flavors}");

        print(i.ingredientName + ", with flavors: " + String.Join(" ", i.flavors.ToArray()));
    }

    // Convert a list of ingredients into a pot of soup, controlled by the potNumber
    public void FillPot(List<FlavorIngredient> flavors, List<AbilityIngredient> abilities, int spoonNumber)
    {
        soup.FillSpoon(flavors, abilities, spoons[spoonNumber]);
    }

    public static event Action DrinkPot;

    // Drink the soup in the pot and activate the abilities that correspond to the soup.
    // Switch this later to change spoons
    public void Drink(int spoonNumber)
    {
        // For testing, take the entire list of both types of ingredients and fill the pot with them
        // Later on, this will be removed, and we'll do it all thru the UI
        print("You used " + spoonNumber + " :)");
        currentSpoon = spoonNumber;
        FillPot(flavorInventory, abilityInventory, spoonNumber);
        flavorInventory.Clear();
        abilityInventory.Clear();
    }

    // This will fetch the abilities from the spoon and return them to the player
    // It will also decrement the number of uses of the spoon. It is expected that this will be called every time the player attacks
    public List<AbilityAbstractClass> UseSpoon()
    {
        List<AbilityAbstractClass> abilities = spoons[currentSpoon].abilities;
        return abilities;
    }

    public LayerMask GetEnemies()
    {
        return instance.enemies;
    }

    // Add an ingredient to the player's inventory
    public void AddToInventory(FlavorIngredient ingredient)
    {
        if (ingredient == null)
        {
            Debug.LogError("AddToInventory: ingredient is null!");
            return;
        }
        flavorInventory.Add(ingredient);
        PrintIngredient(ingredient);
    }

    public void AddToInventory(AbilityIngredient ingredient)
    {
        abilityInventory.Add(ingredient);
    }

    public void RemoveFromInventory(FlavorIngredient ingredient)
    {
        flavorInventory.Remove(ingredient);
    }

    public void RemoveFromInventory(AbilityIngredient ingredient)
    {
        abilityInventory.Remove(ingredient);
    }

    public Transform InventoryContent;
    public GameObject InventorySlot;
    public GameObject DraggableItem;

    public void InventoryItems(String listType, Boolean clean = false)
    {
        if (listType != "flavor" && listType != "ability")
        {
            Debug.LogError($"Invalid List Type For InventoryItems(): {listType}");
        }

        if (clean)
        {
            // clean before use
            foreach (Transform item in InventoryContent)
            {
                Destroy(item.gameObject);
            }
        }

        if (listType == "ability")
        {

            List<AbilityIngredient> currentInventory = abilityInventory;

            foreach (var ingredient in currentInventory)
            {
                // Instantiate the InventorySlot prefab into InventoryContent
                GameObject obj = Instantiate(InventorySlot, InventoryContent);

                if (obj == null)
                {
                    Debug.LogError("Object instantiation failed.");
                    return;
                }

                // Instantiate draggableItem as a child of the InventorySlot
                GameObject draggableInstance = Instantiate(DraggableItem, obj.transform);

                if (draggableInstance == null)
                {
                    Debug.LogError("Draggable item instantiation failed.");
                    return;
                }

                // Find ItemName and ItemIcon transforms once
                Transform itemIconTransform = obj.transform.Find("Item(Clone)");
                if (itemIconTransform == null)
                {
                    Debug.LogError("Could not find ItemIcon in instantiated object.");
                    return;
                }

                // Find the ItemName inside ItemIcon and ensure the components exist
                Transform itemNameTransform = itemIconTransform.Find("ItemName");
                if (itemNameTransform == null)
                {
                    Debug.LogError("Could not find ItemName inside ItemIcon.");
                    return;
                }

                // Get the components from the found transforms
                TextMeshProUGUI itemName = itemNameTransform.GetComponent<TextMeshProUGUI>();
                UnityEngine.UI.Image itemIcon = itemIconTransform.GetComponent<UnityEngine.UI.Image>();

                if (ingredient == null)
                {
                    Debug.LogError($"Ingredient from {listType} inventory is null: check the inspector!!!");
                    return;
                }

                if (itemName == null)
                {
                    Debug.LogError("itemName is null, couldn't find the TextMeshProUGUI component.");
                    return;
                }

                if (itemIcon == null)
                {
                    Debug.LogError("itemIcon is null, couldn't find the Image component.");
                    return;
                }

                // Update the UI components with ingredient values
                itemName.text = ingredient.ingredientName;
                itemIcon.sprite = ingredient.icon;

                // Set the ingredient reference in the draggable item
                DraggableItem draggableItemComponent = draggableInstance.GetComponent<DraggableItem>();
                if (draggableItemComponent != null)
                {
                    draggableItemComponent.ingredientType = ingredient.ingredientType;
                    if (draggableItemComponent.ingredientType == "Ability")
                    {
                        draggableItemComponent.abilityIngredient = ingredient;
                    }
                    else
                    {
                        Debug.LogError("Retrieved Wrong Ingredient Type");
                    }

                }

                // Change the Alpha
                Color currentColor = itemIcon.color;
                currentColor.a = 1f;
                itemIcon.color = currentColor;
            }
        } else if (listType == "flavor")
        {
            List<FlavorIngredient> currentInventory = flavorInventory;

            foreach (var ingredient in currentInventory)
            {
                // Instantiate the InventorySlot prefab into InventoryContent
                GameObject obj = Instantiate(InventorySlot, InventoryContent);

                if (obj == null)
                {
                    Debug.LogError("Object instantiation failed.");
                    return;
                }

                // Instantiate draggableItem as a child of the InventorySlot
                GameObject draggableInstance = Instantiate(DraggableItem, obj.transform);

                if (draggableInstance == null)
                {
                    Debug.LogError("Draggable item instantiation failed.");
                    return;
                }

                // Find ItemName and ItemIcon transforms once
                Transform itemIconTransform = obj.transform.Find("Item(Clone)");
                if (itemIconTransform == null)
                {
                    Debug.LogError("Could not find ItemIcon in instantiated object.");
                    return;
                }

                // Find the ItemName inside ItemIcon and ensure the components exist
                Transform itemNameTransform = itemIconTransform.Find("ItemName");
                if (itemNameTransform == null)
                {
                    Debug.LogError("Could not find ItemName inside ItemIcon.");
                    return;
                }

                // Get the components from the found transforms
                TextMeshProUGUI itemName = itemNameTransform.GetComponent<TextMeshProUGUI>();
                UnityEngine.UI.Image itemIcon = itemIconTransform.GetComponent<UnityEngine.UI.Image>();

                if (ingredient == null)
                {
                    Debug.LogError($"Ingredient from {listType} inventory is null: check the inspector!!!");
                    return;
                }

                if (itemName == null)
                {
                    Debug.LogError("itemName is null, couldn't find the TextMeshProUGUI component.");
                    return;
                }

                if (itemIcon == null)
                {
                    Debug.LogError("itemIcon is null, couldn't find the Image component.");
                    return;
                }

                // Update the UI components with ingredient values
                itemName.text = ingredient.ingredientName;
                itemIcon.sprite = ingredient.icon;

                // Set the ingredient reference in the draggable item
                DraggableItem draggableItemComponent = draggableInstance.GetComponent<DraggableItem>();
                if (draggableItemComponent != null)
                {
                    draggableItemComponent.ingredientType = ingredient.ingredientType;
                    if (draggableItemComponent.ingredientType == "Flavor")
                    {
                        draggableItemComponent.flavorIngredient = ingredient;
                    }
                    else
                    {
                        Debug.LogError("Retrieved Wrong Ingredient Type");
                    }


                }

                // Change the Alpha
                Color currentColor = itemIcon.color;
                currentColor.a = 1f;
                itemIcon.color = currentColor;
            }
        }
    }



    public void Heal(int healAmount)
    {
        instance.health += healAmount;
        Debug.Log("Healing");
        if (instance.health > maxHealth)
        {
            instance.health = maxHealth;
        }
    }

    public override void TakeDamage(int damageAmount, GameObject source)
    {
        
        player.GetComponent<PlayerHealth>().TakeDamage(damageAmount, source);
        if (instance.health <= 0)
        {
            instance.health = 0;
            instance.dead = true;
            // Game over
            Debug.Log("Game Over womp womp");
        }
    }

    // Thoughtlessly reduces health, will not cause iframes or animation
    public override void TakeDamage(int damage)
    {
        instance.health -= damage;
        if (instance.health <= 0)
        {
            instance.health = 0;
            instance.dead = true;
            // Game over
            Debug.Log("Game Over womp womp");
        }
        // Used for testing status effects
        // InitializeStatusEffects();
    }
}
