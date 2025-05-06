using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FlavorIngredient;
using BuffFlavor = FlavorIngredient.BuffFlavor;
using InflictionFlavor = FlavorIngredient.InflictionFlavor;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;

[System.Serializable]
public class SoupSpoon
{
    // ~~~ DEFINITIONS ~~~
    [System.Serializable]
    public class SpoonAbility // one for each type
    {
        //[SerializeField]
        public AbilityAbstractClass ability;

        //[SerializeField]
        public AbilityStats statsWithBuffs;

        public float lastUseTime;
        public int uses = 0;

        public Sprite icon;
        public Sprite iconUI;

        List<SpoonInfliction> inflictions;
        List<InflictionFlavor> inherentInflictions;

        // New spoon ability for new ability ingredient in the soup
        public SpoonAbility(AbilityIngredient ingredient, List<FlavorIngredient.BuffFlavor> buffs)
        {         
            ability = ingredient.abilityType;
            statsWithBuffs = new(ingredient.baseStats, buffs);
            icon = ingredient.Icon;
            iconUI = ingredient.IconUI;
            uses = ingredient.uses;
            inherentInflictions = ingredient.inherentInflictionFlavors;
        }

        public void CalculateInflictions(Dictionary<InflictionType, SpoonInfliction> genericInflictions)
        {
            Dictionary<InflictionType, SpoonInfliction> inflictionTracker = new(genericInflictions);
            foreach (var infliction in inherentInflictions) { 
                if (!inflictionTracker.ContainsKey(infliction.inflictionType))
                    inflictionTracker.Add(infliction.inflictionType, new(infliction));
                inflictionTracker[infliction.inflictionType].AddIngredient(infliction);
            }
            inflictions = inflictionTracker.Values.ToList();
        }

        public List<SpoonInfliction> GetSpoonInflictions()
        {
            return inflictions;
        }

        // This is called if we are adding an ability ingredient we already added
        public void AddIngredient(AbilityIngredient ingredient)
        {
            uses += ingredient.uses;
        }

        public bool OnCooldown()
        {
            return (Time.time - lastUseTime) < statsWithBuffs.cooldown;
        }

        public int GetUses()
        {
            return uses;
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
    public class SpoonInfliction // one for each type
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

        public SpoonInfliction(InflictionFlavor inflictionEffect) { InflictionFlavor = inflictionEffect; add = 0; mult = 1; }

        public SpoonInfliction(SpoonInfliction other) { InflictionFlavor = new(other.InflictionFlavor); add = other.add; mult = other.mult; }

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
    public List<SpoonAbility> spoonAbilities;
    public List<SpoonInfliction> spoonInflictions;
    //public int uses; // -1 = infinite
    public float cooldown;

    // Makes a Soup Spoon
    public SoupSpoon(List<Ingredient> ingredients)
    {
        // Track abilities and inflictions using dictionaries
        Dictionary<AbilityIngredient, SpoonAbility> abilityTracker = new();
        Dictionary<InflictionType, SpoonInfliction> inflictionTracker = new();

        // Track number of each flavor
        Dictionary<BuffFlavor.BuffType, int> FlavorBuffCounter = new();
        Dictionary<InflictionType, int> FlavorInflictionCounter = new();

        // Separate ingredients into ability and flavor categories
        List<AbilityIngredient> abilityIngredients = ingredients.Where(x => x.GetType() == typeof(AbilityIngredient)).Cast<AbilityIngredient>().ToList();
        List<FlavorIngredient> flavorIngredients = ingredients.Where(x => x.GetType() == typeof(FlavorIngredient)).Cast<FlavorIngredient>().ToList();

        // Collect and order buff flavors from flavor ingredients
        List<BuffFlavor> buffFlavors = new();
        flavorIngredients.ForEach(f => buffFlavors = buffFlavors.Concat(f.buffFlavors).ToList());
        //buffFlavors = buffFlavors.OrderBy(x => x.operation).ToList();

        // Collect infliction flavors from flavor ingredients //both flavor and ability ingredients
        List<InflictionFlavor> inflictionFlavors = new();
        flavorIngredients.ForEach(f => inflictionFlavors = inflictionFlavors.Concat(f.inflictionFlavors).ToList());

        // Initialize uses and cooldown
        //uses = 0;
        cooldown = 0;

        float totalCooldown = 0;

        // Populate ability tracker and calculate total uses and cooldown
        foreach (var ingredient in abilityIngredients)
        {
            if (!abilityTracker.ContainsKey(ingredient))
            {
                abilityTracker.Add(ingredient, new(ingredient, buffFlavors));              
            } else
            {
                abilityTracker[ingredient].AddIngredient(ingredient);
            }
            totalCooldown += ingredient.baseStats.cooldown;
        }
        foreach (var ingredient in abilityIngredients) cooldown += Mathf.Pow(ingredient.baseStats.cooldown, 2) / totalCooldown;

        // Set uses to infinite if specified
        //if (infinite) uses = -1;
        
        // Calculate average cooldown based on number of ability ingredients
        cooldown /= abilityIngredients.Count;

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
        spoonAbilities = abilityTracker.Values.ToList();

        void MultiplyFlavorPairing(FlavorIngredient ing, int count)
        {
            foreach (var spoonAbility in spoonAbilities)
            {
                foreach (var buff in ing.buffFlavors)
                {
                    spoonAbility.statsWithBuffs.MultiplyStat(buff.buffType, count);
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
            } else
            {
                var pair = (InflictionType)flavorIngredient.Pairing.GetPairing();
                if (!FlavorInflictionCounter.ContainsKey(pair)) continue;
                MultiplyFlavorPairing(flavorIngredient, FlavorInflictionCounter[pair]);
            }
        }

        foreach (var spoonAbility in spoonAbilities)
        {
            spoonAbility.CalculateInflictions(inflictionTracker);
        }

        // now that all infliction values are set, make it a finalized inflictions list
        spoonInflictions = inflictionTracker.Values.ToList();

        // set initial lastTimeUsed to cooldown to get atk right away
        lastTimeUsed = Time.time - cooldown;

    }

    static void PrintSpoon(SoupSpoon spoon)
    {
        string output = "SPOON Abilities\n";
        foreach (var ability in spoon.spoonAbilities)
        {
            output += $"{ability.ability._abilityName}=D{ability.statsWithBuffs.duration},SIZ{ability.statsWithBuffs.size},CRIT{ability.statsWithBuffs.crit},SP{ability.statsWithBuffs.speed},CO{ability.statsWithBuffs.cooldown}\n";
        }
        output += "INFLICTIONS\n";
        foreach (var infliction in spoon.spoonInflictions)
        {
            output += $"{infliction.InflictionFlavor.inflictionType} = {infliction.amount}";
        }
    }

    // Variable to track the last time the spoon was used
    float lastTimeUsed;

    public float GetCoolDownRatio()
    {
        if ((Time.time - lastTimeUsed) < cooldown)
        {
            return (Time.time - lastTimeUsed) / cooldown;
        } else
        {
            return 1;
        }
    }

    // Method to use the spoon, applying abilities and managing uses
    public bool UseSpoon()
    {
        // Check if the spoon is on cooldown
        if ((Time.time - lastTimeUsed) < cooldown) return false;
        lastTimeUsed = Time.time;

        // Apply each ability using the spoon if there are uses left
        foreach (SpoonAbility ability in spoonAbilities)
        {
            // use ability if there are uses left
            if (ability.uses > 0 || ability.uses == -1)
            {
                ability.ability.UseAbility(ability.statsWithBuffs, ability.GetSpoonInflictions());

            }
            
            // decrement if uses > 0
            if (ability.uses > 0)
            {
                ability.uses--;
            }
        }

        // Decrement uses if applicable
        //if (uses > 0) uses--;

        return true;
    }
}