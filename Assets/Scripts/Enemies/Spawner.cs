using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    // I DONT KNOW A BETTER SOLUTION MOORE HELP
    public List<EnemyBaseClass> possibleEnemies;
    public List<int> weights;
    public int frequency = 10;
    public int cluster = 1;
    private int totalWeight;
    private Dictionary<int, EnemyBaseClass> spawnerDict;
    void Start()
    {
        // Preventative measures
        if(possibleEnemies.Count != weights.Count)
        {
            Debug.LogWarning("Uneven number of weights and possible enemies");
        }

        // Calculate total weight
        totalWeight = 0;
        for(int i = 0; i < weights.Count; i++)
        {
            totalWeight += weights[i];
            if (weights[i] <= 0)
            {
                Debug.LogWarning("Zero or negative weight");
            }
        }

        // Build spawner dictionary
        int count = 0;
        spawnerDict = new Dictionary<int, EnemyBaseClass>();
        for(int i = 0; i < weights.Count; i++)
        {
            for(int j = 0; j < weights[i]; j++)
            {
                spawnerDict.Add(count, possibleEnemies[i]);
                count++;
            }
        }
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        for(int i = 0; i < cluster; i++)
        {
            int x = Random.Range(0, totalWeight);
            Instantiate(spawnerDict[x]);
        }
        yield return new WaitForSeconds(frequency);
        StartCoroutine(Spawn());
    }
}
