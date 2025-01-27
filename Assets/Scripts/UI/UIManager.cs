using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [SerializeField] private GameObject soupPot;
    private static SpoonCounter spoonCounter;
    public int playerSpoons;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start() {
        spoonCounter = soupPot.GetComponent<SpoonCounter>();
    }

    void Update() {
        UpdateAbilities();
        // int newSpoonCount = PlayerManager.instance.GetAbilities().Count;
        // UpdateSpoons(newSpoonCount);
    }

    void UpdateSpoons(int count) {
        if (playerSpoons < count) {
            spoonCounter.AddSpoon(Color.red, playerSpoons);
            playerSpoons++;
        }
        if (playerSpoons > count) {
            spoonCounter.DeleteSpoon(playerSpoons);
            playerSpoons--;
        }
    }

    public void UpdateAbilities() {
        int newSpoonCount = PlayerManager.instance.GetAbilities().Count;
        UpdateSpoons(newSpoonCount);
    }

}

