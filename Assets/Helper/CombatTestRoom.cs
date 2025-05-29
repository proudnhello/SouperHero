using System;
using System.Collections;
using System.Collections.Generic;
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
    public GameObject campfire;

    private Vector3 enemySpawnPos;
    private Vector3 destoryableSpawnPos;
    private Vector3 forageableSpawnPos;
    private Vector3 campfireSpawnPos;

    private int spawnIndex;

    private int enemyNum = 0;
    private int destroyableNum = 0;
    private int forageableNum = 0;
    private int campfireNum = 0;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(debugKey) && debugScreen != null
            && !CookingScreen.Singleton.IsCooking)
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
        setPosition();
        debugging = true;
        Time.timeScale = 0;
        debugScreen.SetActive(true);
        playScreen.SetActive(false);
        PlayerEntityManager.Singleton.input.Disable();
    }

    public void closeDebugMenu()
    {
        resetSpawnPos();
        debugging = false;
        Time.timeScale = 1;
        debugScreen.SetActive(false);
        playScreen.SetActive(true);

        PlayerEntityManager.Singleton.input.Enable();
    }

    void resetSpawnPos()
    {
        setPosition();
    }

    public void enemyButton()
    {
        spawn(0);
        enemyNum++;
    }

    public void destroyableButton()
    {
        spawn(1);
        destroyableNum++;
    }

    public void forageableButton()
    {
        spawn(2);
        forageableNum++;
    }

    public void campfireButton(){
        spawn(3);
        campfireNum++;
    }

    public void spawn(int spawnIndex)
    {
        if (spawnIndex == 0)
        { //enemy spawn

            if (enemyNum == 0)
            {
                enemySpawnPos.y -= 3.0f;
                Instantiate(enemyList[enemyDropdown.value], enemySpawnPos, Quaternion.identity, theParent.transform);
            }
            else
            {
                enemySpawnPos.y -= 2.0f;
                Instantiate(enemyList[enemyDropdown.value], enemySpawnPos, Quaternion.identity, theParent.transform);
            }
        }
        else if (spawnIndex == 1)
        { //Destroyable Spawn
            if (destroyableNum == 0)
            {
                destoryableSpawnPos.x -= 3.0f;
                Instantiate(destroyableList[destroyableDropdown.value], destoryableSpawnPos, Quaternion.identity, theParent.transform);
            }
            else
            {
                destoryableSpawnPos.x -= 2.0f;
                Instantiate(destroyableList[destroyableDropdown.value], destoryableSpawnPos, Quaternion.identity, theParent.transform);
            }
        }
        else if (spawnIndex == 2)
        { //forageable
            if (forageableNum == 0)
            {
                forageableSpawnPos.x += 3.0f;
                Instantiate(forageableList[forageableDropdown.value], forageableSpawnPos, Quaternion.identity, theParent.transform);
            }
            else
            {
                forageableSpawnPos.x += 2.0f;
                Instantiate(forageableList[forageableDropdown.value], forageableSpawnPos, Quaternion.identity, theParent.transform);
            }
        }
        else if(spawnIndex == 3){ // campfire spawn
            if (campfireNum == 0)
            {
                campfireSpawnPos.y += 3.0f;
                Instantiate(campfire, campfireSpawnPos, Quaternion.identity, theParent.transform);
            }
            else
            {
                campfireSpawnPos.y += 2.0f;
                Instantiate(campfire, campfireSpawnPos, Quaternion.identity, theParent.transform);
            }
        }
    }

   
    public void setPosition(){
        enemySpawnPos = playerRef.transform.position;
        destoryableSpawnPos = playerRef.transform.position;
        forageableSpawnPos = playerRef.transform.position;
        campfireSpawnPos = playerRef.transform.position;
    }

}
