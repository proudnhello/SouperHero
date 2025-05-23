using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;



public class SaveManager : MonoBehaviour
{

    public static SaveManager Singleton { get; private set; }

    private string statsPath;
    private string runStatePath;
    private string settingsPath;

    [SerializeField] bool debugAlwaysGenerateNewLevel;

    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;

        GetPaths();

#if UNITY_EDITOR
        if (debugAlwaysGenerateNewLevel) ResetGameState();
#endif
    }

    void GetPaths()
    {
        // Reliable Path Across Devices
        runStatePath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "RunState.json");
        statsPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Stats.json");
        settingsPath = Path.Combine(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Settings.json");
    }

    [ContextMenu("Reset All Save Data")]
    public void ResetData()
    {
        GetPaths();
        try
        {
            File.Delete(runStatePath);
            File.Delete(statsPath);
        } catch (Exception e)
        {
            Debug.LogError("Error deleting save data " + e);
        }
        
    }
    [ContextMenu("Clear Run Loaded")]
    public void ResetGameState()
    {
        GetPaths();
        try
        {
            File.Delete(runStatePath);
        } catch (Exception e)
        {
            Debug.LogError("Error clearing loaded run " + e);
        }
        
    }

    public void SaveRunState(RunStateManager.RunStateData runData)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(runStatePath));

            string runDataJSON = JsonConvert.SerializeObject(runData, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            using (FileStream stream = new FileStream(runStatePath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(runDataJSON);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Run data save error: " + e);
        }
    }


    public RunStateManager.RunStateData LoadRunState()
    {
        if (File.Exists(runStatePath))
        {
            try
            {
                string runDataLoaded = "";
                using (FileStream stream = new FileStream(runStatePath, FileMode.Open))
                {
                    using StreamReader reader = new StreamReader(stream);
                    runDataLoaded = reader.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<RunStateManager.RunStateData>(runDataLoaded);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured while loading run data: " + e);
                return null;
            }
        }
        else
        {
            return null;
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

    public void SaveSettings(SettingsData data)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));

            string json = JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            using (FileStream stream = new FileStream(settingsPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(json);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Settings data save error: " + e);
        }
    }


    public SettingsData LoadSettingsData()
    {
        if (File.Exists(settingsPath))
        {
            try
            {
                string settingsDataLoaded = "";
                using (FileStream stream = new FileStream(settingsPath, FileMode.Open))
                {
                    using StreamReader reader = new StreamReader(stream);
                    settingsDataLoaded = reader.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<SettingsData>(settingsDataLoaded);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured while loading run data: " + e);
                return null;
            }
        }
        else
        {
            return new();
        }
    }

}
