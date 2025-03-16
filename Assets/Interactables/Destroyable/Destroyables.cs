using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyables : MonoBehaviour
{
    public static event Action Destroyed;
    public bool randDrop = false;
    [SerializeField] private Collectable singleCollectable;
    [SerializeField] private List<Collectable> multipleCollectables;

    public void RemoveDestroyable()
    {
        Destroy(this.gameObject);

        if (!randDrop)
        {
            if (singleCollectable != null)
            {
                GameObject gameObj = Instantiate(singleCollectable.gameObject, new Vector2(this.transform.position.x, this.transform.position.y), Quaternion.identity);
                gameObj.GetComponent<Collectable>().Spawn(this.transform.position);
                Destroyed?.Invoke();
            }
        } else
        {
            if (multipleCollectables != null)
            {
                int randomIndex = UnityEngine.Random.Range(0, multipleCollectables.Count);
                GameObject gameObj = Instantiate(multipleCollectables[randomIndex].gameObject, new Vector2(this.transform.position.x, this.transform.position.y), Quaternion.identity);
                gameObj.GetComponent<Collectable>().Spawn(this.transform.position);
                Destroyed?.Invoke();
            }
        }
        
    }
}
