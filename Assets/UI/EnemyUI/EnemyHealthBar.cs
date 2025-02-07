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
        slider.value = enemyClass.GetHealth() / 100f;
        if (enemy.GetHealth() == 0) {
            gameObject.SetActive(false);
        }
    }

}
