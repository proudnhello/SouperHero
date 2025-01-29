using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Serializable]
    public class SpawnableEnemy {
        public GameObject enemy;
        public int weight = 1;
    }
    public List<SpawnableEnemy> possibleEnemies;
    public int frequency = 10;
    public int cluster = 1;
    private int totalWeight;
    private Dictionary<int, GameObject> spawnerDict;
    void Start()
    {
        // Calculate total weight
        totalWeight = 0;
        for(int i = 0; i < possibleEnemies.Count; i++)
        {
            totalWeight += possibleEnemies[i].weight;
            if (possibleEnemies[i].weight <= 0)
            {
                Debug.LogWarning("Zero or negative weight");
            }
        }

        // Build spawner dictionary
        int count = 0;
        spawnerDict = new Dictionary<int, GameObject>();
        for(int i = 0; i < possibleEnemies.Count; i++)
        {
            for(int j = 0; j < possibleEnemies[i].weight; j++)
            {
                spawnerDict.Add(count, possibleEnemies[i].enemy);
                count++;
            }
        }
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        for(int i = 0; i < cluster; i++)
        {
            int x = UnityEngine.Random.Range(0, totalWeight);
            Instantiate(spawnerDict[x]);
        }
        yield return new WaitForSeconds(frequency);
        StartCoroutine(Spawn());
    }
}
