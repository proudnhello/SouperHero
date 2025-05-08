using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static SoupSpoon;
using UnityEngine.Rendering.Universal;


// Gets Items In the Cooking Slots and Call FillPot
public class CookingManager : MonoBehaviour
{
    public static CookingManager Singleton { get; private set; }

    public Transform CookingContent;
    [SerializeField] private GameObject abilityIngWarning;
    public GameObject CookingCanvas;
    private SoupSpoon statSpoon;
    bool isCooking = false;
    [SerializeField] private GameObject campfireWarning;
    public GameObject basketDrop;
    internal CookingSlot currentCookingSlot;

    public List<CookingSlot> cookingSlots;

    public static event Action CookSoup;

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;
    }

    //// Initialize Ingredient List
    //public List<Ingredient> cookingIngredients = new();

    // Initialize Ingredient List
    [SerializeField]
    public List<Collectable> cookingIngredients = new();

    [SerializeField] internal Campfire CurrentCampfire;

    public void EnterCooking(Campfire source)
    {
        CurrentCampfire = source;
        CursorManager.Singleton.ShowCursor();
        CursorManager.Singleton.ShowCookingCursor();
        CookingCanvas.SetActive(true);
        CookingCanvas.transform.position = source.GetCanvasPosition();
        isCooking = true;
        ClearCookingManagerSprites();
        PlayerEntityManager.Singleton.input.Player.Interact.started += ExitCooking;
        foreach(CookingSlot c in cookingSlots)
        {
            c.ingredientReference = null;
            c.faceImage.sprite = null;
            c.usesText.text = "";
        }
    }


    public void ExitCooking(InputAction.CallbackContext ctx = default)
    {
        if (CurrentCampfire != null)
        {
            CurrentCampfire.StopPrepping();
            CurrentCampfire = null;
            CursorManager.Singleton.HideCursor();

            if(CursorManager.Singleton.cookingCursor.currentCollectableReference != null)
            {
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                CursorManager.Singleton.cookingCursor.currentCollectableReference.collectableUI.GetComponent<Image>().raycastTarget = true;

                CursorManager.Singleton.cookingCursor.removeCursorImage();
                CursorManager.Singleton.cursorObject.SetActive(true);
                Encyclopedia.Singleton.setInActive();
            }

            CursorManager.Singleton.HideCookingCursor();
            CookingCanvas.SetActive(false);
            isCooking = false;
            foreach(Collectable c in cookingIngredients)
            {
                c.collectableUI.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                c.collectableUI.GetComponent<Image>().raycastTarget = true;
                c.collectableUI.GetComponent<DraggableItem>().previousParent = c.transform;
            }
            cookingIngredients.Clear();

            PlayerEntityManager.Singleton.input.Player.Interact.started -= ExitCooking;

            // Save game after cooking
            SaveManager.Singleton.SaveGameScene();
        }
    }
    
    // No Parameters For the Exit Button
    public void ExitCooking()
    {
        ExitCooking(default);
    }

    public bool IsCooking()
    {
        return isCooking;
    }


    //private void OnDisable()
    //{

    //    PlayerEntityManager.Singleton.input.Player.Interact.started -= ExitCooking;
    //}

    // Function to add an Ability Ingredient
    public void AddIngredient(Collectable ingredient)
    {
        cookingIngredients.Add(ingredient);
    }

    // Function to remove an Ability Ingredient
    public void RemoveIngredient(Collectable ingredient)
    {
        cookingIngredients.Remove(ingredient);
    }

    // Check if there is an ability ingredient in the pot
    public bool HasAbilityIngredient()
    {
        // Don't cook if there are no ability ingredients
        foreach (Collectable ingredient in cookingIngredients)
        {
            if (ingredient.ingredient.GetType() == typeof(AbilityIngredient))
            {
                return true;
            }
        }

        return false;
    }

    public void DisplayAbilityIngWarning()
    {
        abilityIngWarning.SetActive(true);
    }

    public void HideAbilityIngWarning()
    {
        abilityIngWarning.SetActive(false);
    }

    public void ShowCampfireWarning()
    {
        campfireWarning.SetActive(true);
    }

    public void HideCampfireWarning()
    {
        campfireWarning.SetActive(false);
    }

    // Call this to cook the soup
    public void CookTheSoup()
    {
        // Don't cook if there is no ability ingredient, return early
        if (!HasAbilityIngredient())
        {
            DisplayAbilityIngWarning();
            return;
        } else
        {
            HideAbilityIngWarning();
        }

        // Don't cook if max spoons
        if (PlayerInventory.Singleton.GetSpoons().Count == PlayerInventory.Singleton.maxSpoons)
        {
            return;
        }

        //if (!PlayerEntityManager.Singleton.HasCooked())
        //{
        //    ShowCampfireWarning();
        //    PlayerEntityManager.Singleton.SetCooked(true);
        //}

        // Trigger Transition to Break Animation
        Animator campfireAnimator = CurrentCampfire.GetComponent<Animator>();

        // Turn Off Light
        // Get the Light 2D component from the child object
        Light2D light = CurrentCampfire.transform.Find("Light 2D").GetComponent<Light2D>();
        if (light != null)
        {
            light.enabled = false;
        }

        if (CurrentCampfire.gameObject == null)
        {
            Debug.Log("Campfire Animator not found!");
        }

        if (campfireAnimator == null)
        {
            Debug.Log("Campfire Animator not found!");
        }

        // Currently commented out while the cooking animation is the emptying animation
        // campfireAnimator.SetTrigger("Cooked The Soup");

        // Cook the soup with what is currently in the pot
        List<Ingredient> cookedIngredients = new();
        foreach (Collectable ingredient in cookingIngredients)
        {
            ingredient.ingredient.Icon = ingredient.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite;
            cookedIngredients.Add(ingredient.ingredient);
        }
        PlayerInventory.Singleton.CookSoup(cookedIngredients);

        // Remove From Player Inventory
        foreach (Collectable ingredient in cookingIngredients)
        {
            PlayerInventory.Singleton.RemoveIngredientCollectable(ingredient, true);
        }

        cookingIngredients.Clear();

        ClearCookingManagerSprites();

        CookSoup?.Invoke();

        // Exit Cooking
        ExitCooking();
    }

    // Sets all the sprites in the cooking slot to null and 0 alpha
    public void ClearCookingManagerSprites()
    {
        foreach (Transform slot in CookingContent)
        {
            CookingSlot cookingSlot = slot.gameObject.GetComponent<CookingSlot>();
            cookingSlot.ingredientReference = null;
            cookingSlot.faceImage.sprite = null;

            Color tempColor = cookingSlot.faceImage.color;
            tempColor.a = 0;
            cookingSlot.faceImage.color = tempColor;
            cookingSlot.usesText.text = "";
        }
    }

    // Sets all the images in the cooking slot to 1 alpha
    // Only if the cooking sprite is not null (meaning an ingredient is in it)
    public void CookingManagerSpritesSetOpaque()
    {
        foreach (Transform slot in CookingContent)
        {
            foreach (Transform item in slot)
            {
                Image image = item.gameObject.GetComponent<Image>();
                if (image.sprite != null)
                {
                    Color tempColor = image.color;
                    tempColor.a = 1;
                    item.gameObject.GetComponent<Image>().color = tempColor;
                }
            }
        }
    }

    // Turn a specific cookingslot transparent
    public void CookingSlotSetTransparent(CookingSlot slot)
    {
        // index into first child slot bc should only be 1 child
        // slot.transform.GetChild(0).GetComponent<Image>().sprite = null;
        Image image = slot.transform.GetChild(0).GetComponent<Image>();
        Color tempColor = image.color;
        tempColor.a = 0;
        slot.transform.GetChild(0).GetComponent<Image>().color = tempColor;
    }

    // Turn a specific cookingslot opaque
    public void CookingSlotSetOpaque(CookingSlot slot)
    {
        Image image = slot.transform.GetChild(0).GetComponent<Image>();
        Color tempColor = image.color;
        tempColor.a = 1;
        slot.transform.GetChild(0).GetComponent<Image>().color = tempColor;
    }
}
