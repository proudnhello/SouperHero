using skner.DualGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


public class MapRoom : MonoBehaviour
{
    public enum RoomType
    {
        START,
        INTERMEDIATE,
        CONNECTOR,
        CAMPFIRE,
        BOSS
    }
    public enum Biome
    {
        CAVE,
        DESERT,
        FOREST
    }

    [Serializable]
    public struct GenerationInfo
    {
        internal int UUID;
        public RoomType Type;
        public Biome Biome;
        public Vector2Int GridDimensions; // from bottom left
        public UnsafeList<Vector2Int> NorthDoors;
        public UnsafeList<Vector2Int> SouthDoors;
        public UnsafeList<Vector2Int> EastDoors;
        public UnsafeList<Vector2Int> WestDoors;

        internal Vector2Int Location;

        public GenerationInfo InitInfo(MapRoom room)
        {
            UUID = room.GetHashCode();
            NorthDoors = new(0, Allocator.Persistent);
            SouthDoors = new(0, Allocator.Persistent);
            EastDoors = new(0, Allocator.Persistent);
            WestDoors = new(0, Allocator.Persistent);
            return this;
        }
    }
    public GenerationInfo Info;
    public Vector2Int[] NorthDoors;
    public Vector2Int[] SouthDoors;
    public Vector2Int[] EastDoors;
    public Vector2Int[] WestDoors;

    // ############ DELETE
    [SerializeField]
    private int _blockWidth;
    [SerializeField]
    private int _blockHeight;


    // ORDERED FROM BOTTOM LEFT TO TOP RIGHT
    public List<Block> blocks = new();
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
    // ##############

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
