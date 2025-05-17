using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class AnalyticsManager : MonoBehaviour
{

    public static AnalyticsManager Singleton { get; private set; }

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UnityServices.InitializeAsync();
    }

    public bool isAnalyticsEnabled;
    // Update is called once per frame
    void Update()
    {

    }

    public void OptIn()
    {
        isAnalyticsEnabled = true;
        AnalyticsService.Instance.StartDataCollection();
    }

    public void OptOut()
    {
        isAnalyticsEnabled = false;
        AnalyticsService.Instance.StopDataCollection();
    }

    public void RequestDataDeletion()
    {
        AnalyticsService.Instance.RequestDataDeletion();
    }

}