using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

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
// ####################

// LOCAL VARIABLES
    // MAP CHUNKS LEGEND
    // _0 = Empty
    // _1 = Starting Cave
    // _2 = Boss
    // _3 = Alpha Path
    // _4 = Beta Path
    // 1_ = Cave
    // 2_ = Desert
    // 3_ = Forest
    NativeArray<int> MapChunks;
    private void Start()
    {
        MAP_SEED = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        RunStateManager.Singleton.InitializeGameState(MAP_SEED);

        MapChunks = new NativeArray<int>(MAPSIZE_INCHUNKS.x * MAPSIZE_INCHUNKS.y, Allocator.Persistent);

        var generateChunkPathJob = new GenerateChunkPathJob
        {
            MAPSIZE_INCHUNKS = MAPSIZE_INCHUNKS,
            CAVEBIOMEBOUNDS_INCHUNKS = CAVEBIOMEBOUNDS_INCHUNKS,
            MapChunks = MapChunks,
            seed = MAP_SEED
        };
        JobHandle GenerateChunkPathJobHandle = generateChunkPathJob.Schedule();
        GenerateChunkPathJobHandle.Complete();

        StartCoroutine(WaitForGameReady());

        IEnumerator WaitForGameReady()
        {
            yield return new WaitUntil(() => GenerateChunkPathJobHandle.IsCompleted);
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

            Color chunkType = (MapChunks[i] % 10) switch
            {
                1 => Color.gray,
                2 => Color.red,
                3 => Color.green,
                4 => Color.blue,
                _ => Color.white
            };
            Gizmos.color = chunkType;
            Gizmos.DrawCube(center, size);

            Color chunkBiome = (Mathf.FloorToInt(MapChunks[i] / 10)) switch
            {
                1 => Color.gray,
                2 => Color.yellow,
                3 => Color.green,
                _ => Color.clear
            };
            Gizmos.color = new Color(chunkBiome.r, chunkBiome.g, chunkBiome.b, .25f);
            Gizmos.DrawLineStrip(points, true);

        }
    }
}
