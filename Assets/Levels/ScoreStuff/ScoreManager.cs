using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Saves stats throughout a playthrough
public class ScoreManager : MonoBehaviour
{

    public static ScoreManager Singleton { get; private set; }
    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    public int numEnemiesKilled;
    public int numIngredientsCollected;
    public int numSoupsCooked;
    public int timeElapsed;
    public int numDeaths;
    public int numWins;

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


}
