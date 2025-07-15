// portions of this file were generated using GitHub Copilot
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
        public float amount;
        public FlavorPairing(string pairing, string amount)
        {
            if (Enum.TryParse(pairing, out BuffFlavor.BuffType buffType))
            {
                isBuff = true;
                FlavorPairingBuff = buffType;
            }
            else if (Enum.TryParse(pairing, out InflictionFlavor.InflictionType inflictionType))
            {
                isBuff = false;
                FlavorPairingInfliction = inflictionType;
            }
            if (!float.TryParse(amount, out this.amount)) this.amount = .2f;
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
            TOUGH_Duration,
            HEAVY_Size,
            SWEET_Speed
        }
        public BuffType buffType;
        public int amount;
    }
    [Serializable]
    public class InflictionFlavor
    {
        public enum InflictionType
        {
            SPICY_Burn,
            FROSTY_Freeze,
            SPIKY_Damage,
            SLIMY_Knockback,
            VAMPIRISM_LifeSteal,
            _Health,
            _Water
        }
        public InflictionType inflictionType;
        public int amount;

        public InflictionFlavor(InflictionFlavor other)
        {
            inflictionType = other.inflictionType;
            amount = other.amount;
        }

        public InflictionFlavor() { }
    }
    [Header("Flavors")]
    public List<BuffFlavor> buffFlavors;
    public List<InflictionFlavor> inflictionFlavors;

    public static readonly Dictionary<BuffFlavor.BuffType, Color> buffColorMapping = new Dictionary<BuffFlavor.BuffType, Color>
    {
        { BuffFlavor.BuffType.SWEET_Speed, new Color(0.5f, 0f, 0.5f) }, // Purple
        { BuffFlavor.BuffType.TOUGH_Duration, Color.yellow },
        { BuffFlavor.BuffType.HEAVY_Size, new Color(0f, 1f, 0f) }, // Green
    };

    public static readonly Dictionary<InflictionFlavor.InflictionType, Color> inflictionColorMapping = new Dictionary<InflictionFlavor.InflictionType, Color>
    {
        { InflictionFlavor.InflictionType.SPICY_Burn, Color.red },
        { InflictionFlavor.InflictionType.FROSTY_Freeze, new Color(0f, 1f, 1f) }, // Cyan
        { InflictionFlavor.InflictionType._Health, Color.green },
        { InflictionFlavor.InflictionType.SPIKY_Damage, new Color(1f, 0f, 1f) }, // Magenta
        { InflictionFlavor.InflictionType.SLIMY_Knockback, new Color(0.55f, 0.27f, 0.07f) }, // SaddleBrown
    };
    public static Dictionary<InflictionFlavor.InflictionType, string> inflictionTextMapping = new Dictionary<InflictionFlavor.InflictionType, string>{
        {InflictionFlavor.InflictionType.SPICY_Burn, "Burn Infliction"},
        {InflictionFlavor.InflictionType.FROSTY_Freeze, "Freeze Infliction"},
        {InflictionFlavor.InflictionType._Health, "Health Infliction"},
        {InflictionFlavor.InflictionType.SPIKY_Damage, "Damage Infliction"},
        {InflictionFlavor.InflictionType.SLIMY_Knockback, "Knockback Infliction"}
    };

    public static string GetFlavorHitmarker(InflictionFlavor.InflictionType flavorKey)
    {
        if (inflictionTextMapping.ContainsKey(flavorKey)) return new LocalizedString(LocalizationManager.GetTable(), inflictionTextMapping[flavorKey]).GetLocalizedString();
        return "";
    }

    public static Color GetFlavorHitmarkerColor(InflictionFlavor.InflictionType flavorKey)
    {
        if (inflictionColorMapping.ContainsKey(flavorKey)) return inflictionColorMapping[flavorKey];
        return Color.white;
    }

}






