using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private EnemyBaseClass enemy;
    [SerializeField] private TMP_Text debugText;

    private EnemyBaseClass enemyClass;
    // private float enemyHealth;
    void Start()
    {
        slider = GetComponent<Slider>();
        enemyClass = enemy.GetComponent<EnemyBaseClass>();
    }
    void Update()
    {
        debugText.text = enemyClass.GetHealth().ToString();

        float oldValue = slider.value;
        slider.value = (float) enemyClass.GetHealth() / enemyClass.GetBaseStats().maxHealth;
        float newValue = slider.value;

        if (oldValue != newValue)
        {
            Debug.Log("NEW SLIDER VALUE: " + slider.value);
        }
        
        if (enemy.GetHealth() == 0) {
            gameObject.SetActive(false);
        }
    }

}
