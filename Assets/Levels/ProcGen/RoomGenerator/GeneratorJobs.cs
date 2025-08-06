using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Collections.Generic;
using Utils;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;
using System;
using Random = Unity.Mathematics.Random;
using static MapRoom;
using UnityEditor.Localization.Plugins.XLIFF.V12;

[StructLayout(LayoutKind.Sequential)]
public struct Coord : IEquatable<Coord>
{
    public int x;
    public int y;

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(Coord other)
    {
        return x == other.x && y == other.y;
    }
    public override bool Equals(object obj) => (obj is Coord coord) && Equals(coord);
    public static bool operator ==(Coord left, Coord right) => left.Equals(right);
    public static bool operator !=(Coord left, Coord right) => !left.Equals(right);
    public override int GetHashCode() // https://discussions.unity.com/t/burst-error-bc1091-external-and-internal-calls-are-not-allowed-inside-static-constructors/896874/5
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + x.GetHashCode();
            hash = hash * 23 + y.GetHashCode();
            return hash;
        }
    }
}

// CHUNK TYPE LEGEND
// 00 = Empty
// _1 = Starting Cave
// _2 = Boss
// _3 = Alpha Path
// _4 = Beta Path
// 1_ = Cave
// 2_ = Desert
// 3_ = Forest
public unsafe struct Chunk
{
    public enum Type
    {
        Empty,
        Starting,
        Boss,
        AlphaPath,
        BetaPath
    }
    public UnsafeList<GenerationInfo> Rooms;
    public UnsafeList<int> Grid;
    public Type ChunkType;
    public Biome Biome;
    public Vector2Int NextChunkInPath;
}

[BurstCompile]
struct GenerateChunkPathJob : IJob
{
    public Vector2Int MAPSIZE_INCHUNKS;
    public Vector2Int CAVEBIOMEBOUNDS_INCHUNKS;
    public NativeArray<Chunk> MapChunks;
    public Random RNG;

    Chunk.Type GetChunkType(int x, int y)
    {
        if (x < 0 || x >= MAPSIZE_INCHUNKS.x || y < 0 || y >= MAPSIZE_INCHUNKS.y) return Chunk.Type.Empty;
        return MapChunks[x + y * MAPSIZE_INCHUNKS.y].ChunkType;
    }
    void SetBiomeChunk(int x, int y, Biome biome)
    {
        if (x < 0 || x >= MAPSIZE_INCHUNKS.x || y < 0 || y >= MAPSIZE_INCHUNKS.y) return;
        Chunk chunk = MapChunks[x + y * MAPSIZE_INCHUNKS.y];
        chunk.Biome = biome;
        MapChunks[x + y * MAPSIZE_INCHUNKS.y] = chunk;
    }
    void SetChunk(int x, int y, Chunk.Type type)
    {
        if (x < 0 || x >= MAPSIZE_INCHUNKS.x || y < 0 || y >= MAPSIZE_INCHUNKS.y) return;
        Chunk chunk = MapChunks[x + y * MAPSIZE_INCHUNKS.y];
        chunk.ChunkType = type;
        MapChunks[x + y * MAPSIZE_INCHUNKS.y] = chunk;
    }
    void SetPathChunk(int x, int y, Chunk.Type type, int next_x, int next_y)
    {
        if (x < 0 || x >= MAPSIZE_INCHUNKS.x || y < 0 || y >= MAPSIZE_INCHUNKS.y) return;
        Chunk chunk = MapChunks[x + y * MAPSIZE_INCHUNKS.y];
        chunk.ChunkType = type;
        chunk.NextChunkInPath = new Vector2Int(next_x, next_y);
        MapChunks[x + y * MAPSIZE_INCHUNKS.y] = chunk;
    }
    public void Execute()
    {
        // 1. assign chunk biomes
        for (int x = 0; x < MAPSIZE_INCHUNKS.x; x++)
        {
            for (int y = 0; y < MAPSIZE_INCHUNKS.y; y++)
            {
                if (x >= CAVEBIOMEBOUNDS_INCHUNKS.x &&
                    x <= CAVEBIOMEBOUNDS_INCHUNKS.y &&
                    y >= CAVEBIOMEBOUNDS_INCHUNKS.x &&
                    y <= CAVEBIOMEBOUNDS_INCHUNKS.y)
                {
                    SetBiomeChunk(x, y, Biome.CAVE); 
                }
                else if (x < MAPSIZE_INCHUNKS.x/2)
                {
                   SetBiomeChunk(x, y, Biome.DESERT);
                }
                else
                {
                    SetBiomeChunk(x, y, Biome.FOREST);
                }
            }
        }

        // 2. choose starting cave chunk on left hemisphere
        Coord StartingChunk1 = new(CAVEBIOMEBOUNDS_INCHUNKS.x, RNG.NextInt(CAVEBIOMEBOUNDS_INCHUNKS.x, CAVEBIOMEBOUNDS_INCHUNKS.y + 1));
        SetChunk(StartingChunk1.x, StartingChunk1.y, Chunk.Type.Starting);

        // 3. choose boss chunk
        // fuck it im manually coding each spot assuming the world is 6x6
        UnsafeList<Coord> PotentialBossSpots1 = new(0, Allocator.Persistent)
        {
            new Coord(0,0),new Coord(1,0),new Coord(2,0),new Coord(0,1),new Coord(0,2),new Coord(0,3),new Coord(0,4),new Coord(0,5),new Coord(1,5),new Coord(2,5)
        };
        if (StartingChunk1.y == 2) PotentialBossSpots1.Add(new Coord(1, 4));
        else PotentialBossSpots1.Add(new Coord(1, 1));

        Coord BossChunk1 = PotentialBossSpots1[RNG.NextInt(0, PotentialBossSpots1.Length)];

        // 4. A* alpha path from starting cave to boss
        var AlphaPath = FindAStarPath(StartingChunk1, BossChunk1);
        for (int i = 0; i<AlphaPath.Length; i++)
        {
            Coord coord = AlphaPath[i];
            Coord next_cord;
            if (i == AlphaPath.Length - 1) next_cord = BossChunk1;
            else next_cord = AlphaPath[i + 1];
            SetPathChunk(coord.x, coord.y, Chunk.Type.AlphaPath, next_cord.x, next_cord.y);
        }

        // 5. A* beta path from starting cave to boss
        var BetaPath = FindAStarPath(StartingChunk1, BossChunk1);
        for (int i = 0; i < BetaPath.Length; i++)
        {
            Coord coord = BetaPath[i];
            Coord next_cord;
            if (i == BetaPath.Length - 1) next_cord = BossChunk1;
            else next_cord = BetaPath[i + 1];
            SetPathChunk(coord.x, coord.y, Chunk.Type.BetaPath, next_cord.x, next_cord.y);
        }

        SetChunk(BossChunk1.x, BossChunk1.y, Chunk.Type.Boss);



        // repeat 2-5 for other side
        Coord StartingChunk2 = new(CAVEBIOMEBOUNDS_INCHUNKS.y, RNG.NextInt(CAVEBIOMEBOUNDS_INCHUNKS.x, CAVEBIOMEBOUNDS_INCHUNKS.y + 1));
        SetChunk(StartingChunk2.x, StartingChunk2.y, Chunk.Type.Starting);

        UnsafeList<Coord> PotentialBossSpots2 = new(0, Allocator.Persistent)
        {
            new Coord(3,0),new Coord(4,0),new Coord(5,0),new Coord(5,1),new Coord(5,2),new Coord(5,3),new Coord(5,4),new Coord(5,5),new Coord(4,5),new Coord(3,5)
        };
        if (StartingChunk2.y == 2) PotentialBossSpots2.Add(new Coord(4, 4));
        else PotentialBossSpots2.Add(new Coord(4, 1));

        Coord BossChunk2 = PotentialBossSpots2[RNG.NextInt(0, PotentialBossSpots2.Length)];

        AlphaPath = FindAStarPath(StartingChunk2, BossChunk2);
        for (int i = 0; i < AlphaPath.Length; i++)
        {
            Coord coord = AlphaPath[i];
            Coord next_cord;
            if (i == AlphaPath.Length - 1) next_cord = BossChunk2;
            else next_cord = AlphaPath[i + 1];
            SetPathChunk(coord.x, coord.y, Chunk.Type.AlphaPath, next_cord.x, next_cord.y);
        }

        BetaPath = FindAStarPath(StartingChunk2, BossChunk2);
        for (int i = 0; i < BetaPath.Length; i++)
        {
            Coord coord = BetaPath[i];
            Coord next_cord;
            if (i == BetaPath.Length - 1) next_cord = BossChunk2;
            else next_cord = BetaPath[i + 1];
            SetPathChunk(coord.x, coord.y, Chunk.Type.BetaPath, next_cord.x, next_cord.y);
        }

        SetChunk(BossChunk2.x, BossChunk2.y, Chunk.Type.Boss);
    }
    UnsafeList<Coord> FindAStarPath(Coord START, Coord GOAL)
    {
        static float heuristicDist(Coord a, Coord b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return dx + dy;
        }

        Coord NONE = new(-1, -1);
        // ## PATHFINDING VARIABLES ##
        UnsafePriorityQueue frontier = new()
        {
            Entries = new(0, Allocator.Persistent)
        };
        frontier.Enqueue(START, 0f);
        UnsafeHashMap<Coord, Coord> came_from = new(0, Allocator.Persistent)
        {
            { START, NONE}
        };
        UnsafeHashMap<Coord, int> cost_so_far = new(0, Allocator.Persistent)
        {
            { START, 0}
        };
        Coord current = NONE;

        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();
            if (current == GOAL)
            {
                break;
            }

            UnsafeList<Coord> neighbors = new(0, Allocator.Persistent);

            if (GetChunkType(current.x, current.y - 1) == Chunk.Type.Empty) neighbors.Add(new(current.x, current.y - 1));
            if (GetChunkType(current.x - 1, current.y) == Chunk.Type.Empty) neighbors.Add(new(current.x - 1, current.y));
            if (GetChunkType(current.x + 1, current.y) == Chunk.Type.Empty) neighbors.Add(new(current.x + 1, current.y));
            if (GetChunkType(current.x, current.y + 1) == Chunk.Type.Empty) neighbors.Add(new(current.x, current.y + 1));

            for (int n = 0; n < neighbors.Length; n++)
            {
                Coord next = neighbors[n];
                int new_cost = cost_so_far[current];
                if (!cost_so_far.ContainsKey(next) || new_cost < cost_so_far[next])
                {
                    cost_so_far[next] = new_cost;
                    float priority = new_cost + heuristicDist(GOAL, next);
                    frontier.Enqueue(next, priority);
                    if (!came_from.TryAdd(next, current)) came_from[next] = current;
                }
            }

            neighbors.Dispose();
        }


        UnsafeList<Coord> path = new(0, Allocator.Persistent);
        // don't include START or GOAL in path
        current = came_from[current];
        while (current != NONE)
        {
            path.Add(current);
            current = came_from[current];
        }
        path.RemoveAt(path.Length - 1);

        return path;
    }
}

[BurstCompile]
struct PlaceInitialRoomsJob : IJobParallelFor
{
    public NativeArray<Chunk> MapChunks;
    public RoomDatabase RoomDatabase;
    public Random RNG;

    public void Execute(int index)
    {
        if (MapChunks[index].ChunkType == Chunk.Type.Empty) return;
        Debug.Log(MapChunks[index].ChunkType);
    }

    void PlaceStartingRoom()
    {

    }

    void PlaceCampfireRoom()
    {

    }
}