using Unity.VisualScripting;
using UnityEngine;

#if UNITY_STANDALONE
using Steamworks;
#endif

public class SteamLoginManager : MonoBehaviour
{
    public static SteamLoginManager Singleton { get; private set; }

    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }
    public bool IsConnected()
    {
        return SteamAPI.Init() && SteamManager.Initialized;
    }

    public void UpdateSteamAchievementData(UnlockGameData data)
    {
        foreach (var ach in data.AchievementsData)
        {
            SteamUserStats.GetAchievement(ach.Key, out bool achieved);
            if (achieved) continue;

            AchievementData achData = UnlockDataManager.Singleton.database.GetAchievementData(ach.Key);
            if (achData.AssociatedSteamStat != string.Empty ) // has a steam stat
            {
                SteamUserStats.GetStat(achData.AssociatedSteamStat, out int steamProgress);
                int localProgress = data.AchievementsData[ach.Key];
                if (localProgress > steamProgress) SteamUserStats.SetStat(achData.AssociatedSteamStat, localProgress);
            }
            
            if (data.AchievementsData[ach.Key] >= achData.TotalStatCount)
            {
                SteamUserStats.SetAchievement(ach.Key);
            }
        }
        SteamUserStats.StoreStats();
    }

    public void ReportAchievementProgress(UnlockGameData data, AchievementData achData)
    {
        SteamUserStats.GetAchievement(achData.UUID, out bool achieved);
        if (achieved) return;

        if (achData.AssociatedSteamStat != string.Empty) // has a steam stat
        {
            SteamUserStats.GetStat(achData.AssociatedSteamStat, out int steamProgress);
            int localProgress = data.AchievementsData[achData.UUID];
            if (localProgress > steamProgress)
            {
                SteamUserStats.SetStat(achData.AssociatedSteamStat, localProgress);
                SteamUserStats.StoreStats();
            }
        }

        if (data.AchievementsData[achData.UUID] >= achData.TotalStatCount)
        {
            Debug.Log("ACHIEVED " + achData.UUID);
            SteamUserStats.SetAchievement(achData.UUID);
            SteamUserStats.StoreStats();

            if (achData.RewardedCosmetic != null)
            {
                UnlockGameData unlockData = UnlockDataManager.Singleton.unlockData;
                if (!unlockData.CosmeticsUnlocked.Contains(achData.RewardedCosmetic.UUID)) unlockData.CosmeticsUnlocked.Add(achData.RewardedCosmetic.UUID);
            }
        }
    }
}