using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectableUI : MonoBehaviour, ICursorInteractable
{
    internal Rigidbody2D rb;
    Collectable _Collectable;
    internal Sprite _SpriteReference;
    Image _Image;
    Collider2D _Collider2D;
    public float ColliderRadius
    {
        get => _Collider2D.bounds.size.x;
    }

    // Start is called before the first frame update
    public void Init(Collectable col)
    {
        rb = GetComponent<Rigidbody2D>();
        _Collectable = col;
        _Image = GetComponent<Image>();
        _SpriteReference = _Image.sprite;
        _Collider2D = GetComponent<Collider2D>();
    }

    public Collectable GetCollectable()
    {
        return _Collectable;
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
    }

    public void DropItemOnScreen(Vector3 position)
    {
        transform.position = position;
        _Image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        _Image.raycastTarget = true;
        currentCookingSlot = null;
    }

    IngredientCookingSlot currentCookingSlot; 
    public void MouseUpOn(bool tap) 
    {
        if (CursorManager.Singleton.currentCollectableReference == _Collectable) // add directly to available cooking slot
        {
            if (tap)
            {
                IngredientCookingSlot slot = CookingScreen.Singleton.GetAvailableSoupSlot();
                if (slot != null)
                {
                    currentCookingSlot = slot;
                    currentCookingSlot.AddIngredient(_Collectable);
                } else
                {
                    ReturnIngredientHereFromCursor();
                }
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
            _Collectable.Drop();      
        }
    }
}