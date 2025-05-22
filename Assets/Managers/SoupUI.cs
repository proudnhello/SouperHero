using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class SoupUI : MonoBehaviour
{
    public static SoupUI Singleton { get; private set; }

    [Header("SoupInventory")]
    [SerializeField] private GameObject SoupSelect;
    [SerializeField] private GameObject SoupInventory;
    [SerializeField] private Sprite cookingBubbleSprite;
    [SerializeField] private Material spriteLit;

    [SerializeField] private List<Sprite> tempSoupSprites;

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(gameObject);
        else Singleton = this;
    }

    public void MoveInventory(Vector2 target, float speed)
    {
        StartCoroutine(MoveInventoryUI(SoupSelect, target, speed));
        StartCoroutine(MoveInventoryUI(SoupInventory, target, speed));
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
        if (!CookingManager.Singleton.IsCooking()) //If not cooking, hide selected soup inventory slots
        {
            for (int i = 0; i < 4; i++)
            {
                SoupInventory.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        else //If cooking, display the selected soup inventory slots
        {
            for (int i = 0; i < 4; i++)
            {
                SoupInventory.transform.GetChild(i).gameObject.SetActive(true);
                var soupImg = SoupInventory.transform.GetChild(i).GetComponent<Image>();

                //If the soup is not active, set the slot to be visible
                if (!SoupSelect.transform.GetChild(i).gameObject.activeInHierarchy)
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
    }

    //Helper function to add soup image to icon in slot
    public void AddSoupInSlot(int index)
    {
        //TODO: Use whatever image is attached to the soup instead of temp
        if (index < 4) return; //Don't effect selected soups
        var image = SoupInventory.transform.GetChild(index).GetChild(0).GetComponent<Image>();
        //image.sprite = tempSoupSprites[index];
        //This is temporary!
        //The -4 is to account for the first 4 active soups, since they don't have slot icons
        image.sprite = tempSoupSprites[index-4];
        SetAlpha(image, 1);
    }

    //Helper function to remove soup image from icon in slot
    public void RemoveSoupInSlot(int index) {
        if (index < 4) return; //Don't effect selected soups
        var image = SoupInventory.transform.GetChild(index).GetChild(0).GetComponent<Image>();
        image.sprite = null;
        SetAlpha(image, 0);
    }

    public void SwapSoups(int index1, int index2)
    {
        //TODO: Account for selected soup being in slots 0 - 4. Don't add to slot if that is the case
        (tempSoupSprites[index1 - 4], tempSoupSprites[index2 - 4]) = (tempSoupSprites[index2 - 4], tempSoupSprites[index1 - 4]);
        AddSoupInSlot(index1);
        AddSoupInSlot(index2);
    }

    //Helper function to set alpha
    private void SetAlpha(Image image, int alphaAmount)
    {
        Color tempColor = image.color;
        tempColor.a = alphaAmount;
        image.color = tempColor;
    }
}
