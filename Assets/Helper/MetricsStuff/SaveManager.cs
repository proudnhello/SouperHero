using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class EntitiesClass
{
    public List<String> enemies;
    public List<String> foragables;
    public int seed;
}

public class SaveManager : MonoBehaviour
{

    public static SaveManager Singleton { get; private set; }

    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
        Load();
    }

    // Class that saves metrics across plays
    public DeathMetrics deathMetrics;
    public RoomGenerator roomGenerator;
    StreamWriter writer;
    StreamWriter reader;
    private string statsPath;
    private string enemiesPath;
    private string foragablesPath;

    public void Start()
    {
  
        // Get DeathMetrics INstance
        deathMetrics = DeathMetrics.Instance;


        // Reliable Path Across Devices
        statsPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Stats.json");

        // Debug Path Save in Assets Folder
        //statsPath = Path.Combine(Application.dataPath + Path.AltDirectorySeparatorChar + "SaveData.json");

        // Reliable Path Across Devices
        statsPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Stats.json");
        enemiesPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Enemies.json");
        foragablesPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Foragables.json");

        DeathMetricsManager.Singleton.ProcessStats();
        DeathMetricsManager.Singleton.DisplayStats();

    }

    public void Save(){
        SaveEntities();
        SaveGameStats();
    }

    public void Load(){
        LoadGameStats();
        LoadEntities();
    }

    public void SaveEntities(){
        EntitiesClass et = new EntitiesClass();
        et.enemies = roomGenerator.exportEnemyStrings();
        et.foragables = roomGenerator.exportForagableStrings();
        et.seed = roomGenerator.mapSeed;
        string json = JsonUtility.ToJson(et, true);  // Pretty print for readability
        Debug.Log(json);

        using (StreamWriter writer = new StreamWriter(enemiesPath))
        {
            writer.Write(json);
        }
    }

    public void LoadEntities(){
        if (File.Exists(enemiesPath))
        {
            string json = string.Empty;
            
            using(StreamReader reader = new StreamReader(statsPath))
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

            Debug.Log("Game loaded");
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

        Debug.Log($"Saving New Stats Json at path: {statsPath}");

    }
}
