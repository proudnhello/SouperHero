using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class InventorySlot : MonoBehaviour
{
    public GameObject inventorySlot;
    public bool basketDrop = false;
    public bool worldDrop = false;

    public Collectable ingredientReference;
    public Image faceImage;

    public void updateIngredientImage(Image newImage)
    {
        faceImage.sprite = newImage.sprite;
    }
}
