using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeathMetricsManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI metricsText;
    [SerializeField] MetricsSO metricsSO;
    private TimeSpan timeFormatter;

    // On start add the stats text from the scriptable object that saves the stats
    void Start()
    {
        metricsText.text = "";
        metricsText.text += "Monsters Slain: " + $"<color=#F4B07D>{metricsSO.NumEnemiesKilled}</color>" + "\n";
        metricsText.text += "Ingredients Collected: " + $"<color=#F4B07D>{metricsSO.NumIngredientsCollected}</color>" + "\n";
        metricsText.text += "Soups Cooked: " + $"<color=#F4B07D>{metricsSO.NumSoupsCooked}</color>" + "\n";
        
        // Format time from seconds to minutes (and hours if needed)
        timeFormatter = TimeSpan.FromSeconds(metricsSO.TimeElapsed);
        string timeElapsed;
        if (metricsSO.TimeElapsed < 3600)
        {
            timeElapsed = timeFormatter.ToString("mm':'ss'.'ff");
        }
        else
        {
            timeElapsed = timeFormatter.ToString("hh':'mm':'ss'.'ff");
        }
        metricsText.text += "Time Elapsed: " + $"<color=#F4B07D>{timeElapsed}</color>" + "\n";

        metricsText.text += "Total Deaths: " + $"<color=#F4B07D>{metricsSO.NumDeaths}</color>" + "\n";
        metricsText.text += "Total Wins: " + $"<color=#F4B07D>{metricsSO.NumWins}</color>" + "\n";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
