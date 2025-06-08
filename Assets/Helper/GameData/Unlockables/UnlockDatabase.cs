using System.Collections.Generic;
using UnityEngine;

public class UnlockDatabase : MonoBehaviour
{
    public List<CosmeticData> AllCosmetics;
    public List<AchievementData> AllAchievements;

    Dictionary<string, CosmeticData> UUIDtoCosmeticData = new();
    Dictionary<string, AchievementData> UUIDtoAchievementData = new();

    public CosmeticData GetCosmeticData(string uuid) => UUIDtoCosmeticData[uuid];
    public AchievementData GetAchievementData(string uuid) => UUIDtoAchievementData[uuid];

    public void Init()
    {
        foreach (var cosmetic in AllCosmetics) UUIDtoCosmeticData.Add(cosmetic.UUID, cosmetic);
        foreach (var ach in AllAchievements) UUIDtoAchievementData.Add(ach.UUID, ach);
    }
}