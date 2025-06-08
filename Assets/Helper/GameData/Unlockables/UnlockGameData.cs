using System.Collections.Generic;

public class UnlockGameData
{
    public List<string> CosmeticsUnlocked; // cosmetics unlocked
    public Dictionary<string, int> AchievementsData; // achievement uuid, stat progress

    public UnlockGameData(bool newData = false)
    {
        if (newData)
        {
            CosmeticsUnlocked = new() { "Default" };
            AchievementsData = new();
            foreach (var ach in UnlockDataManager.Singleton.database.AllAchievements)
            {
                AchievementsData.Add(ach.UUID, 0);
            }
        }
    }
}