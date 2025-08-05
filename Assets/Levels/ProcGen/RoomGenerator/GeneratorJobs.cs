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

[BurstCompile]
struct GenerateChunkPathJob : IJob
{
    public Vector2Int MAPSIZE_INCHUNKS;
    public Vector2Int CAVEBIOMEBOUNDS_INCHUNKS;
    public NativeArray<int> MapChunks;
    public uint seed;

    int GetChunk(int x, int y)
    {
        if (x < 0 || x >= MAPSIZE_INCHUNKS.x || y < 0 || y >= MAPSIZE_INCHUNKS.y) return -1;
        return MapChunks[x + y * MAPSIZE_INCHUNKS.y];
    }
    void SetChunk(int x, int y, int value)
    {
        if (x < 0 || x >= MAPSIZE_INCHUNKS.x || y < 0 || y >= MAPSIZE_INCHUNKS.y) return;
        MapChunks[x + y * MAPSIZE_INCHUNKS.y] += value;
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
                    SetChunk(x, y, 10); 
                }
                else if (x < MAPSIZE_INCHUNKS.x/2)
                {
                   SetChunk(x, y, 20);
                }
                else
                {
                    SetChunk(x, y, 30);
                }
            }
        }
        Random rng = new Random(seed);

        // 2. choose starting cave chunk on left hemisphere
        Coord StartingChunk1 = new(CAVEBIOMEBOUNDS_INCHUNKS.x, rng.NextInt(CAVEBIOMEBOUNDS_INCHUNKS.x, CAVEBIOMEBOUNDS_INCHUNKS.y + 1));
        SetChunk(StartingChunk1.x, StartingChunk1.y, 1);

        // 3. choose boss chunk
        // fuck it im manually coding each spot assuming the world is 6x6
        UnsafeList<Coord> PotentialBossSpots1 = new(0, Allocator.Persistent)
        {
            new Coord(0,0),new Coord(1,0),new Coord(2,0),new Coord(0,1),new Coord(0,2),new Coord(0,3),new Coord(0,4),new Coord(0,5),new Coord(1,5),new Coord(2,5)
        };
        if (StartingChunk1.y == 2) PotentialBossSpots1.Add(new Coord(1, 4));
        else PotentialBossSpots1.Add(new Coord(1, 1));

        Coord BossChunk1 = PotentialBossSpots1[rng.NextInt(0, PotentialBossSpots1.Length)];

        // 4. A* alpha path from starting cave to boss
        var AlphaPath = FindAStarPath(StartingChunk1, BossChunk1);
        foreach (var coord in AlphaPath)
        {
            SetChunk(coord.x, coord.y, 3);
        }

        // 5. A* beta path from starting cave to boss
        var BetaPath = FindAStarPath(StartingChunk1, BossChunk1);
        foreach (var coord in BetaPath)
        {
            SetChunk(coord.x, coord.y, 4);
        }

        SetChunk(BossChunk1.x, BossChunk1.y, 2);



        // repeat 2-5 for other side
        Coord StartingChunk2 = new(CAVEBIOMEBOUNDS_INCHUNKS.y, rng.NextInt(CAVEBIOMEBOUNDS_INCHUNKS.x, CAVEBIOMEBOUNDS_INCHUNKS.y + 1));
        SetChunk(StartingChunk2.x, StartingChunk2.y, 1);

        UnsafeList<Coord> PotentialBossSpots2 = new(0, Allocator.Persistent)
        {
            new Coord(3,0),new Coord(4,0),new Coord(5,0),new Coord(5,1),new Coord(5,2),new Coord(5,3),new Coord(5,4),new Coord(5,5),new Coord(4,5),new Coord(3,5)
        };
        if (StartingChunk2.y == 2) PotentialBossSpots2.Add(new Coord(4, 4));
        else PotentialBossSpots2.Add(new Coord(4, 1));

        Coord BossChunk2 = PotentialBossSpots2[rng.NextInt(0, PotentialBossSpots2.Length)];

        AlphaPath = FindAStarPath(StartingChunk2, BossChunk2);
        foreach (var coord in AlphaPath)
        {
            SetChunk(coord.x, coord.y, 3);
        }

        BetaPath = FindAStarPath(StartingChunk2, BossChunk2);
        foreach (var coord in BetaPath)
        {
            SetChunk(coord.x, coord.y, 4);
        }

        SetChunk(BossChunk2.x, BossChunk2.y, 2);
    }
    UnsafeList<Coord> FindAStarPath(Coord START, Coord GOAL)
    {
        float heuristicDist(Coord a, Coord b)
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

            if (GetChunk(current.x, current.y - 1) % 10 == 0) neighbors.Add(new(current.x, current.y - 1));
            if (GetChunk(current.x - 1, current.y) % 10 == 0) neighbors.Add(new(current.x - 1, current.y));
            if (GetChunk(current.x + 1, current.y) % 10 == 0) neighbors.Add(new(current.x + 1, current.y));
            if (GetChunk(current.x, current.y + 1) % 10 == 0) neighbors.Add(new(current.x, current.y + 1));

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