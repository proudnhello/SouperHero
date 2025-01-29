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
        debugText.text = enemyClass.getCurrentHealth().ToString();
        slider.value = enemyClass.getCurrentHealth() / 100f;
        if (enemy.getCurrentHealth() == 0) {
            gameObject.SetActive(false);
        }
    }

}
