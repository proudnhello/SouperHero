using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static MapRoom;
using Random = Unity.Mathematics.Random;
using Unity.Collections.LowLevel.Unsafe;

public class RoomGenerator2 : MonoBehaviour
{
// INSPECTOR VARIABLES
    [Header("Dimensions")]
    [SerializeField] Vector2Int MAPSIZE_INCHUNKS;
    [SerializeField] Vector2Int CHUNKSIZE_INGRIDUNITS;
    [SerializeField] Vector2Int GRIDUNITSIZE_INTILES;

    [Header("Layout")]
    [SerializeField] Vector2Int CAVEBIOMEBOUNDS_INCHUNKS;
    public uint MAP_SEED = 0;

    [Header("Rooms")]
    [SerializeField] MapRoom[] AllRoomsUnsorted;
// ####################

// LOCAL VARIABLES
    public NativeArray<int> Grid;
    NativeArray<Chunk> MapChunks;
    RoomDatabase RoomDatabase;
    Random RNG;
    private void Start()
    {
        MAP_SEED = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        RunStateManager.Singleton.InitializeGameState(MAP_SEED);

        RoomDatabase = new();
        RoomDatabase.Init(AllRoomsUnsorted, MAP_SEED);

        RNG = new Random(MAP_SEED);
        MapChunks = new NativeArray<Chunk>(MAPSIZE_INCHUNKS.x * MAPSIZE_INCHUNKS.y, Allocator.Persistent, NativeArrayOptions.ClearMemory);

        var generateChunkPathJob = new GenerateChunkPathJob
        {
            MAPSIZE_INCHUNKS = MAPSIZE_INCHUNKS,
            CAVEBIOMEBOUNDS_INCHUNKS = CAVEBIOMEBOUNDS_INCHUNKS,
            RNG = RNG,
            MapChunks = MapChunks
        };
        JobHandle GenerateChunkPathJobHandle = generateChunkPathJob.Schedule();

        var placeInitialRoomsJob = new PlaceInitialRoomsJob
        {
            MapChunks = generateChunkPathJob.MapChunks,
            RNG = RNG,
            RoomDatabase = RoomDatabase
        };
        JobHandle PlaceInitialRoomsJobHandle = placeInitialRoomsJob.Schedule(MapChunks.Length, 64, GenerateChunkPathJobHandle);

        PlaceInitialRoomsJobHandle.Complete();

        StartCoroutine(WaitForGameReady());

        IEnumerator WaitForGameReady()
        {
            yield return new WaitUntil(() => PlaceInitialRoomsJobHandle.IsCompleted);
            RunStateManager.Singleton.SaveRunState();
            GameManager.Singleton.StartRun();
        }

    }


    private void OnDrawGizmos()
    {
        Vector2 ChunkPointToWorldPoint(int x, int y)
        {
            x -= MAPSIZE_INCHUNKS.x / 2;
            y -= MAPSIZE_INCHUNKS.y / 2;
            return new Vector2(x * CHUNKSIZE_INGRIDUNITS.x * GRIDUNITSIZE_INTILES.x, y * CHUNKSIZE_INGRIDUNITS.y * GRIDUNITSIZE_INTILES.y);
        }
        for (int i = 0; i < MapChunks.Length; i++)
        {
            int x = i % MAPSIZE_INCHUNKS.x;
            int y = Mathf.FloorToInt(i / MAPSIZE_INCHUNKS.y);
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
            Gizmos.color = chunkType;
            Gizmos.DrawCube(center, size);

            Color chunkBiome = MapChunks[i].Biome switch
            {
                Biome.CAVE => Color.gray,
                Biome.DESERT => Color.yellow,
                Biome.FOREST => Color.green,
                _ => Color.clear
            };
            Gizmos.color = new Color(chunkBiome.r, chunkBiome.g, chunkBiome.b, .25f);
            Gizmos.DrawLineStrip(points, true);

        }
    }
}
