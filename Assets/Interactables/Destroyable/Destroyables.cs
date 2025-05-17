using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyables : MonoBehaviour
{
    public static event Action<Vector3> Destroyed;
    public bool randDrop = false;
    [SerializeField] private Collectable singleCollectable;
    public List<Collectable> multipleCollectables;
    [SerializeField] private float oddsForSomething = .5f;
    public bool DestroyedOnDamage = true;

    int forageableIndex = -1;
    public void TrackSpawn(int index, List<Collectable> newList)
    {
        forageableIndex = index;
        if (randDrop) multipleCollectables = newList;
    }

    public void RemoveDestroyable()
    {
        if (!DestroyedOnDamage) return;

        Destroy(this.gameObject);

        if (!randDrop)
        {
            if (singleCollectable != null)
            {
                GameObject gameObj = Instantiate(singleCollectable.gameObject, new Vector2(this.transform.position.x, this.transform.position.y), Quaternion.identity);
                gameObj.GetComponent<Collectable>().Spawn(this.transform.position);
                Destroyed?.Invoke(transform.position);
            }
        } else
        {
            if (multipleCollectables != null)
            {
                if (UnityEngine.Random.Range(0f, 1f) < oddsForSomething)
                {
                    int randomIndex = UnityEngine.Random.Range(0, multipleCollectables.Count);
                    GameObject gameObj = Instantiate(multipleCollectables[randomIndex].gameObject, new Vector2(this.transform.position.x, this.transform.position.y), Quaternion.identity);
                    gameObj.GetComponent<Collectable>().Spawn(this.transform.position);
                }
                Destroyed?.Invoke(transform.position);
            }
        }

        if (forageableIndex >= 0) RunStateManager.Singleton.TrackBrokenDestroyable(forageableIndex);
    }

    public void ManualDestroy()
    {
        if (forageableIndex >= 0) RunStateManager.Singleton.TrackBrokenDestroyable(forageableIndex);
    }
}
