using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script for all collectable ingredients in the environment
//Based off of the Chest.cs script
public class Collectable : MonoBehaviour
{
    [SerializeField] CollectableObject collectableObj;
    [SerializeField] GameObject collectableUI;

    //Spawn ingredient gameObject at position
    public void Spawn(Vector2 spawnPoint)
    {
        collectableObj.Drop(spawnPoint);
    }

    //
    public void Collect()
    {
        collectableObj.gameObject.SetActive(false);
        collectableUI.SetActive(true);
        AddToPot.Singleton.AddIngredient(collectableUI);
    }
}