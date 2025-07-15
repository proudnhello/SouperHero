using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FlavorIngredient;
using BuffFlavor = FlavorIngredient.BuffFlavor;
using BuffType = FlavorIngredient.BuffFlavor.BuffType;
using InflictionFlavor = FlavorIngredient.InflictionFlavor;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;

[System.Serializable]
public class FinishedSoup : ISoupBowl
{
    // ~~~ DEFINITIONS ~~~
    [System.Serializable]
    public class SoupAbility // one for each type
    {
        //[SerializeField]
        public AbilityAbstractClass ability;

        //[SerializeField]
        public AbilityStats statsWithBuffs;

        public float lastUseTime;
        // public int uses = 0;

        Dictionary<InflictionType, SoupInflictionStat> inflictionTracker;

        // New spoon ability for new ability ingredient in the soup
        public SoupAbility(AbilityIngredient ingredient)
        {
            ability = ingredient.abilityType;
            statsWithBuffs = new(ingredient.baseStats);
            inflictionTracker = new();
            foreach (var infliction in ingredient.inherentInflictionFlavors)
            {
                if (!inflictionTracker.ContainsKey(infliction.inflictionType))
                    inflictionTracker.Add(infliction.inflictionType, new(infliction.inflictionType));
                inflictionTracker[infliction.inflictionType].Add(infliction.amount);
            }
        }

        public void AddDuplicate(AbilityIngredient ingredient)
        {
            foreach (var infliction in ingredient.inherentInflictionFlavors)
            {
                if (!inflictionTracker.ContainsKey(infliction.inflictionType))
                    inflictionTracker.Add(infliction.inflictionType, new(infliction.inflictionType));
                inflictionTracker[infliction.inflictionType].Add(infliction.amount);
            }
            statsWithBuffs.CombineStats(ingredient.baseStats);
        }

        public void ApplyFlavors(List<SoupBuffStat> buffs, List<SoupInflictionStat> inflictions)
        {
            foreach (var inflictionStat in inflictions)
            {
                if (!inflictionTracker.ContainsKey(inflictionStat.InflictionType))
                    inflictionTracker.Add(inflictionStat.InflictionType, new(inflictionStat.InflictionType));
                inflictionTracker[inflictionStat.InflictionType].CombineStats(inflictionStat);
            }
            statsWithBuffs.ApplyBuffs(buffs);
        }

        public List<SoupInflictionStat> GetSpoonInflictions()
        {
            return inflictionTracker.Values.ToList();
        }

        public float GetDamage() => inflictionTracker.ContainsKey(InflictionType.SPIKY_Damage) ? inflictionTracker[InflictionType.SPIKY_Damage].Amount : 0;
        public float GetKnockback() => inflictionTracker.ContainsKey(InflictionType.SLIMY_Knockback) ? inflictionTracker[InflictionType.SLIMY_Knockback].Amount : 0;

        public string PrintAbility()
        {
            string output = $"{ability._abilityName}\n";
            output += $"D{statsWithBuffs.ModifiedDuration} = ({statsWithBuffs.BaseDuration} + {statsWithBuffs.durationAdd}) * {statsWithBuffs.durationMult}\n";
            output += $"SZE{statsWithBuffs.ModifiedSize} = ({statsWithBuffs.BaseSize} + {statsWithBuffs.sizeAdd}) * {statsWithBuffs.sizeMult}\n";
            output += $"SPD{statsWithBuffs.ModifiedSpeed} = ({statsWithBuffs.BaseSpeed} + {statsWithBuffs.speedAdd}) * {statsWithBuffs.speedMult}\n";

            foreach (var infliction in GetSpoonInflictions())
            {
                output += $"{infliction.InflictionType} {infliction.Amount} = {infliction.add} + {infliction.add} * {infliction.mult}\n";
            }
            return output;
        }
    }

    [System.Serializable]
    public class SoupInflictionStat // one for each type
    {
        public InflictionType InflictionType;
        public int add;
        public float mult;
        public float Amount
        {
            get
            {
                return add + add * mult;
            }
        }

        public SoupInflictionStat(InflictionType inflictionEffect) { InflictionType = inflictionEffect; add = 0; mult = 0; }
        public void CombineStats(SoupInflictionStat other)
        {
            add += other.add;
            mult += other.mult;
        }

        public void Add(int amount)
        {
            add += amount;
        }
        public void Multiply(float amount)
        {
            mult += amount;
        }
    }

    [System.Serializable]
    public class SoupBuffStat
    {
        public BuffType BuffType;
        public int add;
        public float mult;
        public float Amount
        {
            get
            {
                return add + add * mult;
            }
        }

        public SoupBuffStat(BuffType buffEffect) { BuffType = buffEffect; add = 0; mult = 0; }

        public void Add(int amount)
        {
            add += amount;
        }
        public void Multiply(float amount)
        {
            mult += amount;
        }
    }

    // ~~~ VARIABLES ~~~
    public List<Ingredient> ingredientList;
    public List<SoupAbility> soupAbilities;
    public Dictionary<InflictionType, SoupInflictionStat> soupInflictionStats;
    public Dictionary<BuffType, SoupBuffStat> soupBuffStats;
    public int uses; // -1 = infinite
    public float cooldown;
    public SoupBase soupBase;

    public int GetUses()
    {
        return uses;
    }

    // Makes a Finished Soup
    public FinishedSoup(List<Ingredient> ingredients, SoupBase stock)
    {

        ingredientList = new(ingredients);
        soupBase = stock;

        // Separate ingredients into ability and flavor categories
        List<AbilityIngredient> abilityIngredients = ingredients.Where(x => x.GetType() == typeof(AbilityIngredient)).Cast<AbilityIngredient>().ToList();
        List<FlavorIngredient> flavorIngredients = ingredients.Where(x => x.GetType() == typeof(FlavorIngredient)).Cast<FlavorIngredient>().ToList();

        // Collect and order buff flavors from flavor ingredients
        List<BuffFlavor> buffFlavors = new();
        flavorIngredients.ForEach(f => buffFlavors = buffFlavors.Concat(f.buffFlavors).ToList());
        buffFlavors.AddRange(stock.inherentBuffFlavors); // add inherent buffs from soup base

        // Collect infliction flavors from flavor ingredients //both flavor and ability ingredients
        List<InflictionFlavor> inflictionFlavors = new();
        flavorIngredients.ForEach(f => inflictionFlavors = inflictionFlavors.Concat(f.inflictionFlavors).ToList());
        inflictionFlavors.AddRange(stock.inherentInflictionFlavors); // add inherent inflictions from soup base

        // Initialize uses and cooldown
        uses = 0;
        cooldown = stock.cooldown;

        // Populate ability tracker and calculate total uses
        Dictionary<AbilityAbstractClass, SoupAbility> abilityTracker = new();
        foreach (var ingredient in abilityIngredients)
        {
            if (!abilityTracker.ContainsKey(ingredient.abilityType))
            {
                abilityTracker.Add(ingredient.abilityType, new(ingredient));
            }
            else abilityTracker[ingredient.abilityType].AddDuplicate(ingredient);
            uses += ingredient.uses;
        }
        // Convert ability track into spoon's finalized list of abilities
        soupAbilities = abilityTracker.Values.ToList();

        // Track inflictions and buffs using dictionaries
        soupInflictionStats = new();
        soupBuffStats = new();
        foreach (var infliction in inflictionFlavors)
        {
            if (!soupInflictionStats.ContainsKey(infliction.inflictionType))
                soupInflictionStats.Add(infliction.inflictionType, new(infliction.inflictionType));
            soupInflictionStats[infliction.inflictionType].Add(infliction.amount);
        }
        foreach (var buff in buffFlavors)
        {
            if (!soupBuffStats.ContainsKey(buff.buffType))
                soupBuffStats.Add(buff.buffType, new(buff.buffType));
            soupBuffStats[buff.buffType].Add(buff.amount);
        }

        // Now based on pairings, multiply corresponding stat
        foreach (var flavorIngredient in flavorIngredients)
        {
            if (flavorIngredient.Pairing.isBuff)
            {
                BuffType pair = (BuffType)flavorIngredient.Pairing.GetPairing();
                if (soupBuffStats.ContainsKey(pair))
                {
                    foreach (var buff in flavorIngredient.buffFlavors) soupBuffStats[buff.buffType].Multiply(flavorIngredient.Pairing.amount * soupBuffStats[pair].add);
                    foreach (var inf in flavorIngredient.inflictionFlavors) soupInflictionStats[inf.inflictionType].Multiply(flavorIngredient.Pairing.amount * soupBuffStats[pair].add);
                }
            }
            else
            {
                InflictionType pair = (InflictionType)flavorIngredient.Pairing.GetPairing();
                if (soupInflictionStats.ContainsKey(pair))
                {
                    foreach (var buff in flavorIngredient.buffFlavors) soupBuffStats[buff.buffType].Multiply(flavorIngredient.Pairing.amount * soupInflictionStats[pair].add);
                    foreach (var inf in flavorIngredient.inflictionFlavors) soupInflictionStats[inf.inflictionType].Multiply(flavorIngredient.Pairing.amount * soupInflictionStats[pair].add);
                }
            }
        }

        // with all the final stats calculated, apply them to abilities
        foreach (var soupAbility in soupAbilities)
        {
            soupAbility.ApplyFlavors(soupBuffStats.Values.ToList(), soupInflictionStats.Values.ToList());
        }

        if (soupBuffStats.TryGetValue(BuffType.SWEET_Speed, out var speed))
        {
            cooldown *= 1 / (1 + speed.Amount); // based on amount of cooldown buff, reduce cooldown (TWEAK THIS ALGORITHM)
        }
        // set initial lastTimeUsed to cooldown to get atk right away
        lastTimeUsed = Time.time - cooldown;
        //PrintSoup(this);

    }

    static void PrintSoup(FinishedSoup spoon)
    {
        string output = spoon.soupBase.baseName + "\nINGREDIENTS\n";
        foreach (var ing in spoon.ingredientList) output += ing.IngredientName + "\n";
        foreach (var ability in spoon.soupAbilities)
        {
            output += ability.PrintAbility();
        }       
        Debug.Log(output);
    }

    bool hasCooldown = false;
    float cooldownPercentage;
    float lastTimeUsed;

    public float GetCooldownPercentage()
    {
        return cooldownPercentage;
    }

    // Method to use the soup, applying abilities and managing uses
    public bool UseSoupAttack()
    {
        // Check if the spoon is on cooldown
        if (hasCooldown) return false;

        if (uses != 0)
        {
            // Apply each ability using the spoon if there are uses left
            foreach (SoupAbility ability in soupAbilities)
            {
                // use ability if there are uses left
                ability.ability.UseAbility(ability.statsWithBuffs, ability.GetSpoonInflictions());
            }
        }
        // Decrement uses if applicable
        if (uses > 0) uses--;

        PlayerEntityManager.Singleton.StartCoroutine(HandleCooldown());

        return true;
    }

    IEnumerator HandleCooldown()
    {
        cooldownPercentage = 1;
        hasCooldown = true;
        float timeLeft = cooldown;
        while (timeLeft > 0)
        {
            cooldownPercentage = timeLeft / cooldown;
            yield return null;
            timeLeft -= Time.deltaTime;
        }
        hasCooldown = false;
        cooldownPercentage = 0;
    }

    public bool DrinkSoup()
    {
        if (hasCooldown || uses < 5 || PlayerEntityManager.Singleton.GetHealth() == 90) return false;

        PlayerEntityManager.Singleton.ModifyHealth(10);

        // Decrement uses if applicable
        uses -= 5;

        PlayerEntityManager.Singleton.StartCoroutine(HandleCooldown());

        return true;
    }
}