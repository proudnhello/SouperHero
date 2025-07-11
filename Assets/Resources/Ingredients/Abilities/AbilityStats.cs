using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
            return (BaseDuration + durationAdd) * durationMult;
        }
        set { BaseDuration = value; }
    }
    float durationAdd, durationMult;


    public float BaseSize;
    public float ModifiedSize
    {
        get
        {
            return (BaseSize + sizeAdd) * sizeMult;
        }
        set { BaseSize = value; }
    }
    float sizeAdd, sizeMult;

    public float BaseSpeed;
    public float ModifiedSpeed
    {
        get
        {
            return (BaseSpeed + speedAdd) * speedMult;
        }
        set { BaseSpeed = value; }
    }
    float speedAdd, speedMult;

    public AbilityStats(AbilityStats baseStats, List<FlavorIngredient.BuffFlavor> buffs)
    {
        this = baseStats;
        durationMult = 1;
        sizeMult = 1;
        speedMult = 1;

        foreach (var buff in buffs)
        {
            switch(buff.buffType)
            {
                case BuffType.TOUGH_Duration:
                    durationAdd += buff.amount;
                    break;
                case BuffType.HEAVY_Size:
                    sizeAdd += buff.amount;
                    break;
                case BuffType.SWEET_Speed:
                    speedAdd += buff.amount;
                    break;
            }
        }
    }

    public void MultiplyStat(BuffType buff, int count) 
    {
        switch (buff)
        {
            case BuffType.TOUGH_Duration:
                durationMult += .2f * count;
                break;
            case BuffType.HEAVY_Size:
                sizeMult += .2f * count;
                break;
            case BuffType.SWEET_Speed:
                speedMult += .2f * count;
                break;
        }
    }
}