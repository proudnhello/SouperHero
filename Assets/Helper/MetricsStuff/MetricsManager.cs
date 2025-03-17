using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Saves stats throughout a playthrough
public class MetricsManager : MonoBehaviour
{

    public static MetricsManager Singleton { get; private set; }
    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    private TimeSpan timePlaying;
    private bool isTimerGoing;
    private float elapsedTime;

    public void Start()
    {
        BeginTimer();
        ResetMetrics();
    }

    public MetricsSO metricsSO;
    public int numEnemiesKilled;
    public int numIngredientsCollected;
    public int numSoupsCooked;
    public float timeElapsed;
    public int numDeaths;
    public int numWins;

    public void ResetMetrics()
    {
        numEnemiesKilled = 0;
        numIngredientsCollected = 0;
        numSoupsCooked = 0;
        timeElapsed = 0f;
        numDeaths = 0;
        numWins = 0;
    }

    public void RecordEnemyKilled()
    {
        numEnemiesKilled++;
    }

    public void RecordIngredientCollected()
    {
        numIngredientsCollected++;
    }

    public void RecordSoupsCooked()
    {
        numSoupsCooked++;
    }

    public void RecordNumDeaths()
    {
        numDeaths++;
    }

    public void RecordNumWins()
    {
        numWins++;
    }

    public void SaveToMetricsToSO()
    {
        metricsSO.NumEnemiesKilled = numEnemiesKilled;
        metricsSO.NumIngredientsCollected = numIngredientsCollected;
        metricsSO.NumSoupsCooked = numSoupsCooked;
        metricsSO.TimeElapsed = timeElapsed;
        metricsSO.NumDeaths = numDeaths;
        metricsSO.NumWins = numWins;
    }

    public void BeginTimer()
    {
        isTimerGoing = true;
        elapsedTime = 0f;
    }

    public void EndTimer()
    {
        isTimerGoing = false;
        timeElapsed = elapsedTime;
        Debug.Log("Elapsed Time: " + elapsedTime.ToString("F2") + " seconds");
    }

    private void Update()
    {
        if (isTimerGoing)
        {
            elapsedTime += Time.deltaTime;
        }
    }

}
