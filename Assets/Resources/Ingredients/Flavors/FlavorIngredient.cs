using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

[CreateAssetMenu(fileName = "New Item", menuName = "Ingredient/New Flavor Ingredient")]
public class FlavorIngredient : Ingredient
{
    public string FlavorProfile;
    public FlavorPairing Pairing;
    
    [Serializable]
    public class FlavorPairing
    {
        public bool isBuff; // 0 = buff, 1 = infliction, -1 = error
        public BuffFlavor.BuffType FlavorPairingBuff; // Only one pairing is set
        public InflictionFlavor.InflictionType FlavorPairingInfliction;
        public FlavorPairing(string pairing)
        {
            if (Enum.TryParse(pairing, out BuffFlavor.BuffType buffType))
            {
                isBuff = true;
                FlavorPairingBuff = buffType;
                Debug.Log("Pairs with " + FlavorPairingBuff);
            }
            else if (Enum.TryParse(pairing, out InflictionFlavor.InflictionType inflictionType))
            {
                isBuff = false;
                FlavorPairingInfliction = inflictionType;
                Debug.Log("Pairs with " + FlavorPairingInfliction);
            } 
            else
            {
                Debug.LogError($"Invalid flavor type enum name: {pairing}.");
            }
        } 

        public int GetPairing()
        {
            if (isBuff) return (int)FlavorPairingBuff;
            else return (int)FlavorPairingInfliction;
        }
    }
    
    [Serializable]
    public class BuffFlavor
    {
        public enum BuffType
        {
            SOUR_Duration,
            SALTY_Crit,
            BITTER_Size,
            SWEET_Speed,
            UMAMI_Vampirism,
            REFRESHING_Cooldown,
        }
        public BuffType buffType;
        public float amount;
    }
    [Serializable]
    public class InflictionFlavor
    {
        public enum InflictionType
        {
            SPICY_Burn,
            FROSTY_Freeze,
            HEARTY_Health,
            SPIKY_Damage,
            GREASY_Knockback,
            UNAMI_Vampirism
        }
        public InflictionType inflictionType;
        public int amount;
        public float statusEffectDuration;

        public InflictionFlavor(InflictionFlavor other)
        {
            inflictionType = other.inflictionType;
            amount = other.amount;
            statusEffectDuration = other.statusEffectDuration;
        }

        public InflictionFlavor() { }
    }
    [Header("Flavors")]
    public List<BuffFlavor> buffFlavors;
    public List<InflictionFlavor> inflictionFlavors;

public static readonly Dictionary<BuffFlavor.BuffType, Color> buffColorMapping = new Dictionary<BuffFlavor.BuffType, Color>
{
    { BuffFlavor.BuffType.SOUR_Duration, Color.yellow },
    { BuffFlavor.BuffType.SALTY_Crit, new Color(0.65f, 0.16f, 0.16f) }, // Brownish
    { BuffFlavor.BuffType.BITTER_Size, new Color(0f, 1f, 0f) }, // Green
    { BuffFlavor.BuffType.SWEET_Speed, new Color(0.5f, 0f, 0.5f) }, // Purple
    { BuffFlavor.BuffType.UMAMI_Vampirism, new Color(0.5f, 0.25f, 0f) } // Brown
};

    public static readonly Dictionary<InflictionFlavor.InflictionType, Color> inflictionColorMapping = new Dictionary<InflictionFlavor.InflictionType, Color>
    {
        { InflictionFlavor.InflictionType.SPICY_Burn, Color.red },
        { InflictionFlavor.InflictionType.FROSTY_Freeze, new Color(0f, 1f, 1f) }, // Cyan
        { InflictionFlavor.InflictionType.HEARTY_Health, Color.green },
        { InflictionFlavor.InflictionType.SPIKY_Damage, new Color(1f, 0f, 1f) }, // Magenta
        { InflictionFlavor.InflictionType.GREASY_Knockback, new Color(0.55f, 0.27f, 0.07f) }, // SaddleBrown
        { InflictionFlavor.InflictionType.UNAMI_Vampirism, new Color(0.58f, 0, 0.82f) } // Purple
    };
    public static Dictionary<InflictionFlavor.InflictionType, string> inflictionTextMapping = new Dictionary<InflictionFlavor.InflictionType, string>{
        {InflictionFlavor.InflictionType.SPICY_Burn, "Burn Infliction"},
        {InflictionFlavor.InflictionType.FROSTY_Freeze, "Freeze Infliction"},
        {InflictionFlavor.InflictionType.HEARTY_Health, "Health Infliction"},
        {InflictionFlavor.InflictionType.SPIKY_Damage, "Damage Infliction"},
        {InflictionFlavor.InflictionType.GREASY_Knockback, "Knockback Infliction"},
        {InflictionFlavor.InflictionType.UNAMI_Vampirism, "Vampirism Infliction"}
    };

    public static string GetFlavorHitmarker(InflictionFlavor.InflictionType flavorKey)
    {   
        LocalizedString localString = new LocalizedString(LocalizationManager.GetTable(), inflictionTextMapping[flavorKey]); 
        return localString.GetLocalizedString();
    }

}






