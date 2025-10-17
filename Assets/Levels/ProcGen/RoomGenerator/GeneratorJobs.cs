using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using Utils;
using static MapRoom;
using static PlaceInitialRoomsJob;
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
    public Vector2 Vec
    {
        get
        {
            return new Vector2(x, y);
        }
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
    public override string ToString()
    {
        return $"Coord = ({x},{y})";
    }
}

public struct FreeRectangle
{
    public Coord Coord;
    public Coord Size;
    public int Recursions;

    public bool IsIn(int bottom_x, int bottom_y, int size_x, int size_y)
    {
        return bottom_x >= Coord.x && bottom_y >= Coord.y &&
            size_x + bottom_x <= Coord.x + Size.x && size_y + bottom_y <= Coord.y + Size.y;
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
    public UnsafeList<FreeRectangle> FreeRectangles;
    public UnsafeList<int> Grid;
    public Type ChunkType;
    public Biome Biome;
    public Coord NextChunkInPath;
    public Coord Coordinate;

    public void InitGrid(int size)
    {
        Grid = new(size, Allocator.Persistent);
        Rooms = new(0, Allocator.Persistent);
        for (int i = 0; i < size; i++) Grid.Add(0);
        FreeRectangles = new(0, Allocator.Persistent);
    }
}

[BurstCompile]
struct GenerateChunkPathJob : IJob
{
    public MapInfo MAP_INFO;
    public NativeArray<Chunk> MapChunks;
    public Random RNG;

    Chunk.Type GetChunkType(int x, int y)
    {
        if (x < 0 || x >= MAP_INFO.MAP_SIZE.x || y < 0 || y >= MAP_INFO.MAP_SIZE.y) return Chunk.Type.Empty;
        return MapChunks[x + y * MAP_INFO.MAP_SIZE.y].ChunkType;
    }
    void SetBiomeChunk(int x, int y, Biome biome)
    {
        if (x < 0 || x >= MAP_INFO.MAP_SIZE.x || y < 0 || y >= MAP_INFO.MAP_SIZE.y) return;
        Chunk chunk = MapChunks[x + y * MAP_INFO.MAP_SIZE.y];
        chunk.Biome = biome;
        chunk.Coordinate = new(x, y);
        MapChunks[x + y * MAP_INFO.MAP_SIZE.y] = chunk;
    }
    void SetChunk(int x, int y, Chunk.Type type)
    {
        if (x < 0 || x >= MAP_INFO.MAP_SIZE.x || y < 0 || y >= MAP_INFO.MAP_SIZE.y) return;
        Chunk chunk = MapChunks[x + y * MAP_INFO.MAP_SIZE.y];
        chunk.ChunkType = type;
        MapChunks[x + y * MAP_INFO.MAP_SIZE.y] = chunk;
    }
    void SetPathChunk(int x, int y, Chunk.Type type, int next_x, int next_y)
    {
        if (x < 0 || x >= MAP_INFO.MAP_SIZE.x || y < 0 || y >= MAP_INFO.MAP_SIZE.y) return;
        Chunk chunk = MapChunks[x + y * MAP_INFO.MAP_SIZE.y];
        chunk.ChunkType = type;
        chunk.NextChunkInPath = new(next_x, next_y);
        MapChunks[x + y * MAP_INFO.MAP_SIZE.y] = chunk;
        Debug.Log(type + " " + chunk.Coordinate + " next is " + chunk.NextChunkInPath);
    }
    public void Execute()
    {
        // 1. assign chunk biomes
        for (int x = 0; x < MAP_INFO.MAP_SIZE.x; x++)
        {
            for (int y = 0; y < MAP_INFO.MAP_SIZE.y; y++)
            {
                if (x >= MAP_INFO.CAVE_BIOME_BOUNDS.x &&
                    x <= MAP_INFO.CAVE_BIOME_BOUNDS.y &&
                    y >= MAP_INFO.CAVE_BIOME_BOUNDS.x &&
                    y <= MAP_INFO.CAVE_BIOME_BOUNDS.y)
                {
                    SetBiomeChunk(x, y, Biome.CAVE); 
                }
                else if (x < MAP_INFO.MAP_SIZE.x/2)
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
        Coord StartingChunk1 = new(MAP_INFO.CAVE_BIOME_BOUNDS.x, RNG.NextInt(MAP_INFO.CAVE_BIOME_BOUNDS.x, MAP_INFO.CAVE_BIOME_BOUNDS.y + 1));
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
        SetPathChunk(StartingChunk1.x, StartingChunk1.y, Chunk.Type.Starting, AlphaPath[AlphaPath.Length - 1].x, AlphaPath[AlphaPath.Length - 1].y);
        for (int i = AlphaPath.Length-1; i>=0; i--)
        {
            Coord coord = AlphaPath[i];
            Coord next_cord;
            if (i == 0) next_cord = BossChunk1;
            else next_cord = AlphaPath[i - 1];
            SetPathChunk(coord.x, coord.y, Chunk.Type.AlphaPath, next_cord.x, next_cord.y);
        }

        // 5. A* beta path from starting cave to boss
        var BetaPath = FindAStarPath(StartingChunk1, BossChunk1);
        for (int i = BetaPath.Length-1; i >= 0; i--)
        {
            Coord coord = BetaPath[i];
            Coord next_cord;
            if (i == 0) next_cord = BossChunk1;
            else next_cord = BetaPath[i - 1];
            SetPathChunk(coord.x, coord.y, Chunk.Type.BetaPath, next_cord.x, next_cord.y);
        }

        SetChunk(BossChunk1.x, BossChunk1.y, Chunk.Type.Boss);



        // repeat 2-5 for other side
        Coord StartingChunk2 = new(MAP_INFO.CAVE_BIOME_BOUNDS.y, RNG.NextInt(MAP_INFO.CAVE_BIOME_BOUNDS.x, MAP_INFO.CAVE_BIOME_BOUNDS.y + 1));
        SetChunk(StartingChunk2.x, StartingChunk2.y, Chunk.Type.Starting);

        UnsafeList<Coord> PotentialBossSpots2 = new(0, Allocator.Persistent)
        {
            new Coord(3,0),new Coord(4,0),new Coord(5,0),new Coord(5,1),new Coord(5,2),new Coord(5,3),new Coord(5,4),new Coord(5,5),new Coord(4,5),new Coord(3,5)
        };
        if (StartingChunk2.y == 2) PotentialBossSpots2.Add(new Coord(4, 4));
        else PotentialBossSpots2.Add(new Coord(4, 1));

        Coord BossChunk2 = PotentialBossSpots2[RNG.NextInt(0, PotentialBossSpots2.Length)];

        AlphaPath = FindAStarPath(StartingChunk2, BossChunk2);
        SetPathChunk(StartingChunk2.x, StartingChunk2.y, Chunk.Type.Starting, AlphaPath[AlphaPath.Length - 1].x, AlphaPath[AlphaPath.Length - 1].y);
        for (int i = AlphaPath.Length-1; i >= 0; i--)
        {
            Coord coord = AlphaPath[i];
            Coord next_cord;
            if (i == 0) next_cord = BossChunk2;
            else next_cord = AlphaPath[i - 1];
            SetPathChunk(coord.x, coord.y, Chunk.Type.AlphaPath, next_cord.x, next_cord.y);
        }

        BetaPath = FindAStarPath(StartingChunk2, BossChunk2);
        for (int i = BetaPath.Length-1; i >= 0; i--)
        {
            Coord coord = BetaPath[i];
            Coord next_cord;
            if (i == 0) next_cord = BossChunk2;
            else next_cord = BetaPath[i - 1];
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
struct PlaceInitialRoomsJob : IJob
{
    public NativeArray<Chunk> MapChunks;
    public RoomDatabase RoomDatabase;
    public MapInfo MAP_INFO;
    public Random RNG;

    Chunk currChunk;
    public void Execute()
    {
        for (int index = 0; index < MapChunks.Length; index++)
        {
            currChunk = MapChunks[index];

            if (currChunk.ChunkType == Chunk.Type.Empty) continue;

            currChunk.InitGrid(MAP_INFO.CHUNK_SIZE.x * MAP_INFO.CHUNK_SIZE.y);

            if (currChunk.ChunkType == Chunk.Type.Starting) PlaceHub();
            if (currChunk.ChunkType == Chunk.Type.Starting || currChunk.ChunkType == Chunk.Type.AlphaPath) PlaceCampfireRoom();
            if (currChunk.ChunkType == Chunk.Type.Starting || currChunk.ChunkType == Chunk.Type.AlphaPath
                || currChunk.ChunkType == Chunk.Type.BetaPath) PlaceIntermediateRooms();

            MapChunks[index] = currChunk;

            if (currChunk.FreeRectangles.Length > 0)
            {
                foreach (var rect in currChunk.FreeRectangles)
                {
                    Debug.Log(currChunk.Coordinate.x + ", " + currChunk.Coordinate.y + " has (" + rect.Coord.x + "," + rect.Coord.y + ") size = " + rect.Size.x + "," + rect.Size.y);
                }
            }
        }
    }

    bool TryClaim(int start_x, int start_y, int end_x, int end_y, int value, int padding)
    {
        if (start_x < 0 || start_y < 0 || end_x > MAP_INFO.CHUNK_SIZE.x || end_y > MAP_INFO.CHUNK_SIZE.y) return false;

        for (int y = start_y; y < end_y; y++)
        {
            for (int x = start_x; x < end_y; x++)
            {
                if (currChunk.Grid[y * MAP_INFO.MAP_SIZE.y + x] != 0) return false;
            }
        }

        for (int y = start_y; y < end_y; y++)
        {
            for (int x = start_x; x < end_y; x++)
            {
                // if within padding region, set to 1, else set to room's UUID
                currChunk.Grid[y * MAP_INFO.MAP_SIZE.y + x] = x < start_x + padding || y < start_y + padding ||
                    x >= end_x - padding || y >= end_y - padding ? 1 : value;
            }
        }
        return true;
    }

    void DivideFreeRectangle(FreeRectangle rect, int bottom_x, int bottom_y, int size_x, int size_y)
    {
        if (bottom_y + size_y < rect.Coord.y + rect.Size.y)
        {
            currChunk.FreeRectangles.Add(new()
            {
                Coord = new(bottom_x, bottom_y + size_y),
                Size = new(rect.Size.x - (bottom_x - rect.Coord.x), rect.Size.y + rect.Coord.y - (size_y + bottom_y)),
                Recursions = rect.Recursions--
            });
        }
        if (bottom_x + size_x < rect.Coord.x + rect.Size.x)
        {
            currChunk.FreeRectangles.Add(new()
            {
                Coord = new(bottom_x + size_x, rect.Coord.y),
                Size = new(rect.Size.x + rect.Coord.x - (bottom_x + size_x), bottom_y - rect.Coord.y + size_y),
                Recursions = rect.Recursions--
            });
        }
        if (bottom_y > rect.Coord.y)
        {
            currChunk.FreeRectangles.Add(new()
            {
                Coord = rect.Coord,
                Size = new(bottom_x - rect.Coord.x + size_x, bottom_y - rect.Coord.y),
                Recursions = rect.Recursions--
            });
        }
        if (bottom_x > rect.Coord.x)
        {
            currChunk.FreeRectangles.Add(new()
            {
                Coord = new(rect.Coord.x, bottom_y),
                Size = new(bottom_x - rect.Coord.x, rect.Coord.y + rect.Size.y - (bottom_y - rect.Coord.y)),
                Recursions = rect.Recursions--
            });
        }
    }

    void PlaceHub()
    {
        // claim the hub's space
        var hubGenerationInfo = RoomDatabase.GetRoom(RoomType.START, Biome.CAVE);
        int hubRadius = hubGenerationInfo.TotalGridSpace.x / 2;

        int startX = currChunk.Coordinate.x == MAP_INFO.CAVE_BIOME_BOUNDS.x ? (MAP_INFO.CHUNK_SIZE.x - hubRadius) : 0;
        int startY = currChunk.Coordinate.y == MAP_INFO.CAVE_BIOME_BOUNDS.x ? (MAP_INFO.CHUNK_SIZE.y - hubRadius) : 0;

        //for (int i = 0; i < 4; i++)
        //{
        //    currChunk.FreeRectangles.Add(new()
        //    {
        //        Coord = new(i % 2 * MAP_INFO.CHUNK_SIZE.x / 2, Mathf.FloorToInt(i / 2) * MAP_INFO.CHUNK_SIZE.x / 2),
        //        Size = new(MAP_INFO.CHUNK_SIZE.x / 2, MAP_INFO.CHUNK_SIZE.y / 2),
        //        Recursions = 2
        //    });
        //}

        TryClaim(startX, startY, startX + hubRadius, startY + hubRadius, hubGenerationInfo.UUID, 0);
        //hubGenerationInfo.RoomSpawn = new(startX, startY);
        //currChunk.Rooms.Add(hubGenerationInfo);
        //UnsafeList<FreeRectangle> OldRectangles = currChunk.FreeRectangles;
        //for (int i = 0; i < OldRectangles.Length; i++)
        //{
        //    if (OldRectangles[i].IsIn(startX, startY, hubRadius, hubRadius))
        //    {
        //        FreeRectangle rect = OldRectangles[i];
        //        currChunk.FreeRectangles.RemoveAt(i);
        //        DivideFreeRectangle(rect, startX, startY, hubRadius, hubRadius);
        //    }
        //}

        FreeRectangle rect = new()
        {
            Coord = new(0, 0),
            Size = new(MAP_INFO.CHUNK_SIZE.x, MAP_INFO.CHUNK_SIZE.y),
            Recursions = 3
        };
        DivideFreeRectangle(rect, startX, startY, hubRadius, hubRadius);

        //foreach (var rect in FreeRectangles)
        //{
        //    Debug.Log(startX + ", " + startY + " in (" + rect.Coord.x + "," + rect.Coord.y + ") size = " + rect.Size.x + "," + rect.Size.y);
        //}

        //Coord maxSize = new(MAP_INFO.CHUNK_SIZE.x/2 - hubRadius - 1, MAP_INFO.CHUNK_SIZE.y/2 - hubRadius - 1);
        //GenerationInfo intermediate = RoomDatabase.GetRoom(RoomType.INTERMEDIATE, Biome.CAVE, maxSize);

        // look into rectangle packing problem -- prob continously update a list of "free rectangles" and whenever you
        // place a room within the rectangle randomly, divide the remainder of the rectangle into 4 new free rectangles around its perimeter
        //
        // each room should have a padding around it to guarantee space for connectors (padding could be represented as simply 1)
    }

    void PlaceCampfireRoom()
    {
        // for rectangle where campfire can go
        int startX = 0, startY = 0, sizeX = 0, sizeY = 0;

        if (currChunk.NextChunkInPath.x < currChunk.Coordinate.x)
        {
            sizeX = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.y; sizeY = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.x;
            startX = 0;
            startY = MAP_INFO.CHUNK_SIZE.y / 2 - sizeY / 2;
        }
        else if (currChunk.NextChunkInPath.x > currChunk.Coordinate.x)
        {
            sizeX = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.y; sizeY = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.x;
            startX = MAP_INFO.CHUNK_SIZE.x - sizeX;
            startY = MAP_INFO.CHUNK_SIZE.y / 2 - sizeY / 2;
        }
        else if (currChunk.NextChunkInPath.y < currChunk.Coordinate.y)
        {
            sizeX = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.x; sizeY = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.y;
            startX = MAP_INFO.CHUNK_SIZE.x / 2 - sizeX / 2;
            startY = 0;
        }
        else if (currChunk.NextChunkInPath.y > currChunk.Coordinate.y )
        {
            sizeX = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.x; sizeY = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.y;
            startX = MAP_INFO.CHUNK_SIZE.x / 2 - sizeX / 2;
            startY = MAP_INFO.CHUNK_SIZE.y - sizeY;
        }

        GenerationInfo campfire = RoomDatabase.GetRoom(RoomType.CAMPFIRE, Biome.CAVE, new(sizeX,sizeY));

        int camp_x = RNG.NextInt(startX, startX + sizeX - campfire.TotalGridSpace.x);
        int camp_y = RNG.NextInt(startY, startY + sizeY - campfire.TotalGridSpace.y);

        TryClaim(camp_x, camp_y, camp_x + campfire.TotalGridSpace.x, camp_y + campfire.TotalGridSpace.y, campfire.UUID, campfire.GridPadding);
        campfire.RoomSpawn = new(camp_x+campfire.GridPadding, camp_y+campfire.GridPadding);
        currChunk.Rooms.Add(campfire);

        if (currChunk.FreeRectangles.Length > 0) // if in start chunk where hub room is already added
        {
            UnsafeList<FreeRectangle> OldRectangles = currChunk.FreeRectangles;
            for (int i = 0; i < OldRectangles.Length; i++)
            {
                if (OldRectangles[i].IsIn(camp_x, camp_y, campfire.TotalGridSpace.x, campfire.TotalGridSpace.y))
                {
                    FreeRectangle rect = OldRectangles[i];
                    currChunk.FreeRectangles.RemoveAt(i);
                    DivideFreeRectangle(rect, camp_x, camp_y, campfire.TotalGridSpace.x, campfire.TotalGridSpace.y);
                    break;
                }
            }
        }
        else
        {
            FreeRectangle rect = new()
            {
                Coord = new(0, 0),
                Size = new(MAP_INFO.CHUNK_SIZE.x, MAP_INFO.CHUNK_SIZE.y),
                Recursions = 3
            };
            DivideFreeRectangle(rect, camp_x, camp_y, campfire.TotalGridSpace.x, campfire.TotalGridSpace.y);
        }

            
    }

    void PlaceIntermediateRooms()
    {

    }
}