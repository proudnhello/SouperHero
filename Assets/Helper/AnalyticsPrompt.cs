using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UnityServices.InitializeAsync();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject analyticsPanel;
    public void OptIn()
    {
        analyticsPanel.SetActive(false);
        AnalyticsService.Instance.StartDataCollection();
        StartTheMenu();
    }

    public void OptOut()
    {
        analyticsPanel.SetActive(false);
        AnalyticsService.Instance.StopDataCollection();
        StartTheMenu();
    }

    public GameObject player;
    public void StartTheMenu()
    {
        player.SetActive(true);
    }
}
