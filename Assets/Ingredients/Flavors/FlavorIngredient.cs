using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Ingredient/New Flavor Ingredient")]
public class FlavorIngredient : Ingredient
{
    public string FlavorProfile;
    
    [Serializable]
    public class BuffFlavor
    {
        public enum BuffType
        {
            SOUR_Duration,
            SALTY_CriticalStrike,
            BITTER_Size,
            SWEET_Speed,
            UMAMI_Vampirism,
            REFRESHING_Cooldown,
        }
        public enum Operation
        {
            Add,
            Multiply
        }
        public BuffType buffType;
        public Operation operation;
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
            GREASY_Knockback
        }
        public enum Operation
        {
            Add,
            Multiply
        }
        public InflictionType inflictionType;
        public Operation operation;
        public int amount;
        public float statusEffectDuration;
    }
    [Header("Flavors")]
    public List<BuffFlavor> buffFlavors;
    public List<InflictionFlavor> inflictionFlavors;
public static readonly Dictionary<BuffFlavor.BuffType, Color> buffColorMapping = new Dictionary<BuffFlavor.BuffType, Color>
{
    { BuffFlavor.BuffType.SOUR_Duration, Color.yellow },
    { BuffFlavor.BuffType.SALTY_CriticalStrike, new Color(0.65f, 0.16f, 0.16f) }, // Brownish
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
    { InflictionFlavor.InflictionType.GREASY_Knockback, new Color(0.55f, 0.27f, 0.07f) } // SaddleBrown
};

}






