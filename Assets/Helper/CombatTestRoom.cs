using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using TMPro;
using System.Linq;
using Unity.VisualScripting;

public class CombatTestRoom : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject playerRef;

    public GameObject playScreen;
    public GameObject pauseScreen;
    public GameObject debugScreen;
    public KeyCode debugKey = KeyCode.J;
    public bool debugging = false;

    public List<GameObject> enemyList;
    public List<GameObject> destroyableList;
    public List<GameObject> forageableList;

    public TMP_Dropdown enemyDropdown;
    public TMP_Dropdown destroyableDropdown;
    public TMP_Dropdown forageableDropdown;

    public GameObject theParent;

    private Vector3 spawnPos;

    private int spawnIndex;

    private int enemyNum = 0;
    private int destroyableNum = 0;
    private int forageableNum = 0;

    void Start()
    {
        spawnPos = playerRef.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(debugKey) && debugScreen != null
            && !CookingManager.Singleton.IsCooking())
        {
            debugging = !debugging;
            if (debugging)
            {
                openDebugMenu();
            }
            else
            {
                closeDebugMenu();
            }
        }

    }

    public void openDebugMenu()
    {
        debugging = true;
        Time.timeScale = 0;
        CursorManager.Singleton.cursorObject.SetActive(false);
        debugScreen.SetActive(true);
        PlayerEntityManager.Singleton.input.Disable();
    }

    public void closeDebugMenu()
    {
        resetSpawnPos();
        debugging = false;
        Time.timeScale = 1;
        CursorManager.Singleton.cursorObject.SetActive(true);
        debugScreen.SetActive(false);
        PlayerEntityManager.Singleton.input.Enable();
    }

    void resetSpawnPos()
    {
        spawnPos = playerRef.transform.position;
    }

    public void enemyButton()
    {
        spawn(0);
        enemyNum++;
    }

    public void destroyableButton()
    {
        spawn(1);
    }

    public void forageableButton()
    {
        spawn(2);
    }

    public void spawn(int spawnIndex)
    {
        if (spawnIndex == 0)
        { //enemy spawn

            if (enemyNum == 0)
            {
                spawnPos.y -= 3.0f;
                Instantiate(enemyList[enemyDropdown.value], spawnPos, Quaternion.identity, theParent.transform);
            }
            else
            {
                spawnPos.y -= 2.0f;
                Instantiate(enemyList[enemyDropdown.value], spawnPos, Quaternion.identity, theParent.transform);
            }

            //Debug.Log(newThing);
        }
        else if (spawnIndex == 1)
        { //Destroyable Spawn
            Instantiate(destroyableList[destroyableDropdown.value]);
        }
        else if (spawnIndex == 2)
        { //forageable
            Instantiate(forageableList[forageableDropdown.value]);
        }


    }


}
