using System;
using System.Collections.Generic;
using UnityEngine;
using static FlavorIngredient;

[CreateAssetMenu(fileName = "ChestDistribution", menuName = "Create Chest Distribution")]
public class ChestDistribution : ScriptableObject 
{
    [Serializable]
    public class Bowl
    {
        public SoupCollectable soup;
        public int odds;
    }

    [SerializeField] Bowl[] highLoot;
    [SerializeField] Bowl[] mediumLoot;
    [SerializeField] Bowl[] lowLoot;


    List<int> highLootWeightedRandom;
    List<int> mediumLootWeightedRandom;
    List<int> lowLootWeightedRandom;
    public SoupCollectable GetBowl(int rarity) // 2 = high, 1 = med, 0 = low
    {
        Bowl[] bowls = (rarity) switch
        {
            2 => highLoot,
            1 => mediumLoot,
            _ => lowLoot
        };

        List<int> weights = (rarity) switch
        {
            2 => highLootWeightedRandom,
            1 => mediumLootWeightedRandom,
            _ => lowLootWeightedRandom
        };

        if (weights == null)
        {
            weights = new();
            for (int b = 0; b < bowls.Length; b++)
                for (int i = 0; i < bowls[b].odds; i++) weights.Add(b);

            if (rarity == 2) highLootWeightedRandom = weights;
            else if (rarity == 1) mediumLootWeightedRandom = weights;
            else lowLootWeightedRandom = weights;
        }

        return bowls[UnityEngine.Random.Range(0, weights.Count)].soup;
    }

}