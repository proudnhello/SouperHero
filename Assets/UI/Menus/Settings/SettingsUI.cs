using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown languageDropdown;
    public Toggle fullscreenToggle;
    public Toggle useGrayscale;
    public Toggle cursorMoveToggle;

    [SerializeField] GameObject movementKeybindObject;
    [SerializeField] GameObject cursorMovementKeybindObject;


    List<Resolution> SelectedResolutionList = new List<Resolution>();

    private void Awake()
    {
        Resolution[] resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> resolutionStringList = new List<string>();
        string newRes;
        foreach (Resolution res in resolutions)
        {
            newRes = res.width.ToString() + " x " + res.height.ToString();
            if (!resolutionStringList.Contains(newRes))
            {
                resolutionStringList.Add(newRes);
                SelectedResolutionList.Add(res);
            }
        }

        resolutionDropdown.AddOptions(resolutionStringList);
    }

    private void Start()
    {
        resolutionDropdown.value = SettingsManager.Singleton.Resolution;
        languageDropdown.value = SettingsManager.Singleton.Language;
        fullscreenToggle.isOn = SettingsManager.Singleton.FullScreen;
        useGrayscale.isOn = SettingsManager.Singleton.UseGrayscale;
        cursorMoveToggle.isOn = SettingsManager.Singleton.CursorMovement;
        CursorMove(cursorMoveToggle.isOn);
    }

    public void ChangeResolution(int selectedRes)
    {
        Screen.SetResolution(SelectedResolutionList[selectedRes].width, SelectedResolutionList[selectedRes].height, Screen.fullScreen);
        SettingsManager.Singleton.Resolution = selectedRes;
    }

    public void SetFullscreen(bool full)
    {
        SettingsManager.Singleton.FullScreen = full;
    }

    public void ChangeLanguage(int langID)
    {
        SettingsManager.Singleton.Language = langID;
    }

    public void UseGrayscale(bool use)
    {
        SettingsManager.Singleton.UseGrayscale = use;
    }

    public void CursorMove(bool isOn)
    {
        if (isOn)
        {
            movementKeybindObject.SetActive(false);
            cursorMovementKeybindObject.SetActive(true);
        }
        else
        {
            movementKeybindObject.SetActive(true);
            cursorMovementKeybindObject.SetActive(false);
        }
        SettingsManager.Singleton.CursorMovement = isOn;
    }
}