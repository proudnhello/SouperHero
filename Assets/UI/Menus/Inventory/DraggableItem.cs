using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using static FlavorIngredient;
using System.Collections.Generic;
using UnityEditor;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Image image;
    public Transform parentAfterDrag;
    [HideInInspector] public string ingredientType;
    public Transform previousParent;

    [Header("Do Not Edit, Ingredient is Set In CookingUI's Enable()")]
    public Ingredient ingredient = null;
    public bool isDragging = false;
    public bool droppedOn = false;
    public bool worldDropped = false;
    public bool validPlacement = true;
    [SerializeField] private float ingredientRadius;

    Collectable collectable;
    public static float alphaOnPickup = .25f;

    private void Awake()
    {
        collectable = transform.parent.gameObject.GetComponent<Collectable>();
        ingredientRadius = 0.5f;
    }

    public void OnBeginDrag(PointerEventData eventData)         
    {
        CursorManager.Singleton.cursorObject.SetActive(false);
        CursorManager.Singleton.ShowCookingCursor();
        CursorManager.Singleton.isDragging = true;

        CursorManager.Singleton.cookingCursor.switchCursorImageTo(collectable, image);
        Encyclopedia.Singleton.PullUpEntry(collectable.ingredient);

        parentAfterDrag = previousParent;

        image.color = new Color(1.0f, 1.0f, 1.0f, alphaOnPickup);
        image.raycastTarget = false;

        isDragging = true;
        worldDropped = false;

        //CookingManager.Singleton.enableWorldDrop();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Encyclopedia.Singleton.PullUpEntry(collectable.ingredient);

        // Checking ingredient placement position
        // PointerEventData pointerData = new PointerEventData(EventSystem.current)
        // {
        //     position = Input.mousePosition
        // };
        // List<RaycastResult> results = new List<RaycastResult>();
        // EventSystem.current.RaycastAll(pointerData, results);

        // if(results.Count == 0)
        // {   // Nothing in raycast which means no collisions
        //     CursorManager.Singleton.cookingCursor.resetImageColor();
        //     validPlacement = true;
        // }

        // Checking for the basket colliders and ingredients using physics because UI raycasts suck I guess
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPoint, ingredientRadius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Ingredient") || collider.CompareTag("BasketWall"))
            {
                CursorManager.Singleton.cookingCursor.changeToInvalidColor();
                validPlacement = false;
                return;
            }
        }
        CursorManager.Singleton.cookingCursor.resetImageColor();
        validPlacement = true;
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
                    }
                }
            }
            else if (!parentAfterDrag.gameObject.CompareTag("WorldDrop"))
            {
                image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                BasketUI.Singleton.AddIngredient(CursorManager.Singleton.cookingCursor.currentCollectableReference, false);
            }
        }
        previousParent = parentAfterDrag;
        return true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        CookingSlot dropTarget = null;

        foreach (RaycastResult result in results)
        {
            dropTarget = result.gameObject.GetComponent<CookingSlot>();
            if (dropTarget != null)
            {
                dropTarget.OnDrop(eventData);
                break;
            }
        }


        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Bounds b = GetComponent<Collider2D>().bounds;

        if (dropTarget == null && !isDragging && !droppedOn)
        {
            image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            worldDropped = true;

            previousParent = collectable.gameObject.transform;

            CursorManager.Singleton.cookingCursor.removeCursorImage();
            CursorManager.Singleton.cursorObject.SetActive(true);

            if (BasketUI.Singleton.basketChange.bounds.Intersects(new Bounds(new Vector3(position.x, position.y, transform.root.position.z), b.size)))
            {
                image.raycastTarget = true;
            }
        } else if (CookingManager.Singleton.IsCooking() && isDragging)
        {   // When in the cooking UI
            image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            if (droppedOn)  // dropped on an already occupied slot
            {
                image.raycastTarget = true;
                CursorManager.Singleton.cookingCursor.removeCursorImage();
                CursorManager.Singleton.cursorObject.SetActive(true);
            }

            if(validPlacement)
            {   // Places into new position only if area is clear
                transform.position = new Vector3(position.x, position.y, transform.root.position.z);
            }
            worldDropped = true;

            previousParent = collectable.gameObject.transform;

            CursorManager.Singleton.cookingCursor.removeCursorImage();
            CursorManager.Singleton.cursorObject.SetActive(true);

            if (BasketUI.Singleton.basketChange.bounds.Intersects(new Bounds(new Vector3(position.x, position.y, transform.root.position.z), b.size)))
            {
                image.raycastTarget = true;
            }
        } else if (!CookingManager.Singleton.IsCooking() && dropTarget == null && isDragging)
        {   // Dropped when not in cooking UI
            image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            if(validPlacement)
            {   // Places into new position only if area is clear
                transform.position = new Vector3(position.x, position.y, transform.root.position.z);
            }
            worldDropped = true;
            isDragging = false;

            CursorManager.Singleton.cookingCursor.removeCursorImage();
            CursorManager.Singleton.cursorObject.SetActive(true);
            CursorManager.Singleton.HideCookingCursor();

            if (BasketUI.Singleton.basketChange.bounds.Intersects(new Bounds(new Vector3(position.x, position.y, transform.root.position.z), b.size)))
            {
                image.raycastTarget = true;
            }
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("WorldDrop"))
        {
            if(previousParent.TryGetComponent<CookingSlot>(out CookingSlot previousSlot))
            {
                previousSlot.ingredientReference = null;
                previousSlot.faceImage.sprite = null;
                previousSlot.usesText.text = "";
                CookingManager.Singleton.RemoveIngredient(CursorManager.Singleton.cookingCursor.currentCollectableReference);
                CookingManager.Singleton.CookingSlotSetTransparent(previousSlot);
            }
            Collectable collectable = transform.parent.GetComponent<Collectable>();

            collectable.gameObject.transform.SetParent(null);
            collectable.gameObject.transform.localScale = Vector3.one;
            collectable.Spawn(PlayerEntityManager.Singleton.gameObject.transform.position);
            collectable.collectableObj.gameObject.SetActive(true);
            if (!CookingManager.Singleton.IsCooking())
            {
                OnEndDrag(null);
            }
            collectable.collectableUI.gameObject.SetActive(false);

            PlayerInventory.Singleton.RemoveIngredientCollectable(collectable, false);
            collectable.collectableObj.SetInteractable(true);
            collectable.collectableObj.SetHighlighted(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Encyclopedia.Singleton.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ingredient = transform.parent.GetComponent<Collectable>().ingredient;
        Encyclopedia.Singleton.PullUpEntry(collectable.ingredient);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ingredient = transform.parent.GetComponent<Collectable>().ingredient;
        Encyclopedia.Singleton.PullUpEntry(collectable.ingredient);
        
        // I wanna give a huge shoutout to the person whoever wrote this 300 line function, only for it all to be thrown away.
        // Giving credits to Cursor for helping edit this 300 line function when I wanted to put colors in
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
