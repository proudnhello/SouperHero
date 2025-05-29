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
    ICursorInteractable lastCursorInteract;
    private void MouseDown(InputAction.CallbackContext ctx)
    {
        mouseDownPosition = transform.localPosition;

        PointerEventData m_PointerEventData = new PointerEventData(EventSystem.current) { position = transform.localPosition};
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(m_PointerEventData, hits);
        if (hits.Count > 0)
        {
            if (hits[0].gameObject.TryGetComponent(out ICursorInteractable interactable))
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
        validCollectablePlacement = true;
        while (true)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, currentCollectableReference.collectableUI.ColliderRadius);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Ingredient") || collider.CompareTag("BasketWall"))
                {
                    _CursorImage.color = INVALID_PLACEMENT_COLOR;
                    validCollectablePlacement = false;
                    yield return null;
                    continue;
                }
                if (collider.gameObject.TryGetComponent(out IngredientCookingSlot slot))
                {
                    if (slot.ingredientReference != null || !CookingScreen.Singleton.AtCookingScreen)
                    {
                        _CursorImage.color = INVALID_PLACEMENT_COLOR;
                        validCollectablePlacement = false;
                        yield return null;
                        continue;
                    }
                }
            }
            _CursorImage.color = VALID_PLACEMENT_COLOR;
            validCollectablePlacement = true;
            yield return null;
        }
    }

    private void MouseUp(InputAction.CallbackContext ctx)
    {
        if (currentCollectableReference == null) return;

        if (!validCollectablePlacement)
        {
            lastCursorInteract.ReturnIngredientHereFromCursor();
            currentCollectableReference = null;
            ChangeToCrosshairSprite();
            return;
        }

        PointerEventData m_PointerEventData = new PointerEventData(EventSystem.current) { position = transform.position };
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(m_PointerEventData, hits);

        if (hits.Count > 0)
        {
            if (hits[0].gameObject.TryGetComponent(out ICursorInteractable interactable)) interactable.MouseUpOn(Vector2.Distance(transform.position, mouseDownPosition) < MOUSE_DISTANCE_FOR_TAP);
        }

        if (currentCollectableReference != null) // if CursorInteractable doesn't modify cursor, then just drop it
        {
            if (lastCursorInteract is IngredientCookingSlot) ((IngredientCookingSlot)lastCursorInteract).RemoveIngredient();
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
    }

    void ChangeToCollectableSprite(Sprite sprite)
    {
        _CursorImage.sprite = CrosshairSprite;
        _CursorImage.rectTransform.sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
        _CursorImage.color = new Color(1, 1, 1, .5f);
    }

    void Update()
    {
        Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }

    public void OnExitCooking()
    {
        currentCollectableReference = null;
        ChangeToCrosshairSprite();
    }

    public void DropCollectable()
    {
        ChangeToCrosshairSprite();
        Collectable c = currentCollectableReference;
        currentCollectableReference = null;
    }
}
