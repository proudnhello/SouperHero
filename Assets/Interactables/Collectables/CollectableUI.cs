using UnityEngine;

public class CollectableUI : MonoBehaviour
{
    Collectable _Collectable;
    // Start is called before the first frame update
    public void Init(Collectable col)
    {
        _Collectable = col;
    }
}