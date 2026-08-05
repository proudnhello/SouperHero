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
using UnityEngine.Tilemaps;


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
    public enum ConnectorType
    {
        None,
        Two_NS,
        Two_EW,
        Two_NE,
        Two_SE,
        Two_SW,
        Two_NW,
        Three_NSE,
        Three_SEW,
        Three_NSW,
        Three_NEW,
        Four
    }

    [Serializable]
    public struct GenerationInfo
    {
        internal int UUID;
        internal Coord RoomSpawn;

        public RoomType Type;
        public Biome Biome;
        public ConnectorType ConnectorType;
        public Vector2Int GridDimensions; // from bottom left
        public int GridPadding; // total space taken is GridDimensions + GridPadding * 2
        internal Vector2Int TotalGridSpace
        {
            get => new(GridDimensions.x + GridPadding * 2, GridDimensions.y + GridPadding * 2);
        }
        public UnsafeList<(Coord coord, Door.Direction dir)> Doors;

        public GenerationInfo InitInfo(MapRoom room)
        {
            UUID = room.GetHashCode() + 32; // +32 since we need the first 5 bits for assigning connectors to grid
            Doors = new(0, Allocator.Persistent);
            foreach (var door in room.Doors) Doors.Add((new(door.Pos), door.dir));
            return this;
        }

        public GenerationInfo Null()
        {
            UUID = -1;
            return this;
        }
    }
    public GenerationInfo Info;
    public Door[] Doors;

    // ############ DELETE
    [SerializeField]
    private int _blockWidth;
    [SerializeField]
    private int _blockHeight;


    // ORDERED FROM BOTTOM LEFT TO TOP RIGHT
    public List<Block> blocks = new();
    [Header("Room Content")]
    [SerializeField] ContentRegion[] contentRegions;
    public Transform entities;

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
    [Serializable]
    public class Door
    {
        public GameObject Open;
        public GameObject Closed;
        public Vector2Int Pos;
        [Serializable]
        public enum Direction
        {
            North,
            South,
            East,
            West
        }
        public Direction dir;
        internal bool isOpen;
    }

    private void Start()
    {
        //Array.ForEach(GetComponentsInChildren<DualGridTilemapModule>(), (x) => {
        //    if (x.gameObject.activeInHierarchy) x.RefreshRenderTilemap();
        //});
    }

    private void OnDisable()
    {
        RoomGenerator2.SpawnEntities -= InitializeContent;
    }

    //private void OnGUI()
    //{
    //    if (GUILayout.Button("Refresh Tilemaps"))
    //    {
    //        Array.ForEach(GetComponentsInChildren<DualGridTilemapModule>(), (x) => {
    //            if (x.gameObject.activeInHierarchy) x.RefreshRenderTilemap();
    //        });
    //    }
    //}


    bool hasBeenInitialized = false;
    public virtual Tilemap InitializeTiles(int difficultyPointBalance)
    {
        if (hasBeenInitialized) return null;

        foreach (var door in Doors) // above the hasBeenInitialized check since HUB will be called twice (since its in two chunks)
        {
            if (door.Open == null) continue;

            if (door.isOpen) { door.Open.SetActive(true); door.Closed.SetActive(false); }
            else { door.Open.SetActive(false); door.Closed.SetActive(true); }
        }


        Array.ForEach(GetComponentsInChildren<DualGridTilemapModule>(), (x) => {
            //if (x.gameObject.activeInHierarchy) { 
            //    x.transform.GetChild(0).parent = transform.parent; 
            //    x.gameObject.SetActive(false); 
            //}
            if (x.gameObject.activeInHierarchy) x.RefreshRenderTilemap();
        });

        RoomGenerator2.SpawnEntities += InitializeContent;

        return null;
    }

    public virtual void InitializeContent()
    {
        if (hasBeenInitialized) return;
        hasBeenInitialized = true;

        Array.ForEach(GetComponentsInChildren<CompositeCollider2D>(), (x) => {
            if (CollisionLayers.Singleton.InEnvironmentLayer(x.gameObject)) Destroy(x);
        });
        Array.ForEach(GetComponentsInChildren<Rigidbody2D>(), (x) => {
            if (CollisionLayers.Singleton.InEnvironmentLayer(x.gameObject)) Destroy(x);
        });

        int difficultyPointBalance = 0;
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

            spawn.SpawnEnemy(entities);
        }

        DestroyableSpawnLocation[] destroyableSpawnLocations = GetComponentsInChildren<DestroyableSpawnLocation>();
        foreach (var spawn in destroyableSpawnLocations)
        {
            if (!spawn.gameObject.activeInHierarchy) continue;

            spawn.SpawnDestroyable(entities);
        }

        return;
    }
}
