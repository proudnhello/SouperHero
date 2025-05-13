using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;

public class LanguageNavigation : MonoBehaviour
{
    public void ChangeLanguage(int langID)
    {
        LocalizationManager.Singleton.ChangeLanguage(langID);
    }
    // Goes back to Main Menu
    public void MainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
