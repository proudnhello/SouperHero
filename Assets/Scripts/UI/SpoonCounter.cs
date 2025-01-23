using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class SpoonCounter : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] List<GameObject> spoonList;
    private int playerSpoons;
    private List<AbilityAbstractClass> abilities;

    void Start() {
        // Initialize with inactive spoons
        abilities = PlayerManager.instance.GetAbilities();
        foreach (GameObject spoon in spoonList) {
            spoon.SetActive(false);
        }
    }

    void Update()
    {
        // Only update UI if the number of abilities changes
        int newSpoonCount = PlayerManager.instance.GetAbilities().Count;
        if (newSpoonCount != playerSpoons)
        {
            UpdateSpoons();
        }
    }
    public void UpdateSpoons() {
        if (playerSpoons < abilities.Count) {
            AddSpoon();
        }
        if (playerSpoons > abilities.Count) {
            DeleteSpoon();
        }
    }

    void AddSpoon() {
        Debug.Log("Spoon Count: " + playerSpoons + abilities);
        spoonList[playerSpoons].SetActive(true);
        playerSpoons++;

    }

    void DeleteSpoon() {
        spoonList[playerSpoons].SetActive(true);
        playerSpoons--;

    }
}
