using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

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

    private List<List<GameObject>> objectsToSpawn;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(debugKey) && debugScreen != null 
            && !CookingManager.Singleton.IsCooking()){
                debugging = !debugging;
                if(debugging){
                    openDebugMenu();
                } else {
                    closeDebugMenu();
                }
            } 
            
    }

    public void openDebugMenu(){
        debugging = true;
        Time.timeScale = 0;
        CursorManager.Singleton.cursorObject.SetActive(false);
        debugScreen.SetActive(true);
        PlayerEntityManager.Singleton.input.Disable();
    }

    public void closeDebugMenu(){
        debugging = false;
        Time.timeScale = 1;
        CursorManager.Singleton.cursorObject.SetActive(true);
        debugScreen.SetActive(false);
        PlayerEntityManager.Singleton.input.Enable();
    }

    public void spawnAllObjects(){
        //loop through and spawn things at respective locations
    }
    


}
