using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Singleton { get; private set; }

    [SerializeField] Sprite CrosshairSprite;
    Image _CursorImage;

    [Header("Values")]
    [SerializeField] Color VALID_PLACEMENT_COLOR;
    [SerializeField] Color INVALID_PLACEMENT_COLOR;
    [SerializeField] float MOUSE_DISTANCE_FOR_TAP;
    [SerializeField] float TIME_FOR_TAP = .25f;
    [SerializeField] LayerMask BOWL_SLOT_LAYER;
    public TooltipCursorTrigger TooltipTrigger;

    public bool IsHoldingSomething { get => currentCollectableReference != null || currentBowlReference != null; }
    internal Collectable currentCollectableReference;
    internal ISoupBowl currentBowlReference;

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
        _CursorImage = GetComponent<Image>();
        _CursorImage.sprite = CrosshairSprite;
    }

    private void Start()
    {
        CookingScreen.ExitCookingScreen += OnExitCooking;
        PlayerEntityManager.Singleton.input.UI.Click.started += MouseDown;
        PlayerEntityManager.Singleton.input.UI.Click.canceled += MouseUp;
    }

    private void OnDisable()
    {
        CookingScreen.ExitCookingScreen -= OnExitCooking;
        PlayerEntityManager.Singleton.input.UI.Click.started -= MouseDown;
        PlayerEntityManager.Singleton.input.UI.Click.canceled -= MouseUp;
    }

    Vector2 mouseDownPosition;
    float mouseDownTime;
    ICursorInteractable lastCursorInteract;
    private void MouseDown(InputAction.CallbackContext ctx)
    {
        mouseDownPosition = Input.mousePosition;
        mouseDownTime = Time.time;

        PointerEventData m_PointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(m_PointerEventData, hits);
        foreach (var hit in hits)
        {
            if (hit.gameObject.TryGetComponent(out ICursorInteractable interactable))
            {
                lastCursorInteract = interactable;
                interactable.MouseDownOn();
                return;
            }
        }
    }
    private void MouseUp(InputAction.CallbackContext ctx)
    {
        if (!IsHoldingSomething) return;

        if (IWhileDragging != null) StopCoroutine(IWhileDragging);
        IWhileDragging = null;

        if (SoupInventoryUI.Singleton.IsOpen && Vector2.Distance(Input.mousePosition, mouseDownPosition) < MOUSE_DISTANCE_FOR_TAP
            && (Time.time - mouseDownTime) < TIME_FOR_TAP)
        {
            lastCursorInteract.Tap();
            if (!IsHoldingSomething)
            {
                ChangeToCrosshairSprite();
                return;
            }
        }

        if (!validCollectablePlacement) // not valid or has been dropped
        {
            lastCursorInteract.ReturnItemHereFromCursor();
            currentCollectableReference = null;
            currentBowlReference = null;
            ChangeToCrosshairSprite();
            return;
        }
       
        PointerEventData m_PointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(m_PointerEventData, hits);

        foreach (var hit in hits)
        {
            if (hit.gameObject.TryGetComponent(out ICursorInteractable interactable))
            {
                interactable.MouseUpOn();
                break;
            }
        }

        if (currentCollectableReference != null) // if CursorInteractable doesn't modify cursor, then just drop it
        {
            if (lastCursorInteract is IngredientCookingSlot slot) slot.RemoveIngredient();
            currentCollectableReference.collectableUI.DropItemOnScreen(transform.position);
            currentCollectableReference = null;
        }
        else if (currentBowlReference != null)
        {
            lastCursorInteract.ReturnItemHereFromCursor();
            currentBowlReference = null;
        }

        ChangeToCrosshairSprite();
    }


    IEnumerator IWhileDragging;
    bool validCollectablePlacement;

    #region Collectable
    public void PickupCollectable(Collectable collectable)
    {
        ChangeToCollectableSprite(collectable.collectableUI._SpriteReference);
        currentCollectableReference = collectable;
        if (IWhileDragging != null) StopCoroutine(IWhileDragging);
        StartCoroutine(IWhileDragging = WhileDraggingCollectable());
    }
    IEnumerator WhileDraggingCollectable()
    {
        float ColliderSize(float multiplier)
        {
            return (Camera.main.orthographicSize * .065f - .1f) * multiplier; // by default for orthographic size of 10 = .55 radius
        }
        while (currentCollectableReference != null)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ColliderSize(currentCollectableReference.collectableUI.ColliderRadiusMult));
            validCollectablePlacement = true;
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Ingredient") || collider.CompareTag("BasketWall"))
                {
                    validCollectablePlacement = false;
                    break;
                }
                if (collider.gameObject.TryGetComponent(out IngredientCookingSlot slot))
                {
                    if (slot.ingredientReference != null || !CookingScreen.Singleton.AtCookingScreen)
                    {
                        validCollectablePlacement = false;
                        break;
                    }
                }
            }
            _CursorImage.color = validCollectablePlacement ? VALID_PLACEMENT_COLOR : INVALID_PLACEMENT_COLOR;
            yield return null;
        }
    }

    public void DropCollectable()
    {
        ChangeToCrosshairSprite();
        currentCollectableReference = null;
    }

    public void DropBowl()
    {
        ChangeToCrosshairSprite();
        currentBowlReference = null;
    }

    public void TryDropCollectable(Collectable collectable)
    {
        if (collectable == currentCollectableReference) DropCollectable();
    }
    void ChangeToCollectableSprite(Sprite sprite)
    {
        _CursorImage.sprite = sprite;
        _CursorImage.rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
        _CursorImage.rectTransform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
    }
    public void ManuallyReturnItemFromCursor()
    {
        lastCursorInteract.ReturnItemHereFromCursor();
        currentCollectableReference = null;
        currentBowlReference = null;
    }
    #endregion

    #region Bowl
    public void ExitSoupInventory()
    {
        if (currentBowlReference != null)
        {
            lastCursorInteract.ReturnItemHereFromCursor();
            currentBowlReference = null;
            if (IWhileDragging != null) StopCoroutine(IWhileDragging);
            ChangeToCrosshairSprite();
        }
    }
    public void PickupBowl(ISoupBowl bowl)
    {
        if (bowl is FinishedSoup soup)
        {
            ChangeToBowlSprite(soup.soupBase.finishedSprite);
        }
        else if (bowl is SoupBase soupB)
        {
            ChangeToBowlSprite(soupB.baseSprite);
        }
        currentBowlReference = bowl;
        if (IWhileDragging != null) StopCoroutine(IWhileDragging);
        StartCoroutine(IWhileDragging = WhileDraggingBowl());
    }

    IEnumerator WhileDraggingBowl()
    {
        while (currentBowlReference != null)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.zero, BOWL_SLOT_LAYER);
            validCollectablePlacement = false;
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("BowlSlot") || 
                    (hit.collider.CompareTag("CookingBowlSlot") && currentBowlReference is not FinishedSoup))
                {
                    validCollectablePlacement = true;
                    break;
                }
            }
            _CursorImage.color = validCollectablePlacement ? VALID_PLACEMENT_COLOR : INVALID_PLACEMENT_COLOR;
            yield return null;
        }
    }

    void ChangeToBowlSprite(Sprite sprite)
    {
        _CursorImage.sprite = sprite;
        _CursorImage.rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
        _CursorImage.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
    #endregion
    void ChangeToCrosshairSprite()
    {
        _CursorImage.sprite = CrosshairSprite;
        _CursorImage.rectTransform.sizeDelta = new Vector2(CrosshairSprite.texture.width, CrosshairSprite.texture.height);
        _CursorImage.color = Color.white;
        _CursorImage.rectTransform.localScale = Vector3.one;
    }

    void Update()
    {
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }

    public void OnExitCooking()
    {
        if (currentCollectableReference != null) currentCollectableReference.collectableUI.ReturnItemHereFromCursor();
        currentCollectableReference = null;
        ChangeToCrosshairSprite();
    }


}
