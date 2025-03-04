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
    private static string abilityCSVPath = "/Resources/CSVs/Ability Ingredients.csv";
    [MenuItem("Utilities/Generate Abilities")]
    public static void GenerateAbilityIngredients()
    {
        string folderPath = "Assets/CSVSOs/AbilityIngredientSOs/";
        ClearFolderBeforeCreatingAssets(folderPath);

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

            AssetDatabase.CreateAsset(abilityIngredient, $"{folderPath}{abilityIngredient.IngredientName}.asset");

            // Set Collectable
            if (splitData[0] != "Default Spoon")
            {
                // Set This To a Collectable
                Collectable ingredientCollectable = FindCollectableByName(splitData[0]);
                ingredientCollectable.ingredient = abilityIngredient;
            }
            else
            {
                PlayerInventory[] inventories = FindPlayerInventory();
                List<Ingredient> defaultSpoonIngredients = new()
                {
                    abilityIngredient
                };

                foreach (PlayerInventory i in inventories)
                {
                    i.defaultSpoonIngredients = defaultSpoonIngredients;
                }
          
            }
        }

        AssetDatabase.SaveAssets();
    }

    static void ClearFolderBeforeCreatingAssets(string folderPath)
    {

        // Ensure the folder exists
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath);

            foreach (string file in files)
            {
                if (!file.EndsWith(".meta")) // Avoid deleting meta files explicitly
                {
                    bool deleted = AssetDatabase.DeleteAsset(file);
                    if (!deleted)
                    {
                        Debug.LogWarning($"Failed to delete {file}");
                    }
                }
            }

            AssetDatabase.Refresh(); // Refresh the editor to reflect changes
        }
        else
        {
            Debug.LogWarning($"Folder '{folderPath}' does not exist.");
        }
    }


static AbilityAbstractClass FindAbilityByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogError("Sprite name is null or empty.");
            return null;
        }

        // sprites need to be in Resources folder to be found when unused
        var foundAbility = Resources.Load<AbilityAbstractClass>($"Ingredients/AbilityTypes/AbilitySOs/{name}");

        if (foundAbility == null)
        {
            Debug.LogError("No sprite found.");
            return null;
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




    //static PlayerInventory FindPlayerInventory()
    //{
    //    string name = "Player";

    //    // Use AssetDatabase to search for the prefab asset by name in the Assets folder
    //    string[] guids = AssetDatabase.FindAssets(name, new[] { "Assets/Entities/Player" });

    //    foreach (string guid in guids)
    //    {
    //        Debug.Log("GUIDS: " + guid);
    //    }

    //    if (guids.Length == 0)
    //    {
    //        Debug.LogError($"No prefab found with name: {name}");
    //        return null;
    //    }

    //    // Convert GUID to asset path
    //    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
    //    Debug.Log("Asset Path: " + path);

    //    // Load the prefab at the specified path
    //    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
    //    if (prefab != null)
    //    {
    //        // Get the PlayerInventory component from the prefab
    //        PlayerInventory foundInventory = prefab.GetComponent<PlayerInventory>();
    //        if (foundInventory == null)
    //        {
    //            Debug.LogError("Prefab does not contain a PlayerInventory component.");
    //            return null;
    //        }
    //        return foundInventory;
    //    }

    //    Debug.LogError("No Inventory Found");
    //    return null;
    //}


    static PlayerInventory[] FindPlayerInventory()
    {
        // sprites need to be in Resources folder to be found when unused
        var inventory = Resources.FindObjectsOfTypeAll<PlayerInventory>();
        if (inventory == null || inventory.Length == 0)
        {
            Debug.LogError("No collectable found.");
            return null;
        }

        return inventory;
    }

    //static PlayerInventory FindPlayerInventory()
    //{
    //    string name = "Player";
    //    string[] guids = AssetDatabase.FindAssets(name, new[] { "Assets/Entities/Player" }); // Search in the Prefabs folder or specify the folder you want.

    //    if (guids.Length == 0)
    //    {
    //        Debug.LogError($"No prefab found with name: {name}");
    //        return null;
    //    }

    //    string path = AssetDatabase.GUIDToAssetPath(guids[0]); // Get the path of the first match
    //    PlayerInventory foundInventory = AssetDatabase.LoadAssetAtPath<PlayerInventory>(path);

    //    if (foundInventory == null)
    //    {
    //        Debug.LogError($"No prefab found with name: {name}");
    //    }

    //    return foundInventory;
    //}

}
