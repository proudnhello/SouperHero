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
        currentMetrics = new();
    }

    private float startTime;
    private float elapsedTime;

    internal MetricsData lastMetrics;
    internal MetricsData currentMetrics;

    public void StartRun()
    {
        startTime = Time.time;
        currentMetrics.ResetStats();
    }

    public void EndRun(bool successfulRun)
    {
        lastMetrics = SaveManager.Singleton.LoadMetricsData();
        elapsedTime = Time.time - startTime;
        currentMetrics.FinishRun(lastMetrics, successfulRun, elapsedTime);
        SaveManager.Singleton.SaveMetricsData(currentMetrics);
    }

    public void RecordEnemyKilled()
    {
        currentMetrics.numEnemiesKilled++;
    }

    public void RecordIngredientCollected()
    {
        currentMetrics.numIngredientsCollected++;
    }

    public void RecordSoupsCooked()
    {
        currentMetrics.numSoupsCooked++;
    }
}
