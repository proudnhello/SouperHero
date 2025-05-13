using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableSpawnLocation : MonoBehaviour
{
    internal int index = -1;
    public List<GameObject> possibleDestroyables;
    public List<Collectable> possibleRandomDrops;

    public void SpawnDestroyable()
    {
        GameObject destroyableToSpawn = possibleDestroyables[UnityEngine.Random.Range(0, possibleDestroyables.Count)];
        index = RunStateManager.Singleton.GetNewDestroyableIndex();
        if (RunStateManager.Singleton.HasDestroyableBeenBroken(index))
        {
            GameObject destroyable = Instantiate(destroyableToSpawn, transform.position, Quaternion.identity, transform);
            destroyable.GetComponent<Destroyables>().SetIndex(index);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, .5f); // Green with custom alpha
        Gizmos.DrawSphere(transform.position, .5f);
    }
}
