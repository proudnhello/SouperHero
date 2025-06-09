using Unity.VisualScripting;
using UnityEngine;

public class UnlockDataManager : MonoBehaviour
{
    public static UnlockDataManager Singleton { get; private set; }
    public UnlockDatabase database;
    public UnlockGameData unlockData;
    private void Awake()
    {
        if (Singleton != null && Singleton != this) Destroy(this);
        else Singleton = this;
    }
    private void Start()
    {
        database.Init();
        unlockData = SaveManager.Singleton.LoadUnlockData();
        unlockData ??= new(true);
        if (SteamLoginManager.Singleton.IsConnected())
        {
            Debug.Log("Connected to Steam!");
            SteamLoginManager.Singleton.UpdateSteamAchievementData(unlockData);
        }
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        unlockData = new(true);
    }

    [ContextMenu("Complete Data")]
    public void CompleteData()
    {
        foreach (var cosmetic in database.AllCosmetics)
        {
            if (!unlockData.CosmeticsUnlocked.Contains(cosmetic.UUID)) unlockData.CosmeticsUnlocked.Add(cosmetic.UUID);
        }
        foreach (var ach in database.AllAchievements)
        {
            unlockData.AchievementsData[ach.UUID] = ach.TotalStatCount;
        }
    }

    public bool IsCosmeticUnlocked(string uuid)
    {
        return unlockData.CosmeticsUnlocked.Contains(uuid);
    }

    public bool IsAchievementUnlocked(string uuid)
    {
        return unlockData.AchievementsData[uuid] >= database.GetAchievementData(uuid).TotalStatCount;
    }

    public void ReportAchievementProgress(string uuid, int totalSteps, bool increment = false)
    {
        Debug.Log(uuid);
        if (IsAchievementUnlocked(uuid)) return;

        AchievementData achData = database.GetAchievementData(uuid);

        if (increment)
        {
            unlockData.AchievementsData[uuid]++;
        }
        else
        {
            unlockData.AchievementsData[uuid] = Mathf.Clamp(Mathf.Max(unlockData.AchievementsData[uuid], totalSteps), 0, achData.TotalStatCount);
        }

        SteamLoginManager.Singleton.ReportAchievementProgress(unlockData, achData);

        if (achData.RewardedCosmetic != null)
        {
            if (!unlockData.CosmeticsUnlocked.Contains(achData.RewardedCosmetic.UUID)) unlockData.CosmeticsUnlocked.Add(achData.RewardedCosmetic.UUID);
        }


    }
}