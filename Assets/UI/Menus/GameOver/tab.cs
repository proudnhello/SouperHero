using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Tab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Sprite defaultTab;
    [SerializeField] Sprite hoverTab;
    [SerializeField] int spriteScalar = 5;
    [SerializeField] TextMeshProUGUI text;

    private Image image;
    private RectTransform rectTransform;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        image.sprite = defaultTab;
        AdjustSizeToSprite(defaultTab);

        if (text != null)
        {
            text.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (image != null && hoverTab != null)
        {
            image.sprite = hoverTab;
            AdjustSizeToSprite(hoverTab);
            if (text != null)
            {
                text.enabled = true;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (image != null && defaultTab != null)
        {
            image.sprite = defaultTab;
            AdjustSizeToSprite(defaultTab);
            if (text != null)
            {
                text.enabled = false;
            }
        }
    }

    private void AdjustSizeToSprite(Sprite sprite)
    {
        if (sprite == null || rectTransform == null) return;

        // Get the pixel size of the sprite
        Vector2 spriteSize = sprite.rect.size * spriteScalar;

        // Set the RectTransform to match the sprite’s size
        rectTransform.sizeDelta = spriteSize;
    }
}
