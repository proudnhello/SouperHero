using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using TMPro;

public class CombatTestRoom : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject playScreen;
    public GameObject pauseScreen;
    public GameObject debugScreen;
    public KeyCode debugKey = KeyCode.J;
    public bool debugging = false;

    public List<GameObject> enemyList;
    public List<GameObject> destroyableList;
    public List<GameObject> forageableList;

    private Dictionary<GameObject, int> enemyDict;
    private Dictionary<GameObject, int> destroyableDict;
    private Dictionary<GameObject, int> forageableDict;

    public TMP_InputField enemyInputNumber;
    public TMP_InputField destroyableInputNumber;
    public TMP_InputField forageableInputNumber;

    public TMP_Dropdown enemyDropdown;
    public TMP_Dropdown destroyableDropdown;
    public TMP_Dropdown forageableDropdown;

    public TMP_Text enemyQueueText;
    public TMP_Text destroyableQueueText;
    public TMP_Text forageableQueueText;

    private List<Dictionary<GameObject, int>> allObjectsToSpawn;


    void Start()
    {
        enemyDict = new Dictionary<GameObject, int>();
        destroyableDict = new Dictionary<GameObject, int>();
        forageableDict = new Dictionary<GameObject, int>();

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
        debugging = false;
        Time.timeScale = 1;
        CursorManager.Singleton.cursorObject.SetActive(true);
        debugScreen.SetActive(false);
        PlayerEntityManager.Singleton.input.Enable();
    }

    public void spawnAllObjects()
    {
        //loop through and spawn things at respective locations
    }

    public void plusEnemy()
    {
        Int32.TryParse(enemyInputNumber.text, out int temp);
        if (temp >= 99)
        {
            temp = 99;
        }
        else
        {
            temp += 1;
        }
        enemyInputNumber.text = temp.ToString();
    }

    public void plusDestoryable()
    {
        // enemyInputNumber.text -= 1;
        Int32.TryParse(destroyableInputNumber.text, out int temp);
        if (temp >= 99)
        {
            temp = 99;
        }
        else
        {
            temp += 1;
        }
        destroyableInputNumber.text = temp.ToString();

    }

    public void plusForageable()
    {
        Int32.TryParse(forageableInputNumber.text, out int temp);
        if (temp >= 99)
        {
            temp = 99;
        }
        else
        {
            temp += 1;
        }
        forageableInputNumber.text = temp.ToString();

    }

    public void minusEnemy()
    {
        Int32.TryParse(enemyInputNumber.text, out int temp);
        if (temp <= 0)
        {
            temp = 0;
        }
        else
        {
            temp -= 1;
        }
        enemyInputNumber.text = temp.ToString();
    }

    public void minusDestroyable()
    {
        Int32.TryParse(destroyableInputNumber.text, out int temp);
        if (temp <= 0)
        {
            temp = 0;
        }
        else
        {
            temp -= 1;
        }
        destroyableInputNumber.text = temp.ToString();
    }

    public void minusForageable()
    {
        Int32.TryParse(forageableInputNumber.text, out int temp);
        if (temp <= 0)
        {
            temp = 0;
        }
        else
        {
            temp -= 1;
        }
        forageableInputNumber.text = temp.ToString();
    }

    public void addEnemy()
    {
        Int32.TryParse(enemyInputNumber.text, out int numberOf);
        GameObject dropdownGO = enemyList[enemyDropdown.value];

        try{
            enemyDict.Add(dropdownGO, numberOf);
        } catch (ArgumentException){
            enemyDict[dropdownGO] += numberOf;
        }
        
        updateEnemyQueueText();
    }

    public void updateEnemyQueueText()
    {
        //key - value
        enemyQueueText.text = "";
        foreach (KeyValuePair<GameObject, int> kvp in enemyDict)
        {
            //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            enemyQueueText.text += string.Format("{0}, {1}X\n", kvp.Key.name, kvp.Value);
        }
        //Debug.Log(enemyDict.Values);
    }



}
