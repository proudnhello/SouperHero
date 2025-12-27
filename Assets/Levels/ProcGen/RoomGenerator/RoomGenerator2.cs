using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static MapRoom;
using static UnityEditor.Recorder.OutputPath;
using Random = Unity.Mathematics.Random;

[Serializable]
public struct MapInfo
{
    public Vector2Int MAP_SIZE; // in chunks
    public Vector2Int CHUNK_SIZE; // in grid units
    public Vector2Int GRID_SIZE; // in unity world units/tiles
    public Vector2Int CAVE_BIOME_BOUNDS; // in chunks
    public Vector2Int CAMPFIRE_PLACEMENT_AREA_SIZE; // in grid units (larger, smaller)
}

public class RoomGenerator2 : MonoBehaviour
{
    [Header("Layout")]
    public MapInfo MAP_INFO;
    public uint MAP_SEED = 0;

    [Header("Rooms")]
    [SerializeField] MapRoom[] AllRoomsUnsorted;
// ####################

// LOCAL VARIABLES
    public NativeArray<int> Grid;
    NativeArray<Chunk> MapChunks;
    RoomDatabase RoomDatabase;
    Random RNG;
    Dictionary<int, MapRoom> UUIDtoRoom;
    private void Start()
    {
        MAP_SEED = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        RunStateManager.Singleton.InitializeGameState(MAP_SEED);

        RoomDatabase = new();
        RoomDatabase.Init(AllRoomsUnsorted, MAP_SEED);
        UUIDtoRoom = AllRoomsUnsorted.ToDictionary(val => val.Info.UUID, val => val);

        RNG = new Random(MAP_SEED);
        MapChunks = new NativeArray<Chunk>(MAP_INFO.MAP_SIZE.x * MAP_INFO.MAP_SIZE.y, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        var generateChunkPathJob = new GenerateChunkPathJob
        {
            MAP_INFO = MAP_INFO,
            RNG = RNG,
            MapChunks = MapChunks
        };
        JobHandle GenerateChunkPathJobHandle = generateChunkPathJob.Schedule();

        var placeInitialRoomsJob = new PlaceInitialRoomsJob
        {
            MapChunks = generateChunkPathJob.MapChunks,
            RNG = RNG,
            RoomDatabase = RoomDatabase,
            MAP_INFO = MAP_INFO,
        };
        JobHandle PlaceInitialRoomsJobHandle = placeInitialRoomsJob.Schedule(GenerateChunkPathJobHandle);

        var placeConnectorsJob = new PlaceConnectorsJob
        {
            MapChunks = generateChunkPathJob.MapChunks,
            RNG = RNG,
            RoomDatabase = RoomDatabase,
            MAP_INFO = MAP_INFO,
        };
        JobHandle PlaceConnectorsJobHandle = placeConnectorsJob.Schedule(PlaceInitialRoomsJobHandle);

        PlaceConnectorsJobHandle.Complete();

        StartCoroutine(WaitForGameReady());

        IEnumerator WaitForGameReady()
        {
            yield return new WaitUntil(() => PlaceConnectorsJobHandle.IsCompleted);

            Transform RoomHolder = new GameObject("RoomHolder").transform;

            // spawn hub manually
            var hubRoom = UUIDtoRoom[RoomDatabase.GetRoom(RoomType.START, Biome.CAVE).UUID].gameObject;
            Vector2 hubSpawnPos = new Vector2(3, 3) * MAP_INFO.CHUNK_SIZE * MAP_INFO.GRID_SIZE - new Vector2(2, 2) * MAP_INFO.GRID_SIZE;
            MapRoom spawnRoom = Instantiate(hubRoom, hubSpawnPos, Quaternion.identity, RoomHolder).GetComponent<MapRoom>();

            PlayerSpawnLocation spawnLocation = spawnRoom.GetComponentInChildren<PlayerSpawnLocation>();
            RunStateManager.Singleton.InitialPlacePlayer(spawnLocation);

            foreach (var chunk in MapChunks)
            {
                if (chunk.ChunkType == Chunk.Type.Empty) continue;
                if (chunk.ChunkType != Chunk.Type.Starting) continue;
                int door = 0;
                foreach (var room in chunk.Rooms)
                {
                    MapRoom mRoom;
                    if (room.Type == RoomType.START)
                    {
                        mRoom = spawnRoom;
                    }
                    else
                    {
                        Vector2 spawnPos = new Vector2(chunk.Coordinate.x, chunk.Coordinate.y) * MAP_INFO.CHUNK_SIZE * MAP_INFO.GRID_SIZE + // chunk bottom left
                            new Vector2(room.RoomSpawn.x, room.RoomSpawn.y) * MAP_INFO.GRID_SIZE;
                        //Debug.Log($"In {chunk.Coordinate.x}, {chunk.Coordinate.y}, Place room " + room.Type + " " + room.UUID + " at " + room.RoomSpawn);
                        mRoom = Instantiate(UUIDtoRoom[room.UUID].gameObject, spawnPos, Quaternion.identity, RoomHolder).GetComponent<MapRoom>();
                    }
                    foreach (var d in mRoom.Doors)
                    {
                        if (chunk.DoorStates[door] == 0) d.isOpen = true;
                        door++;
                    }
                    mRoom.InitializeContents(0);
                }
            }
            
            RunStateManager.Singleton.SaveRunState();
            GameManager.Singleton.StartRun();
        }
    }
    private void OnDrawGizmos()
    {
        Vector2 ChunkPointToWorldPoint(int x, int y)
        {
            return new Vector2(x * MAP_INFO.CHUNK_SIZE.x * MAP_INFO.GRID_SIZE.x, y * MAP_INFO.CHUNK_SIZE.y * MAP_INFO.GRID_SIZE.y);
        }
        for (int i = 0; i < MapChunks.Length; i++)
        {
            int x = i % MAP_INFO.MAP_SIZE.x;
            int y = Mathf.FloorToInt(i / MAP_INFO.MAP_SIZE.y);
            Vector3[] points = new Vector3[4]
            {
                ChunkPointToWorldPoint(x, y),
                ChunkPointToWorldPoint(x, y+1),
                ChunkPointToWorldPoint(x+1, y+1),
                ChunkPointToWorldPoint(x+1, y)
            };

            Vector3 size = new Vector3(points[3].x - points[0].x, points[1].y - points[0].y, 0);
            Vector3 center = new Vector3(size.x / 2 + points[0].x,
                                         size.y / 2 + points[0].y,
                                         0);

            Color chunkType = (MapChunks[i].ChunkType) switch
            {
                Chunk.Type.Starting => Color.gray,
                Chunk.Type.Boss => Color.red,
                Chunk.Type.AlphaPath => Color.green,
                Chunk.Type.BetaPath => Color.blue,
                _ => Color.white
            };
            chunkType = new Color(chunkType.r, chunkType.g, chunkType.b, .25f);
            Gizmos.color = chunkType;
            Gizmos.DrawCube(center, size);

            Color chunkBiome = MapChunks[i].Biome switch
            {
                Biome.CAVE => Color.gray,
                Biome.DESERT => Color.yellow,
                Biome.FOREST => Color.green,
                _ => Color.clear
            };
            Gizmos.color = new Color(chunkBiome.r, chunkBiome.g, chunkBiome.b, 1f);
            Gizmos.DrawLineStrip(points, true);

            Gizmos.color = Color.red;
            foreach (var rect in MapChunks[i].FreeRectangles)
            {
                Vector2 bL = ChunkPointToWorldPoint(x, y);
                Vector3[] rectPts = new Vector3[4]
                {
                    bL + rect.Coord.Vec * MAP_INFO.GRID_SIZE,
                    bL + (rect.Coord.Vec + new Vector2(0, rect.Size.y))  * MAP_INFO.GRID_SIZE,
                    bL + (rect.Coord.Vec + new Vector2(rect.Size.x, rect.Size.y)) * MAP_INFO.GRID_SIZE,
                    bL + (rect.Coord.Vec + new Vector2(rect.Size.x, 0)) * MAP_INFO.GRID_SIZE
                };
                Gizmos.DrawLineStrip(rectPts, true);
            }

        }
    }
}
