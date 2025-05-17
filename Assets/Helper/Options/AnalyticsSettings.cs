using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnalyticsSettings : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        allowAnalytics.isOn = AnalyticsManager.Singleton.isAnalyticsEnabled;
    }

    public Toggle allowAnalytics;
    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleAnalytics()
    {
        if (allowAnalytics.isOn)
        {
            AnalyticsManager.Singleton.OptIn();
        }
        else
        {
            AnalyticsManager.Singleton.OptOut();
        }

    }
}
