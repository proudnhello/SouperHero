using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

public class AnalyticsUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject analyticsPanel;
    public void YesToAnalytics()
    {
        analyticsPanel.SetActive(false);
        AnalyticsManager.Singleton.OptIn();
        StartTheMenu();
    }

    public void NoToAnalytics()
    {
        analyticsPanel.SetActive(false);
        AnalyticsManager.Singleton.OptOut();
        StartTheMenu();
    }

    public GameObject player;
    public void StartTheMenu()
    {
        player.SetActive(true);
    }
}
