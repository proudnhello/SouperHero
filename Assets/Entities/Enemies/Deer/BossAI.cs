using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : EnemyBaseClass
{
    bool vunerable = false;

    public ParticleSystem shieldParticles; // Should not be public, but needed for shield hit effect on death, so whatever
    [SerializeField] int shieldParticleCount = 20;

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
    [SerializeField] GameObject spawnEffectPrefab;

    [Header("Boss Sound Effects")]
    [SerializeField] FMODUnity.EventReference shieldDown;
    [SerializeField] FMODUnity.EventReference shieldUp;
    [SerializeField] FMODUnity.EventReference bossShieldHit;

    [Header("Wave Details")]

    [SerializeField] List<GameObject> spawnPoints;

    List<EnemyBaseClass> spawnedEnemies = new List<EnemyBaseClass>(); 
    Wave currentWave;
    List<int> enemiesToSpawn = new List<int>();
    List<int> viableIndexes = new List<int>();
    [SerializeField] List<Wave> possibleWaves = new List<Wave>();

    
    

    public bool holdSpawning = true;

    GameObject shield;
    // Start is called before the first frame update
    void Start()
    {
        initEnemy();
        // The boss stands still, so mark these as false
        agent.updatePosition = false;
        agent.updateRotation = false;

        entityRenderer = new BossRenderer(this);

        // The boss is immortal until a wave is defeated, so mark as [title card drop]
        invincible = false;
        vunerable = true;

        shield = gameObject.transform.Find("Shield").gameObject;
        shieldParticles = GetComponent<ParticleSystem>();
    }

    public void TriggerBossFight()
    {
        if (inactive)
        {
            inactive = false;
            StartCoroutine(BossHealthbarManager.Instance.StartBossFight(this));
            holdSpawning = true;
            invincible = true;
            shield.SetActive(true);
        }
    }
    
    override protected void UpdateAI()
    {
        // Boss is inactive until triggered
        if (inactive || holdSpawning)
        {
            return;
        }
        
        // If vunerable, count down timer until no longer vunerable. Start new wave when that happens
        if (vunerable)
        {
            vunerableTimer -= Time.deltaTime;
            // print("Vunerable for: " + vunerableTimer);
            if (vunerableTimer <= 0f)
            {
                vunerable = false;
                invincible = true;
                shield.SetActive(true);
                currentWave = possibleWaves[UnityEngine.Random.Range(0, possibleWaves.Count)];
                StartWave(currentWave);
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
            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, enemyInstance.transform.position, Quaternion.identity);
            }
            //print("Spawned enemy: " + currentWave.enemies[index].enemy.name + ". Remaining to spawn: " + enemiesToSpawn[index]);
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
            StartCoroutine(ShieldPowerDownEffect());
        }
    }

    IEnumerator ShieldPowerDownEffect()
    {
        float effectDuration = Math.Min(vunerableDuration/3, 1f);
        float timer = 0f;
        Renderer shieldRenderer = shield.GetComponent<Renderer>();
        Color initialColor = shieldRenderer.material.color;
        // Play sound effect for shield down
        AudioManager.Singleton.PlayOneShot(AudioManager.SoundType.BossSFX, (int)AudioManager.BossSFXIndex.ShieldDown, transform.position);
        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float t = timer / effectDuration;
            Color newColor = Color.Lerp(initialColor, Color.clear, t);
            shieldRenderer.material.color = newColor;
            yield return null;
        }
        yield return new WaitForSeconds(vunerableDuration - effectDuration - effectDuration);

        timer = 0f;
        AudioManager.Singleton.PlayOneShot(AudioManager.SoundType.BossSFX, (int)AudioManager.BossSFXIndex.ShieldUp, transform.position);
        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float t = timer / effectDuration;
            Color newColor = Color.Lerp(Color.clear, initialColor, t);
            shieldRenderer.material.color = newColor;
            yield return null;
        }
        shieldRenderer.material.color = initialColor;
    }

    private void StartWave(Wave wave)
    {
        enemiesToSpawn.Clear();
        viableIndexes.Clear();
        //("Starting wave: " + wave.waveName);
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

    // Override to produce shield effect when hit while invincible
    public override void ApplyInfliction(List<FinishedSoup.SoupInflictionStat> spoonInflictions, Transform source)
    {
        if (invincible)
        {
            // Rotate the shape of the particle system to face the opposite direction of the source of the infliction
            // Find the direction vector pointing away from the player
            Vector2 directionAway = (transform.position - source.position).normalized;
            // Calculate the angle in degrees
            float angle = Mathf.Atan2(directionAway.y, directionAway.x) * Mathf.Rad2Deg;
            // Set the rotation of the particle system
            var shape = shieldParticles.shape;
            shape.rotation = new Vector3(-angle, 90, 0);

            // Play shield effect
            shieldParticles.Emit(shieldParticleCount);
            // Play shield hit sound effect
            AudioManager.Singleton.PlayOneShot(AudioManager.SoundType.BossSFX, (int)AudioManager.BossSFXIndex.ShieldHit, transform.position);
        }
        base.ApplyInfliction(spoonInflictions, source);
    }
    
}
