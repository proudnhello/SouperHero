using FMOD;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectableUI : MonoBehaviour, ICursorInteractable, ITooltipSource
{
    internal Rigidbody2D rb;
    Collectable _Collectable;
    internal Sprite _SpriteReference;
    Image _Image;
    public float ColliderRadiusMult = 1f;
    Collider2D colliderUI;

    // Start is called before the first frame update
    public void Init(Collectable col)
    {
        rb = GetComponent<Rigidbody2D>();
        colliderUI = GetComponent<Collider2D>();
        _Collectable = col;
        _Image = GetComponent<Image>();
        _SpriteReference = _Image.sprite;
    }

    public void PickUp()
    {
        _Image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        _Image.raycastTarget = true;
        currentCookingSlot = null;
        rb.rotation = 0;
        transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
    }

    public void MouseDownOn()
    {
        CursorManager.Singleton.PickupCollectable(_Collectable);
        IngredientBioDisplay.Singleton.DragIngredient(_Collectable.ingredient);
        _Image.color = new Color(1.0f, 1.0f, 1.0f, .25f);
        _Image.raycastTarget = false;
    }

    public void ReturnItemHereFromCursor() 
    {
        _Image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        _Image.raycastTarget = true;
        if (currentCookingSlot != null) currentCookingSlot.RemoveIngredient();
        currentCookingSlot = null;
        IngredientBioDisplay.Singleton.ReleaseDrag();
        if (!CursorManager.Singleton.TooltipTrigger.IsCursorHoveringOnTooltip(colliderUI))
        {
            IngredientBioDisplay.Singleton.TryHideHoverBio(_Collectable.ingredient);
        }
    }

    public void DropItemOnScreen(Vector3 position)
    {
        transform.position = position;
        _Image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        _Image.raycastTarget = true;
        currentCookingSlot = null;
        rb.velocity = Vector2.zero;
        rb.rotation = 0;
        IngredientBioDisplay.Singleton.ReleaseDrag();
    }

    IngredientCookingSlot currentCookingSlot; 
    public void PlaceInCookingSlot(IngredientCookingSlot slot)
    {
        if (currentCookingSlot != null && currentCookingSlot != slot) currentCookingSlot.RemoveIngredient();
        currentCookingSlot = slot;
    }

    public void Tap()
    {
        if (CursorManager.Singleton.currentCollectableReference == _Collectable) // add directly to available cooking slot
        {
            IngredientCookingSlot slot = CookingScreen.Singleton.GetAvailableSoupSlot(_Collectable.ingredient);
            if (slot != null)
            {
                currentCookingSlot = slot;
                currentCookingSlot.AddIngredient(_Collectable);
            }
            else
            {
                ReturnItemHereFromCursor();
            }

            CursorManager.Singleton.DropCollectable();
            IngredientBioDisplay.Singleton.ReleaseDrag();
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("IngredientCatcher"))
        {
            if (currentCookingSlot != null) currentCookingSlot.RemoveIngredient();
            currentCookingSlot = null;
            if (CursorManager.Singleton.TryDropCollectable(_Collectable)) IngredientBioDisplay.Singleton.ReleaseDrag();
            _Collectable.Drop();
        }
    }

    #region HOVERING
    public void OnHoverEnter()
    {
        if (IHoverTimerForBio != null) StopCoroutine(IHoverTimerForBio);
        StartCoroutine(IHoverTimerForBio = HoverTimerForBio(true));
    }

    IEnumerator IHoverTimerForBio;
    IEnumerator HoverTimerForBio(bool enter)
    {
        if (enter)
        {
            yield return new WaitForSeconds(IngredientBioDisplay.Singleton.HoverTimeToDisplay);
            IngredientBioDisplay.Singleton.TryDisplayHoverBio(_Collectable.ingredient);
        }
        else
        {
            yield return new WaitForSeconds(IngredientBioDisplay.Singleton.HoverTimeToDisplay);
            IngredientBioDisplay.Singleton.TryHideHoverBio(_Collectable.ingredient);
        }
    }


    public void OnHoverExit()
    {
        if (IHoverTimerForBio != null) StopCoroutine(IHoverTimerForBio);
        StartCoroutine(IHoverTimerForBio = HoverTimerForBio(false));
    }
    #endregion
}