using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Security;
using TMPro;

public class IngredientCookingSlot : MonoBehaviour, ICursorInteractable
{
    [SerializeField] Image faceImage;
    [SerializeField] Image slotOutline;
    [SerializeField] Image slotIcon;
    [SerializeField] Sprite[] slotIconSprites;
    internal Collectable ingredientReference;

    public enum SlotType
    {
        Ability,
        Flavor,
        Wildcard
    }
    internal SlotType currentSlotType;

    public void Init()
    {
        ingredientReference = null;
        faceImage.gameObject.SetActive(false);
        faceImage.color = Color.white;
        slotOutline.gameObject.SetActive(true);
    }

    public void SetSlotType(SlotType type)
    {
        currentSlotType = type;
        slotIcon.sprite = slotIconSprites[(int)type];
        slotIcon.transform.localScale = new Vector3(.8f, .8f, .8f);
    }

    public void AddIngredient(Collectable ingredient)
    {
        ingredientReference = ingredient;
        ingredientReference.collectableUI.PlaceInCookingSlot(this);
        faceImage.gameObject.SetActive(true);
        faceImage.sprite = ingredientReference.collectableUI._SpriteReference;
        faceImage.color = Color.white;
        slotOutline.gameObject.SetActive(false);
        slotIcon.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        CookingScreen.Singleton.CheckIfSoupIsValid();
    }

    public void MouseDownOn()
    {
        if (ingredientReference != null)
        {
            faceImage.color = new Color(1, 1, 1, .25f);
            CursorManager.Singleton.PickupCollectable(ingredientReference);
            Encyclopedia.Singleton.PullUpEntry(ingredientReference.ingredient);
        }
    }

    public void ReturnIngredientHereFromCursor()
    {
        faceImage.color = Color.white;
    }

    public void RemoveIngredient()
    {
        ingredientReference = null;
        faceImage.gameObject.SetActive(false);
        slotIcon.transform.localScale = new Vector3(.8f, .8f, .8f);
        slotOutline.gameObject.SetActive(true);
        CookingScreen.Singleton.CheckIfSoupIsValid();
    }

    public void Tap()
    {
        if (ingredientReference != null)
        {
            ingredientReference.collectableUI.ReturnIngredientHereFromCursor();
            CursorManager.Singleton.DropCollectable();
        }
    }
    public void MouseUpOn()
    {
        if (CursorManager.Singleton.currentCollectableReference != null)
        {
            if (currentSlotType == SlotType.Wildcard || (CursorManager.Singleton.currentCollectableReference.ingredient is AbilityIngredient && currentSlotType == SlotType.Ability) ||
                (CursorManager.Singleton.currentCollectableReference.ingredient is FlavorIngredient && currentSlotType == SlotType.Flavor))
            {
                AddIngredient(CursorManager.Singleton.currentCollectableReference);
                CursorManager.Singleton.DropCollectable();
            } else
            {
                CursorManager.Singleton.ManuallyReturnIngredientFromCursor();
            }
        }
    }

    public void OnCook()
    {
        ingredientReference = null;
        faceImage.gameObject.SetActive(false);
        slotOutline.gameObject.SetActive(true);
    }

}
