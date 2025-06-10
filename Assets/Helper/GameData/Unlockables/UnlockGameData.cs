using System.Collections.Generic;

public class UnlockGameData
{
    public List<string> CosmeticsUnlocked; // cosmetics unlocked
    public Dictionary<string, int> AchievementsData; // achievement uuid, stat progress
    public string selectedCosmetic;

    public UnlockGameData(bool newData = false)
    {
        if (newData)
        {
            CosmeticsUnlocked = new() { "Default" };
            selectedCosmetic = CosmeticsUnlocked[0];
            AchievementsData = new();
            foreach (var ach in UnlockDataManager.Singleton.database.AllAchievements)
            {
                AchievementsData.Add(ach.UUID, 0);
            }
        }
    }
}