using skner.DualGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Flags]
public enum RoomType
{
    START = 0,
    INTERMEDIATE = 1,
    CONNECTOR = 2,
    ALL = START | INTERMEDIATE | CONNECTOR,
    NOT_CONNECTOR = START | INTERMEDIATE
}

public class MapRoom : MonoBehaviour
{
    [SerializeField]
    private int _blockWidth;
    [SerializeField]
    private int _blockHeight;

    // ORDERED FROM BOTTOM LEFT TO TOP RIGHT
    public List<Block> blocks = new();

    [Serializable]
    public class ContentRegion 
    { 
        public ContentOption[] contentOptions;
        internal bool hasBeenChosen = false;
    }
    [Serializable]
    public class ContentOption
    {
        public GameObject contentHolder;
        public int difficultyPointsRequired;
    }

    private void Start()
    {
        Array.ForEach(GetComponentsInChildren<DualGridTilemapModule>(), (x) => {
            if (x.gameObject.activeInHierarchy) x.RefreshRenderTilemap();
        });
    }

    [Header("Room Content")]
    [SerializeField] ContentRegion[] contentRegions;

    public int BlockWidth()
    {
        return _blockWidth;
    }

    public int BlockHeight()
    {
        return _blockHeight;
    }

    public Block At(int row, int col)
    {
        return blocks[col * (_blockWidth) + row];
    }

    bool hasBeenInitialized = false;
    public virtual void InitializeContents(int difficultyPointBalance)
    {
        if (hasBeenInitialized) return;
        hasBeenInitialized = true;

        int region = UnityEngine.Random.Range(0, contentRegions.Length);
        // loop through each region, choose an option, subtract difficulty points required, until all regions are chosen
        // EASY REGION = 0, so even with 0 points left, a region will always be chosen
        for (int i = 0; i < contentRegions.Length; i++, region = (region + 1) % contentRegions.Length)
        {
            ContentRegion currentRegion = contentRegions[region];
            foreach (var option in currentRegion.contentOptions)
            {
                if (option.difficultyPointsRequired <= difficultyPointBalance && !currentRegion.hasBeenChosen)
                {
                    currentRegion.hasBeenChosen = true;
                    difficultyPointBalance -= option.difficultyPointsRequired;
                    option.contentHolder.SetActive(true);
                    Array.ForEach(option.contentHolder.GetComponentsInChildren<DualGridTilemapModule>(), (x) => {
                        if (x.gameObject.activeInHierarchy) x.RefreshRenderTilemap();
                    });
                }
                else
                {
                    option.contentHolder.SetActive(false); // disable all other options just in case
                }
            }
        }

        EnemySpawnLocation[] enemySpawnLocations = GetComponentsInChildren<EnemySpawnLocation>();
        foreach (var spawn in enemySpawnLocations)
        {
            if (!spawn.gameObject.activeInHierarchy) continue;

            spawn.SpawnEnemy();
        }

        DestroyableSpawnLocation[] destroyableSpawnLocations = GetComponentsInChildren<DestroyableSpawnLocation>();
        foreach (var spawn in destroyableSpawnLocations)
        {
            if (!spawn.gameObject.activeInHierarchy) continue;

            spawn.SpawnDestroyable();
        }
    }
}
