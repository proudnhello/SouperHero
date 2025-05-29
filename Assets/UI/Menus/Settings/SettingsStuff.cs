using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsStuff : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    Resolution[] resolutions;
    List<Resolution> SelectedResolutionList = new List<Resolution>();
    int selectedRes;

    // Start is called before the first frame update
    void Start()
    {
        resolutions = Screen.resolutions;

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

    public void ChangeResolution()
    {
        selectedRes = resolutionDropdown.value;
        Screen.SetResolution(SelectedResolutionList[selectedRes].width, SelectedResolutionList[selectedRes].height, Screen.fullScreen);
    }

    public void SetFullscreen(bool full)
    {
        Screen.fullScreen = full;
    }
}
