using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Unity.VisualScripting;
using System;
//Input manager finds the current input system being used
//In theory, should swap between UI and Player input when pausing the game
//Right now the input is simply disabled. This is controlled in the GameManager
public class SaveManager : MonoBehaviour
{
    public void SaveGame(){
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        if(File.Exists(Application.persistentDataPath + "save.dat")){
            file = File.Open(Application.persistentDataPath + "save.dat", FileMode.Open);
        }
        else{
            file = File.Create(Application.persistentDataPath + "/save.dat");
        }

        SaveData data = new SaveData();
    }
}

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