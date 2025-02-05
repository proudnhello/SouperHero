using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCurrentEffect : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private EnemyBaseClass enemy;
    [SerializeField] private TMP_Text effectText;

    private EnemyBaseClass enemyClass;
    // private float enemyHealth;
    void Start()
    {
        slider = GetComponent<Slider>();
        enemyClass = enemy.GetComponent<EnemyBaseClass>();
    }
    void Update()
    {
        // effectText.text 
        
    }

}
