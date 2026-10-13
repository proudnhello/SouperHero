using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Saves stats throughout a playthrough
public class MetricsTracker : MonoBehaviour
{

    public static MetricsTracker Singleton { get; private set; }
    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    private void Start()
    {
        metricsData = SaveManager.Singleton.LoadMetricsData();
    }

    private float startTime;
    private float elapsedTime;

    internal MetricsData metricsData;

    public void StartRun()
    {
        startTime = Time.time;
        metricsData.NewRun();
    }

    public void EndRun(bool successfulRun)
    {
        elapsedTime = Time.time - startTime;
        metricsData.FinishRun(successfulRun, elapsedTime);
        SaveManager.Singleton.SaveMetricsData(metricsData);
    }

    public void RecordEnemyKilled()
    {
        if (GameManager.Singleton.TutorialRunning) return;
        metricsData.curr_NumEnemiesKilled++;
        UnlockDataManager.Singleton.ReportAchievementProgress("Kill100", 1, true);
    }

    public void RecordIngredientCollected()
    {
        if (GameManager.Singleton.TutorialRunning) return;
        metricsData.curr_NumIngredientsCollected++;
    }

    public void RecordSoupsCooked()
    {
        if (GameManager.Singleton.TutorialRunning) return;
        metricsData.curr_NumSoupsCooked++;
        UnlockDataManager.Singleton.ReportAchievementProgress("SoupsCooked1", 1, true);
        UnlockDataManager.Singleton.ReportAchievementProgress("SoupsCooked10", 1, true);
        UnlockDataManager.Singleton.ReportAchievementProgress("SoupsCooked50", 1, true);
    }
}
