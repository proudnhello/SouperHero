using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public class HealthCounter : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] List<GameObject> heartList;
    private int heartCount = 0;
    private int playerHealth = 0; 
    [Header("Debug")]
    public TMP_Text healthText;

    void Start() {
        // Initialize with all hearts as empty
        foreach(GameObject heart in heartList) {
            heart.SetActive(false);
        }
        playerHealth = PlayerManager.Singleton.GetHealth() / 10;
        for (int i = 0; i < playerHealth; i++) {
            heartList[i].SetActive(true);
            heartCount++;
        }
        healthText.text = PlayerManager.Singleton.GetHealth().ToString();
        PlayerHealth.HealthChange += HealthChange;
    }

    // What did I say about not having duplicate copies of stats?
    // Hacky fix to make sure the health is updated
    public void FixedUpdate()
    {
        HealthChange();
    }

    private void OnDisable()
    {
        PlayerHealth.HealthChange -= HealthChange;
    }

    public void HealthChange() {
        healthText.text = PlayerManager.Singleton.GetHealth().ToString();
        playerHealth = PlayerManager.Singleton.GetHealth() / 10;
        if (heartCount < playerHealth - 1) {
            AddHealth();
        }
        if (heartCount > playerHealth - 1 && heartCount > 0) {
            RemoveHealth();
        }
    }

    void AddHealth() {
        heartList[heartCount-1].SetActive(true);
        heartCount++;

    }

    void RemoveHealth() {
        if (heartCount-1 < 0)
        {
            Debug.Log("Error: heartCount is already 0!!!");
            return;
        }

        heartList[heartCount-1].SetActive(false);
        heartCount--;

    }
}
