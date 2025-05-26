using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.VolumeComponent;
using Image = UnityEngine.UI.Image;

public class SoupUI : MonoBehaviour
{
    public static SoupUI Singleton { get; private set; }

    [Header("SoupInventory")]
    [SerializeField] private GameObject Rope;
    [SerializeField] private Sprite cookingBubbleSprite;
    [SerializeField] private Material spriteLit;

    [SerializeField] private List<Sprite> tempSoupSprites; //Temporary

    [SerializeField] TMP_Text[] usesTextComponents;
    public static event Action<int> AddSpoon;
    public static event Action<int> RemoveSpoon;


    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;
    }

    public void MoveInventory(Vector2 target, float speed)
    {
        StartCoroutine(MoveInventoryUI(Rope, target, speed));
        StartCoroutine(MoveInventoryUI(this.gameObject, target, speed));
    }

    //Move inventory UI elements using MoveTowards
    private IEnumerator MoveInventoryUI(GameObject obj, Vector2 target, float speed)
    {
        RectTransform rectTransform = obj.GetComponent<RectTransform>(); //Get the rectTransform, since it's a UI element
        Vector2 targetPosition = rectTransform.anchoredPosition + target; //New target position
        var step = speed * Time.deltaTime;

        while (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) > 0.001f)
        {
            rectTransform.anchoredPosition = Vector2.MoveTowards(rectTransform.anchoredPosition, targetPosition, step);
            yield return null;
        }
    }

    //Display selected soups' inventory slots
    public void DisplayInventorySlots()
    {
        for (int i = 0; i < 4; i++)
        {
            //SoupInventory.transform.GetChild(i).gameObject.SetActive(true);
            var soupImg = this.transform.GetChild(i).GetComponent<Image>();

            //If the soup is not active and is cooking, set the slot to be visible
            if (!this.transform.GetChild(i).GetChild(0).gameObject.activeInHierarchy && CookingManager.Singleton.IsCooking())
            {
                soupImg.sprite = cookingBubbleSprite;
                soupImg.material = null;
                SetAlpha(soupImg, 1);
            }
            else
            {
                soupImg.sprite = null;
                soupImg.material = spriteLit;
                SetAlpha(soupImg, 0);
            }
        }
    }

    //Helper function to add soup image to icon in slot
    //Lo: This is temporary!
    //TODO: Once art is complete, use whatever image is attached to the soup instead of temp
    public void AddSoupInSlot(int index)
    {
        if (index < 4) return; //Don't effect selected soups
        var image = this.transform.GetChild(index).GetChild(0).GetComponent<Image>();
        //The -4 is to account for the first 4 active soups, since they don't have slot icons. Fix later
        image.sprite = tempSoupSprites[index-4];
        SetAlpha(image, 1);
    }

    //Helper function to remove soup image from icon in slot
    public void RemoveSoupInSlot(int index) {
        if (index < 4) return; //Don't affect selected soups
        var image = this.transform.GetChild(index).GetChild(0).GetComponent<Image>();
        image.sprite = null;
        SetAlpha(image, 0);
    }

    public void SwapSoups(int[] indices)
    {
        foreach (var index in indices)
        {
            SetUsesText(index);

            //Checking if swapping active soup and empty slot
            if (index < 4 &&
                PlayerInventory.Singleton.GetSpoons()[index] != null &&
                PlayerInventory.Singleton.GetSpoons()[index].spoonAbilities.Count > 0)
            {
                AddSpoon?.Invoke(index);
            } else if (index < 4)
            {
                var currSpoon = PlayerInventory.Singleton.GetCurrentSpoon();
                if (PlayerInventory.Singleton.GetCurrentSpoon() == index) //Check if index is current spoon
                {
                    PlayerInventory.Singleton.currentSpoon = PlayerInventory.Singleton.FindNextAvalaibleIndex(currSpoon, false);
                }
                RemoveSpoon?.Invoke(index);
            }
        }
        SwapImages(indices);
    }

    //Set uses text for the game object at the specified index
    public void SetUsesText(int index)
    {
        SoupSpoon soupSpoon = PlayerInventory.Singleton.GetSpoons()[index];
        if (soupSpoon == null) { 
            usesTextComponents[index].gameObject.SetActive(false);
            return;
        }

        usesTextComponents[index].gameObject.SetActive(true);
        switch(soupSpoon.uses)
        {
            case -1:
                usesTextComponents[index].text = "∞";
                break;
            case 0:
                usesTextComponents[index].text = "";
                break;
            default:
                usesTextComponents[index].text = soupSpoon.uses.ToString();
                break;
        }
    }

    // Lo: This shit don't work and is really stupid
    // >:(
    private void SwapImages(int[] indices)
    {
        var index1 = indices[0]; var index2 = indices[1];

        GameObject tempIndex = Instantiate(this.gameObject.transform.GetChild(index1).GetChild(0).gameObject);
        var tempIndex1 = tempIndex.GetComponent<Image>();

        //Debug.Log(tempIndex1.sprite.ToString());

        var index1Img = this.gameObject.transform.GetChild(index1).GetChild(0).GetComponent<Image>();
        var index2Img = this.gameObject.transform.GetChild(index2).GetChild(0).GetComponent<Image>();

        index1Img.sprite = index2Img.sprite;
        index1Img.material = index2Img.material;
        index1Img.material.color = index2Img.material.color;

        index2Img.sprite = tempIndex1.sprite;
        index2Img.material = tempIndex1.material;
        index2Img.material.color = tempIndex1.material.color;

        //Debug.Log(tempIndex1.sprite.ToString());

        Destroy(tempIndex);
    }

    //Helper function to set alpha
    private void SetAlpha(Image image, int alphaAmount)
    {
        Color tempColor = image.color;
        tempColor.a = alphaAmount;
        image.color = tempColor;
    }
}
