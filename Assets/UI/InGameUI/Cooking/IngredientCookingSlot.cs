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
    internal Collectable ingredientReference;

    public void AddIngredient(Collectable ingredient)
    {
        ingredientReference = ingredient;
        ingredientReference.collectableUI.PlaceInCookingSlot(this);
        faceImage.gameObject.SetActive(true);
        faceImage.sprite = ingredientReference.collectableUI._SpriteReference;
        faceImage.color = Color.white;
        CookingScreen.Singleton.DisplayChangedSlots();
    }

    public void MouseDownOn()
    {
        if (ingredientReference != null)
        {
            faceImage.color = new Color(1, 1, 1, .25f);
            CursorManager.Singleton.PickupCollectable(ingredientReference);
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
        CookingScreen.Singleton.DisplayChangedSlots();
    }

    public void MouseUpOn(bool tap)
    {
        if (CursorManager.Singleton.currentCollectableReference != null)
        {
            AddIngredient(CursorManager.Singleton.currentCollectableReference);
            CursorManager.Singleton.DropCollectable();
        }
    }

    public void OnCook()
    {
        ingredientReference = null;
        faceImage.gameObject.SetActive(false);
    }

}
