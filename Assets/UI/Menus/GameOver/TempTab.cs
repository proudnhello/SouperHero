using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TempTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Sprite defaultTab;
    [SerializeField] Sprite hoverTab;
    [SerializeField] int spriteScalar = 5;
    [SerializeField] TextMeshProUGUI text;

    private Image image;
    private RectTransform rectTransform;
    public bool isSelected = false;

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

    private void Start()
    {
        if (isSelected)
        {
            ShowTab();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTab();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!isSelected)
        {
            HideTab();
        }
    }

    private void AdjustSizeToSprite(Sprite sprite)
    {
        if (sprite == null || rectTransform == null) return;

        // Get the pixel size of the sprite
        Vector2 spriteSize = sprite.rect.size * spriteScalar;

        // Set the RectTransform to match the sprite�s size
        rectTransform.sizeDelta = spriteSize;
    }

    public void ShowTab()
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

    public void HideTab()
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

    public void KeepSelected(bool selection)
    {
        isSelected = selection;
        if(!isSelected)
        {
            HideTab();
        }
    }
}
