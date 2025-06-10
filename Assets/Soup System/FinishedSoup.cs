using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FlavorIngredient;
using BuffFlavor = FlavorIngredient.BuffFlavor;
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

        public Sprite icon;
        public Sprite iconUI;

        List<SoupInfliction> inflictions;
        List<InflictionFlavor> inherentInflictions;

        // New spoon ability for new ability ingredient in the soup
        public SoupAbility(AbilityIngredient ingredient, List<FlavorIngredient.BuffFlavor> buffs)
        {
            ability = ingredient.abilityType;
            statsWithBuffs = new(ingredient.baseStats, buffs);
            icon = ingredient.abilityType.icon;
            iconUI = ingredient.abilityType.icon;
            // uses = ingredient.uses;
            inherentInflictions = ingredient.inherentInflictionFlavors;
        }

        public void CalculateInflictions(Dictionary<InflictionType, SoupInfliction> genericInflictions)
        {
            Dictionary<InflictionType, SoupInfliction> inflictionTracker = new(genericInflictions);
            foreach (var infliction in inherentInflictions)
            {
                if (!inflictionTracker.ContainsKey(infliction.inflictionType))
                    inflictionTracker.Add(infliction.inflictionType, new(infliction));
                inflictionTracker[infliction.inflictionType].AddIngredient(infliction);
            }
            inflictions = inflictionTracker.Values.ToList();
        }

        public List<SoupInfliction> GetSpoonInflictions()
        {
            return inflictions;
        }

        // This is called if we are adding an ability ingredient we already added
        public void AddIngredient(AbilityIngredient ingredient)
        {
            // uses += ingredient.uses;
        }

        public bool OnCooldown()
        {
            return (Time.time - lastUseTime) < statsWithBuffs.cooldown;
        }

        public void PrintAbility()
        {
            string output = $"{ability._abilityName}=\n";
            foreach (var infliction in inflictions)
            {
                output += $"{infliction.InflictionFlavor.inflictionType} = {infliction.amount}\n";
            }
            Debug.Log(output);
        }
    }

    [System.Serializable]
    public class SoupInfliction // one for each type
    {
        public InflictionFlavor InflictionFlavor;
        public int add;
        public float mult;
        public float amount
        {
            get
            {
                return add * mult;
            }
        }

        public SoupInfliction(InflictionFlavor inflictionEffect) { InflictionFlavor = inflictionEffect; add = 0; mult = 1; }

        public SoupInfliction(SoupInfliction other) { InflictionFlavor = new(other.InflictionFlavor); add = other.add; mult = other.mult; }

        public void AddIngredient(InflictionFlavor effect)
        {
            add += effect.amount;
        }
        public void Multiply(int count)
        {
            mult += .2f * count;
        }
    }

    // ~~~ VARIABLES ~~~
    public List<Ingredient> ingredientList;
    public List<SoupAbility> soupAbilities;
    public List<SoupInfliction> soupInflictions;
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
        ingredientList = ingredients;
        soupBase = stock;

        // Track abilities and inflictions using dictionaries
        Dictionary<AbilityIngredient, SoupAbility> abilityTracker = new();
        Dictionary<InflictionType, SoupInfliction> inflictionTracker = new();

        // Track number of each flavor
        Dictionary<BuffFlavor.BuffType, int> FlavorBuffCounter = new();
        Dictionary<InflictionType, int> FlavorInflictionCounter = new();

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

        // Populate ability tracker and calculate total uses and cooldown
        foreach (var ingredient in abilityIngredients)
        {
            if (!abilityTracker.ContainsKey(ingredient))
            {
                abilityTracker.Add(ingredient, new(ingredient, buffFlavors));
            }
            else
            {
                abilityTracker[ingredient].AddIngredient(ingredient);
            }
            uses += ingredient.uses;
        }

        // Populate infliction tracker with infliction flavors
        foreach (var infliction in inflictionFlavors)
        {
            if (!inflictionTracker.ContainsKey(infliction.inflictionType))
                inflictionTracker.Add(infliction.inflictionType, new(infliction));
            inflictionTracker[infliction.inflictionType].AddIngredient(infliction);

            // Track number of each infliction
            if (FlavorInflictionCounter.ContainsKey(infliction.inflictionType)) FlavorInflictionCounter[infliction.inflictionType]++;
            else FlavorInflictionCounter.Add(infliction.inflictionType, 1);
        }

        // Track number of each buff
        foreach (var buff in buffFlavors)
        {
            if (FlavorBuffCounter.ContainsKey(buff.buffType)) FlavorBuffCounter[buff.buffType]++;
            else FlavorBuffCounter.Add(buff.buffType, 1);
        }

        // Convert ability track into spoon's finalized list of abilities
        soupAbilities = abilityTracker.Values.ToList();

        void MultiplyFlavorPairing(FlavorIngredient ing, int count)
        {
            foreach (var soupAbility in soupAbilities)
            {
                foreach (var buff in ing.buffFlavors)
                {
                    soupAbility.statsWithBuffs.MultiplyStat(buff.buffType, count);
                }
            }

            foreach (var infliction in ing.inflictionFlavors)
            {
                inflictionTracker[infliction.inflictionType].Multiply(count);
            }
        }

        // Now based on pairings, multiply corresponding stat
        foreach (var flavorIngredient in flavorIngredients)
        {
            if (flavorIngredient.Pairing.isBuff)
            {
                var pair = (BuffFlavor.BuffType)flavorIngredient.Pairing.GetPairing();
                if (!FlavorBuffCounter.ContainsKey(pair)) continue;
                MultiplyFlavorPairing(flavorIngredient, FlavorBuffCounter[pair]);
            }
            else
            {
                var pair = (InflictionType)flavorIngredient.Pairing.GetPairing();
                if (!FlavorInflictionCounter.ContainsKey(pair)) continue;
                MultiplyFlavorPairing(flavorIngredient, FlavorInflictionCounter[pair]);
            }
        }

        foreach (var soupAbility in soupAbilities)
        {
            soupAbility.CalculateInflictions(inflictionTracker);
        }

        // now that all infliction values are set, make it a finalized inflictions list
        soupInflictions = inflictionTracker.Values.ToList();

        // set initial lastTimeUsed to cooldown to get atk right away
        lastTimeUsed = Time.time - cooldown;
        PrintSoup(this);

    }

    static void PrintSoup(FinishedSoup spoon)
    {
        string output = "SPOON Abilities\n";
        foreach (var ability in spoon.soupAbilities)
        {
            output += $"{ability.ability._abilityName}=D{ability.statsWithBuffs.duration},SIZ{ability.statsWithBuffs.size},CRIT{ability.statsWithBuffs.crit},SP{ability.statsWithBuffs.speed},CO{ability.statsWithBuffs.cooldown}\n";
        }
        output += "INFLICTIONS\n";
        foreach (var infliction in spoon.soupInflictions)
        {
            output += $"{infliction.InflictionFlavor.inflictionType} = {infliction.amount}";
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