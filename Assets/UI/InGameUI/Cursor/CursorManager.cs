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

    // For selecting bowls
    [Header("Values")]
    [SerializeField] Color VALID_PLACEMENT_COLOR;
    [SerializeField] Color INVALID_PLACEMENT_COLOR;
    [SerializeField] float MOUSE_DISTANCE_FOR_TAP;
    [SerializeField] float TIME_FOR_TAP = .25f;

    internal int selectedSlot = -1;
    public bool IsHoldingSomething { get => currentCollectableReference != null; }
    internal Collectable currentCollectableReference;



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

    public void PickupCollectable(Collectable collectable)
    {
        ChangeToCollectableSprite(collectable.collectableUI._SpriteReference);
        currentCollectableReference = collectable;
        if (IWhileDraggingCollectable != null) StopCoroutine(IWhileDraggingCollectable);
        StartCoroutine(IWhileDraggingCollectable = WhileDraggingCollectable());
    }

    IEnumerator IWhileDraggingCollectable;
    bool validCollectablePlacement;
    IEnumerator WhileDraggingCollectable()
    {
        while (currentCollectableReference != null)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, currentCollectableReference.collectableUI.ColliderRadius);
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

    private void MouseUp(InputAction.CallbackContext ctx)
    {
        if (currentCollectableReference == null) return;

        if (IWhileDraggingCollectable != null) StopCoroutine(IWhileDraggingCollectable);
        IWhileDraggingCollectable = null;

        if (CookingScreen.Singleton.IsCooking && Vector2.Distance(Input.mousePosition, mouseDownPosition) < MOUSE_DISTANCE_FOR_TAP
            && (Time.time - mouseDownTime) < TIME_FOR_TAP)
        {
            lastCursorInteract.Tap();
            if (currentCollectableReference == null)
            {
                ChangeToCrosshairSprite();
                return;
            }
        }

        if (!validCollectablePlacement) // not valid or has been dropped
        {
            lastCursorInteract.ReturnIngredientHereFromCursor();
            currentCollectableReference = null;
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

        ChangeToCrosshairSprite();
    }

    void ChangeToCrosshairSprite()
    {
        _CursorImage.sprite = CrosshairSprite;
        _CursorImage.rectTransform.sizeDelta = new Vector2(CrosshairSprite.texture.width, CrosshairSprite.texture.height);
        _CursorImage.color = Color.white;
        _CursorImage.rectTransform.localScale = Vector3.one;
    }

    void ChangeToCollectableSprite(Sprite sprite)
    {
        _CursorImage.sprite = sprite;
        _CursorImage.rectTransform.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
        _CursorImage.rectTransform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
    }

    void Update()
    {
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }

    public void OnExitCooking()
    {
        if (currentCollectableReference != null) currentCollectableReference.collectableUI.ReturnIngredientHereFromCursor();
        currentCollectableReference = null;
        ChangeToCrosshairSprite();
    }

    public void DropCollectable()
    {
        ChangeToCrosshairSprite();
        currentCollectableReference = null;
    }

    public void TryDropCollectable(Collectable collectable)
    {
        if (collectable == currentCollectableReference) DropCollectable();
    }
}
