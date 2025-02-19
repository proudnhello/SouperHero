using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyables : MonoBehaviour
{

    [SerializeField] private Collectable collectable;
    public void RemoveDestroyable()
    {
        Destroy(this.gameObject);

        if (collectable != null) {
            GameObject gameObj = Instantiate(collectable.gameObject, new Vector2(this.transform.position.x, this.transform.position.y), Quaternion.identity);
            gameObj.GetComponent<Collectable>().Spawn(this.transform.position);
        }
    }
}
