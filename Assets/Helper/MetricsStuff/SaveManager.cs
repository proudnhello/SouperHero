using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Data.Common;
using System.Numerics;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[Serializable]
public class EntitiesClass
{
    public List<String> enemies;
    public List<String> foragables;
    public int seed;
}

[Serializable]
public class PlayerDataClass
{
    public UnityEngine.Vector2 playerPos;
}

public class SaveManager : MonoBehaviour
{

    public static SaveManager Singleton { get; private set; }

    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;

        // Get DeathMetrics INstance
        deathMetrics = DeathMetrics.Instance;


        // Reliable Path Across Devices
        statsPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Stats.json");

        // Debug Path Save in Assets Folder
        //statsPath = Path.Combine(Application.dataPath + Path.AltDirectorySeparatorChar + "SaveData.json");

        // Reliable Path Across Devices
        statsPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Stats.json");
        entitiesPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Entities.json");
        playerPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Player.json");

        // Load game scene if game scene
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            LoadGameScene();
        }
        
    }

    // Class that saves metrics across plays
    public DeathMetrics deathMetrics;
    public RoomGenerator roomGenerator;
    private string statsPath;
    private string entitiesPath;
    private string playerPath;

    public void Start()
    {
        DeathMetricsManager.Singleton.ProcessStats();
        DeathMetricsManager.Singleton.DisplayStats();
    }

    public void SaveGameScene(){
        SavePlayer();
        SaveEntities();
    }

    public void LoadGameScene(){
        LoadPlayerData();
        LoadEntities();
    }

    public void SavePlayer(){
        PlayerDataClass pdc = new PlayerDataClass();
        pdc.playerPos = PlayerEntityManager.Singleton.transform.position;

        string json = JsonUtility.ToJson(pdc, true);  // Pretty print for readability

        using (StreamWriter writer = new StreamWriter(playerPath))
        {
            writer.Write(json);
        }
    }

    public void LoadPlayerData(){
        if (File.Exists(playerPath))
        {
            string json = string.Empty;
            
            using(StreamReader reader = new StreamReader(playerPath))
            {
                json = reader.ReadToEnd();
            }

            PlayerDataClass data = JsonUtility.FromJson<PlayerDataClass>(json);

            PlayerEntityManager.Singleton.transform.position = data.playerPos;
        }
    }

    public void SaveEntities(){
        EntitiesClass et = new EntitiesClass();
        et.enemies = roomGenerator.exportEnemyStrings();
        et.foragables = roomGenerator.exportForagableStrings();
        et.seed = roomGenerator.newSeed;
        string json = JsonUtility.ToJson(et, true);  // Pretty print for readability

        using (StreamWriter writer = new StreamWriter(entitiesPath))
        {
            writer.Write(json);
        }
    }

    public void LoadEntities(){
        if (File.Exists(entitiesPath))
        {
            string json = string.Empty;
            
            using(StreamReader reader = new StreamReader(entitiesPath))
            {
                json = reader.ReadToEnd();
            }

            EntitiesClass data = JsonUtility.FromJson<EntitiesClass>(json);

            roomGenerator.importEnemyStrings(data.enemies);
            roomGenerator.importForagableStrings(data.foragables);
            roomGenerator.mapSeed = data.seed;
        }
    }

    public void SaveGameStats()
    {

        string json = JsonUtility.ToJson(deathMetrics, true);  // Pretty print for readability

        using (StreamWriter writer = new StreamWriter(statsPath))
        {
            writer.Write(json);
        }

    }

    public void LoadGameStats()
    {
        if (File.Exists(statsPath))
        {
            string json = string.Empty;
            
            using(StreamReader reader = new StreamReader(statsPath))
            {
                json = reader.ReadToEnd();
            }

            DeathMetrics data = JsonUtility.FromJson<DeathMetrics>(json);

            deathMetrics = data;
        }
        else
        {
            Debug.LogWarning("No save file found!");
        }
    }

    public void ResetGameStats()
    {

        deathMetrics.ResetStats();

        string json = JsonUtility.ToJson(deathMetrics, true);  // Pretty print for readability

        using (StreamWriter writer = new StreamWriter(statsPath))
        {
            writer.Write(json);
        }

        //Debug.Log($"Saving New Stats Json at path: {statsPath}");
    }
}
