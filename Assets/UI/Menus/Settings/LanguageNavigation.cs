using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;

public class LanguageNavigation : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    public void ChangeLanguage()
    {
        int langID = dropdown.value;
        LocalizationManager.Singleton.ChangeLanguage(langID);
    }
}
