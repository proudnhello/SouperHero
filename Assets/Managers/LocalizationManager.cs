using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class LocalizationManager : MonoBehaviour
{
    public LocalizationManager Singleton { get; private set; }

    public static TableReference tableReference = "Languages";      // the localization table we'll use by default for non-dialogue text
    public static TableReference DialogueTableReference = "Dialogue";   // the localization table we'll use for dialogue text

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(this);
        }
        else
        {
            Singleton = this;
            DontDestroyOnLoad(this);
        }
    }

    private bool active = false;

    public static TableReference GetTable()
    {
        return tableReference;
    }

    public static TableReference GetDialogueTable()
    {
        return DialogueTableReference;
    }

    public static string GetLocalizedString(string key)
    {
        LocalizedString localString = new LocalizedString(LocalizationManager.GetTable(), key);
        return localString.GetLocalizedString();
    }

    public static string GetLocalizedDialogue(string key)
    {
        LocalizedString localString = new LocalizedString(LocalizationManager.GetDialogueTable(), key);
        return localString.GetLocalizedString();
    }

    public void ChangeLanguage(int langID)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[langID];
    }
    
    IEnumerator SetLanguage(int langID)
    {
        active = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[langID];
        active = false;
    }
}
