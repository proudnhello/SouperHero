/*
 * This file was modified with LLMs: 
 * https://github.com/djlouie/project-soup-chat-logs/blob/main/logs/log15.md
 * https://github.com/djlouie/project-soup-chat-logs/blob/main/logs/log08.md
 */

using System.Collections.Generic;

using UnityEngine;


public class HealthCounter : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] List<GameObject> heartList;
    private int playerHealth = 0;

    [SerializeField]
    Sprite fullHeart;
    [SerializeField]
    Sprite emptyHeart;

    void Start()
    {
        // Initialize with all hearts as empty
        foreach (GameObject heart in heartList)
        {
            heart.SetActive(false);
        }
        playerHealth = PlayerEntityManager.Singleton.GetHealth() / 10;

        for (int i = 0; i < playerHealth; i++)
        {
            heartList[i].SetActive(true);
        }
        PlayerEntityManager.HealthChange += HealthChange;
    }
    private void OnDisable()
    {
        PlayerEntityManager.HealthChange -= HealthChange;
    }


    public void HealthChange()
    {
        float playerHealth = PlayerEntityManager.Singleton.GetHealth() / 10f;

        float maxHealth = PlayerEntityManager.Singleton.GetBaseStats().maxHealth / 10f;

        for (int i = 0; i < Mathf.CeilToInt(maxHealth); i++)
        {
            if (i > Mathf.FloorToInt(playerHealth))
            {
                heartList[i].transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
            }
            else if (i < Mathf.FloorToInt(playerHealth))
            {
                heartList[i].transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            }
        }

        heartList[Mathf.FloorToInt(playerHealth)].transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(100 * (playerHealth % 10) / 10, 100 * (playerHealth % 10) / 10);

    }
}
