using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Ingredient/New Flavor Ingredient")]
public class FlavorIngredient : Ingredient
{
    [Header("Encyclopedia")]
    public Sprite EncyclopediaEntry;
    public string Location;
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
            UMAMI_Vampirism
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
}






