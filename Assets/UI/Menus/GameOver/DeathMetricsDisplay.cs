using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeathMetricsDisplay : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI metricsText;



    private TimeSpan timeFormatter;

    public static DeathMetricsDisplay Singleton { get; private set; }

    public void Start()
    {
        DisplayStats(MetricsTracker.Singleton.currentMetrics, MetricsTracker.Singleton.lastMetrics);
    }

    public void DisplayStats(MetricsData runData, MetricsData savedData)
    {
        metricsText.text = "";

        if (runData.new_MaxNumEnemiesKilled)
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Monsters Slain") +
                $"<color=#F4B07D>{runData.numEnemiesKilled}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("New Best") + ")</color>" +
                "\n";
        }
        else
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Monsters Slain") +
                $"<color=#F4B07D>{runData.numEnemiesKilled}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("Best") + $"{savedData.numEnemiesKilled})</color>" +
                "\n";
        }

        if (runData.new_MaxNumIngredientsCollected)
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Ingredients Collected") +
                $"<color=#F4B07D>{runData.numIngredientsCollected}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("New Best") + ")</color>" +
                "\n";
        }
        else
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Ingredients Collected") +
                $"<color=#F4B07D>{runData.numIngredientsCollected}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("Best") + $"{savedData.numIngredientsCollected})</color>" +
                "\n";
        }

        if (runData.new_MaxNumSoupsCooked)
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Soups Cooked") +
                $"<color=#F4B07D>{runData.numSoupsCooked}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("New Best") + ")</color>" +
                "\n";
        }
        else
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Soups Cooked") +
                $"<color=#F4B07D>{runData.numSoupsCooked}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("Best") +  $"{savedData.numSoupsCooked})</color>" +
                "\n";
        }


        // Format time from seconds to minutes (and hours if needed)
        timeFormatter = TimeSpan.FromSeconds(runData.lastRunTime);
        string timeElapsedStr;
        if (runData.lastRunTime < 3600)
        {
            timeElapsedStr = timeFormatter.ToString("mm':'ss'.'ff");
        }
        else
        {
            timeElapsedStr = timeFormatter.ToString("hh':'mm':'ss'.'ff");
        }

        metricsText.text += LocalizationManager.GetLocalizedString("Time Elapsed") +
            $"<color=#F4B07D>{timeElapsedStr}</color>" +
            "\n";

        // check if they have won before
        if (runData.totalWins > 0)
        {
            // Format time from seconds to minutes (and hours if needed)
            timeFormatter = TimeSpan.FromSeconds(runData.lastRunTime);
            string minWinTimeElapsedStr;
            if (runData.lastRunTime < 3600)
            {
                minWinTimeElapsedStr = timeFormatter.ToString("mm':'ss'.'ff");
            }
            else
            {
                minWinTimeElapsedStr = timeFormatter.ToString("hh':'mm':'ss'.'ff");
            }

            // check if they won this time
            if (runData.lastRunSuccessful)
            {
                if (runData.new_MinWinTimeElapsed)
                {
                    metricsText.text += "Win Time Elapsed: " +
                        $"<color=#F4B07D>{timeElapsedStr}</color>" +
                         $"<color=#A9A9A9> (New Best!)</color>" +
                        "\n";
                }
                else
                {
                    metricsText.text += "Win Time Elapsed: " +
                        $"<color=#F4B07D>{timeElapsedStr}</color>" +
                         $"<color=#A9A9A9> (Best: {minWinTimeElapsedStr})</color>" +
                        "\n";
                }
            }
            else
            {
                metricsText.text += "Win Time Elapsed: " +
                        $"<color=#F4B07D>N/A</color>" +
                         $"<color=#A9A9A9> (Best: {minWinTimeElapsedStr})</color>" +
                        "\n";
            }
        }

        metricsText.text += LocalizationManager.GetLocalizedString("Total Deaths") + $"<color=#F4B07D>{runData.totalDeaths}</color>" + "\n";
        metricsText.text += LocalizationManager.GetLocalizedString("Total Wins") + $"<color=#F4B07D>{runData.totalWins}</color>" + "\n";
    }
}
