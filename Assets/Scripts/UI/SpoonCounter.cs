using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class SpoonCounter : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] List<GameObject> spoonList;
    // private int playerSpoons;

    void Start() {
        // Initialize with inactive spoons
        foreach (GameObject spoon in spoonList) {
            spoon.SetActive(false);
        }
    }

    void Update()
    {
        // Only update UI if the number of abilities changes
        // playerSpoons = UIManager.instance.playerSpoons;
    }

    public void AddSpoon(Color color, int playerSpoons) {
        Debug.Log("Spoon Count: " + playerSpoons);
        spoonList[playerSpoons].SetActive(true);
        UnityEngine.UI.Image spoonImage = spoonList[playerSpoons].GetComponent<UnityEngine.UI.Image>();
        spoonImage.color = color;

    }

    public void DeleteSpoon(int playerSpoons) {
        spoonList[playerSpoons-1].SetActive(false);

    }
}
