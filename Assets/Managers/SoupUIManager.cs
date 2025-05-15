using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoupUIManager : MonoBehaviour
{
    public static SoupUIManager Singleton { get; private set; }

    [Header("SoupInventory")]
    [SerializeField] private GameObject SoupSelect;
    [SerializeField] private GameObject SoupInventory;
    [SerializeField] private Sprite cookingBubbleSprite;
    [SerializeField] private Material spriteLit;

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
                /*
                var soupImg = SoupInventory.transform.GetChild(i).GetComponent<Image>();
                soupImg.sprite = null;
                soupImg.material = spriteLit;
                SetToAlpha(soupImg, 0);
                */
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
                    SetToAlpha(soupImg, 1);
                }
                else
                {
                    soupImg.sprite = null;
                    soupImg.material = spriteLit;
                    SetToAlpha(soupImg, 0);
                }
            }
        }
    }

    //Helper function to set alpha
    private void SetToAlpha(Image image, int alphaAmount)
    {
        Color tempColor = image.color;
        tempColor.a = alphaAmount;
        image.color = tempColor;
    }
}
