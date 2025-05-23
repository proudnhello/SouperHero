using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public class SettingsData
{
    public int language = 0;
    public int useGrayscale = 0;
    public int resolution = 0;
    public int fullScreen = 1;
}


public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Singleton { get; private set; }
    internal SettingsData settingsData;

    [SerializeField] GrayscaleRenderFeature grayscaleRenderFeature;
    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    private void Start()
    {
        settingsData = SaveManager.Singleton.LoadSettingsData();
        LocalizationManager.Singleton.ChangeLanguage(Language);
        Screen.fullScreen = FullScreen;
        grayscaleRenderFeature.UseGrayscale = UseGrayscale;
    }

    // idea from https://www.reddit.com/r/csharp/comments/11a2dvw/why_use_get_set_at_all/
    private void SetProperty(ref int d, int value)
    {
        d = value;
        SaveManager.Singleton.SaveSettings(settingsData);
    }

    public int Language
    {
        get => settingsData.language;
        set
        {
            SetProperty(ref settingsData.language, value);
            LocalizationManager.Singleton.ChangeLanguage(value);
        }
    }

    public bool UseGrayscale
    {
        get => settingsData.useGrayscale == 1;
        set
        {
            SetProperty(ref settingsData.useGrayscale, value ? 1 : 0);
            grayscaleRenderFeature.UseGrayscale = value;
        }
    }

    public int Resolution
    {
        get => settingsData.resolution;
        set => SetProperty(ref settingsData.resolution, value);
    }

    public bool FullScreen
    {
        get => settingsData.fullScreen == 1;
        set
        {
            SetProperty(ref settingsData.fullScreen, value ? 1 : 0);
            Screen.fullScreen = value;
        }
    }

}

