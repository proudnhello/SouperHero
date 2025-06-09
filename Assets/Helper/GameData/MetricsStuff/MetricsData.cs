using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetricsData
{

    public int curr_NumEnemiesKilled;
    public int curr_NumIngredientsCollected;
    public int curr_NumSoupsCooked;
    public float curr_RunTime;

    public int best_NumEnemiesKilled;
    public int best_NumIngredientsCollected;
    public int best_NumSoupsCooked;
    public float best_WinTime;

    public int totalDeaths;
    public int totalWins;

    public MetricsData()
    {
        this.curr_NumEnemiesKilled = best_NumEnemiesKilled = 0;
        this.curr_NumIngredientsCollected = best_NumIngredientsCollected = 0;
        this.curr_NumSoupsCooked = best_NumSoupsCooked = 0;
        this.curr_RunTime = best_WinTime = Mathf.Infinity;
        this.totalDeaths = 0;
        this.totalWins = 0;
    }

    // Save this data below for the metrics display
    public bool new_MaxNumEnemiesKilled, new_MaxNumIngredientsCollected, new_MaxNumSoupsCooked, new_FastestRun;
    public bool wasRunSuccessful;

    public void NewRun()
    {
        this.curr_NumEnemiesKilled = 0;
        this.curr_NumIngredientsCollected = 0;
        this.curr_NumSoupsCooked = 0;
        this.curr_RunTime = Mathf.Infinity;
    }

    public void FinishRun(bool successfulRun, float runTime)
    {
        new_MaxNumEnemiesKilled = curr_NumEnemiesKilled > best_NumEnemiesKilled;
        best_NumEnemiesKilled = Mathf.Max(curr_NumEnemiesKilled, best_NumEnemiesKilled);

        new_MaxNumIngredientsCollected = curr_NumIngredientsCollected > best_NumIngredientsCollected;
        best_NumIngredientsCollected = Mathf.Max(curr_NumIngredientsCollected, best_NumIngredientsCollected);

        new_MaxNumSoupsCooked = curr_NumSoupsCooked > best_NumSoupsCooked;
        best_NumSoupsCooked = Mathf.Max(curr_NumSoupsCooked, best_NumSoupsCooked);

        curr_RunTime = runTime;
        new_FastestRun = successfulRun && curr_RunTime < best_WinTime;
        if (successfulRun)
        {
            best_WinTime = Mathf.Min(runTime, best_WinTime);
            totalWins++;
            UnlockDataManager.Singleton.ReportAchievementProgress("Win1", 1, true);
        }
        else
        {
            totalDeaths++;
        }

        // if (successfulRun) totalWins++;
        // else totalDeaths++;

        wasRunSuccessful = successfulRun;
    }


}
