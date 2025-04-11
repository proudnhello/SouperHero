using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityManager : MonoBehaviour
{
    internal int difficulty;
    internal int lootLevel = 3;

    [Serializable]
    public class SpawnableEnemy {
        public GameObject enemy;
        public int weight = 1;
        public int difficulty = 1;
        public int index = -1;
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
        // DO NOT CHANGE INDEX IN EDITOR
        public int index = -1;
    }
    [Header("Foragables")] 
    public List<SpawnableForagable> possibleForagables;
    public List<ForagableSpawnLocation> foragableSpawns;
    private int totalForagables;
    private int totalForagableWeight;
    private Dictionary<int, SpawnableEnemy> enemyDict;
    private Dictionary<int, SpawnableForagable> foragableDict;
    //void Start()
    //{
    //    buildEnemyDictionary();
    //    buildForagableDictionary();
    //    createEnemies();
    //    createForagables();
    //}
    public bool hasSpawned = false;
    public bool hasExportedEnemies = false;
    public bool hasExportedForagables = false;
    public bool SpawnEntities()
    {
        if (hasSpawned) return false;

        BuildEnemyDictionary();
        BuildForagableDictionary();
        CreateEnemies();
        CreateForagables();

        hasSpawned = true;

        return hasSpawned;
    }

    public bool LoadEntities(String enemies, String foragables)
    {
        if (hasSpawned) return false;
        
        BuildEnemyDictionary();
        BuildForagableDictionary();

        ImportEnemies(enemies);
        ImportForagables(foragables);

        hasSpawned = true;

        return true;
    }

    public void BuildEnemyDictionary()
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
            possibleEnemies[i].index = i;
            for(int j = 0; j < possibleEnemies[i].weight; j++)
            {
                enemyDict.Add(count, possibleEnemies[i]);
                count++;
            }
        }
    }
    
    public void CreateEnemies()
    {
        int difficultyCounter = 0;
        while (difficultyCounter < difficulty && totalEnemies < enemySpawns.Count){
            // Generate a random enemy using the weighted dictionary
            int x = UnityEngine.Random.Range(0, totalEnemyWeight);
            GameObject enemy = Instantiate(enemyDict[x].enemy, enemySpawns[totalEnemies].transform);
            
            // Handle initial properties of the new enemy
            enemy.transform.position = enemySpawns[totalEnemies].transform.position;
            enemy.GetComponent<EnemyBaseClass>().setSpawn(enemySpawns[totalEnemies]);

            // Update state of the entity manager
            enemySpawns[totalEnemies].enemy = enemy;
            enemySpawns[totalEnemies].index = enemyDict[x].index;
            difficultyCounter += enemyDict[x].difficulty;
            totalEnemies++;
        }
    }

    public String ExportEnemies(){
        if (hasExportedEnemies) return ":(";
        String ret = "";
        for(int i = 0; i < enemySpawns.Count; i++){
            if(enemySpawns[i].enemy != null){
                ret += enemySpawns[i].index;
            }
            else{
                ret += "-1";
            }
            if(i < enemySpawns.Count - 1){
                ret += ",";
            }
        }
        hasExportedEnemies = true;
        return ret;
    }

    public void ImportEnemies(String import){
        String[] strings = import.Split(',');
        Debug.Log("EnemyList Size: " + possibleEnemies.Count);
        for(int i = 0; i < strings.Length; i++){
            if(strings[i] != "-1" && i < enemySpawns.Count){
                Debug.Log("String index: " + strings[i]);
                GameObject enemy = Instantiate(possibleEnemies[Int32.Parse(strings[i])].enemy);
                enemy.transform.position = enemySpawns[i].transform.position;
                enemy.GetComponent<EnemyBaseClass>().setSpawn(enemySpawns[i]);

                enemySpawns[i].enemy = enemy;
                enemySpawns[i].index = possibleEnemies[Int32.Parse(strings[i])].index;
            }
        }
    }

    public void BuildForagableDictionary()
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

    public void CreateForagables(){
        int lootCounter = 0;
        while(lootCounter < lootLevel && totalForagables < foragableSpawns.Count){
            int x = UnityEngine.Random.Range(0, totalForagableWeight);
            GameObject foragable = Instantiate(foragableDict[x].foragable, foragableSpawns[totalForagables].transform);
            foragable.transform.position = foragableSpawns[totalForagables].transform.position;
            foragableSpawns[totalForagables].foragable = foragable;
            lootCounter += foragableDict[x].value;
            totalForagables++;
        }
    }

    public String ExportForagables(){
        if(hasExportedForagables) return ":(";
        String ret = "";
        for(int i = 0; i < foragableSpawns.Count; i++){
            if(foragableSpawns[i].foragable != null){
                ret += foragableSpawns[i].index;
            }
            else{
                ret += "-1";
            }
            if(i < foragableSpawns.Count - 1){
                ret += ",";
            }
        }
        hasExportedForagables = true;
        return ret;
    }

    public void ImportForagables(String import){
        String[] strings = import.Split(',');
        for(int i = 0; i < strings.Length; i++){
            if(strings[i] != "-1" && i < foragableSpawns.Count){
                foragableSpawns[i].foragable = possibleForagables[Int32.Parse(strings[i])].foragable;
            }
        }
    }

    void Update()
    {
        //Debug.Log(exportEnemies());
    }
}
