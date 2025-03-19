using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{

    public static SaveManager Singleton { get; private set; }

    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    // Class that saves metrics across plays
    public DeathMetrics deathMetrics;
    public RoomGenerator roomGenerator;
    private string saveDataPath;

    public void Start()
    {
  
        // Get DeathMetrics INstance
        deathMetrics = DeathMetrics.Instance;


        // Reliable Path Across Devices
        saveDataPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveData.json");

        // Debug Path Save in Assets Folder
        //saveDataPath = Path.Combine(Application.dataPath + Path.AltDirectorySeparatorChar + "SaveData.json");

        // Reliable Path Across Devices
        saveDataPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveData.json");

        DeathMetricsManager.Singleton.ProcessStats();
        DeathMetricsManager.Singleton.DisplayStats();

    }

    public void Save(){
        SaveEntities();
        SaveGameStats();
    }

    public void Load(){
        LoadGameStats();
    }

    public void Reset()
    {
        ResetGameStats();   
    }

    public void SaveEntities(){
        List<String> enemies = roomGenerator.exportEnemyStrings();
        string json = JsonUtility.ToJson(enemies, true);  // Pretty print for readability

        using (StreamWriter writer = new StreamWriter(saveDataPath))
        {
            writer.Write(json);
        }

        Debug.Log($"Saving New Stats Json at path: {saveDataPath}");
    }

    public void SaveGameStats()
    {

        string json = JsonUtility.ToJson(deathMetrics, true);  // Pretty print for readability

        using (StreamWriter writer = new StreamWriter(saveDataPath))
        {
            writer.Write(json);
        }

        Debug.Log($"Saving New Stats Json at path: {saveDataPath}");

    }

    public void LoadGameStats()
    {
        if (File.Exists(saveDataPath))
        {
            string json = string.Empty;
            
            using(StreamReader reader = new StreamReader(saveDataPath))
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

        using (StreamWriter writer = new StreamWriter(saveDataPath))
        {
            writer.Write(json);
        }

        Debug.Log($"Saving New Stats Json at path: {saveDataPath}");

    }

    public void SaveEnemies(){

    }
}
