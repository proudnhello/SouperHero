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

    }
}
