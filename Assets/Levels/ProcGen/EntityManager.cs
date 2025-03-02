using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    [Header("Spawning Parameters")]
    [SerializeField] int difficulty;
    [SerializeField] int lootLevel;
    [Serializable]
    public class SpawnableEnemy {
        public GameObject enemy;
        public int weight = 1;
        public int difficulty = 1;
    }
    [Header("Enemies")] 
    public List<SpawnableEnemy> possibleEnemies;
    public List<EnemySpawnLocation> enemySpawns;
    private int totalEnemies;
    private int totalEnemyWeight;

    [Serializable]
    public class SpawnableForagable {
        public GameObject foragable;
        public int weight = 1;
        public int value = 1;
    }
    [Header("Foragables")] 
    public List<SpawnableForagable> possibleForagables;
    public List<ForagableSpawnLocation> foragableSpawns;
    private int totalForagables;
    private int totalForagableWeight;
    private Dictionary<int, SpawnableEnemy> enemyDict;
    private Dictionary<int, SpawnableForagable> foragableDict;
    void Start()
    {
        buildEnemyDictionary();
        buildForagableDictionary();
        createEnemies();
        createForagables();
    }

    void buildEnemyDictionary()
    {
        totalEnemyWeight = 0;
        for(int i = 0; i < possibleEnemies.Count; i++)
        {
            totalEnemyWeight += possibleEnemies[i].weight;
            if (possibleEnemies[i].weight <= 0)
            {
                Debug.LogWarning("Zero or negative weight, turned into weight of 1");
                possibleEnemies[i].weight = 1;
            }
        }

        int count = 0;
        enemyDict = new Dictionary<int, SpawnableEnemy>();
        for(int i = 0; i < possibleEnemies.Count; i++)
        {
            for(int j = 0; j < possibleEnemies[i].weight; j++)
            {
                enemyDict.Add(count, possibleEnemies[i]);
                count++;
            }
        }
    }

    void createEnemies(){
        int difficultyCounter = 0;
        while(difficultyCounter < difficulty && totalEnemies < enemySpawns.Count){
            int x = UnityEngine.Random.Range(0, totalEnemyWeight);
            GameObject enemy = Instantiate(enemyDict[x].enemy, enemySpawns[totalEnemies].transform);
            enemy.transform.position = enemySpawns[totalEnemies].transform.position;
            enemySpawns[totalEnemies].enemy = enemy;
            difficultyCounter += enemyDict[x].difficulty;
            totalEnemies++;
        }
    }

    void buildForagableDictionary()
    {
        totalForagableWeight = 0;
        for(int i = 0; i < possibleForagables.Count; i++)
        {
            totalForagableWeight += possibleForagables[i].weight;
            if (possibleForagables[i].weight <= 0)
            {
                Debug.LogWarning("Zero or negative weight, turned into weight of 1");
                possibleForagables[i].weight = 1;
            }
        }

        int count = 0;
        foragableDict = new Dictionary<int, SpawnableForagable>();
        for(int i = 0; i < possibleForagables.Count; i++)
        {
            for(int j = 0; j < possibleForagables[i].weight; j++)
            {
                foragableDict.Add(count, possibleForagables[i]);
                count++;
            }
        }
    }

    void createForagables(){
        int lootCounter = 0;
        while(lootCounter < lootLevel && totalForagables < foragableSpawns.Count){
            int x = UnityEngine.Random.Range(0, totalForagableWeight);
            Debug.Log(foragableSpawns[0]);
            GameObject foragable = Instantiate(foragableDict[x].foragable, foragableSpawns[totalForagables].transform);
            foragable.transform.position = foragableSpawns[totalForagables].transform.position;
            foragableSpawns[totalForagables].foragable = foragable;
            lootCounter += foragableDict[x].value;
            totalForagables++;
        }
    }

    void Update()
    {
        
    }
}
