using System;
using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;
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
        playerHealth = PlayerEntityManager.Singleton.GetHealth() / 10;

        Debug.Log("Player Health In UI:" + playerHealth);
        for (int i = 0; i < playerHealth; i++) {
            heartList[i].SetActive(true);
            heartCount++;
        }
        Debug.Log("Heart Count In UI:" + heartCount);
        healthText.text = PlayerEntityManager.Singleton.GetHealth().ToString();
        PlayerEntityManager.HealthChange += HealthChange;
    }

    // What did I say about not having duplicate copies of stats?
    // Hacky fix to make sure the health is updated
    public void FixedUpdate()
    {
        HealthChange();
    }

    private void OnDisable()
    {
        PlayerEntityManager.HealthChange -= HealthChange;
    }

    public void HealthChange() {
        healthText.text = PlayerEntityManager.Singleton.GetHealth().ToString();
        playerHealth = PlayerEntityManager.Singleton.GetHealth() / 10;
        if (heartCount < playerHealth) {
            AddHealth();
        }
        if (heartCount > playerHealth && heartCount > 0) {
            RemoveHealth();
        }
    }

    void AddHealth() {
        //heartList[heartCount-1].SetActive(true);

        //Set heart to be fully opaque
        Color newColor = heartList[heartCount - 1].gameObject.GetComponent<Image>().color;
        newColor.a = 1f;
        heartList[heartCount - 1].gameObject.GetComponent<Image>().color = newColor;

        heartCount++;


    }

    void RemoveHealth() {
        if (heartCount-1 < 0)
        {
            Debug.Log("Error: heartCount is already 0!!!");
            return;
        }

        //heartList[heartCount-1].SetActive(false);

        //Set heart container to be slightly translucent
        Color newColor = heartList[heartCount - 1].gameObject.GetComponent<Image>().color;
        newColor.a = 0.3f;
        heartList[heartCount - 1].gameObject.GetComponent<Image>().color = newColor;

        heartCount--;

    }
}
