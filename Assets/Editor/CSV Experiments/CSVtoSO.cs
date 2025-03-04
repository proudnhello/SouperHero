using UnityEngine;
using UnityEditor;
using System.IO;

public class CSVtoSO
{
    private static string abilityCSVPath = "/Editor/CSV Experiments/Detailed Ingredient List - Detailed Current Ability Ingredients.csv";
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
            abilityIngredient.IngredientName = splitData[0];
            Debug.Log($"Split Data 3: {splitData[3]}");
            abilityIngredient.baseStats.duration = float.Parse(splitData[3]);
            abilityIngredient.baseStats.size = float.Parse(splitData[4]);
            abilityIngredient.baseStats.crit = float.Parse(splitData[5]);
            abilityIngredient.baseStats.speed = float.Parse(splitData[6]);
            abilityIngredient.baseStats.cooldown = float.Parse(splitData[7]);
            abilityIngredient.uses = int.Parse(splitData[8]);

            AssetDatabase.CreateAsset(abilityIngredient, $"Assets/AbilitiesTest/{abilityIngredient.IngredientName}.asset");
        }


        AssetDatabase.SaveAssets();
    }
}
