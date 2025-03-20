using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMetrics
{

    public int maxNumEnemiesKilled;
    public int maxNumIngredientsCollected;
    public int maxNumSoupsCooked;
    public float minWinTimeElapsed;
    public int totalDeaths;
    public int totalWins;

    private static DeathMetrics _instance = null;

    public static DeathMetrics Instance
    {
        get
        { 
            if (_instance == null)
            {
                return new DeathMetrics(0, 0, 0, Mathf.Infinity, 0, 0);
            }
            return _instance; 
        }

    }

    private DeathMetrics(int maxNumEnemiesKilled, int maxNumIngredientsCollected, int maxNumSoupsCooked, float minWinTimeElapsed, int totalDeaths, int totalWins)
    {
        this.maxNumEnemiesKilled = maxNumEnemiesKilled;
        this.maxNumIngredientsCollected = maxNumIngredientsCollected;
        this.maxNumSoupsCooked = maxNumSoupsCooked;
        this.minWinTimeElapsed = minWinTimeElapsed;
        this.totalDeaths = totalDeaths;
        this.totalWins = totalWins;
    }

    public void ResetStats()
    {
        this.maxNumEnemiesKilled = 0;
        this.maxNumIngredientsCollected = 0;
        this.maxNumSoupsCooked = 0;
        this.minWinTimeElapsed = Mathf.Infinity;
        this.totalDeaths = 0;
        this.totalWins = 0;
    }

}
