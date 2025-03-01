using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CollectableUI : MonoBehaviour
{
    internal Rigidbody2D rb;
    Collectable _Collectable;
    // Start is called before the first frame update
    public void Init(Collectable col)
    {
        rb = GetComponent<Rigidbody2D>();
        _Collectable = col;
    }
    
    public Collectable GetCollectable()
    {
        return _Collectable;
    }
}