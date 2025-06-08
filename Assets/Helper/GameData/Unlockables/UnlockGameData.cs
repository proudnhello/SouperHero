using System.Collections.Generic;

public class UnlockGameData
{
    public List<string> CosmeticsUnlocked;
    public Dictionary<string, int> AchievementsData;

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