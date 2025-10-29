using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : EnemyBaseClass
{
    bool vunerable = false;

    [SerializeField] Color immuneColor;
    [SerializeField] Color normalColor;

    [Serializable]
    public struct EnemyWaveCounter
    {
        public EnemyBaseClass enemy; // Enemy prefab to spawn
        public int count; // Number of this enemy to spawn
    }

    [Serializable] 
    public struct Wave 
    {
        public string waveName; // Name of the wave (probably won't be used in-game, more for in editor) 
        public int maxEnemiesAtOnce; // Max number of enemies that can be alive at once from this wave 
        public List<EnemyWaveCounter> enemies; // List of enemies and their counts to spawn in this wave 
    }

    [Header("Boss Details")]
    [SerializeField] bool inactive = true;
    [SerializeField] float vunerableDuration = 5f; 
    private float vunerableTimer = 0f;
    [SerializeField] float spawnDelay = 0.5f; 
    private float spawnTimer = 0f;
    [SerializeField] List<GameObject> spawnPoints;

    List<EnemyBaseClass> spawnedEnemies = new List<EnemyBaseClass>(); 
    Wave currentWave;
    List<int> enemiesToSpawn = new List<int>();
    List<int> viableIndexes = new List<int>();      
    [SerializeField] List<Wave> possibleWaves = new List<Wave>();

    // Start is called before the first frame update
    void Start()
    {
        initEnemy();
        // The boss stands still, so mark these as false
        agent.updatePosition = false;
        agent.updateRotation = false;

        // The boss is immortal until a wave is defeated, so mark as [title card drop]
        invincible = false;
        vunerable = true;

        // BossHealthbarManager.Instance.StartBossFight(this); // TODO: Add a starting trigger before this
    }

    
    override protected void UpdateAI()
    {
        // Boss is inactive until triggered
        if (inactive)
        {
            return;
        }
        
        // If vunerable, count down timer until no longer vunerable. Start new wave when that happens
        if (vunerable)
        {
            vunerableTimer -= Time.deltaTime;
            print("Vunerable for: " + vunerableTimer);
            if (vunerableTimer <= 0f)
            {
                vunerable = false;
                invincible = true;
                currentWave = possibleWaves[UnityEngine.Random.Range(0, possibleWaves.Count)];
                StartWave(currentWave);
                GetComponent<SpriteRenderer>().color = immuneColor;
            }
            return;
        }

        // Reduce spawn timer and spawn enemies if able
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && spawnedEnemies.Count < currentWave.maxEnemiesAtOnce && viableIndexes.Count > 0)
        {
            int index = viableIndexes[UnityEngine.Random.Range(0, viableIndexes.Count)];
            EnemyBaseClass enemyInstance = Instantiate(currentWave.enemies[index].enemy, spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].transform.position, Quaternion.identity);
            enemyInstance.PissOff();
            enemyInstance.spawnedBy = this;
            spawnedEnemies.Add(enemyInstance);
            enemiesToSpawn[index]--;
            print("Spawned enemy: " + currentWave.enemies[index].enemy.name + ". Remaining to spawn: " + enemiesToSpawn[index]);
            if (enemiesToSpawn[index] <= 0)
            {
                viableIndexes.Remove(index);
            }
            spawnTimer = spawnDelay;
        }

        // If all enemies have been spawned and defeated, make boss vunerable
        if (spawnedEnemies.Count == 0 && viableIndexes.Count == 0)
        {
            vunerable = true;
            invincible = false;
            vunerableTimer = vunerableDuration;
            GetComponent<SpriteRenderer>().color = normalColor;
        }
    }

    private void StartWave(Wave wave)
    {
        enemiesToSpawn.Clear();
        viableIndexes.Clear();
        print("Starting wave: " + wave.waveName);
        for (int i = 0; i < wave.enemies.Count; i++)
        {
            enemiesToSpawn.Add(wave.enemies[i].count);
            viableIndexes.Add(i);
        }
        spawnTimer = spawnDelay;
    }


    public void SpawnedEnemyDeath(EnemyBaseClass enemy)
    {
        spawnedEnemies.Remove(enemy);
    }
    
    override protected void Die()
    {
        BossHealthbarManager.Instance.EndBossFight();
        base.Die();
    }
    
}
