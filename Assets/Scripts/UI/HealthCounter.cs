using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] List<GameObject> heartList;
    private int heartCount = 0;
    private int playerHealth = PlayerManager.instance.health / 10;
    [Header("Debug")]
    public GameObject healthText;

    void Start() {
        // Initialize with all hearts as empty
        foreach(GameObject heart in heartList) {
            heart.SetActive(false);
        }

        for (int i = 0; i < playerHealth; i++) {
            heartList[i].SetActive(true);
            heartCount++;
        }
    }

    void Update()
    {
        HealthChange(); 
    }

    public void HealthChange() {
        healthText.GetComponent<TMP_Text>().text = PlayerManager.instance.health.ToString();
        playerHealth = PlayerManager.instance.health / 10;
        if (heartCount < playerHealth - 1) {
            AddHealth();
        }
        if (heartCount > playerHealth - 1) {
            RemoveHealth();
        }
    }

    void AddHealth() {
        Debug.Log("Health Count: " + heartCount + "Player Health " + playerHealth);
        heartList[heartCount].SetActive(true);
        heartCount++;

    }

    void RemoveHealth() {
        heartList[heartCount].SetActive(false);
        heartCount--;

    }
}
