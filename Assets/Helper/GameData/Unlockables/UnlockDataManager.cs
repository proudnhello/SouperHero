using Steamworks;
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
        PlayerCosmeticRenderer.Singleton.SetPlayerCosmetic(database.GetCosmeticData(unlockData.selectedCosmetic).Material);
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
        PlayerCosmeticRenderer.Singleton.SetPlayerCosmetic(database.GetCosmeticData(unlockData.selectedCosmetic).Material);
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

    public void SetCosmetic(CosmeticData  selectedCosmetic)
    {
        unlockData.selectedCosmetic = selectedCosmetic.UUID;
        SaveManager.Singleton.SaveUnlockData(unlockData);
    }

    public CosmeticData GetCurrentCosmetic()
    {
        return database.GetCosmeticData(unlockData.selectedCosmetic);
    }

    public void ReportAchievementProgress(string uuid, int totalSteps, bool increment = false)
    {
        if (IsAchievementUnlocked(uuid)) return;

        AchievementData achData = database.GetAchievementData(uuid);
        Debug.Log(achData);

        if (increment)
        {
            unlockData.AchievementsData[uuid]++;
        }
        else
        {
            unlockData.AchievementsData[uuid] = Mathf.Clamp(Mathf.Max(unlockData.AchievementsData[uuid], totalSteps), 0, achData.TotalStatCount);
        }

        SteamLoginManager.Singleton.ReportAchievementProgress(unlockData, achData);

        if (unlockData.AchievementsData[achData.UUID] >= achData.TotalStatCount)
        {
            Debug.Log("ACHIEVED " + achData.UUID);
            if (achData.RewardedCosmetic != null)
            {
                if (!unlockData.CosmeticsUnlocked.Contains(achData.RewardedCosmetic.UUID)) unlockData.CosmeticsUnlocked.Add(achData.RewardedCosmetic.UUID);
            }
        }

        SaveManager.Singleton.SaveUnlockData(unlockData);
    }
}