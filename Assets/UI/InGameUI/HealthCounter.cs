using System;
using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;

public class HealthCounter : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] List<GameObject> heartList;
    private int heartCount = 0;
    private int playerHealth = 0; 

    [SerializeField]
    Sprite fullHeart;
    [SerializeField]
    Sprite emptyHeart;

    void Start() {
        // Initialize with all hearts as empty
        foreach(GameObject heart in heartList) {
            heart.SetActive(false);
        }
        playerHealth = PlayerEntityManager.Singleton.GetHealth() / 10;

        for (int i = 0; i < playerHealth; i++) {
            heartList[i].SetActive(true);
            heartCount++;
        }
        PlayerEntityManager.HealthChange += HealthChange;
    }

    //// What did I say about not having duplicate copies of stats?
    //// Hacky fix to make sure the health is updated
    //public void FixedUpdate()
    //{
    //    HealthChange();
    //}

    private void OnDisable()
    {
        PlayerEntityManager.HealthChange -= HealthChange;
    }

    public void HealthChange() {
        playerHealth = Mathf.CeilToInt(PlayerEntityManager.Singleton.GetHealth() / 10f);
        if (heartCount < playerHealth) {
            AddHealth(heartCount, playerHealth);
        }
        if (heartCount > playerHealth && heartCount > 0) {
            RemoveHealth(heartCount, playerHealth);
        }
    }

    void AddHealth(int heart, int pHealth) {

        //Set heart to empty heart sprite
        int heartCounter = heart;
        while (heartCounter <= pHealth)
        {
            heartList[heartCounter - 1].GetComponent<Image>().sprite = fullHeart;
            heartCounter++;
        }

        heartCount = pHealth;


    }

    void RemoveHealth(int heart, int pHealth) {
        if (heartCount-1 < 0)
        {
            return;
        }

        // Set heart to full heart sprite
        int heartCounter = heart;
        while (heartCounter > pHealth)
        {
            heartList[heartCounter - 1].GetComponent<Image>().sprite = emptyHeart;
            heartCounter--;
        }

        heartCount = pHealth;

    }
}
