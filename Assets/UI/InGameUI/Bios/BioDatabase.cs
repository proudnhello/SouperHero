using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BuffType = FlavorIngredient.BuffFlavor.BuffType;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;

public class BioDatabase : MonoBehaviour
{
    public static BioDatabase Singleton { get; private set; }
    [Serializable]
    public struct FlavorIconInfo
    {
        public string KEY;
        public Sprite ICON;
        public bool isBuffType;
        public BuffType buffType;
        public InflictionType inflictionType;
        public Color COLOR;
    }

    [SerializeField] List<FlavorIconInfo> flavorInfo;
    internal Dictionary<string, FlavorIconInfo> FlavorIcons = new();
    internal Dictionary<BuffType, FlavorIconInfo> BuffFlavorIcons = new();
    internal Dictionary<InflictionType, FlavorIconInfo> InflictionFlavorIcons = new();

    [Serializable]
    public struct AbilityIconInfo
    {
        public AbilityAbstractClass ability;
        public Sprite icon;
    }
    [SerializeField] List<AbilityIconInfo> abilityInfo;
    internal Dictionary<AbilityAbstractClass, Sprite> AbilityIcons = new();

    private void Awake()
    {
        Generate();
    }

    public void Generate()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
        }
        foreach (var flavor in flavorInfo)
        {
            FlavorIcons.Add(flavor.KEY, flavor);
            if (flavor.isBuffType) BuffFlavorIcons.Add(flavor.buffType, flavor);
            else InflictionFlavorIcons.Add(flavor.inflictionType, flavor);
        }
        foreach (var ab in abilityInfo) AbilityIcons.Add(ab.ability, ab.icon);
    }
}