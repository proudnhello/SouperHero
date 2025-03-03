using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using static FlavorIngredient;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    public GameObject draggableItem;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public string ingredientType;
    public bool needsBasketDrop;
    public bool needsWorldDrop;
    public Transform pseudoParent;

    [Header("Do Not Edit, Ingredient is Set In CookingUI's Enable()")]
    public Ingredient ingredient = null;
    public void OnBeginDrag(PointerEventData eventData)
    {
        CursorManager.Singleton.cookingCursor.switchCursorImageTo(transform.parent.gameObject.GetComponent<Collectable>(), image);
        parentAfterDrag = pseudoParent;

        //GameObject content = GameObject.Find("Content");
        //transform.parent.transform.SetParent(content.transform, true);
        //transform.parent.transform.SetParent(transform.parent.transform.root, true);
        //transform.parent.transform.SetAsLastSibling();

        //Rigidbody2D rb = GetComponent<Rigidbody2D>();
        //rb.simulated = false;
        //rb.velocity = Vector3.zero;
        //GetComponent<Collider2D>().enabled = false;

        //CookingManager.Singleton.enableWorldDrop();

        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //RectTransformUtility.ScreenPointToWorldPointInRectangle(
        //transform.parent.transform.parent as RectTransform,
        //Input.mousePosition,
        //eventData.pressEventCamera,
        //out Vector3 worldPos
        //);

        //transform.position = worldPos;
    }

    public void resetParent()
    {
        if (!parentAfterDrag.gameObject.CompareTag("BasketDrop"))
        {
            image.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            image.raycastTarget = false;

            if (parentAfterDrag.gameObject.CompareTag("CookingSlot"))
            {
                //if(parentAfterDrag == pseudoParent)
                //{
                //    CursorManager.Singleton.cookingCursor.removeCursorImage();
                //    return;
                //}
                parentAfterDrag.gameObject.GetComponent<CookingSlot>().ingredientReference = CursorManager.Singleton.cookingCursor.currentCollectableReference;
                parentAfterDrag.gameObject.GetComponent<CookingSlot>().updateIngredientImage(image);
            }
        }
        else
        {

        }
        pseudoParent = parentAfterDrag;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        resetParent();
        CursorManager.Singleton.cookingCursor.removeCursorImage();
        //Rigidbody2D rb = GetComponent<Rigidbody2D>();

        //CookingManager.Singleton.disableWorldDrop();

        //if (needsWorldDrop)
        //{
        //    GameObject content = GameObject.Find("Content");
        //    transform.parent.transform.SetParent(content.transform, true);

        //    transform.parent.transform.localPosition = Vector3.zero;
        //    GetComponent<RectTransform>().localPosition = Vector3.zero;
        //    transform.localScale = Vector3.one;

        //    rb.isKinematic = false;
        //    rb.simulated = true;
        //    GetComponent<Collider2D>().enabled = true;
        //    needsWorldDrop = false;
        //    image.raycastTarget = true;
        //    return;
        //}

        //// set the parent to the parent after drag
        //transform.parent.transform.SetParent(parentAfterDrag, true);

        //transform.parent.transform.localPosition = Vector3.zero;
        //transform.parent.transform.localScale = Vector3.one;

        //GetComponent<RectTransform>().localPosition = Vector3.zero;
        //GetComponent<RectTransform>().localRotation = Quaternion.identity;

        //if (needsBasketDrop)
        //{
        //    needsBasketDrop = false;
        //    rb.simulated = true;
        //    GetComponent<Collider2D>().enabled = true;
        //    BasketUI.Singleton.AddIngredient(transform.parent.gameObject.GetComponent<Collectable>(), false);
        //}

        // return raycast to true
        //image.raycastTarget = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ingredient = transform.parent.GetComponent<Collectable>().ingredient;
        //Debug.Log($"Mouse entered UI element {ingredient.ingredientName}!");

        CookingManager.Singleton.DisplayItemStats();
        GameObject itemStatsScreen = CookingManager.Singleton.itemStatsScreen;

        Transform background = itemStatsScreen.transform.Find("Background");
        Transform header = background.transform.Find("Header");
        Transform body = background.transform.Find("Body");

        // move the screen
        itemStatsScreen.transform.SetParent(this.transform);
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 actualSize = new Vector2(rt.rect.width * rt.lossyScale.x, rt.rect.height * rt.lossyScale.y);
        itemStatsScreen.transform.position = new Vector2(this.transform.position.x, this.transform.position.y) + new Vector2(actualSize.x / 2, -actualSize.y / 2);
        //Debug.Log(itemStatsScreen.transform.position);
        //Debug.Log(rt.rect.size);
        //Debug.Log(this.transform.position);

        // bring to the front
        itemStatsScreen.transform.SetParent(transform.root);
        itemStatsScreen.transform.SetAsLastSibling();


        // set text
        TextMeshProUGUI headerText = header.GetComponent<TextMeshProUGUI>();
        headerText.text = ingredient.IngredientName;

        TextMeshProUGUI bodyText = body.GetComponent<TextMeshProUGUI>();

        if (ingredient.GetType() == typeof(AbilityIngredient))
        {
            AbilityIngredient abilityIngredient = ingredient as AbilityIngredient;
            bodyText.text = $"<color=purple>Ability Ingredient</color>\nType: {abilityIngredient.ability._abilityName}\n\n";

            foreach (InflictionFlavor inflictionFlavor in abilityIngredient.inherentInflictionFlavors)
            {
                string color = ColorUtility.ToHtmlStringRGB(FlavorIngredient.inflictionColorMapping[inflictionFlavor.inflictionType]);
                switch (inflictionFlavor.inflictionType)
                {
                    case FlavorIngredient.InflictionFlavor.InflictionType.SPICY_Burn:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            if (inflictionFlavor.amount > 0) {
                                bodyText.text += $"<color=#{color}>Spicy:</color> + {inflictionFlavor.amount}\n";
                            }
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            if (inflictionFlavor.amount > 1) {
                                bodyText.text += $"<color=#{color}>Spicy:</color> x {inflictionFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.FROSTY_Freeze:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            if (inflictionFlavor.amount > 0) {
                                bodyText.text += $"<color=#{color}>Frosty:</color> + {inflictionFlavor.amount}\n";
                            }
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            if (inflictionFlavor.amount > 1) {
                                bodyText.text += $"<color=#{color}>Frosty:</color> x {inflictionFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.HEARTY_Health:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            if (inflictionFlavor.amount > 0) {
                                bodyText.text += $"<color=#{color}>Hearty:</color> + {inflictionFlavor.amount}\n";
                            }
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            if (inflictionFlavor.amount > 1) {
                                bodyText.text += $"<color=#{color}>Hearty:</color> x {inflictionFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            if (inflictionFlavor.amount > 0) {
                                bodyText.text += $"<color=#{color}>Spiky:</color> + {inflictionFlavor.amount}\n";
                            }
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            if (inflictionFlavor.amount > 1) {
                                bodyText.text += $"<color=#{color}>Spiky:</color> x {inflictionFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.GREASY_Knockback:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            if (inflictionFlavor.amount > 0) {
                                bodyText.text += $"<color=#{color}>Greasy:</color> + {inflictionFlavor.amount}\n";
                            }
                        }
                        else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            if (inflictionFlavor.amount > 1) {
                                bodyText.text += $"<color=#{color}>Greasy:</color> x {inflictionFlavor.amount}\n";
                            }
                        }
                        break;
                }
            }

            bodyText.text += "\nBase Stats:\n";
            if (abilityIngredient.baseStats.duration > 0)
                bodyText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.SOUR_Duration])}>Sour (Duration):</color> {abilityIngredient.baseStats.duration}\n";
            if (abilityIngredient.baseStats.size > 0)
                bodyText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.BITTER_Size])}>Bitter (Size):</color> {abilityIngredient.baseStats.size}\n";
            if (abilityIngredient.baseStats.crit > 0)
                bodyText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.SALTY_CriticalStrike])}>Salty (Critical Strike):</color> {abilityIngredient.baseStats.crit}\n";
            if (abilityIngredient.baseStats.speed > 0)
                bodyText.text += $"<color=#{ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[FlavorIngredient.BuffFlavor.BuffType.SWEET_Speed])}>Sweet (Speed):</color> {abilityIngredient.baseStats.speed}\n";
            bodyText.text += $"<color=blue>Cooldown:</color> {abilityIngredient.baseStats.cooldown}\n";

        }
        else if (ingredient.GetType() == typeof(FlavorIngredient))
        {
            FlavorIngredient flavorIngredient = ingredient as FlavorIngredient;
            bodyText.text = "<color=yellow>Flavor Ingredient</color>\n\n";

            foreach (InflictionFlavor inflictionFlavor in flavorIngredient.inflictionFlavors)
            {
                string color = ColorUtility.ToHtmlStringRGB(FlavorIngredient.inflictionColorMapping[inflictionFlavor.inflictionType]);
                switch (inflictionFlavor.inflictionType)
                {
                    case FlavorIngredient.InflictionFlavor.InflictionType.SPICY_Burn:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            if (inflictionFlavor.amount > 0){
                                bodyText.text += $"<color=#{color}>Spicy:</color> + {inflictionFlavor.amount}\n";
                            }
                        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            if (inflictionFlavor.amount != 0 && inflictionFlavor.amount != 1){
                                bodyText.text += $"<color=#{color}>Spicy:</color> x {inflictionFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.FROSTY_Freeze:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            if (inflictionFlavor.amount > 0){
                                bodyText.text += $"<color=#{color}>Frosty:</color> + {inflictionFlavor.amount}\n";
                            }
                        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            if (inflictionFlavor.amount != 0 && inflictionFlavor.amount != 1){
                                bodyText.text += $"<color=#{color}>Frosty:</color> x {inflictionFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.HEARTY_Health:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            if (inflictionFlavor.amount > 0){
                                bodyText.text += $"<color=#{color}>Hearty:</color> + {inflictionFlavor.amount}\n";
                            }
                        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            if (inflictionFlavor.amount != 0 && inflictionFlavor.amount != 1){
                                bodyText.text += $"<color=#{color}>Hearty:</color> x {inflictionFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.SPIKY_Damage:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            if (inflictionFlavor.amount > 0){
                                bodyText.text += $"<color=#{color}>Spiky:</color> + {inflictionFlavor.amount}\n";
                            }
                        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            if (inflictionFlavor.amount != 0 && inflictionFlavor.amount != 1){
                                bodyText.text += $"<color=#{color}>Spiky:</color> x {inflictionFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.InflictionFlavor.InflictionType.GREASY_Knockback:
                        if (inflictionFlavor.operation == InflictionFlavor.Operation.Add)
                        {
                            if (inflictionFlavor.amount > 0){
                                bodyText.text += $"<color=#{color}>Greasy:</color> + {inflictionFlavor.amount}\n";
                            }
                        } else if (inflictionFlavor.operation == InflictionFlavor.Operation.Multiply)
                        {
                            if (inflictionFlavor.amount != 0 && inflictionFlavor.amount != 1){
                                bodyText.text += $"<color=#{color}>Greasy:</color> x {inflictionFlavor.amount}\n";
                            }
                        }
                        break;
                }
            }

            foreach (BuffFlavor buffFlavor in flavorIngredient.buffFlavors)
            {
                string color = ColorUtility.ToHtmlStringRGB(FlavorIngredient.buffColorMapping[buffFlavor.buffType]);
                switch (buffFlavor.buffType)
                {
                    case FlavorIngredient.BuffFlavor.BuffType.BITTER_Size:
                        if (buffFlavor.operation == BuffFlavor.Operation.Add)
                        {
                            if (buffFlavor.amount > 0){
                                bodyText.text += $"<color=#{color}>Bitter:</color> + {buffFlavor.amount}\n";
                            }
                        }
                        else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
                        {
                            if (buffFlavor.amount != 0 && buffFlavor.amount != 1){
                                bodyText.text += $"<color=#{color}>Bitter:</color> x {buffFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.BuffFlavor.BuffType.SALTY_CriticalStrike:
                        if (buffFlavor.operation == BuffFlavor.Operation.Add)
                        {
                            if (buffFlavor.amount > 0){
                                bodyText.text += $"<color=#{color}>Salty:</color> + {buffFlavor.amount}\n";
                            }
                        }
                        else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
                        {
                            if (buffFlavor.amount != 0 && buffFlavor.amount != 1){
                                bodyText.text += $"<color=#{color}>Salty:</color> x {buffFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.BuffFlavor.BuffType.SOUR_Duration:
                        if (buffFlavor.operation == BuffFlavor.Operation.Add)
                        {
                            if (buffFlavor.amount > 0){
                                bodyText.text += $"<color=#{color}>Sour:</color> + {buffFlavor.amount}\n";
                            }
                        }
                        else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
                        {
                            if (buffFlavor.amount != 0 && buffFlavor.amount != 1){
                                bodyText.text += $"<color=#{color}>Sour:</color> x {buffFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.BuffFlavor.BuffType.UMAMI_Vampirism:
                        if (buffFlavor.operation == BuffFlavor.Operation.Add)
                        {
                            if (buffFlavor.amount > 0){
                                bodyText.text += $"<color=#{color}>Cooldown:</color> + {buffFlavor.amount}\n";
                            }
                        }
                        else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
                        {
                            if (buffFlavor.amount != 0 && buffFlavor.amount != 1){
                                bodyText.text += $"<color=#{color}>Cooldown:</color> x {buffFlavor.amount}\n";
                            }
                        }
                        break;
                    case FlavorIngredient.BuffFlavor.BuffType.SWEET_Speed:
                        if (buffFlavor.operation == BuffFlavor.Operation.Add)
                        {
                            if (buffFlavor.amount > 0){
                                bodyText.text += $"<color=#{color}>Sweet:</color> + {buffFlavor.amount}\n";
                            }
                        }
                        else if (buffFlavor.operation == BuffFlavor.Operation.Multiply)
                        {
                            if (buffFlavor.amount != 0 && buffFlavor.amount != 1){
                                bodyText.text += $"<color=#{color}>Sweet:</color> x {buffFlavor.amount}\n";
                            }
                        }
                        break;
                }
            }
        }
        else
        {
            Debug.LogError("Invalid Ingredient Type");
        }

    }

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    //Debug.Log($"Mouse exited UI element {ingredient.ingredientName}!");
    //    CookingManager.Singleton.HideItemStats();

    //    GameObject itemStatsScreen = CookingManager.Singleton.itemStatsScreen;
    //    GameObject CookingCanvas = CookingManager.Singleton.CookingCanvas;
    //    itemStatsScreen.transform.SetParent(CookingManager.Singleton.CookingCanvas.transform);

    //}

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
