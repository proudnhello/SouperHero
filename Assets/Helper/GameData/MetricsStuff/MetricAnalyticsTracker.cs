using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

public class MetricAnalyticsTracker : MonoBehaviour
{
    public static MetricAnalyticsTracker Singleton { get; private set; }
    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    MetricsAnalytics metricsAnalytics;
    // Start is called before the first frame update
    void Start()
    {
        // metricsAnalytics = SaveManager.Singleton.LoadMetricsAnalytics();

        if (metricsAnalytics == null)
        {
            metricsAnalytics = new();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // A class that stores player metrics in lists
    public class MetricsAnalytics
    {

        public List<int> curr_NumEnemiesKilled;
        public List<int> curr_NumIngredientsCollected;
        public List<int> curr_NumSoupsCooked;
        public List<float> curr_RunTime;

        public List<int> best_NumEnemiesKilled;
        public List<int> best_NumIngredientsCollected;
        public List<int> best_NumSoupsCooked;
        public List<float> best_WinTime;

        public List<int> totalDeaths;
        public List<int> totalWins;

        public List<int> runNumber;

        public MetricsAnalytics()
        {
            this.curr_NumEnemiesKilled = new();
            this.best_NumEnemiesKilled = new();
            this.curr_NumIngredientsCollected = new();
            this.best_NumIngredientsCollected = new();
            this.curr_NumSoupsCooked = new();
            this.best_NumSoupsCooked = new();
            this.curr_RunTime = new();
            this.best_WinTime = new();
            this.totalDeaths = new();
            this.totalWins = new();
            this.runNumber = new();
        }

        // Save this data below for the metrics display
        public bool new_MaxNumEnemiesKilled, new_MaxNumIngredientsCollected, new_MaxNumSoupsCooked, new_FastestRun;
        public bool wasRunSuccessful;
    }

    public List<int> hist_NumEnemiesKilled;
    public List<int> hist_NumIngredientsCollected;
    public List<int> hist_NumSoupsCooked;
    public List<float> hist_RunTime;
    public void RecordCurrentStatsHistory()
    {
        //MetricsData metrics = MetricsTracker.Singleton.metricsData;
        //hist_NumEnemiesKilled.Add(metrics.curr_NumEnemiesKilled);
        //hist_NumIngredientsCollected.Add(metrics.curr_NumIngredientsCollected);
        //hist_NumSoupsCooked.Add(metrics.curr_NumSoupsCooked);
        //hist_RunTime.Add(metrics.curr_RunTime);

        AddNewRow();
    }


    public void AddNewRow()
    {
        // Get current metrics
        MetricsData metrics = MetricsTracker.Singleton.metricsData;

        // Add new player
        int newPlayerIndex = metricsAnalytics.runNumber.Count;
        metricsAnalytics.runNumber.Add(newPlayerIndex);

        // Add new row of data
        metricsAnalytics.curr_NumEnemiesKilled.Add(metrics.curr_NumEnemiesKilled);
        metricsAnalytics.best_NumEnemiesKilled.Add(metrics.best_NumEnemiesKilled);
        metricsAnalytics.curr_NumIngredientsCollected.Add(metrics.curr_NumIngredientsCollected);
        metricsAnalytics.best_NumIngredientsCollected.Add(metrics.best_NumIngredientsCollected);
        metricsAnalytics.curr_NumSoupsCooked.Add(metrics.curr_NumSoupsCooked);
        metricsAnalytics.best_NumSoupsCooked.Add(metrics.best_NumSoupsCooked);
        metricsAnalytics.curr_RunTime.Add(metrics.curr_RunTime);
        metricsAnalytics.best_WinTime.Add(metrics.best_WinTime);
        metricsAnalytics.totalDeaths.Add(metrics.totalDeaths);
        metricsAnalytics.totalWins.Add(metrics.totalWins);

        // SaveManager.Singleton.SaveMetricsAnalytics(metricsAnalytics);
        Debug.Log("Analytics Saved!");
    }
}
