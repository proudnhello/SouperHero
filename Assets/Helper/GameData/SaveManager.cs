using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Data.Common;
using System.Numerics;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;



public class SaveManager : MonoBehaviour
{

    public static SaveManager Singleton { get; private set; }

    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;

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
            LoadGameState();
        }
        
    }

    private string statsPath;
    private string entitiesPath;
    private string playerPath;

    public void SaveGameState(){
        SavePlayerData();
        SaveEntities();
    }

    public void LoadGameState(){
        LoadPlayerData();
        LoadEntities();
    }

    public void ResetGameState()
    {
        File.Delete(entitiesPath);
        File.Delete(playerPath);
    }

    public void ResetData()
    {

    }

    [Serializable]
    public class PlayerDataClass
    {
        public UnityEngine.Vector2 playerPos;
    }

    public void SavePlayerData()
    {
        PlayerDataClass pdc = new PlayerDataClass();
        pdc.playerPos = PlayerEntityManager.Singleton.transform.position;

        string json = JsonUtility.ToJson(pdc, true);  // Pretty print for readability

        using (StreamWriter writer = new StreamWriter(playerPath))
        {
            writer.Write(json);
        }
    }

    public void LoadPlayerData()
    {
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

    [Serializable]
    public class EntitiesClass
    {
        public List<String> enemies;
        public List<String> foragables;
        public int seed;
    }

    public void SaveEntities()
    {
        try
        {
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
        catch(Exception e)
        {
            return;
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

    public void SaveMetricsData(MetricsData data)
    {
        string json = JsonUtility.ToJson(data, true);  // Pretty print for readability

        using (StreamWriter writer = new StreamWriter(statsPath))
        {
            writer.Write(json);
        }
    }

    public MetricsData LoadMetricsData()
    {
        if (File.Exists(statsPath))
        {
            string json = string.Empty;
            
            using(StreamReader reader = new StreamReader(statsPath))
            {
                json = reader.ReadToEnd();
            }

            return JsonUtility.FromJson<MetricsData>(json);
        }
        else
        {
            Debug.LogWarning("No save file found!");
            return new();
        }
    }
}
