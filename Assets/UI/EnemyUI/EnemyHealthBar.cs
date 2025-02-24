using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private EnemyBaseClass enemy;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text statusText;

    private EnemyBaseClass enemyClass;
    // private float enemyHealth;
    void Start()
    {
        slider = GetComponent<Slider>();
        enemyClass = enemy.GetComponent<EnemyBaseClass>();
    }
    void Update()
    {

        // calculate health ratio
        healthText.text = enemyClass.GetHealth().ToString();

        // set value to health ratio
        float oldValue = slider.value;
        slider.value = (float) enemyClass.GetHealth() / enemyClass.GetBaseStats().maxHealth;
        float newValue = slider.value;

        // display status's
        statusText.text = "";
        foreach (var key in enemy.inflictionHandler.GetActiveStatuses().Keys)
        {
            statusText.text += key.ToString().Split('_')[1] + ", ";
        }

        // deactivate if health is 0
        if (enemy.GetHealth() == 0) {
            gameObject.SetActive(false);
            statusText.enabled = false;
        }
    }

}
