using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using BuffType = FlavorIngredient.BuffFlavor.BuffType;

[System.Serializable]
public struct AbilityStats
{
    // STATS -- I APOLOGIZE IN ADVANCE THIS IS JUST TO FINISH UP THE SPRINT I WANT TO FIX THIS NEXT QUARTER
    public float BaseDuration;
    public float ModifiedDuration
    {
        get
        {
            return BaseDuration + durationAdd + (BaseDuration + durationAdd) * durationMult;
        }
        set { BaseDuration = value; }
    }
    internal float durationAdd, durationMult;


    public float BaseSize;
    public float ModifiedSize
    {
        get
        {
            return BaseSize + sizeAdd + (BaseSize + sizeAdd) * sizeMult;
        }
        set { BaseSize = value; }
    }
    internal float sizeAdd, sizeMult;

    public float BaseSpeed;
    public float ModifiedSpeed
    {
        get
        {
            return BaseSpeed + speedAdd + (BaseSpeed + speedAdd) * speedMult;
        }
        set { BaseSpeed = value; }
    }
    internal float speedAdd, speedMult;

    public AbilityStats(AbilityStats baseStats)
    {
        this = baseStats;
        durationMult = 0;
        sizeMult = 0;
        speedMult = 0;
        
    }

    public void CombineStats(AbilityStats newStats)
    {
        durationAdd += newStats.durationAdd;
        sizeAdd += newStats.sizeAdd;
        speedAdd += newStats.speedAdd;
        durationMult += newStats.durationMult;
        sizeMult += newStats.sizeMult;
        speedMult += newStats.speedMult;
    }

    public void ApplyBuffs(List<FinishedSoup.SoupBuffStat> buffs)
    {
        if (buffs == null) return;

        foreach (var buff in buffs)
        {
            switch (buff.BuffType)
            {
                case BuffType.TOUGH_Duration:
                    durationAdd += buff.add;
                    durationMult += buff.mult;
                    break;
                case BuffType.HEAVY_Size:
                    sizeAdd += buff.add;
                    sizeMult += buff.mult;
                    break;
                case BuffType.SWEET_Speed:
                    speedAdd += buff.add;
                    speedMult += buff.mult;
                    break;
            }
        }
    }
}