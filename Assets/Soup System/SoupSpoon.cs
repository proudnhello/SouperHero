using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BuffFlavor = FlavorIngredient.BuffFlavor;
using InflictionFlavor = FlavorIngredient.InflictionFlavor;
using InflictionType = FlavorIngredient.InflictionFlavor.InflictionType;

public class SoupSpoon
{
    // ~~~ DEFINITIONS ~~~
    public class SpoonAbility // one for each type
    {
        public AbilityAbstractClass ability;
        public AbilityStats statsWithBuffs;
        float lastUseTime;

        public SpoonAbility(AbilityIngredient ingredient, List<FlavorIngredient.BuffFlavor> buffs)
        {
            ability = ingredient.ability;
            statsWithBuffs = new(ingredient.baseStats, buffs);
        }

        public bool Use()
        {
            return (Time.time - lastUseTime) < statsWithBuffs.cooldown;
        }
    }

    public class SpoonInfliction // one for each type
    {
        public InflictionFlavor InflictionFlavor;
        public int add;
        public float mult;

        public SpoonInfliction(InflictionFlavor inflictionEffect) { InflictionFlavor = inflictionEffect; add = 0; mult = 1; }

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

    public SoupSpoon(List<Ingredient> ingredients, bool infinite = false)
    {
        Dictionary<AbilityAbstractClass, SpoonAbility> abilityTracker = new();
        Dictionary<InflictionType, SpoonInfliction> inflictionTracker = new();

        List<AbilityIngredient> abilityIngredients = ingredients.Where(x => x.GetType() == typeof(AbilityIngredient)).Cast<AbilityIngredient>().ToList();
        List<FlavorIngredient> flavorIngredients = ingredients.Where(x => x.GetType() == typeof(FlavorIngredient)).Cast<FlavorIngredient>().ToList();

        List<BuffFlavor> buffFlavors = new();
        flavorIngredients.ForEach(f => buffFlavors = buffFlavors.Concat(f.buffFlavors).ToList());
        buffFlavors = buffFlavors.OrderBy(x => x.operation).ToList();

        List<InflictionFlavor> inflictionFlavors = new();
        flavorIngredients.ForEach(f => inflictionFlavors = inflictionFlavors.Concat(f.inflictionFlavors).ToList());
        abilityIngredients.ForEach(f => inflictionFlavors = inflictionFlavors.Concat(f.inherentInflictionFlavors).ToList());

        uses = 0;
        cooldown = 0;
        foreach (var ingredient in abilityIngredients)
        {
            if (!abilityTracker.ContainsKey(ingredient.ability)) abilityTracker.Add(ingredient.ability, new(ingredient, buffFlavors));
            uses += ingredient.uses;
            cooldown += ingredient.baseStats.cooldown;
        }
        if (infinite) uses = -1;
        cooldown /= abilityIngredients.Count;

        foreach (var infliction in inflictionFlavors)
        {
            if (!inflictionTracker.ContainsKey(infliction.inflictionType)) inflictionTracker.Add(infliction.inflictionType, new(infliction));
            inflictionTracker[infliction.inflictionType].AddIngredient(infliction);
        }

        spoonAbilities = abilityTracker.Values.ToList();
        spoonInflictions = inflictionTracker.Values.ToList();
        Debug.Log("inflictions = " + spoonInflictions.Count);
    }

    float lastTimeUsed;
    public void UseSpoon()
    {
        if ((Time.time - lastTimeUsed) < cooldown) return;
        lastTimeUsed = Time.time;

        foreach (var ability in spoonAbilities)
        {
            ability.ability.UseAbility(ability.statsWithBuffs, spoonInflictions);
        }

        if (uses > 0) uses--;
    }
}