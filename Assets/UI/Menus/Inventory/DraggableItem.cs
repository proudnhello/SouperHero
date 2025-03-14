using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using static FlavorIngredient;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public Transform parentAfterDrag;
    [HideInInspector] public string ingredientType;
    public Transform previousParent;

    [Header("Do Not Edit, Ingredient is Set In CookingUI's Enable()")]
    public Ingredient ingredient = null;
    public bool isDragging = false;

    Collectable collectable;
    public static float alphaOnPickup = .25f;

    private void Awake()
    {
        collectable = transform.parent.gameObject.GetComponent<Collectable>();
    }

    public void OnBeginDrag(PointerEventData eventData)         
    {
        CursorManager.Singleton.cursorObject.SetActive(false);
        CursorManager.Singleton.ShowCookingCursor();
        CursorManager.Singleton.isDragging = true;
        CursorManager.Singleton.basketDrop.SetActive(true);

        CursorManager.Singleton.cookingCursor.switchCursorImageTo(collectable, image);
        Encyclopedia.Singleton.PullUpEntry(collectable.ingredient);
        print("Begin Drag");
        
        if(!parentAfterDrag)
        {
            parentAfterDrag = CookingManager.Singleton.basketDrop.transform;
        } else
        {
            parentAfterDrag = previousParent;
        }

        image.color = new Color(1.0f, 1.0f, 1.0f, alphaOnPickup);
        image.raycastTarget = false;

        isDragging = true;

        //CookingManager.Singleton.enableWorldDrop();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Encyclopedia.Singleton.PullUpEntry(collectable.ingredient);
    }

    public bool resetParent()
    {
        image.color = new Color(1.0f, 1.0f, 1.0f, alphaOnPickup);
        image.raycastTarget = false;
        if (parentAfterDrag.gameObject.GetComponent<CookingSlot>().ingredientReference == null)
        {
            if (!parentAfterDrag.gameObject.CompareTag("BasketDrop") && !parentAfterDrag.gameObject.CompareTag("WorldDrop"))
            {
                image.color = new Color(1.0f, 1.0f, 1.0f, alphaOnPickup);
                image.raycastTarget = false;

                if (parentAfterDrag.gameObject.CompareTag("CookingSlot"))
                {
                    if (parentAfterDrag == previousParent)
                    {
                        return false;
                    }
                    parentAfterDrag.gameObject.GetComponent<CookingSlot>().ingredientReference = CursorManager.Singleton.cookingCursor.currentCollectableReference;
                    parentAfterDrag.gameObject.GetComponent<CookingSlot>().updateIngredientImage(image);

                    // set this cooking slot image alpha to 1
                    CookingManager.Singleton.CookingSlotSetOpaque(parentAfterDrag.gameObject.GetComponent<CookingSlot>());
                    // set previous slot image alpha to 0
                    CookingSlot previousCookingSlot = CookingManager.Singleton.currentCookingSlot;
                    if (previousCookingSlot != null)
                    {
                        CookingManager.Singleton.CookingSlotSetTransparent(previousCookingSlot);
                        Debug.Log("PREVIOUS COOKING SLOT: " + previousCookingSlot.name);
                    }
                }
            }
            else if (!parentAfterDrag.gameObject.CompareTag("WorldDrop"))
            {
                image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                image.raycastTarget = true;
                BasketUI.Singleton.AddIngredient(CursorManager.Singleton.cookingCursor.currentCollectableReference, false);
            }
        }
        previousParent = parentAfterDrag;
        return true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            CursorManager.Singleton.cookingCursor.removeCursorImage();
            CookingManager.Singleton.disableWorldDrop();
            isDragging = false;
            image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            image.raycastTarget = true;
            CookingManager.Singleton.currentCookingSlot = null;
            Encyclopedia.Singleton.Hide();
        }
        //if(CursorManager.Singleton.isDragging)
        //{
        //    CursorManager.Singleton.basketDrop.SetActive(true);
        //    CursorManager.Singleton.isDragging = false;
        //    if (!CursorManager.Singleton.isInBasket)
        //    {
        //        CursorManager.Singleton.cursorObject.SetActive(true);
        //        CursorManager.Singleton.HideCookingCursor();
        //    }
        //}
        if (!CookingManager.Singleton.IsCooking())
        {
            CursorManager.Singleton.cursorObject.SetActive(true);
            CursorManager.Singleton.HideCookingCursor();
            Encyclopedia.Singleton.setInActive();
            CursorManager.Singleton.basketDrop.SetActive(false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Encyclopedia.Singleton.Hide();
        print("exit");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ingredient = transform.parent.GetComponent<Collectable>().ingredient;
        Encyclopedia.Singleton.PullUpEntry(collectable.ingredient);
        print("enter");
        
        // I wanna give a huge shoutout to the person whoever wrote this 300 line function, only for it all to be thrown away.
        // o7
        /*CookingManager.Singleton.DisplayItemStats();
        GameObject itemStatsScreen = CookingManager.Singleton.itemStatsScreen;

        //Transform background = itemStatsScreen.transform.Find("Background");
        //Transform header = background.transform.Find("Header");
        //Transform body = background.transform.Find("Body");

        //// move the screena
        //itemStatsScreen.transform.SetParent(this.transform);
        //RectTransform rt = GetComponent<RectTransform>();
        //Vector2 actualSize = new Vector2(rt.rect.width * rt.lossyScale.x, rt.rect.height * rt.lossyScale.y);
        //itemStatsScreen.transform.position = new Vector2(this.transform.position.x, this.transform.position.y) + new Vector2(actualSize.x / 2, -actualSize.y / 2);
        ////Debug.Log(itemStatsScreen.transform.position);
        ////Debug.Log(rt.rect.size);
        ////Debug.Log(this.transform.position);

        //// bring to the front
        //itemStatsScreen.transform.SetParent(transform.root);
        //itemStatsScreen.transform.SetAsLastSibling();


        //// set text
        //TextMeshProUGUI headerText = header.GetComponent<TextMeshProUGUI>();
        //headerText.text = ingredient.IngredientName;

        //TextMeshProUGUI bodyText = body.GetComponent<TextMeshProUGUI>();

        //if (ingredient.GetType() == typeof(AbilityIngredient))
        //{
        //    AbilityIngredient abilityIngredient = ingredient as AbilityIngredient;
        //    bodyText.text = $"<color=purple>Ability Ingredient</color>\nType: {abilityIngredient.abilityType._abilityName}\n\n";

        //    foreach (InflictionFlavor inflictionFlavor in abilityIngredient.inherentInflictionFlavors)
        //    {
        //        string color = ColorUtility.ToHtmlStringRGB(FlavorIngredient.inflictionColorMapping[inflictionFlavor.inflictionType]);
        //        //switch (inflictionFlavor.inflictionType)
        //        //{
        //        //    case FlavorIngredient.InflictionFlavor.InflictionType.SPICY_Burn:
        //        //        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
        //        //        {
        //        //            if (inflictionFlavor.amount > 0) {
        //        //                bodyText.text += $"<color=#{color}>Spicy:</color> + {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
        //        //        {
        //        //            if (inflictionFlavor.amount > 1) {
        //        //                bodyText.text += $"<color=#{color}>Spicy:</color> x {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        break;
        //        //    case FlavorIngredient.InflictionFlavor.InflictionType.FROSTY_Freeze:
        //        //        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
        //        //        {
        //        //            if (inflictionFlavor.amount > 0) {
        //        //                bodyText.text += $"<color=#{color}>Frosty:</color> + {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
        //        //        {
        //        //            if (inflictionFlavor.amount > 1) {
        //        //                bodyText.text += $"<color=#{color}>Frosty:</color> x {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        break;
        //        //    case FlavorIngredient.InflictionFlavor.InflictionType.HEARTY_Health:
        //        //        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
        //        //        {
        //        //            if (inflictionFlavor.amount > 0) {
        //        //                bodyText.text += $"<color=#{color}>Hearty:</color> + {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
        //        //        {
        //        //            if (inflictionFlavor.amount > 1) {
        //        //                bodyText.text += $"<color=#{color}>Hearty:</color> x {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        break;
        //        //    case FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage:
        //        //        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
        //        //        {
        //        //            if (inflictionFlavor.amount > 0) {
        //        //                bodyText.text += $"<color=#{color}>Spiky:</color> + {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
        //        //        {
        //        //            if (inflictionFlavor.amount > 1) {
        //        //                bodyText.text += $"<color=#{color}>Spiky:</color> x {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        break;
        //        //    case FlavorIngredient.InflictionFlavor.InflictionType.GREASY_Knockback:
        //        //        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
        //        //        {
        //        //            if (inflictionFlavor.amount > 0) {
        //        //                bodyText.text += $"<color=#{color}>Greasy:</color> + {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
        //        //        {
        //        //            if (inflictionFlavor.amount > 1) {
        //        //                bodyText.text += $"<color=#{color}>Greasy:</color> x {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        break;
        //        //}
        //    }

        //    bodyText.text += "\nBase Stats:\n";
        //    if (abilityIngredient.baseStats.duration > 0)
        //        bodyText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.SOUR_Duration])}>Sour (Duration):</color> {abilityIngredient.baseStats.duration}\n";
        //    if (abilityIngredient.baseStats.size > 0)
        //        bodyText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.BITTER_Size])}>Bitter (Size):</color> {abilityIngredient.baseStats.size}\n";
        //    if (abilityIngredient.baseStats.crit > 0)
        //        bodyText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.SALTY_Crit])}>Salty (Critical Strike):</color> {abilityIngredient.baseStats.crit}\n";
        //    if (abilityIngredient.baseStats.speed > 0)
        //        bodyText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.SWEET_Speed])}>Sweet (Speed):</color> {abilityIngredient.baseStats.speed}\n";
        //    bodyText.text += $"<color=blue>Cooldown:</color> {abilityIngredient.baseStats.cooldown}\n";

        //}
        //else if (ingredient.GetType() == typeof(FlavorIngredient))
        //{
        //    FlavorIngredient flavorIngredient = ingredient as FlavorIngredient;
        //    bodyText.text = "<color=yellow>Flavor Ingredient</color>\n\n";

        //    foreach (InflictionFlavor inflictionFlavor in flavorIngredient.inflictionFlavors)
        //    {
        //        string color = ColorUtility.ToHtmlStringRGB(FlavorIngredient.inflictionColorMapping[inflictionFlavor.inflictionType]);
        //        //switch (inflictionFlavor.inflictionType)
        //        //{
        //        //    case FlavorIngredient.InflictionFlavor.InflictionType.SPICY_Burn:
        //        //        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
        //        //        {
        //        //            if (inflictionFlavor.amount > 0){
        //        //                bodyText.text += $"<color=#{color}>Spicy:</color> + {inflictionFlavor.amount}\n";
        //        //            }
        //        //        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
        //        //        {
        //        //            if (inflictionFlavor.amount != 0 && inflictionFlavor.amount != 1){
        //        //                bodyText.text += $"<color=#{color}>Spicy:</color> x {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        break;
        //        //    case FlavorIngredient.InflictionFlavor.InflictionType.FROSTY_Freeze:
        //        //        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
        //        //        {
        //        //            if (inflictionFlavor.amount > 0){
        //        //                bodyText.text += $"<color=#{color}>Frosty:</color> + {inflictionFlavor.amount}\n";
        //        //            }
        //        //        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
        //        //        {
        //        //            if (inflictionFlavor.amount != 0 && inflictionFlavor.amount != 1){
        //        //                bodyText.text += $"<color=#{color}>Frosty:</color> x {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        break;
        //        //    case FlavorIngredient.InflictionFlavor.InflictionType.HEARTY_Health:
        //        //        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
        //        //        {
        //        //            if (inflictionFlavor.amount > 0){
        //        //                bodyText.text += $"<color=#{color}>Hearty:</color> + {inflictionFlavor.amount}\n";
        //        //            }
        //        //        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
        //        //        {
        //        //            if (inflictionFlavor.amount != 0 && inflictionFlavor.amount != 1){
        //        //                bodyText.text += $"<color=#{color}>Hearty:</color> x {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        break;
        //        //    case FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage:
        //        //        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
        //        //        {
        //        //            if (inflictionFlavor.amount > 0){
        //        //                bodyText.text += $"<color=#{color}>Spiky:</color> + {inflictionFlavor.amount}\n";
        //        //            }
        //        //        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
        //        //        {
        //        //            if (inflictionFlavor.amount != 0 && inflictionFlavor.amount != 1){
        //        //                bodyText.text += $"<color=#{color}>Spiky:</color> x {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        break;
        //        //    case FlavorIngredient.InflictionFlavor.InflictionType.GREASY_Knockback:
        //        //        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
        //        //        {
        //        //            if (inflictionFlavor.amount > 0){
        //        //                bodyText.text += $"<color=#{color}>Greasy:</color> + {inflictionFlavor.amount}\n";
        //        //            }
        //        //        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
        //        //        {
        //        //            if (inflictionFlavor.amount != 0 && inflictionFlavor.amount != 1){
        //        //                bodyText.text += $"<color=#{color}>Greasy:</color> x {inflictionFlavor.amount}\n";
        //        //            }
        //        //        }
        //        //        break;
        //        //}
        //    }

            foreach (BuffFlavor buffFlavor in flavorIngredient.buffFlavors)
            {
                string color = ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[buffFlavor.buffType]);
            //    switch (buffFlavor.buffType)
            //    {
            //        case FlavorIngredient.BuffFlavor.BuffType.BITTER_Size:
            //            if (buffFlavor.operation == BuffFlavor.Operation.Add)
            //            {
            //                if (buffFlavor.amount > 0){
            //                    bodyText.text += $"<color=#{color}>Bitter:</color> + {buffFlavor.amount}\n";
            //                }
            //            }
            //            else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
            //            {
            //                if (buffFlavor.amount != 0 && buffFlavor.amount != 1){
            //                    bodyText.text += $"<color=#{color}>Bitter:</color> x {buffFlavor.amount}\n";
            //                }
            //            }
            //            break;
            //        case FlavorIngredient.BuffFlavor.BuffType.SALTY_Crit:
            //            if (buffFlavor.operation == BuffFlavor.Operation.Add)
            //            {
            //                if (buffFlavor.amount > 0){
            //                    bodyText.text += $"<color=#{color}>Salty:</color> + {buffFlavor.amount}\n";
            //                }
            //            }
            //            else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
            //            {
            //                if (buffFlavor.amount != 0 && buffFlavor.amount != 1){
            //                    bodyText.text += $"<color=#{color}>Salty:</color> x {buffFlavor.amount}\n";
            //                }
            //            }
            //            break;
            //        case FlavorIngredient.BuffFlavor.BuffType.SOUR_Duration:
            //            if (buffFlavor.operation == BuffFlavor.Operation.Add)
            //            {
            //                if (buffFlavor.amount > 0){
            //                    bodyText.text += $"<color=#{color}>Sour:</color> + {buffFlavor.amount}\n";
            //                }
            //            }
            //            else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
            //            {
            //                if (buffFlavor.amount != 0 && buffFlavor.amount != 1){
            //                    bodyText.text += $"<color=#{color}>Sour:</color> x {buffFlavor.amount}\n";
            //                }
            //            }
            //            break;
            //        case FlavorIngredient.BuffFlavor.BuffType.UMAMI_Vampirism:
            //            if (buffFlavor.operation == BuffFlavor.Operation.Add)
            //            {
            //                if (buffFlavor.amount > 0){
            //                    bodyText.text += $"<color=#{color}>Cooldown:</color> + {buffFlavor.amount}\n";
            //                }
            //            }
            //            else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
            //            {
            //                if (buffFlavor.amount != 0 && buffFlavor.amount != 1){
            //                    bodyText.text += $"<color=#{color}>Cooldown:</color> x {buffFlavor.amount}\n";
            //                }
            //            }
            //            break;
            //        case FlavorIngredient.BuffFlavor.BuffType.SWEET_Speed:
            //            if (buffFlavor.operation == BuffFlavor.Operation.Add)
            //            {
            //                if (buffFlavor.amount > 0){
            //                    bodyText.text += $"<color=#{color}>Sweet:</color> + {buffFlavor.amount}\n";
            //                }
            //            }
            //            else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
            //            {
            //                if (buffFlavor.amount != 0 && buffFlavor.amount != 1){
            //                    bodyText.text += $"<color=#{color}>Sweet:</color> x {buffFlavor.amount}\n";
            //                }
            //            }
            //            break;
            //    }
            }
        }
        else
        {
            Debug.LogError("Invalid Ingredient Type");
        }*/

    }

    public void OnDestroy()
    {
        Transform itemStatsScreenTransform = this.transform.Find("IngredientStats");
        if (itemStatsScreenTransform != null)
        {
            CookingManager.Singleton.HideItemStats();
            GameObject CookingCanvas = CookingManager.Singleton.CookingCanvas;
            itemStatsScreenTransform.SetParent(CookingManager.Singleton.CookingCanvas.transform);
        } 
    }

    public void OnDisable()
    {
        Transform itemStatsScreenTransform = this.transform.Find("IngredientStats");
        if (itemStatsScreenTransform != null)
        {
            CookingManager.Singleton.HideItemStats();
            GameObject CookingCanvas = CookingManager.Singleton.CookingCanvas;
            itemStatsScreenTransform.SetParent(CookingManager.Singleton.CookingCanvas.transform);
        }
    }
}
