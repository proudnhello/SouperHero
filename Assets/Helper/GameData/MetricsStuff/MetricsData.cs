using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetricsData
{
    public int numEnemiesKilled;
    public int numIngredientsCollected;
    public int numSoupsCooked;
    public float bestWinTime;
    public int totalDeaths;
    public int totalWins;

    public MetricsData(int maxNumEnemiesKilled = 0, int maxNumIngredientsCollected = 0, int maxNumSoupsCooked = 0, float minWinTimeElapsed = Mathf.Infinity, int totalDeaths = 0, int totalWins = 0)
    {
        this.numEnemiesKilled = maxNumEnemiesKilled;
        this.numIngredientsCollected = maxNumIngredientsCollected;
        this.numSoupsCooked = maxNumSoupsCooked;
        this.bestWinTime = minWinTimeElapsed;
        this.totalDeaths = totalDeaths;
        this.totalWins = totalWins;
    }

    // Save this data below for the metrics display
    public bool new_MaxNumEnemiesKilled, new_MaxNumIngredientsCollected, new_MaxNumSoupsCooked, new_MinWinTimeElapsed;
    public float lastRunTime;
    public bool lastRunSuccessful;

    public void FinishRun(MetricsData lastMetrics, bool successfulRun, float runTime)
    {
        numEnemiesKilled = Mathf.Max(numEnemiesKilled, lastMetrics.numEnemiesKilled);
        new_MaxNumEnemiesKilled = numEnemiesKilled != lastMetrics.numEnemiesKilled;

        numIngredientsCollected = Mathf.Max(numIngredientsCollected, lastMetrics.numIngredientsCollected);
        new_MaxNumIngredientsCollected = numIngredientsCollected != lastMetrics.numIngredientsCollected;

        numSoupsCooked = Mathf.Max(numSoupsCooked, lastMetrics.numSoupsCooked);
        new_MaxNumSoupsCooked = numSoupsCooked != lastMetrics.numSoupsCooked;

        lastRunTime = runTime;
        bestWinTime = successfulRun ? Mathf.Min(runTime, lastMetrics.bestWinTime) : Mathf.Infinity;      
        new_MinWinTimeElapsed = bestWinTime != lastMetrics.bestWinTime;

        totalDeaths = lastMetrics.totalDeaths;
        totalWins = lastMetrics.totalWins;
        if (successfulRun) totalWins++;
        else totalDeaths++;

        lastRunSuccessful = successfulRun;
    }

    public void ResetStats()
    {
        this.numEnemiesKilled = 0;
        this.numIngredientsCollected = 0;
        this.numSoupsCooked = 0;
        this.bestWinTime = Mathf.Infinity;
        this.totalDeaths = 0;
        this.totalWins = 0;
    }

}
