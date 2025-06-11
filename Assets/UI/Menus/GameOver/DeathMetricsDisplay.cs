/*
 * This file was modified with LLMs: 
 * https://github.com/djlouie/project-soup-chat-logs/blob/main/logs/log03.md
 * https://github.com/djlouie/project-soup-chat-logs/blob/main/logs/log02.md
 */

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
        DisplayStats(MetricsTracker.Singleton.metricsData);
    }

    public void DisplayStats(MetricsData metricsData)
    {
        if (metricsData == null) return;

        metricsText.text = "";

        if (metricsData.new_MaxNumEnemiesKilled)
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Monsters Slain") +
                $"<color=#F4B07D>{metricsData.best_NumEnemiesKilled}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("New Best") + ")</color>" +
                "\n";
        }
        else
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Monsters Slain") +
                $"<color=#F4B07D>{metricsData.curr_NumEnemiesKilled}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("Best") + $"{metricsData.best_NumEnemiesKilled})</color>" +
                "\n";
        }

        if (metricsData.new_MaxNumIngredientsCollected)
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Ingredients Collected") +
                $"<color=#F4B07D>{metricsData.curr_NumIngredientsCollected}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("New Best") + ")</color>" +
                "\n";
        }
        else
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Ingredients Collected") +
                $"<color=#F4B07D>{metricsData.curr_NumIngredientsCollected}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("Best") + $"{metricsData.best_NumIngredientsCollected})</color>" +
                "\n";
        }

        if (metricsData.new_MaxNumSoupsCooked)
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Soups Cooked") +
                $"<color=#F4B07D>{metricsData.curr_NumSoupsCooked}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("New Best") + ")</color>" +
                "\n";
        }
        else
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Soups Cooked") +
                $"<color=#F4B07D>{metricsData.curr_NumSoupsCooked}</color>" +
                $"<color=#A9A9A9> (" + LocalizationManager.GetLocalizedString("Best") +  $"{metricsData.best_NumSoupsCooked})</color>" +
                "\n";
        }


        // Format time from seconds to minutes (and hours if needed)
        timeFormatter = TimeSpan.FromSeconds(metricsData.curr_RunTime);
        string timeElapsedStr;
        if (metricsData.curr_RunTime < 3600)
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
        if (metricsData.totalWins > 0)
        {
            Debug.Log($"THE TOTAL WINS: {metricsData.totalWins}");

            // Format time from seconds to minutes (and hours if needed)
            TimeSpan bestTimeFormatter = TimeSpan.FromSeconds(metricsData.best_WinTime);
            string bestRunTime;
            if (metricsData.best_WinTime < 3600)
            {
                bestRunTime = bestTimeFormatter.ToString("mm':'ss'.'ff");
            }
            else
            {
                bestRunTime = bestTimeFormatter.ToString("hh':'mm':'ss'.'ff");
            }

            // check if they won this time
            if (metricsData.wasRunSuccessful)
            {
                if (metricsData.new_FastestRun)
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
                         $"<color=#A9A9A9> (Best: {bestRunTime})</color>" +
                        "\n";
                }
            }
            else
            {
                metricsText.text += "Best Win Time: " +
                        $"<color=#F4B07D>{bestRunTime}</color>" +
                        "\n";
            }
        }

        metricsText.text += LocalizationManager.GetLocalizedString("Total Deaths") + $"<color=#F4B07D>{metricsData.totalDeaths}</color>" + "\n";
        metricsText.text += LocalizationManager.GetLocalizedString("Total Wins") + $"<color=#F4B07D>{metricsData.totalWins}</color>" + "\n";
    }

    public void DisplayStatsNoParams()
    {
        DisplayStats(MetricsTracker.Singleton.metricsData);
    }
}
