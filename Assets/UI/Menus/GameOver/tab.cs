using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Sprite defaultTab;
    [SerializeField] Sprite hoverTab;
    [SerializeField] int spriteScalar = 5;

    private Image image;
    private RectTransform rectTransform;

    private void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        image.sprite = defaultTab;
        AdjustSizeToSprite(defaultTab);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (image != null && hoverTab != null)
        {
            image.sprite = hoverTab;
            AdjustSizeToSprite(hoverTab);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (image != null && defaultTab != null)
        {
            image.sprite = defaultTab;
            AdjustSizeToSprite(defaultTab);
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
