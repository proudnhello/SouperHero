using UnityEngine;

[CreateAssetMenu(menuName = "Unlockables/Achievement")]
public class AchievementData : ScriptableObject
{
    public string UUID;
    public string AssociatedSteamStat;
    public int TotalStatCount;
    public CosmeticData RewardedCosmetic;
}