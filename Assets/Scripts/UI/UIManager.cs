using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject healthText;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start() {

    }
    public void HealthChange() {
        healthText.GetComponent<TMP_Text>().text = PlayerManager.instance.health.ToString();

    }

    void AbilityChange() {
        // abilityText.GetComponent<TMP_Text>().text = 
    }

    void Update()
    {
        HealthChange();
    }
}
