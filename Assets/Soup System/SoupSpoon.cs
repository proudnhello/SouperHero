using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        float lastUseTime;

        public SpoonAbility(AbilityIngredient ingredient, List<FlavorIngredient.BuffFlavor> buffs)
        {
            ability = ingredient.abilityType;
            statsWithBuffs = new(ingredient.baseStats, buffs);
            //Debug.Log($"SIZE STATS WITH BUFFS {statsWithBuffs.size}");
        }

        public bool Use()
        {
            return (Time.time - lastUseTime) < statsWithBuffs.cooldown;
        }
    }

    [System.Serializable]
    public class SpoonInfliction // one for each type
    {
        public InflictionFlavor InflictionFlavor;
        public int add;
        public float mult;

        public SpoonInfliction(InflictionFlavor inflictionEffect) { InflictionFlavor = inflictionEffect; add = 0; mult = 1; }

        public SpoonInfliction(SpoonInfliction other) { InflictionFlavor = new(other.InflictionFlavor); add = other.add; mult = other.mult; }

        public void AddIngredient(InflictionFlavor effect)
        {
            if (effect.operation == InflictionFlavor.Operation.Add) add += effect.amount;
            else mult += effect.amount;
        }
    }

    // ~~~ VARIABLES ~~~
    public List<SpoonAbility> spoonAbilities;
    public List<SpoonInfliction> spoonInflictions;
    public int uses; // -1 = infinite
    public float cooldown;

    // Makes a Soup Spoon
    public SoupSpoon(List<Ingredient> ingredients, bool infinite = false)
    {
        // Track abilities and inflictions using dictionaries
        Dictionary<AbilityIngredient, SpoonAbility> abilityTracker = new();
        Dictionary<InflictionType, SpoonInfliction> inflictionTracker = new();

        // Separate ingredients into ability and flavor categories
        List<AbilityIngredient> abilityIngredients = ingredients.Where(x => x.GetType() == typeof(AbilityIngredient)).Cast<AbilityIngredient>().ToList();
        List<FlavorIngredient> flavorIngredients = ingredients.Where(x => x.GetType() == typeof(FlavorIngredient)).Cast<FlavorIngredient>().ToList();

        // Collect and order buff flavors from flavor ingredients
        List<BuffFlavor> buffFlavors = new();
        flavorIngredients.ForEach(f => buffFlavors = buffFlavors.Concat(f.buffFlavors).ToList());
        buffFlavors = buffFlavors.OrderBy(x => x.operation).ToList();

        // Collect infliction flavors from both flavor and ability ingredients
        List<InflictionFlavor> inflictionFlavors = new();
        flavorIngredients.ForEach(f => inflictionFlavors = inflictionFlavors.Concat(f.inflictionFlavors).ToList());
        abilityIngredients.ForEach(f => inflictionFlavors = inflictionFlavors.Concat(f.inherentInflictionFlavors).ToList());

        // Initialize uses and cooldown
        uses = 0;
        cooldown = 0;

        // Populate ability tracker and calculate total uses and cooldown
        foreach (var ingredient in abilityIngredients)
        {
            if (!abilityTracker.ContainsKey(ingredient)) 
                abilityTracker.Add(ingredient, new(ingredient, buffFlavors));
            uses += ingredient.uses;
            cooldown += ingredient.baseStats.cooldown;
        }
        
        // Set uses to infinite if specified
        if (infinite) uses = -1;
        
        // Calculate average cooldown based on number of ability ingredients
        cooldown /= abilityIngredients.Count;

        // Populate infliction tracker with infliction flavors
        foreach (var infliction in inflictionFlavors)
        {
            if (!inflictionTracker.ContainsKey(infliction.inflictionType)) 
                inflictionTracker.Add(infliction.inflictionType, new(infliction));
            inflictionTracker[infliction.inflictionType].AddIngredient(infliction);
        }

        // Convert trackers to lists for use in abilities and inflictions
        spoonAbilities = abilityTracker.Values.ToList();
        spoonInflictions = inflictionTracker.Values.ToList();
        
        // set initial lastTimeUsed to cooldown to get atk right away
        lastTimeUsed = Time.time - cooldown;
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

        // Apply each ability using the spoon
        foreach (var ability in spoonAbilities)
        {
            ability.ability.UseAbility(ability.statsWithBuffs, spoonInflictions);
        }

        // Decrement uses if applicable
        if (uses > 0) uses--;

        // Call Changed Spoon
        PlayerInventory.Singleton.UpdateSpooon();

        return true;
    }
}