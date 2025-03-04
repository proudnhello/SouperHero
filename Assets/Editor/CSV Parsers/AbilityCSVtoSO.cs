using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using NUnit.Framework;
using static FlavorIngredient;
using System.Collections.Generic;
using MacFsWatcher;
using System.Linq;
using Unity.VisualScripting;

public class AbilityCSVtoSO
{
    private static string abilityCSVPath = "/Editor/CSV Parsers/CSVs/Ability Ingredients.csv";
    [MenuItem("Utilities/Generate Abilities")]
    public static void GenerateAbilityIngredients()
    {
        string path = Application.dataPath + abilityCSVPath;

        string[] data = File.ReadAllLines(Application.dataPath + abilityCSVPath);
        if (data.Length > 0 && string.IsNullOrWhiteSpace(data[^1]))
        {
            Debug.Log("Last line is empty!");
        }

        bool first = true;
        foreach (string s in data)
        {
            // first row is headers
            if (first)
            {
                first = false;
                continue;
            }

            string[] splitData = s.Split(',');

            AbilityIngredient abilityIngredient = ScriptableObject.CreateInstance<AbilityIngredient>();
            
            // Set Ingredient Name
            abilityIngredient.IngredientName = splitData[0];

            // Set Base Stats
            abilityIngredient.baseStats.duration = float.Parse(splitData[3]);
            abilityIngredient.baseStats.size = float.Parse(splitData[4]);
            abilityIngredient.baseStats.crit = float.Parse(splitData[5]);
            abilityIngredient.baseStats.speed = float.Parse(splitData[6]);
            abilityIngredient.baseStats.cooldown = float.Parse(splitData[7]);
            abilityIngredient.uses = int.Parse(splitData[8]);

            // Set Ingredient Flavors
            FlavorIngredient.InflictionFlavor.InflictionType inflictionType;
            List<InflictionFlavor> inherentInflictionFlavors = new();

            for (int i=9; i<15; i += 3)
            {
                if (!string.IsNullOrEmpty(splitData[i]))
                {
                    if (Enum.TryParse(splitData[i], out inflictionType))
                    {
                        FlavorIngredient.InflictionFlavor inflictionFlavor = new();
                        inflictionFlavor.inflictionType = inflictionType;

                        FlavorIngredient.InflictionFlavor.Operation operation;
                        if (Enum.TryParse("Add", out operation))
                        {
                            inflictionFlavor.operation = operation;
                        }
                        else
                        {
                            Debug.LogError("Invalid operation enum name.");
                        }

                        inflictionFlavor.amount = int.Parse(splitData[i + 1]);

                        inflictionFlavor.statusEffectDuration = float.Parse(splitData[i + 2]);

                        // add parsed infliction flavor
                        inherentInflictionFlavors.Add(inflictionFlavor);
                    }
                    else
                    {
                        Debug.LogError($"Invalid infliction type enum name: {splitData[i]}.");
                    }
                }

            }

            abilityIngredient.inherentInflictionFlavors = inherentInflictionFlavors;

            // Set Ability Type
            AbilityAbstractClass abilityType = FindAbilityByName(splitData[2]);
            abilityIngredient.abilityType = abilityType;

            // Set Icon
            if (!string.IsNullOrWhiteSpace(splitData[15]))
            {
                Sprite icon = FindSpriteByName(splitData[15]);
                abilityIngredient.Icon = icon;
            }

            AssetDatabase.CreateAsset(abilityIngredient, $"Assets/CSVSOs/AbilityIngredientSOs/{abilityIngredient.IngredientName}.asset");

            // Set Collectable
            if (splitData[0] != "Default Spoon")
            {
                // Set This To a Collectable
                Collectable ingredientCollectable = FindCollectableByName(splitData[0]);
                ingredientCollectable.ingredient = abilityIngredient;
            }
            else
            {
                PlayerInventory inventory = FindPlayerInventory();
                List<Ingredient> defaultSpoonIngredients = new()
                {
                    abilityIngredient
                };
                inventory.defaultSpoonIngredients = defaultSpoonIngredients;
            }
        }

        AssetDatabase.SaveAssets();
    }

    static AbilityAbstractClass FindAbilityByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogError("Ability name is null or empty.");
            return null;
        }

        var abilities = Resources.FindObjectsOfTypeAll<AbilityAbstractClass>();
        if (abilities == null || abilities.Length == 0)
        {
            Debug.LogError("No abilities found.");
            return null;
        }

        AbilityAbstractClass foundAbility = abilities.FirstOrDefault(a => a.name == name);
        if (foundAbility == null)
        {
            Debug.LogError($"No ability found with name: {name}");
        }

        return foundAbility;
    }

    static Sprite FindSpriteByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogError("Sprite name is null or empty.");
            return null;
        }

        // sprites need to be in Resources folder to be found when unused
        var foundSprite = Resources.Load<Sprite>($"Placeholder Items (Replace)/{name}");

        if (foundSprite == null)
        {
            Debug.LogError("No sprite found.");
            return null;
        }

        return foundSprite;
    }

    static Collectable FindCollectableByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogError("Collectable name is null or empty.");
            return null;
        }

        // sprites need to be in Resources folder to be found when unused
        var collectable = Resources.FindObjectsOfTypeAll<Collectable>();
        if (collectable == null || collectable.Length == 0)
        {
            Debug.LogError("No collectable found.");
            return null;
        }

        Collectable foundSprite = collectable.FirstOrDefault(a => a.name == name);
        if (foundSprite == null)
        {
            Debug.LogError($"No collectable found with name: {name}");
        }

        return foundSprite;
    }

    static PlayerInventory FindPlayerInventory()
    {
        // sprites need to be in Resources folder to be found when unused
        var inventory = Resources.FindObjectsOfTypeAll<PlayerInventory>();
        if (inventory == null || inventory.Length == 0)
        {
            Debug.LogError("No collectable found.");
            return null;
        }

        string name = "Player";
        PlayerInventory foundInventory = inventory.FirstOrDefault(a => a.name == name);
        if (foundInventory == null)
        {
            Debug.LogError($"No collectable found with name: {name}");
        }

        return foundInventory;
    }
}
