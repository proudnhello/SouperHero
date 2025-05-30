using FMOD;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectableUI : MonoBehaviour, ICursorInteractable
{
    internal Rigidbody2D rb;
    Collectable _Collectable;
    internal Sprite _SpriteReference;
    Image _Image;
    public float ColliderRadius = 10f;

    // Start is called before the first frame update
    public void Init(Collectable col)
    {
        rb = GetComponent<Rigidbody2D>();
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
    }

    public void MouseDownOn()
    {
        CursorManager.Singleton.PickupCollectable(_Collectable);
        _Image.color = new Color(1.0f, 1.0f, 1.0f, .25f);
        _Image.raycastTarget = false;
    }

    public void ReturnIngredientHereFromCursor() 
    {
        _Image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        _Image.raycastTarget = true;
        if (currentCookingSlot != null) currentCookingSlot.RemoveIngredient();
        currentCookingSlot = null;
    }

    public void DropItemOnScreen(Vector3 position)
    {
        transform.position = position;
        _Image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        _Image.raycastTarget = true;
        currentCookingSlot = null;
        rb.velocity = Vector2.zero;
        rb.rotation = 0;
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
            IngredientCookingSlot slot = CookingScreen.Singleton.GetAvailableSoupSlot();
            if (slot != null)
            {
                currentCookingSlot = slot;
                currentCookingSlot.AddIngredient(_Collectable);
            }
            else
            {
                ReturnIngredientHereFromCursor();
            }

            CursorManager.Singleton.DropCollectable();
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("IngredientCatcher"))
        {
            if (currentCookingSlot != null) currentCookingSlot.RemoveIngredient();
            currentCookingSlot = null;
            CursorManager.Singleton.TryDropCollectable(_Collectable);
            _Collectable.Drop();
        }
    }
}