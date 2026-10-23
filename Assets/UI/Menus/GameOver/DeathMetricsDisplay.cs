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
        GameManager.OnEndRun += DisplayStats;
    }

    private void OnDisable()
    {
        GameManager.OnEndRun -= DisplayStats;
    }

    public void DisplayStats(bool s)
    {
        if (MetricsTracker.Singleton == null) return;

        MetricsData metricsData = MetricsTracker.Singleton.metricsData;


        metricsText.text = "";

        if (metricsData.new_MaxNumEnemiesKilled)
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Monsters Slain") +
                $"<color=#000515>{metricsData.best_NumEnemiesKilled}</color>" +
                $"<color=#AC0000> (" + LocalizationManager.GetLocalizedString("New Best") + ")</color>" +
                "\n";
        }
        else
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Monsters Slain") +
                $"<color=#000515>{metricsData.curr_NumEnemiesKilled}</color>" +
                $"<color=#AC0000> (" + LocalizationManager.GetLocalizedString("Best") + $"{metricsData.best_NumEnemiesKilled})</color>" +
                "\n";
        }

        if (metricsData.new_MaxNumIngredientsCollected)
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Ingredients Collected") +
                $"<color=#000515>{metricsData.curr_NumIngredientsCollected}</color>" +
                $"<color=#AC0000> (" + LocalizationManager.GetLocalizedString("New Best") + ")</color>" +
                "\n";
        }
        else
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Ingredients Collected") +
                $"<color=#000515>{metricsData.curr_NumIngredientsCollected}</color>" +
                $"<color=#AC0000> (" + LocalizationManager.GetLocalizedString("Best") + $"{metricsData.best_NumIngredientsCollected})</color>" +
                "\n";
        }

        if (metricsData.new_MaxNumSoupsCooked)
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Soups Cooked") +
                $"<color=#000515>{metricsData.curr_NumSoupsCooked}</color>" +
                $"<color=#AC0000> (" + LocalizationManager.GetLocalizedString("New Best") + ")</color>" +
                "\n";
        }
        else
        {
            metricsText.text += LocalizationManager.GetLocalizedString("Soups Cooked") +
                $"<color=#000515>{metricsData.curr_NumSoupsCooked}</color>" +
                $"<color=#AC0000> (" + LocalizationManager.GetLocalizedString("Best") +  $"{metricsData.best_NumSoupsCooked})</color>" +
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
            $"<color=#000515>{timeElapsedStr}</color>" +
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
                        $"<color=#000515>{timeElapsedStr}</color>" +
                         $"<color=#AC0000> (New Best!)</color>" +
                        "\n";
                }
                else
                {
                    metricsText.text += "Win Time Elapsed: " +
                        $"<color=#000515>{timeElapsedStr}</color>" +
                         $"<color=#AC0000> (Best: {bestRunTime})</color>" +
                        "\n";
                }
            }
            else
            {
                metricsText.text += "Best Win Time: " +
                        $"<color=#000515>{bestRunTime}</color>" +
                        "\n";
            }
        }

        metricsText.text += LocalizationManager.GetLocalizedString("Total Deaths") + $"<color=#000515>{metricsData.totalDeaths}</color>" + "\n";
        metricsText.text += LocalizationManager.GetLocalizedString("Total Wins") + $"<color=#000515>{metricsData.totalWins}</color>" + "\n";
    }
}
