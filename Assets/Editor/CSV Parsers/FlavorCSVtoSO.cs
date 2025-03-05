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

public class FlavorCSVtoSO
{
    private static string flavorCSVPath = "/Resources/CSVs/Flavor Ingredients.csv";
    [MenuItem("Utilities/Generate Flavors")]
    public static void GenerateFlavorIngredients()
    {

        string folderPath = "Assets/Resources/Ingredients/Flavors/_STATS/";
        ClearFolderBeforeCreatingAssets(folderPath);

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

            for (int i=1; i<9; i += 4)
            {
                if (!string.IsNullOrEmpty(splitData[i]))
                {
                    if (Enum.TryParse(splitData[i], out InflictionFlavor.InflictionType inflictionType))
                    {
                        InflictionFlavor inflictionFlavor = new()
                        {
                            inflictionType = inflictionType
                        };

                        if (Enum.TryParse(splitData[i + 1], out InflictionFlavor.Operation operation))
                        {
                            inflictionFlavor.operation = operation;
                        }
                        else
                        {
                            Debug.LogError("Invalid operation enum name.");
                        }

                        inflictionFlavor.amount = int.Parse(splitData[i + 2]);

                        inflictionFlavor.statusEffectDuration = float.Parse(splitData[i + 3]);

                        // add parsed infliction flavor
                        inherentInflictionFlavors.Add(inflictionFlavor);
                    } else if (Enum.TryParse(splitData[i], out BuffFlavor.BuffType buffType))
                    {
                        BuffFlavor buffFlavor = new()
                        {
                            buffType = buffType
                        };

                        if(Enum.TryParse(splitData[i + 1], out BuffFlavor.Operation operation))
                        {
                            buffFlavor.operation = operation;
                        }
                        else
                        {
                            Debug.LogError("Invalid operation enum name.");
                        }

                        buffFlavor.amount = int.Parse(splitData[i + 2]);

                        // add parsed infliction flavor
                        inherentBuffFlavors.Add(buffFlavor);
                    }
                    else
                    {
                        Debug.LogError($"Invalid infliction type enum name: {splitData[i]}.");
                    }
                }

            }

            flavorIngredient.inflictionFlavors = inherentInflictionFlavors;
            flavorIngredient.buffFlavors = inherentBuffFlavors;

            // Set Icon
            if (!string.IsNullOrWhiteSpace(splitData[9]))
            {
                Sprite icon = FindSpriteByName(splitData[9]);
                flavorIngredient.Icon = icon;
            }

            AssetDatabase.CreateAsset(flavorIngredient, $"{folderPath}{flavorIngredient.IngredientName}.asset");

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
}
