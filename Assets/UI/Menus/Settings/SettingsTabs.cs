using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsTabs : MonoBehaviour
{
    public List<GameObject> UI_List = new List<GameObject>();

    private void Start()
    {
        ShowUI(0);
    }

    public void ShowUI(int index)
    {
        // Iterate through UI
        for (int i = 0; i < UI_List.Count; i++)
        {
            if (i == index)
            {
                // Show UI
                UI_List[i].SetActive(true);
            }
            else
            {
                // Hide UI
                UI_List[i].SetActive(false);
            }
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
