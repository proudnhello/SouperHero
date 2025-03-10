using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Unity.VisualScripting;
using System;
using Unity.Profiling;
//Input manager finds the current input system being used
//In theory, should swap between UI and Player input when pausing the game
//Right now the input is simply disabled. This is controlled in the GameManager
public class SaveManager : MonoBehaviour
{
    public static SaveManager Singleton { get; private set; }
    private String saveDataPath = Application.persistentDataPath + "save.dat";
    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }
    public void SaveGame(){
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        if(File.Exists(saveDataPath)){
            file = File.Open(saveDataPath, FileMode.Open);
        }
        else{
            file = File.Create(saveDataPath);
        }

        SaveData data = new SaveData();
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Game saved");
    }

    public void LoadGame(){
        if(File.Exists(saveDataPath)){
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(saveDataPath, FileMode.Open);   
            SaveData data = (SaveData)bf.Deserialize(file);

            // TODO: figure out what to reload
        }

    }
}

// TODO: figure out what else to save
[Serializable]
public class SaveData{
    public List<String> roomEnemies;
    public List<String> roomForagables;
    public Vector2 playerPosition;
    
    public SaveData(){
        roomEnemies = new List<String>();
        roomForagables = new List<String>();
    }
}