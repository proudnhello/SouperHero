using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetStatsTab : MonoBehaviour
{
    public void ResetStatsFile()
    {
        SaveManager.Singleton.ResetGameStats();
        DeathMetricsManager.Singleton.ProcessStats();
        DeathMetricsManager.Singleton.DisplayStats();
    }
}
