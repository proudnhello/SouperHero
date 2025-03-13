using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpawnableEnemy = EntityManager.SpawnableEnemy;

public class BossRoom : MonoBehaviour
{
    public GameObject[] closedDoors;
    public GameObject[] openDoors;
    bool fightStarted = false;
    public Wave[] waves;
    List<GameObject> enemiesInWave;
    public List<GameObject> spawnLocations;
    int currentWave = 0;
    float waveCheckInterval = 0.5f;

    public enum WaveType
    {
        Instant,
        Gradual
    }

    [Serializable]
    public class Wave
    {
        public List<SpawnableEnemy> enemies;
        public int difficulty;
        public WaveType type;
        // Only for Gradual
        public float timeBetweenSpawns;
        public int spawnsAtATime;
    }

    private void Start()
    {
        enemiesInWave = new List<GameObject>();
    }

    public void BeginFight()
    {
        if (fightStarted)
        {
            return;
        }
        print("Begin Boss Fight");
        fightStarted = true;
        foreach (var door in closedDoors)
        {
            door.SetActive(true);
        }
        foreach (var door in openDoors)
        {
            door.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (!fightStarted)
        {
            return;
        }

        waveCheckInterval -= Time.fixedDeltaTime;
        if (waveCheckInterval <= 0)
        {
            waveCheckInterval = 0.5f;
            // If no enemies are left, see if we can spawn the next wave
            foreach (var enemy in enemiesInWave)
            {
                if (enemy != null)
                {
                    print(enemy.name + " is still alive");
                    return;
                }
            }
            // If we get here, all enemies are dead, so spawn the next wave
            if (currentWave < waves.Length)
            {
                enemiesInWave.Clear();
                SpawnWave(waves[currentWave]);
                currentWave++;
            }
            else
            {
                GameManager.instance.WinScreen();
            }
        }

    }

    Dictionary<int, SpawnableEnemy> BuildEnemyDictionary(List<SpawnableEnemy> possibleEnemies)
    {
        int totalEnemyWeight = 0;

        for (int i = 0; i < possibleEnemies.Count; i++)
        {
            totalEnemyWeight += possibleEnemies[i].weight;
            if (possibleEnemies[i].weight <= 0)
            {
                Debug.LogWarning("Zero or negative weight, turned into weight of 1");
                possibleEnemies[i].weight = 1;
            }
        }

        int count = 0;
        Dictionary<int, SpawnableEnemy> enemyDict = new Dictionary<int, SpawnableEnemy>();
        for (int i = 0; i < possibleEnemies.Count; i++)
        {
            possibleEnemies[i].index = i;
            for (int j = 0; j < possibleEnemies[i].weight; j++)
            {
                enemyDict.Add(count, possibleEnemies[i]);
                count++;
            }
        }
        return enemyDict;
    }

    void SpawnWave(Wave wave)
    {
        Dictionary<int, SpawnableEnemy> enemyDict = BuildEnemyDictionary(wave.enemies);

        // For an instant wave, spawn all enemies at once
        if(wave.type == WaveType.Instant)
        {
            while (wave.difficulty > 0)
            {
                SpawnEnemy(wave, enemyDict, true);
            }
        }
        else if(wave.type == WaveType.Gradual)
        {
            print("IMPLEMENT THIS");
        }
    }

    // When spawning an enemy, select a random enemy from the list of possible enemies and spawn it at a random spawn location.
    // Use enemy dict, as it is a weighted dictionary of possible enemies
    // The boolean active determines if the enemy should be active or not
    // Decrement the wave difficulty and add the enemy to the list of enemies currently alive
    // Forget the saving stuff, b/c they'll never get to a save point in the boss room
    void SpawnEnemy(Wave wave, Dictionary<int, SpawnableEnemy> enemyDict, bool active)
    {
        SpawnableEnemy enemy = enemyDict[UnityEngine.Random.Range(0, enemyDict.Count)];
        GameObject spawnPoint = spawnLocations[UnityEngine.Random.Range(0, spawnLocations.Count)];

        GameObject newEnemy = Instantiate(enemy.enemy, spawnPoint.transform);
        newEnemy.transform.position = spawnPoint.transform.position;
        newEnemy.SetActive(active);
        newEnemy.GetComponent<EnemyBaseClass>().AttackPlayer();

        wave.difficulty -= enemy.difficulty;
        enemiesInWave.Add(newEnemy);
    }
}
