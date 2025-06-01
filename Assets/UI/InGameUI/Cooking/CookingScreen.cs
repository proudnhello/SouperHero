using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using System.Linq;

// Gets Items In the Cooking Slots and Call FillPot
public class CookingScreen : MonoBehaviour
{
    public static CookingScreen Singleton { get; private set; }

    public static event Action EnterCookingScreen;
    public static event Action ExitCookingScreen;
    public static event Action<bool> CookingScreenIsOut;
    public static event Action CookSoup;

    [SerializeField] RectTransform CookingContent;
    private FinishedSoup statSpoon;

    [SerializeField] IngredientCookingSlot[] IngredientCookingSlots;
    public BowlCookingSlot BowlCookingSlot;


    [Header("Move Cooking Anim")]
    [SerializeField] float ClosedYPos;
    [SerializeField] float OpenYPos;
    [SerializeField] AnimationCurve MoveCookingAnimationCurve;
    [SerializeField] float MoveCookingAnimationTime;

    bool isCooking = false;
    public bool IsCooking
    {
        get { return isCooking; }
    }
    internal bool AtCookingScreen;
    internal Campfire CurrentCampfire;

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;
        CookingContent.localPosition = new Vector2(CookingContent.localPosition.x, ClosedYPos);
        CookingContent.gameObject.SetActive(false);

        foreach (IngredientCookingSlot c in IngredientCookingSlots)
        {
            c.Init();
        }
    }

    public void EnterCooking(Campfire source)
    {
        CurrentCampfire = source;
        isCooking = true;
        CookingContent.gameObject.SetActive(true);
        PlayerEntityManager.Singleton.input.Player.PauseGame.started += ExitCooking;

        EnterCookingScreen?.Invoke();
        if (IMoveCookingUI != null) StopCoroutine(IMoveCookingUI);
        StartCoroutine(IMoveCookingUI = MoveCookingUI(true));
    }

    private void OnDisable()
    {
        PlayerEntityManager.Singleton.input.Player.PauseGame.started -= ExitCooking;
    }

    float moveCookingAnimTimeProgressed = 0;
    IEnumerator IMoveCookingUI;
    private IEnumerator MoveCookingUI(bool open)
    {
        while (moveCookingAnimTimeProgressed >= 0 && moveCookingAnimTimeProgressed <= MoveCookingAnimationTime)
        {
            var percentCompleted = Mathf.Clamp01(moveCookingAnimTimeProgressed / MoveCookingAnimationTime);
            var scaledPercentaged = MoveCookingAnimationCurve.Evaluate(percentCompleted);
            var newYPos = Mathf.Lerp(ClosedYPos, OpenYPos, scaledPercentaged);

            CookingContent.localPosition = new Vector2(CookingContent.localPosition.x, newYPos);

            yield return null;

            moveCookingAnimTimeProgressed = open ? moveCookingAnimTimeProgressed + Time.deltaTime : moveCookingAnimTimeProgressed - Time.deltaTime;
        }

        moveCookingAnimTimeProgressed = open ? MoveCookingAnimationTime : 0;
        CookingContent.localPosition = new Vector2(CookingContent.localPosition.x, open ? OpenYPos : ClosedYPos);

        if (!open) // has fully closed
        {
            CookingContent.gameObject.SetActive(false);
            foreach (IngredientCookingSlot c in IngredientCookingSlots)
            {
                if (c.ingredientReference != null) c.ingredientReference.collectableUI.ReturnIngredientHereFromCursor();
                c.RemoveIngredient();
            }
            BowlCookingSlot.RemoveBowl();
        } else // has fully opened
        {
            AtCookingScreen = true;
            CookingScreenIsOut?.Invoke(AtCookingScreen);
        }
    }

    public void ExitCooking(InputAction.CallbackContext ctx = default)
    {
        CurrentCampfire.StopPrepping();
        CurrentCampfire = null;
        isCooking = false;

        PlayerEntityManager.Singleton.input.Player.PauseGame.started -= ExitCooking;

        // Save game after cooking
        RunStateManager.Singleton.SaveRunState();
        PlayerInventory.Singleton.SaveInventory();

        ExitCookingScreen?.Invoke();
        AtCookingScreen = false;
        CookingScreenIsOut?.Invoke(AtCookingScreen);

        if (IMoveCookingUI != null) StopCoroutine(IMoveCookingUI);
        StartCoroutine(IMoveCookingUI = MoveCookingUI(false));
    }

    // No Parameters For the Exit Button
    public void ExitCooking()
    {
        ExitCooking(default);
    }

    public IngredientCookingSlot GetAvailableSoupSlot()
    {
        for (int i = 0; i < IngredientCookingSlots.Length; i++)
        {
            if (IngredientCookingSlots[i].ingredientReference == null) return IngredientCookingSlots[i];
        }
        return null;
    }

    internal bool SoupIsValid;
    public void CheckIfSoupIsValid()
    {
        SoupIsValid = false;
        if (!IsCooking) return;

        if (BowlCookingSlot.soupBaseReference == null) return;

        foreach (var slot in IngredientCookingSlots)
        {
            if (slot.ingredientReference == null) continue;
            if (slot.ingredientReference.ingredient is AbilityIngredient)
            {
                SoupIsValid = true;
                break;
            }
        }
    }

    public void CookTheSoup()
    {
        SoupIsValid = false;
        // Cook the soup with what is currently in the pot
        List<Ingredient> cookedIngredients = new();
        foreach (var slot in IngredientCookingSlots)
        {
            if (slot.ingredientReference == null) continue;
            cookedIngredients.Add(slot.ingredientReference.ingredient);
            PlayerInventory.Singleton.RemoveIngredientCollectable(slot.ingredientReference, true);
            slot.OnCook();
        }

        FinishedSoup soup = new(cookedIngredients, BowlCookingSlot.soupBaseReference);
        PlayerInventory.Singleton.BowlIsCooked(BowlCookingSlot.soupSlotReference, soup);

        cookedIngredients.Clear();
        BowlCookingSlot.RemoveBowl();

        CookSoup?.Invoke();
        MetricsTracker.Singleton.RecordSoupsCooked();
    }
}
