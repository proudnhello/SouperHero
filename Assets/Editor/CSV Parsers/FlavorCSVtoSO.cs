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
using System.Drawing.Printing;

public class FlavorCSVtoSO
{

    // All the folder paths
    // The path to folder we make new SOs in
    static readonly string writeFolderPath = "Assets/Resources/Ingredients/Flavors/_STATS/";
    // The path to folder where the icon sprites are
    static readonly string iconPath = "EncyclopediaIcons/";
    // The path to the flavor CSV
    private static string flavorCSVPath = "/Resources/CSVs/Flavor Ingredients.csv";
    // Path to where collectables are
    static readonly string collectablePath = "Ingredients/Flavors/Collectables/";


    [MenuItem("Utilities/Generate Flavors")]
    public static void GenerateFlavorIngredients()
    {

        ClearFolderBeforeCreatingAssets(writeFolderPath);

        string path = Application.dataPath + flavorCSVPath;

        string[] data = File.ReadAllLines(Application.dataPath + flavorCSVPath);
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

            FlavorIngredient flavorIngredient = ScriptableObject.CreateInstance<FlavorIngredient>();
            
            // Set Ingredient Name
            flavorIngredient.IngredientName = splitData[0];

            // Set Ingredient Flavors
            List<InflictionFlavor> inherentInflictionFlavors = new();
            List<BuffFlavor> inherentBuffFlavors = new();

            for (int i=1; i<7; i += 3)
            {
                if (!string.IsNullOrEmpty(splitData[i]))
                {
                    if (Enum.TryParse(splitData[i], out InflictionFlavor.InflictionType inflictionType))
                    {
                        InflictionFlavor inflictionFlavor = new()
                        {
                            inflictionType = inflictionType
                        };

                        inflictionFlavor.amount = int.Parse(splitData[i + 1]);
                        inflictionFlavor.statusEffectDuration = float.Parse(splitData[i + 2]);

                        // add parsed infliction flavor
                        inherentInflictionFlavors.Add(inflictionFlavor);
                    } else if (Enum.TryParse(splitData[i], out BuffFlavor.BuffType buffType))
                    {
                        BuffFlavor buffFlavor = new()
                        {
                            buffType = buffType
                        };


                        buffFlavor.amount = int.Parse(splitData[i + 1]);

                        // add parsed infliction flavor
                        inherentBuffFlavors.Add(buffFlavor);
                    }
                    else
                    {
                        Debug.LogError($"Invalid infliction type enum name: {splitData[i]}.");
                    }
                }

            }

            flavorIngredient.Pairing = new(splitData[10]);

            flavorIngredient.inflictionFlavors = inherentInflictionFlavors;
            flavorIngredient.buffFlavors = inherentBuffFlavors;

            // Set Icon
            if (!string.IsNullOrWhiteSpace(splitData[7]))
            {
                Sprite icon = FindSpriteByName(splitData[7]);
                flavorIngredient.EncyclopediaImage = icon;
            }

            flavorIngredient.Source = splitData[8];
            flavorIngredient.FlavorProfile = splitData[9];

            AssetDatabase.CreateAsset(flavorIngredient, $"{writeFolderPath}{flavorIngredient.IngredientName}.asset");

            // Set This To a Collectable
            Collectable ingredientCollectable = FindCollectableByName(splitData[0]);
            ingredientCollectable.ingredient = flavorIngredient;
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

    static Sprite FindSpriteByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogError("Sprite name is null or empty.");
            return null;
        }

        // sprites need to be in Resources folder to be found when unused
        var foundSprite = Resources.Load<Sprite>($"{iconPath}{name}");

        if (foundSprite == null)
        {
            Debug.LogError("No sprite found for " + name);
            return null;
        }

        return foundSprite;
    }

    //static Collectable FindCollectableByName(string name)
    //{
    //    if (string.IsNullOrWhiteSpace(name))
    //    {
    //        Debug.LogError("Collectable name is null or empty.");
    //        return null;
    //    }

    //    // sprites need to be in Resources folder to be found when unused
    //    var collectable = Resources.FindObjectsOfTypeAll<Collectable>();
    //    if (collectable == null || collectable.Length == 0)
    //    {
    //        Debug.LogError("No collectable found.");
    //        return null;
    //    }

    //    Collectable foundSprite = collectable.FirstOrDefault(a => a.name == name);
    //    if (foundSprite == null)
    //    {
    //        Debug.LogError($"No collectable found with name: {name}");
    //    }

    //    return foundSprite;
    //}

    // Find the collectables with same name as AbilityIngredient SOs
    // To set the collectable with the new SO
    static Collectable FindCollectableByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogError("Collectable name is null or empty.");
            return null;
        }

        // sprites need to be in Resources folder to be found when unused
        var foundCollectable = Resources.Load<Collectable>($"{collectablePath}{name}");

        if (foundCollectable == null)
        {
            Debug.LogError($"No collectable found with name: {name}.");
            return null;
        }

        return foundCollectable;
    }
}
