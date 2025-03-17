using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeathMetricsManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI metricsText;

    // Current Run Stats
    public MetricsSO metricsSO;
    public int numEnemiesKilled;
    public int numIngredientsCollected;
    public int numSoupsCooked;
    public float timeElapsed;
    public int numDeaths;
    public int numWins;

    // Previous Playthrough Stats
    public int maxNumEnemiesKilled;
    public int maxNumIngredientsCollected;
    public int maxNumSoupsCooked;
    public float minTimeElapsed;
    public int totalDeaths;
    public int totalWins;

    // Bool if new best stats
    bool newMaxNumEnemiesKilled;
    bool newMaxNumIngredientsCollected;
    bool newMaxNumSoupsCooked;
    bool newMinTimeElapsed;

    private TimeSpan timeFormatter;

    public static DeathMetricsManager Singleton { get; private set; }

    public void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }

    // On start add the stats text from the scriptable object that saves the stats
    void Start()
    {
        //ProcessStats();
        //DisplayStats();
    }

    public void DisplayStats()
    {
        metricsText.text = "";
        //metricsText.text += LocalizationManager.GetLocalizedString("Monsters Slain") + $"<color=#F4B07D>{metricsSO.NumEnemiesKilled}</color>" + "\n";
        //metricsText.text += LocalizationManager.GetLocalizedString("Ingredients Collected") + $"<color=#F4B07D>{metricsSO.NumIngredientsCollected}</color>" + "\n";
        //metricsText.text += LocalizationManager.GetLocalizedString("Soups Cooked") + $"<color=#F4B07D>{metricsSO.NumSoupsCooked}</color>" + "\n";

        //// Format time from seconds to minutes (and hours if needed)
        //timeFormatter = TimeSpan.FromSeconds(metricsSO.TimeElapsed);
        //string timeElapsed;
        //if (metricsSO.TimeElapsed < 3600)

        //if (newMaxNumEnemiesKilled)
        //{
        //    metricsText.text += "Monsters Slain: " +
        //        $"<color=#F4B07D>{numEnemiesKilled}</color>" +
        //        $"<color=#A9A9A9> (New Best!)</color>" +
        //        "\n";
        //}
        //else
        //{
        //    metricsText.text += "Monsters Slain: " +
        //        $"<color=#F4B07D>{numEnemiesKilled}</color>" +
        //        $"<color=#A9A9A9> (Best: {maxNumEnemiesKilled})</color>" +
        //        "\n";
        //}
        //metricsText.text += LocalizationManager.GetLocalizedString("Time Elapsed") + $"<color=#F4B07D>{timeElapsed}</color>" + "\n";

        //metricsText.text += LocalizationManager.GetLocalizedString("Total Deaths") + $"<color=#F4B07D>{metricsSO.NumDeaths}</color>" + "\n";
        //metricsText.text += LocalizationManager.GetLocalizedString("Total Wins") + $"<color=#F4B07D>{metricsSO.NumWins}</color>" + "\n";

        if (newMaxNumIngredientsCollected)
        {
            metricsText.text += "Ingredients Collected: " +
                $"<color=#F4B07D>{numIngredientsCollected}</color>" +
                $"<color=#A9A9A9> (New Best!)</color>" +
                "\n";
        }
        else
        {
            metricsText.text += "Ingredients Collected: " +
                $"<color=#F4B07D>{numIngredientsCollected}</color>" +
                $"<color=#A9A9A9> (Best: {maxNumIngredientsCollected})</color>" +
                "\n";
        }

        if (newMaxNumSoupsCooked)
        {
            metricsText.text += "Soups Cooked: " +
                $"<color=#F4B07D>{numSoupsCooked}</color>" +
                $"<color=#A9A9A9> (New Best!)</color>" +
                "\n";
        }
        else
        {
            metricsText.text += "Soups Cooked: " +
                $"<color=#F4B07D>{numSoupsCooked}</color>" +
                $"<color=#A9A9A9> (Best: {maxNumSoupsCooked})</color>" +
                "\n";
        }


        // Format time from seconds to minutes (and hours if needed)
        timeFormatter = TimeSpan.FromSeconds(timeElapsed);
        string timeElapsedStr;
        if (metricsSO.TimeElapsed < 3600)
        {
            timeElapsedStr = timeFormatter.ToString("mm':'ss'.'ff");
        }
        else
        {
            timeElapsedStr = timeFormatter.ToString("hh':'mm':'ss'.'ff");
        }

        //// Format time from seconds to minutes (and hours if needed)
        //timeFormatter = TimeSpan.FromSeconds(minTimeElapsed);
        //string minTimeElapsedStr;
        //if (metricsSO.TimeElapsed < 3600)
        //{
        //    minTimeElapsedStr = timeFormatter.ToString("mm':'ss'.'ff");
        //}
        //else
        //{
        //    minTimeElapsedStr = timeFormatter.ToString("hh':'mm':'ss'.'ff");
        //}

        //if (newMinTimeElapsed)
        //{
        //    metricsText.text += "Time Elapsed: " +
        //        $"<color=#F4B07D>{timeElapsedStr}</color>" +
        //         $"<color=#A9A9A9> (New Best!)</color>" +
        //        "\n";
        //}
        //else
        //{
        //    metricsText.text += "Time Elapsed: " +
        //        $"<color=#F4B07D>{timeElapsedStr}</color>" +
        //         $"<color=#A9A9A9> (Best: {minTimeElapsedStr})</color>" +
        //        "\n";
        //}

        metricsText.text += "Time Elapsed: " +
            $"<color=#F4B07D>{timeElapsedStr}</color>" +
            "\n";

        metricsText.text += "Total Deaths: " + $"<color=#F4B07D>{totalDeaths}</color>" + "\n";
        metricsText.text += "Total Wins: " + $"<color=#F4B07D>{totalWins}</color>" + "\n";
    }

    // Process Stats
    public void ProcessStats()
    {
        LoadMaxStats();
        ReadMetricsFromSO();
        SetNewBestStats();
        SaveNewMaxStats();
    }

    // Load Previous Playthrough Best Stats From JSON
    private void LoadMaxStats()
    {
        SaveManager.Singleton.LoadGameStats();

        if (SaveManager.Singleton == null)
        {
            Debug.LogError("SaveManager.Singleton is null!");
            return;
        }

        if (SaveManager.Singleton.deathMetrics == null)
        {
            Debug.LogError("SaveManager.Singleton.deathMetrics is null!");
            return;
        }

        maxNumEnemiesKilled = SaveManager.Singleton.deathMetrics.maxNumEnemiesKilled;
        maxNumIngredientsCollected = SaveManager.Singleton.deathMetrics.maxNumIngredientsCollected;
        maxNumSoupsCooked = SaveManager.Singleton.deathMetrics.maxNumSoupsCooked;
        minTimeElapsed = SaveManager.Singleton.deathMetrics.minTimeElapsed;
        totalDeaths = SaveManager.Singleton.deathMetrics.totalDeaths;
        totalWins = SaveManager.Singleton.deathMetrics.totalWins;
    }

    public void ReadMetricsFromSO()
    {
        numEnemiesKilled = metricsSO.NumEnemiesKilled;
        numIngredientsCollected = metricsSO.NumIngredientsCollected;
        numSoupsCooked = metricsSO.NumSoupsCooked;
        timeElapsed = metricsSO.TimeElapsed;
        numDeaths = metricsSO.NumDeaths;
        numWins = metricsSO.NumWins;
    }

    // sets new best stats, and sets booleans if new best stats were made
    private void SetNewBestStats()
    {
        if (numEnemiesKilled > maxNumEnemiesKilled)
        {
            newMaxNumEnemiesKilled = true;
            maxNumEnemiesKilled = numEnemiesKilled;
        }
        else
        {
            newMaxNumEnemiesKilled = false;
        }

        if (numIngredientsCollected > maxNumIngredientsCollected)
        {
            newMaxNumIngredientsCollected = true;
            maxNumIngredientsCollected= numIngredientsCollected;
        }
        else
        {
            newMaxNumIngredientsCollected = false;
        }

        if (numSoupsCooked > maxNumSoupsCooked)
        {
            newMaxNumSoupsCooked = true;
            maxNumSoupsCooked= numSoupsCooked;
        }
        else
        {
            newMaxNumSoupsCooked = false;
        }

        if (timeElapsed < minTimeElapsed)
        {
            newMinTimeElapsed = true;
            minTimeElapsed = timeElapsed;
        }
        else
        {
            newMinTimeElapsed = false;
        }

        totalDeaths += numDeaths;
        totalWins += numWins;
    }

    private void SaveNewMaxStats()
    {
        if (newMaxNumEnemiesKilled)
        {
            SaveManager.Singleton.deathMetrics.maxNumEnemiesKilled = maxNumEnemiesKilled;
        }
        if (newMaxNumIngredientsCollected)
        {
            SaveManager.Singleton.deathMetrics.maxNumIngredientsCollected = maxNumIngredientsCollected;
        }
        if (newMaxNumSoupsCooked)
        {
            SaveManager.Singleton.deathMetrics.maxNumSoupsCooked = maxNumSoupsCooked;
        }
        if (newMinTimeElapsed)
        {
            SaveManager.Singleton.deathMetrics.minTimeElapsed = minTimeElapsed;
        }

        SaveManager.Singleton.deathMetrics.totalDeaths = totalDeaths;
        SaveManager.Singleton.deathMetrics.totalWins = totalWins;

        SaveManager.Singleton.SaveGameStats();

    }

}
