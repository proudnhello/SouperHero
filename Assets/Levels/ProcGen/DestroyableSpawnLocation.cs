using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyableSpawnLocation : MonoBehaviour
{
    internal int index = -1;
    public List<PossibleDestroyable> possibleDestroyables;
    public List<Collectable> possibleRandomDrops;

    [Serializable]
    public class PossibleDestroyable
    {
        public GameObject destroyableChoice;
        public int weight;
    }

    PossibleDestroyable GetDestroyable()
    {
        int totalWeight = possibleDestroyables.Sum(x => x.weight);
        int rand = UnityEngine.Random.Range(0, totalWeight);
        PossibleDestroyable selectedDestroyable = null;
        foreach (var destroy in possibleDestroyables)
        {
            if (rand < destroy.weight)
            {
                selectedDestroyable = destroy;
                break;
            }
            rand -= destroy.weight;
        }
        return selectedDestroyable;
    }

    public void SpawnDestroyable()
    {
        PossibleDestroyable destroyableToSpawn = GetDestroyable();
        if (destroyableToSpawn == null) return;
        index = RunStateManager.Singleton.GetNewDestroyableIndex();
        if (RunStateManager.Singleton.HasDestroyableBeenBroken(index))
        {
            GameObject destroyable = Instantiate(destroyableToSpawn.destroyableChoice, transform.position, Quaternion.identity, transform);
            destroyable.GetComponent<Destroyables>().TrackSpawn(index, possibleRandomDrops);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, .5f); // Green with custom alpha
        Gizmos.DrawSphere(transform.position, .5f);
    }
}
