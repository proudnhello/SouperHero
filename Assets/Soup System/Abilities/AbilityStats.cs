using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using BuffType = FlavorIngredient.BuffFlavor.BuffType;
using Operation = FlavorIngredient.BuffFlavor.Operation;

[Serializable]
public struct AbilityStats
{
    // STATS
    public float duration;
    public float size;
    public float crit;
    public float speed;
    public float cooldown;
    public float damage;
    public float knockback;

    AbilityStats EmptyStats(int stat) {
        return new AbilityStats { duration = stat, cooldown = stat, size = stat, speed = stat, damage = stat, crit = stat, }; 
    }

    public AbilityStats(AbilityStats baseStats, List<FlavorIngredient.BuffFlavor> buffs)
    {
        this = baseStats;

        AbilityStats add = EmptyStats(0);
        AbilityStats mult = EmptyStats(1);
        foreach (var buff in buffs)
        {
            switch(buff.buffType)
            {
                case BuffType.SOUR_Duration:
                    if (buff.operation == Operation.Add) add.duration += buff.amount;
                    else mult.duration *= buff.amount; 
                    break;
                case BuffType.BITTER_Size:
                    if (buff.operation == Operation.Add) add.size += buff.amount;
                    else mult.size *= buff.amount;
                    break;
                case BuffType.SALTY_CriticalStrike:
                    if (buff.operation == Operation.Add) add.crit += buff.amount;
                    else mult.crit *= buff.amount;
                    break;
                case BuffType.SWEET_Speed:
                    if (buff.operation == Operation.Add) add.speed += buff.amount;
                    else mult.speed *= buff.amount;
                    break;
            }
        }

        duration = (duration + add.duration) * mult.duration;
        size = (size + add.size) * mult.size;
        crit = (crit + add.crit) * mult.crit;
        speed = (speed + add.speed) * mult.speed;
    }
}