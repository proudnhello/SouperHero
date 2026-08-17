using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using Utils;
using static MapRoom;
using static PlaceInitialRoomsJob;
using static UnityEditor.PlayerSettings;
using static UnityEngine.InputManagerEntry;
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
    public Coord(Vector2Int coord)
    {
        this.x = coord.x;
        this.y = coord.y;
    }
    public Coord(Vector2 coord)
    {
        this.x = Mathf.FloorToInt(coord.x);
        this.y = Mathf.FloorToInt(coord.y);
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
    public static Coord operator +(Coord left, Vector2Int right) => new Coord(left.x + right.x, left.y + right.y);
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
    public override string ToString()
    {
        return $"Rect: = ({Coord.x},{Coord.y}) to ({Coord.x+Size.x},{Coord.y+Size.y})";
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
    public Coord NextChunkInAlphaPath;
    public Coord NextChunkInBetaPath;
    public bool HasBetaConnection;
    public Coord Coordinate;

    // connector placement stuff
    public struct DoorSpot
    {
        public Coord coord;
        public Door.Direction dir;
        public DoorSpot(Coord coord_, Door.Direction dir_) { coord = coord_ ; dir = dir_; }

        public static implicit operator DoorSpot((Coord coord, Door.Direction dir) value)
        {
            DoorSpot ret = new DoorSpot();
            ret.coord = value.coord;
            ret.dir = value.dir;
            return ret;
        }
    }
    public UnsafeList<DoorSpot> Doors; // for every room in Rooms, its doors will be in the same order in Doors
    public UnsafeList<int> DoorStates; // 1 = on, 0 = taken, -1 = extrema
    public UnsafeList<int> DoorRoomIDs; // doors of same room will have same id
    public int DoorRoomIDTracker;
    public int ValidDoorsLeft;

    public DoorSpot HubDoorSpot;
    public int HubDoorID;
    public DoorSpot CampfireDoorSpot;
    public int CampfireDoorID;
    public bool hasPreviousChunkPath1;
    public DoorSpot PreviousChunkPath1;
    public bool hasPreviousChunkPath2;
    public DoorSpot PreviousChunkPath2;

    public DoorSpot BossDoorSpot;

    public void InitGrid(int size)
    {
        Grid = new(size, Allocator.Persistent);
        Rooms = new(0, Allocator.Persistent);
        for (int i = 0; i < size; i++) Grid.Add(0);
        FreeRectangles = new(0, Allocator.Persistent);
        Doors = new(0, Allocator.Persistent);
        DoorStates = new(0, Allocator.Persistent);
        DoorRoomIDs = new(0, Allocator.Persistent);
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
    void SetStartingPathChunk(int x, int y, Chunk.Type type, int next_x, int next_y, int next_x2, int next_y2)
    {
        if (x < 0 || x >= MAP_INFO.MAP_SIZE.x || y < 0 || y >= MAP_INFO.MAP_SIZE.y) return;
        Chunk chunk = MapChunks[x + y * MAP_INFO.MAP_SIZE.y];
        chunk.ChunkType = type;
        chunk.NextChunkInAlphaPath = new(next_x, next_y);
        chunk.NextChunkInBetaPath = new(next_x2, next_y2);
        chunk.HasBetaConnection = true;
        MapChunks[x + y * MAP_INFO.MAP_SIZE.y] = chunk;
    }

    void SetPathChunk(int x, int y, Chunk.Type type, int next_x, int next_y)
    {
        if (x < 0 || x >= MAP_INFO.MAP_SIZE.x || y < 0 || y >= MAP_INFO.MAP_SIZE.y) return;
        Chunk chunk = MapChunks[x + y * MAP_INFO.MAP_SIZE.y];
        chunk.ChunkType = type;
        chunk.NextChunkInAlphaPath = new(next_x, next_y);
        MapChunks[x + y * MAP_INFO.MAP_SIZE.y] = chunk;
        //Debug.Log(type + " " + chunk.Coordinate + " next is " + chunk.NextChunkInPath);
    }

    void SetAlphaChunkBetaConnection(int x, int y, int next_x, int next_y)
    {
        Chunk chunk = MapChunks[x + y * MAP_INFO.MAP_SIZE.y];
        chunk.NextChunkInBetaPath = new(next_x, next_y);
        chunk.HasBetaConnection = true;
        MapChunks[x + y * MAP_INFO.MAP_SIZE.y] = chunk;
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
                    //SetBiomeChunk(x, y, Biome.FOREST);
                    SetBiomeChunk(x, y, Biome.CAVE);
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

        SetStartingPathChunk(StartingChunk1.x, StartingChunk1.y, Chunk.Type.Starting, 
            AlphaPath[AlphaPath.Length - 1].x, AlphaPath[AlphaPath.Length - 1].y,
            BetaPath[BetaPath.Length - 1].x, BetaPath[BetaPath.Length - 1].y);
        SetPathChunk(BossChunk1.x, BossChunk1.y, Chunk.Type.Boss, AlphaPath[0].x, AlphaPath[0].y); // set to hold previous chunk

        for (int i = AlphaPath.Length - 1; i >= 0; i--)
        {
            Coord coord = AlphaPath[i];
            if (GetChunkType(coord.x - 1, coord.y) == Chunk.Type.BetaPath && BetaPath.Contains(new Coord(coord.x - 1, coord.y))) 
                SetAlphaChunkBetaConnection(coord.x, coord.y, coord.x - 1, coord.y);
            if (GetChunkType(coord.x, coord.y - 1) == Chunk.Type.BetaPath && BetaPath.Contains(new Coord(coord.x, coord.y - 1)))
                SetAlphaChunkBetaConnection(coord.x, coord.y, coord.x, coord.y - 1);
            if (GetChunkType(coord.x + 1, coord.y) == Chunk.Type.BetaPath && BetaPath.Contains(new Coord(coord.x + 1, coord.y)))
                SetAlphaChunkBetaConnection(coord.x, coord.y, coord.x + 1, coord.y);
            if (GetChunkType(coord.x, coord.y + 1) == Chunk.Type.BetaPath && BetaPath.Contains(new Coord(coord.x, coord.y + 1)))
                SetAlphaChunkBetaConnection(coord.x, coord.y, coord.x, coord.y + 1);
        }

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
        //SetPathChunk(StartingChunk2.x, StartingChunk2.y, Chunk.Type.Starting, AlphaPath[AlphaPath.Length - 1].x, AlphaPath[AlphaPath.Length - 1].y);
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

        SetStartingPathChunk(StartingChunk2.x, StartingChunk2.y, Chunk.Type.Starting,
            AlphaPath[AlphaPath.Length - 1].x, AlphaPath[AlphaPath.Length - 1].y,
            BetaPath[BetaPath.Length - 1].x, BetaPath[BetaPath.Length - 1].y);
        SetPathChunk(BossChunk2.x, BossChunk2.y, Chunk.Type.Boss, AlphaPath[0].x, AlphaPath[0].y); // set to hold previous chunk

        for (int i = AlphaPath.Length - 1; i >= 0; i--)
        {
            Coord coord = AlphaPath[i];
            if (GetChunkType(coord.x - 1, coord.y) == Chunk.Type.BetaPath && BetaPath.Contains(new Coord(coord.x - 1, coord.y)))
                SetAlphaChunkBetaConnection(coord.x, coord.y, coord.x - 1, coord.y);
            if (GetChunkType(coord.x, coord.y - 1) == Chunk.Type.BetaPath && BetaPath.Contains(new Coord(coord.x, coord.y - 1)))
                SetAlphaChunkBetaConnection(coord.x, coord.y, coord.x, coord.y - 1);
            if (GetChunkType(coord.x + 1, coord.y) == Chunk.Type.BetaPath && BetaPath.Contains(new Coord(coord.x + 1, coord.y)))
                SetAlphaChunkBetaConnection(coord.x, coord.y, coord.x + 1, coord.y);
            if (GetChunkType(coord.x, coord.y + 1) == Chunk.Type.BetaPath && BetaPath.Contains(new Coord(coord.x, coord.y + 1)))
                SetAlphaChunkBetaConnection(coord.x, coord.y, coord.x, coord.y + 1);
        }
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

            if (currChunk.Coordinate.x >= 2 && currChunk.Coordinate.x <= 3 && currChunk.Coordinate.y >= 2 && currChunk.Coordinate.y <= 3) PlaceHub();
            if (currChunk.ChunkType == Chunk.Type.Starting || currChunk.ChunkType == Chunk.Type.AlphaPath
                || currChunk.ChunkType == Chunk.Type.BetaPath) PlaceCampfireRoom();
            //if (currChunk.ChunkType == Chunk.Type.BetaPath) InitBetaPath();
            if (currChunk.ChunkType == Chunk.Type.Starting || currChunk.ChunkType == Chunk.Type.AlphaPath
                || currChunk.ChunkType == Chunk.Type.BetaPath) PlaceIntermediateRooms();
            if (currChunk.ChunkType == Chunk.Type.Boss) PlaceBossRoom();

            MapChunks[index] = currChunk;
        }
    }

    bool TryClaim(int start_x, int start_y, int end_x, int end_y, int value, int padding)
    {
        if (start_x < 0 || start_y < 0 || end_x > MAP_INFO.CHUNK_SIZE.x || end_y > MAP_INFO.CHUNK_SIZE.y) return false;

        for (int y = start_y; y < end_y; y++)
        {
            for (int x = start_x; x < end_x; x++)
            {
                if (currChunk.Grid[y * MAP_INFO.CHUNK_SIZE.y + x] > 1) return false;
            }
        }
        //if (currChunk.ChunkType == Chunk.Type.Starting)
        //{
        //    string s = "BEFORE";
        //    for (int i = 0; i < currChunk.Grid.Length; i++)
        //    {
        //        if (i % MAP_INFO.CHUNK_SIZE.x == 0) s += "\n";
        //        s += currChunk.Grid[i] + "\t";
        //    }
        //    Debug.Log(s);
        //}
        for (int y = start_y; y < end_y; y++)
        {
            for (int x = start_x; x < end_x; x++)
            {
                // if within padding region, set to 1, else set to room's UUID
                currChunk.Grid[y * MAP_INFO.CHUNK_SIZE.y + x] = x < start_x + padding || y < start_y + padding ||
                    x >= end_x - padding || y >= end_y - padding ? 1 : value;
            }
        }
        //if (currChunk.ChunkType == Chunk.Type.Starting)
        //{
        //    string s = "AFTER\n";
        //    for (int i = 0; i < currChunk.Grid.Length; i++)
        //    {
        //        if (i % MAP_INFO.CHUNK_SIZE.x == 0) s += "\n";
        //        s += currChunk.Grid[i] + "\t";
        //    }
        //    Debug.Log(s);
        //}

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
                Recursions = rect.Recursions-1
            });
        }
        if (bottom_x + size_x < rect.Coord.x + rect.Size.x)
        {
            currChunk.FreeRectangles.Add(new()
            {
                Coord = new(bottom_x + size_x, rect.Coord.y),
                Size = new(rect.Size.x + rect.Coord.x - (bottom_x + size_x), bottom_y - rect.Coord.y + size_y),
                Recursions = rect.Recursions-1
            });
        }
        if (bottom_y > rect.Coord.y)
        {
            currChunk.FreeRectangles.Add(new()
            {
                Coord = rect.Coord,
                Size = new(bottom_x - rect.Coord.x + size_x, bottom_y - rect.Coord.y),
                Recursions = rect.Recursions-1
            });
        }
        if (bottom_x > rect.Coord.x)
        {
            currChunk.FreeRectangles.Add(new()
            {
                Coord = new(rect.Coord.x, bottom_y),
                Size = new(bottom_x - rect.Coord.x, rect.Size.y - (bottom_y - rect.Coord.y)),
                Recursions = rect.Recursions-1
            });
        }
    }

    void PlaceHub()
    {
        // claim the hub's space
        var hubGenerationInfo = RoomDatabase.GetRoom(RoomType.START, Biome.CAVE);
        int hubRadius = hubGenerationInfo.TotalGridSpace.x / 2;

        bool left = currChunk.Coordinate.x == MAP_INFO.CAVE_BIOME_BOUNDS.x;
        bool bottom = currChunk.Coordinate.y == MAP_INFO.CAVE_BIOME_BOUNDS.x;

        int startX = left ? (MAP_INFO.CHUNK_SIZE.x - hubRadius) : 0;
        int startY = bottom ? (MAP_INFO.CHUNK_SIZE.y - hubRadius) : 0;

        TryClaim(startX, startY, startX + hubRadius, startY + hubRadius, 1, 0);

        hubRadius = hubGenerationInfo.GridDimensions.x / 2;
        startX = left ? (MAP_INFO.CHUNK_SIZE.x - hubRadius) : 0;
        startY = bottom ? (MAP_INFO.CHUNK_SIZE.y - hubRadius) : 0;

        TryClaim(startX, startY, startX + hubRadius, startY + hubRadius, hubGenerationInfo.UUID, 0);
        // sadly, claiming door pos gotta be manual oops. order is = (bottom-left, bottom-right, top-left, top-right)

        FreeRectangle rect = new()
        {
            Coord = new(0, 0),
            Size = new(MAP_INFO.CHUNK_SIZE.x, MAP_INFO.CHUNK_SIZE.y),
            Recursions = 4
        };
        DivideFreeRectangle(rect, startX, startY, hubRadius, hubRadius);

        if (currChunk.ChunkType != Chunk.Type.Starting) return;

        int doorIndex = left && bottom ?  0:
                               !left && bottom ? 1 :
                               left && !bottom ? 2 :
                               3;
        currChunk.HubDoorSpot = hubGenerationInfo.Doors[doorIndex];



        currChunk.HubDoorSpot.coord = left && bottom ? new Coord(startX + hubGenerationInfo.Doors[0].coord.x, startY + hubGenerationInfo.Doors[0].coord.y) :
                                      !left && bottom ? new Coord(-hubRadius + hubGenerationInfo.Doors[1].coord.x, startY + hubGenerationInfo.Doors[1].coord.y) :
                                      left && !bottom ? new Coord(startX + hubGenerationInfo.Doors[2].coord.x, -hubRadius + hubGenerationInfo.Doors[2].coord.y) :
                                       new Coord(-hubRadius + hubGenerationInfo.Doors[3].coord.x, -hubRadius + hubGenerationInfo.Doors[3].coord.y);
        currChunk.Rooms.Add(hubGenerationInfo);
        for (int i = 0; i < hubGenerationInfo.Doors.Length; i++)
        {
            currChunk.Doors.Add(default);
            currChunk.DoorRoomIDs.Add(currChunk.DoorRoomIDTracker);
            if (i == doorIndex) currChunk.DoorStates.Add(0);
            else currChunk.DoorStates.Add(-2);
        }
        currChunk.HubDoorID = currChunk.DoorRoomIDTracker;
        currChunk.DoorRoomIDTracker++;

    }

    void PlaceCampfireRoom()
    {
        FreeRectangle bounds = new()
        {
            Coord = new(0, 0),
            Size = new(MAP_INFO.CHUNK_SIZE.x, MAP_INFO.CHUNK_SIZE.y),
            Recursions = 4
        };

        if (currChunk.FreeRectangles.Length > 0) // if in start chunk where hub room is already added
        {
            UnsafeList<FreeRectangle> OldRectangles = new(0, Allocator.Persistent);
            foreach (var rect in currChunk.FreeRectangles) OldRectangles.Add(rect);
            currChunk.FreeRectangles.Clear();
            for (int i = 0; i < OldRectangles.Length; i++)
            {
                FreeRectangle rect = OldRectangles[i];
                if (rect.Size.x > 2 && rect.Size.y > 2) // take bigger of 2 hub rects
                {
                    bounds = rect;
                }
                else currChunk.FreeRectangles.Add(rect); // add other to existing recs
            }
        }

        // for rectangle where campfire can go
        int startX = 0, startY = 0, sizeX = 0, sizeY = 0;
        int exitDoor = 0; // N = 0, S = 1, E = 2, W = 3

        if (currChunk.NextChunkInAlphaPath.x < currChunk.Coordinate.x)
        {
            sizeX = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.y; sizeY = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.x;
            startX = 0;
            startY = MAP_INFO.CHUNK_SIZE.y / 2 - sizeY / 2;
            exitDoor = 3;
        }
        else if (currChunk.NextChunkInAlphaPath.x > currChunk.Coordinate.x)
        {
            sizeX = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.y; sizeY = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.x;
            startX = MAP_INFO.CHUNK_SIZE.x - sizeX;
            startY = MAP_INFO.CHUNK_SIZE.y / 2 - sizeY / 2;
            exitDoor = 2;
        }
        else if (currChunk.NextChunkInAlphaPath.y < currChunk.Coordinate.y)
        {
            sizeX = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.x; sizeY = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.y;
            startX = MAP_INFO.CHUNK_SIZE.x / 2 - sizeX / 2;
            startY = 0;
            exitDoor = 1;
        }
        else if (currChunk.NextChunkInAlphaPath.y > currChunk.Coordinate.y )
        {
            sizeX = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.x; sizeY = MAP_INFO.CAMPFIRE_PLACEMENT_AREA_SIZE.y;
            startX = MAP_INFO.CHUNK_SIZE.x / 2 - sizeX / 2;
            startY = MAP_INFO.CHUNK_SIZE.y - sizeY;
            exitDoor = 0;
        }

        startX = Mathf.Max(startX, bounds.Coord.x);
        startY = Mathf.Max(startY, bounds.Coord.y);
        sizeX = Mathf.Min(sizeX, bounds.Size.x);
        sizeY = Mathf.Min(sizeY, bounds.Size.y);

        GenerationInfo campfire = RoomDatabase.GetRoom(RoomType.CAMPFIRE, currChunk.Biome, new(sizeX,sizeY));

        int camp_x = RNG.NextInt(startX, startX + sizeX - campfire.TotalGridSpace.x);
        int camp_y = RNG.NextInt(startY, startY + sizeY - campfire.TotalGridSpace.y);

        TryClaim(camp_x, camp_y, camp_x + campfire.TotalGridSpace.x, camp_y + campfire.TotalGridSpace.y, campfire.UUID, campfire.GridPadding);
        campfire.RoomSpawn = new(camp_x+campfire.GridPadding, camp_y+campfire.GridPadding);
        currChunk.Rooms.Add(campfire);

        currChunk.CampfireDoorSpot = campfire.Doors[exitDoor];
        currChunk.CampfireDoorSpot.coord = new Coord(camp_x + campfire.GridPadding + currChunk.CampfireDoorSpot.coord.x, camp_y + campfire.GridPadding + currChunk.CampfireDoorSpot.coord.y);
        currChunk.CampfireDoorID = currChunk.DoorRoomIDTracker;

        for (int i = 0; i < campfire.Doors.Length; i++)
        {
            currChunk.Doors.Add(new(new Coord(camp_x + campfire.GridPadding + campfire.Doors[i].coord.x, camp_y + campfire.GridPadding + campfire.Doors[i].coord.y), campfire.Doors[i].dir));
            currChunk.DoorRoomIDs.Add(currChunk.DoorRoomIDTracker);

            if (i != exitDoor)
            {
                currChunk.DoorStates.Add(1);
            }
            else
            {
                currChunk.DoorStates.Add(0);
            }
        }
        currChunk.DoorRoomIDTracker++;

        DivideFreeRectangle(bounds, camp_x, camp_y, campfire.TotalGridSpace.x, campfire.TotalGridSpace.y);

        //if (currChunk.Coordinate.x == 3 && currChunk.Coordinate.y == 2)
        //{
        //    foreach (var rect in currChunk.FreeRectangles) Debug.Log(rect.Coord + ": " + rect.Size);
        //}
    }

    void InitBetaPath()
    {
        //for (int i = 0; i < 4; i++)
        //{
        //    currChunk.FreeRectangles.Add(new()
        //    {
        //        Coord = new(i % 2 * MAP_INFO.CHUNK_SIZE.x / 2, Mathf.FloorToInt(i / 2) * MAP_INFO.CHUNK_SIZE.x / 2),
        //        Size = new(MAP_INFO.CHUNK_SIZE.x / 2, MAP_INFO.CHUNK_SIZE.y / 2),
        //        Recursions = 2
        //    });
        //}
    }

    void PlaceIntermediateRooms()
    {
        bool MoreRecursions;
        do
        {
            MoreRecursions = false;
            UnsafeList<FreeRectangle> OldRectangles = new(0, Allocator.Persistent);
            foreach (var rect in currChunk.FreeRectangles) OldRectangles.Add(rect);
            currChunk.FreeRectangles.Clear();
            for (int i = 0; i < OldRectangles.Length; i++)
            {
                FreeRectangle rect = OldRectangles[i];
                //Debug.Log($"{i} in {currChunk.Coordinate.x}, {currChunk.Coordinate.y} inspecting {rect}, recurs={rect.Recursions}");
                if (rect.Recursions > 0)
                {
                    var intermediate = RoomDatabase.GetRoom(RoomType.INTERMEDIATE, currChunk.Biome, rect.Size);
                    if (intermediate.UUID == -1) // none small enough that fit
                    {
                        rect.Recursions = 0;
                        currChunk.FreeRectangles.Add(rect);
                    } else
                    {
                        int int_x = RNG.NextInt(rect.Coord.x, rect.Coord.x + rect.Size.x - intermediate.TotalGridSpace.x);
                        int int_y = RNG.NextInt(rect.Coord.y, rect.Coord.y + rect.Size.y - intermediate.TotalGridSpace.y);

                        TryClaim(int_x, int_y, int_x + intermediate.TotalGridSpace.x, int_y + intermediate.TotalGridSpace.y, intermediate.UUID, intermediate.GridPadding);
                        intermediate.RoomSpawn = new(int_x + intermediate.GridPadding, int_y + intermediate.GridPadding);
                        currChunk.Rooms.Add(intermediate);

                        for (int d = 0; d < intermediate.Doors.Length; d++)
                        {
                            currChunk.Doors.Add(new(new Coord(intermediate.RoomSpawn.x + intermediate.Doors[d].coord.x, intermediate.RoomSpawn.y + intermediate.Doors[d].coord.y), intermediate.Doors[d].dir));
                            currChunk.DoorStates.Add(1);
                            currChunk.DoorRoomIDs.Add(currChunk.DoorRoomIDTracker);
                        }
                        currChunk.DoorRoomIDTracker++;

                        DivideFreeRectangle(rect, int_x, int_y, intermediate.TotalGridSpace.x, intermediate.TotalGridSpace.y);

                        MoreRecursions = rect.Recursions != 1;
                    }
                }
                else
                {
                    currChunk.FreeRectangles.Add(rect);
                }
            }
        }
        while (MoreRecursions);
    }

    void PlaceBossRoom()
    {
        // for rectangle where boss can go (basically half of chunk)
        int bossDoor = 0; // N = 0, S = 1, E = 2, W = 3
        if (currChunk.NextChunkInAlphaPath.y > currChunk.Coordinate.y) bossDoor = 0;
        else if (currChunk.NextChunkInAlphaPath.y < currChunk.Coordinate.y) bossDoor = 1;
        else if (currChunk.NextChunkInAlphaPath.x > currChunk.Coordinate.x) bossDoor = 2;
        else if (currChunk.NextChunkInAlphaPath.x < currChunk.Coordinate.x) bossDoor = 3;


        int campStartX = currChunk.NextChunkInAlphaPath.x > currChunk.Coordinate.x ? (MAP_INFO.CHUNK_SIZE.x / 2) + 1 : 0;
        int campStartY = currChunk.NextChunkInAlphaPath.y > currChunk.Coordinate.y ? (MAP_INFO.CHUNK_SIZE.y / 2) + 1 : 0;
        int campSizeX = currChunk.NextChunkInAlphaPath.y != currChunk.Coordinate.y ? MAP_INFO.CHUNK_SIZE.x - 2 : (MAP_INFO.CHUNK_SIZE.x / 2)-1;
        int campSizeY = currChunk.NextChunkInAlphaPath.y != currChunk.Coordinate.y ? (MAP_INFO.CHUNK_SIZE.y / 2)-1 : MAP_INFO.CHUNK_SIZE.y - 2;

        int bossStartX = currChunk.NextChunkInAlphaPath.x < currChunk.Coordinate.x ? MAP_INFO.CHUNK_SIZE.x / 2 : 1;
        int bossStartY = currChunk.NextChunkInAlphaPath.y < currChunk.Coordinate.y ? MAP_INFO.CHUNK_SIZE.y / 2 : 1;
        int bossSizeX = currChunk.NextChunkInAlphaPath.y != currChunk.Coordinate.y ? MAP_INFO.CHUNK_SIZE.x : (MAP_INFO.CHUNK_SIZE.x / 2)-1;
        int bossSizeY = currChunk.NextChunkInAlphaPath.y != currChunk.Coordinate.y ? (MAP_INFO.CHUNK_SIZE.y / 2)-1 : MAP_INFO.CHUNK_SIZE.y;


        //Debug.Log($"{currChunk.NextChunkInPath} -> {currChunk.Coordinate}");
        //Debug.Log($"({startX1}, {startY1}), ({startX2}, {startY2}), {sizeX}, {sizeY}");
        GenerationInfo bossRoom = RoomDatabase.GetRoom(RoomType.BOSS, currChunk.Biome, new(bossSizeX, bossSizeY));

        int boss_x = RNG.NextInt(bossStartX, bossStartX + bossSizeX - bossRoom.TotalGridSpace.x);
        int boss_y = RNG.NextInt(bossStartY, bossStartY + bossSizeY - bossRoom.TotalGridSpace.y);

        TryClaim(boss_x, boss_y, boss_x + bossRoom.TotalGridSpace.x, boss_y + bossRoom.TotalGridSpace.y, bossRoom.UUID, bossRoom.GridPadding);
        bossRoom.RoomSpawn = new(boss_x + bossRoom.GridPadding, boss_y + bossRoom.GridPadding);
        currChunk.Rooms.Add(bossRoom);

        currChunk.BossDoorSpot = bossRoom.Doors[bossDoor];
        currChunk.BossDoorSpot.coord = new Coord(boss_x + bossRoom.GridPadding + currChunk.BossDoorSpot.coord.x, boss_y + bossRoom.GridPadding + currChunk.BossDoorSpot.coord.y);

        for (int i = 0; i < bossRoom.Doors.Length; i++)
        {
            currChunk.Doors.Add(default);
            currChunk.DoorRoomIDs.Add(currChunk.DoorRoomIDTracker);
            currChunk.DoorStates.Add(i == bossDoor ? 0 : -1);
        }
        currChunk.DoorRoomIDTracker++;

        GenerationInfo campfire = RoomDatabase.GetRoom(RoomType.CAMPFIRE, currChunk.Biome, new(campSizeX, campSizeY));

        int camp_x = RNG.NextInt(campStartX, campStartX + campSizeX - campfire.TotalGridSpace.x);
        int camp_y = RNG.NextInt(campStartY, campStartY + campSizeY - campfire.TotalGridSpace.y);

        TryClaim(camp_x, camp_y, camp_x + campfire.TotalGridSpace.x, camp_y + campfire.TotalGridSpace.y, campfire.UUID, campfire.GridPadding);
        campfire.RoomSpawn = new(camp_x + campfire.GridPadding, camp_y + campfire.GridPadding);
        currChunk.Rooms.Add(campfire);


        for (int i = 0; i < campfire.Doors.Length; i++)
        {
            currChunk.Doors.Add(new(new Coord(camp_x + campfire.GridPadding + campfire.Doors[i].coord.x, camp_y + campfire.GridPadding + campfire.Doors[i].coord.y), campfire.Doors[i].dir));
            currChunk.DoorRoomIDs.Add(currChunk.DoorRoomIDTracker);
            currChunk.DoorStates.Add(1);
        }
        currChunk.CampfireDoorID = currChunk.DoorRoomIDTracker;
        currChunk.DoorRoomIDTracker++;

        FreeRectangle campfireRect = new()
        {
            Coord = new(campStartX, campStartY),
            Size = new(campSizeX, campSizeY),
            Recursions = 2
        };
        DivideFreeRectangle(campfireRect, camp_x, camp_y, campfire.TotalGridSpace.x, campfire.TotalGridSpace.y);

        //PlaceIntermediateRooms(); // in remainder of chunk wow cool i can use this this is accidentally good programming
    }

    //void RemoveInvalidDoors()
    //{
    //    for (int d = 0; d < currChunk.Doors.Length; d++)
    //    {
    //        Chunk.DoorSpot door = currChunk.Doors[d];
    //        if (door.coord.x < 0 || door.coord.x >= MAP_INFO.CHUNK_SIZE.x || door.coord.y < 0 || door.coord.y >= MAP_INFO.CHUNK_SIZE.y) continue;

    //        var cell = currChunk.Grid[door.coord.x + door.coord.y * MAP_INFO.CHUNK_SIZE.y];
    //        if (cell >= 32)
    //        {
    //            Debug.Log("invalid door at " + currChunk.Coordinate + ": " + door.coord);
    //            currChunk.DoorStates[d] = -2;
    //        }
    //    }
    //}
}

[BurstCompile]
struct PlaceConnectorsJob : IJob
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

            if (currChunk.ChunkType != Chunk.Type.Boss) ExtendCampfirePath();
            if (currChunk.HasBetaConnection) ExtendToBetaPath();

            MapChunks[index] = currChunk;
        }

        for (int index = 0; index < MapChunks.Length; index++)
        {
            
            currChunk = MapChunks[index];
            if (currChunk.ChunkType == Chunk.Type.Empty) continue;

            if (currChunk.ChunkType == Chunk.Type.Starting) FillHub();
            else if (currChunk.ChunkType == Chunk.Type.AlphaPath || currChunk.ChunkType == Chunk.Type.BetaPath) FillIntermediate();
            else if (currChunk.ChunkType == Chunk.Type.Boss) FillBoss();
            // set 2 extrema as invalid
            int x_max = 0;
            int x_min = int.MaxValue;
            int y_max = 0;
            int y_min = int.MaxValue;
            for (int d = 0; d < currChunk.Doors.Length; d++)
            {
                if (currChunk.DoorStates[d] != 1) continue;
                x_max = Mathf.Max(x_max, currChunk.Doors[d].coord.x);
                x_min = Mathf.Min(x_min, currChunk.Doors[d].coord.x);
                y_max = Mathf.Max(y_max, currChunk.Doors[d].coord.y);
                y_min = Mathf.Min(y_min, currChunk.Doors[d].coord.y);
            }
            int hasRemovedCount = 0;
            for (int d = 0; d < currChunk.Doors.Length && hasRemovedCount < 2; d++)
            {
                if (currChunk.DoorStates[d] != 1) continue;

                if (currChunk.Doors[d].coord.x == x_max) { currChunk.DoorStates[d] = -1; hasRemovedCount++; }
                else if (currChunk.Doors[d].coord.x == x_min) { currChunk.DoorStates[d] = -1; hasRemovedCount++; }
                else if (currChunk.Doors[d].coord.y == y_max) { currChunk.DoorStates[d] = -1; hasRemovedCount++; }
                else if (currChunk.Doors[d].coord.y == y_min) { currChunk.DoorStates[d] = -1; hasRemovedCount++; }
            }

            if (currChunk.ChunkType != Chunk.Type.Boss) FillRemainingPath();

            // loop through grid and place appropriate connector rooms in currChunk.Rooms
            for (int i = 0; i < currChunk.Grid.Length; i++)
            {
                int cell = currChunk.Grid[i];
                // is < byte 00011111
                if (cell < 32 && cell > 1)
                {
                    GenerationInfo connector = RoomDatabase.GetConnector(currChunk.Biome, (byte)cell);
                    connector.RoomSpawn = new(i % MAP_INFO.CHUNK_SIZE.x, Mathf.FloorToInt(i / MAP_INFO.CHUNK_SIZE.x));
                    //Debug.Log($"CONNECTOR {currChunk.Coordinate.x}, {currChunk.Coordinate.y} of {connector.ConnectorType}, Place room " + connector.Type + " " + connector.UUID + " at " + connector.RoomSpawn);
                    currChunk.Rooms.Add(connector);
                }
            }

            MapChunks[index] = currChunk;

        }
    }

    void FillHub()
    {
        // connect from hub to closest intermediate
        (Chunk.DoorSpot ClosestDoorSpot, int closestDoorIndex) = FindNearestDoors(currChunk.HubDoorSpot.coord, currChunk.HubDoorID, true, false);
        FindAndPlaceConnectorPath(currChunk.HubDoorSpot, ClosestDoorSpot);
        currChunk.DoorStates[closestDoorIndex] = 0;

        // connect from same intermediate (different door) to final campfire room

        // if closest room is already campfire room, stop
        if (currChunk.DoorRoomIDs[closestDoorIndex] == currChunk.CampfireDoorID) return;

        int i = 0;
        for (; i < currChunk.Doors.Length; i++)
        {
            if (currChunk.DoorRoomIDs[i] == currChunk.CampfireDoorID && currChunk.DoorStates[i] == 1) break;
        }
        Chunk.DoorSpot unusedCampfireDoor = currChunk.Doors[i];

        int j = 0;
        //Debug.Log("goal id is " + currChunk.DoorRoomIDs[closestDoorIndex]);
        for (; j < currChunk.Doors.Length; j++)
        {
            if (currChunk.DoorRoomIDs[j] == currChunk.DoorRoomIDs[closestDoorIndex] && currChunk.DoorStates[j] == 1)
            {
                //Debug.Log("DOOR IS GOOD");
                break;
            }
        }

        // THIS KEESP ERRORINGNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN
        Chunk.DoorSpot unusedInterDoor = currChunk.Doors[j]; // um this errored where j reached currChunk.Doors.Length so was out of bounds, why?
        FindAndPlaceConnectorPath(unusedInterDoor, unusedCampfireDoor);
        currChunk.DoorStates[i] = 0;
        currChunk.DoorStates[j] = 0;
    }

    void FillIntermediate()
    {
        if (currChunk.hasPreviousChunkPath1)
        {
            // connect from previous chunk to nearest intermediate
            (Chunk.DoorSpot ClosestDoorSpot, int closestDoorIndex) = FindNearestDoors(currChunk.PreviousChunkPath1.coord, -1, true);
            FindAndPlaceConnectorPath(currChunk.PreviousChunkPath1, ClosestDoorSpot);
            currChunk.DoorStates[closestDoorIndex] = 0;

            // connect from that intermediate to final campfire room unless intermediate == campfire room
            if (currChunk.DoorRoomIDs[closestDoorIndex] != currChunk.CampfireDoorID)
            {
                int i = 0;
                for (; i < currChunk.Doors.Length; i++)
                {
                    if (currChunk.DoorRoomIDs[i] == currChunk.CampfireDoorID && currChunk.DoorStates[i] == 1) break;
                }
                Chunk.DoorSpot unusedCampfireDoor = currChunk.Doors[i];

                int j = Mathf.Max(0, closestDoorIndex - 8); // rather than starting at 0 always, be smart, choose start index thats close?
                for (; j < currChunk.Doors.Length; j++)
                {
                    if (currChunk.DoorRoomIDs[j] == currChunk.DoorRoomIDs[closestDoorIndex] && currChunk.DoorStates[j] == 1) break;
                }
                Chunk.DoorSpot unusedInterDoor = currChunk.Doors[j];
                FindAndPlaceConnectorPath(unusedInterDoor, unusedCampfireDoor);
                currChunk.DoorStates[i] = 0;
                currChunk.DoorStates[j] = 0;
            }
        }

        if (currChunk.hasPreviousChunkPath2)
        {
            // connect from previous chunk to nearest intermediate
            (Chunk.DoorSpot ClosestDoorSpot, int closestDoorIndex) = FindNearestDoors(currChunk.PreviousChunkPath2.coord, -1, true);
            FindAndPlaceConnectorPath(currChunk.PreviousChunkPath2, ClosestDoorSpot);
            currChunk.DoorStates[closestDoorIndex] = 0;

            if (currChunk.hasPreviousChunkPath1) return;
            // for first beta path chunk
            // connect from that intermediate to final campfire room unless intermediate == campfire room
            if (currChunk.DoorRoomIDs[closestDoorIndex] != currChunk.CampfireDoorID)
            {
                int i = 0;
                for (; i < currChunk.Doors.Length; i++)
                {
                    if (currChunk.DoorRoomIDs[i] == currChunk.CampfireDoorID && currChunk.DoorStates[i] == 1) break;
                }
                Chunk.DoorSpot unusedCampfireDoor = currChunk.Doors[i];

                FindAndPlaceConnectorPath(ClosestDoorSpot, unusedCampfireDoor);
                currChunk.DoorStates[i] = 0;
            }
        }
    }

    void FillBoss()
    {
        (Chunk.DoorSpot ClosestCampfireDoor, int closestCampfireDoorIndex) = FindNearestDoors(currChunk.BossDoorSpot.coord, currChunk.CampfireDoorID, true, true);
        FindAndPlaceConnectorPath(currChunk.BossDoorSpot, ClosestCampfireDoor);
        currChunk.DoorStates[closestCampfireDoorIndex] = 0;

        if (currChunk.hasPreviousChunkPath1)
        {
            (ClosestCampfireDoor, closestCampfireDoorIndex) = FindNearestDoors(currChunk.PreviousChunkPath1.coord, currChunk.CampfireDoorID, true, true);
            FindAndPlaceConnectorPath(currChunk.PreviousChunkPath1, ClosestCampfireDoor);
            currChunk.DoorStates[closestCampfireDoorIndex] = 0;
        }

        if (currChunk.hasPreviousChunkPath2)
        {
            (ClosestCampfireDoor, closestCampfireDoorIndex) = FindNearestDoors(currChunk.PreviousChunkPath2.coord, currChunk.CampfireDoorID, true, true);
            FindAndPlaceConnectorPath(currChunk.PreviousChunkPath2, ClosestCampfireDoor);
            currChunk.DoorStates[closestCampfireDoorIndex] = 0;
        }

    }

    void FillRemainingPath() // connect all remaining doors
    {
        int doorsLeft = 0;
        foreach (var door in currChunk.DoorStates) if (door == 1) doorsLeft++;

        while (doorsLeft > 1)
        {
            int startDoorIndex = RNG.NextInt(0, currChunk.DoorStates.Length);
            while (currChunk.DoorStates[startDoorIndex] != 1) startDoorIndex = (startDoorIndex + 1) % currChunk.DoorStates.Length;

            (Chunk.DoorSpot ClosestDoorSpot, int closestDoorIndex) = FindNearestDoors(currChunk.Doors[startDoorIndex].coord, currChunk.DoorRoomIDs[startDoorIndex], false, false);
            if (closestDoorIndex < 0) return;
            FindAndPlaceConnectorPath(currChunk.Doors[startDoorIndex], ClosestDoorSpot);
            currChunk.DoorStates[startDoorIndex] = 0;
            currChunk.DoorStates[closestDoorIndex] = 0;

            doorsLeft -= 2;
        }
    }

    void ExtendCampfirePath() // get campfire door facing next chunk and extend path
    {
        Vector2Int dirToNextChunk = Vector2Int.zero;
        Door.Direction lastCoordDoorDirection = Door.Direction.North;
        if (currChunk.NextChunkInAlphaPath.x < currChunk.Coordinate.x)
        {
            dirToNextChunk = new Vector2Int(-1, 0);
            lastCoordDoorDirection = Door.Direction.East; // face opposite way since imagine a door at the end, it'd be facing opposite you are huh
        }
        else if (currChunk.NextChunkInAlphaPath.x > currChunk.Coordinate.x)
        {
            dirToNextChunk = new Vector2Int(1, 0);
            lastCoordDoorDirection = Door.Direction.West;
        }
        else if (currChunk.NextChunkInAlphaPath.y < currChunk.Coordinate.y)
        {
            dirToNextChunk = new Vector2Int(0, -1);
            lastCoordDoorDirection = Door.Direction.North;
        }
        else if (currChunk.NextChunkInAlphaPath.y > currChunk.Coordinate.y)
        {
            dirToNextChunk = new Vector2Int(0, 1);
            lastCoordDoorDirection = Door.Direction.South;
        }

        UnsafeList<Coord> path = new(0, Allocator.Persistent);
        Coord currCoord = currChunk.CampfireDoorSpot.coord;
        Coord lastCoord = currChunk.CampfireDoorSpot.coord;
        while (GetGridCell(this, currCoord.x, currCoord.y) != int.MaxValue) // while still in bounds of chunk
        {
            path.Add(currCoord);
            lastCoord = currCoord;
            currCoord += dirToNextChunk;
        }

        SetGridPath(path);
        SetDoorGridCell(currChunk.CampfireDoorSpot);
        SetDoorGridCell(new Chunk.DoorSpot(lastCoord, lastCoordDoorDirection));

        currCoord = new Coord(currCoord.x < 0 ? MAP_INFO.CHUNK_SIZE.x - 1 : currCoord.x % MAP_INFO.CHUNK_SIZE.x,
                              currCoord.y < 0 ? MAP_INFO.CHUNK_SIZE.y - 1 : currCoord.y % MAP_INFO.CHUNK_SIZE.y);

        int c = currChunk.NextChunkInAlphaPath.x + currChunk.NextChunkInAlphaPath.y * MAP_INFO.MAP_SIZE.y;
        Chunk nextChunk = MapChunks[c];

        Door.Direction oppositeDir = lastCoordDoorDirection switch
        {
            Door.Direction.North => Door.Direction.South,
            Door.Direction.South => Door.Direction.North,
            Door.Direction.East => Door.Direction.West,
            Door.Direction.West => Door.Direction.East,
            _ => Door.Direction.North
        };

        if (nextChunk.hasPreviousChunkPath1) // like the boss room where two paths converge
        {
            nextChunk.PreviousChunkPath2 = new Chunk.DoorSpot(currCoord, oppositeDir);
            nextChunk.hasPreviousChunkPath2 = true;
        } 
        else
        {
            nextChunk.PreviousChunkPath1 = new Chunk.DoorSpot(currCoord, oppositeDir);
            nextChunk.hasPreviousChunkPath1 = true;
        }

        MapChunks[c] = nextChunk;
    }

    void ExtendToBetaPath() // get extrema door and extend towards beta path chunk
    {
        int StartDoorIdx = -1;
        Vector2Int dirToNextChunk = Vector2Int.zero;
        Door.Direction lastCoordDoorDirection = Door.Direction.North;

        if (currChunk.NextChunkInBetaPath.x < currChunk.Coordinate.x)
        {
            int x_min = int.MaxValue;
            for (int d = 0; d < currChunk.Doors.Length; d++)
            {
                if (currChunk.DoorStates[d] != 1) continue;
                if (currChunk.Doors[d].coord.x < x_min) { x_min = currChunk.Doors[d].coord.x; StartDoorIdx = d; }
            }

            dirToNextChunk = new Vector2Int(-1, 0);
            lastCoordDoorDirection = Door.Direction.East; // face opposite way since imagine a door at the end, it'd be facing opposite you are huh
        }
        else if (currChunk.NextChunkInBetaPath.x > currChunk.Coordinate.x)
        {
            int x_max = 0;
            for (int d = 0; d < currChunk.Doors.Length; d++)
            {
                if (currChunk.DoorStates[d] != 1) continue;
                if (currChunk.Doors[d].coord.x > x_max) { x_max = currChunk.Doors[d].coord.x; StartDoorIdx = d; }
            }

            dirToNextChunk = new Vector2Int(1, 0);
            lastCoordDoorDirection = Door.Direction.West;
        }
        else if (currChunk.NextChunkInBetaPath.y < currChunk.Coordinate.y)
        {
            int y_min = int.MaxValue;
            for (int d = 0; d < currChunk.Doors.Length; d++)
            {
                if (currChunk.DoorStates[d] != 1) continue;
                if (currChunk.Doors[d].coord.y < y_min) { y_min = currChunk.Doors[d].coord.y; StartDoorIdx = d; }
            }

            dirToNextChunk = new Vector2Int(0, -1);
            lastCoordDoorDirection = Door.Direction.North;
        }
        else if (currChunk.NextChunkInBetaPath.y > currChunk.Coordinate.y)
        {
            int y_max = 0;
            for (int d = 0; d < currChunk.Doors.Length; d++)
            {
                if (currChunk.DoorStates[d] != 1) continue;
                if (currChunk.Doors[d].coord.y > y_max) { y_max = currChunk.Doors[d].coord.y; StartDoorIdx = d; }
            }

            dirToNextChunk = new Vector2Int(0, 1);
            lastCoordDoorDirection = Door.Direction.South;
        }

        UnsafeList<Coord> path = new(0, Allocator.Persistent);
        Coord currCoord = currChunk.Doors[StartDoorIdx].coord;
        Coord lastCoord = currChunk.Doors[StartDoorIdx].coord;
        while (GetGridCell(this, currCoord.x, currCoord.y) != int.MaxValue) // while still in bounds of chunk
        {
            path.Add(currCoord);
            lastCoord = currCoord;
            currCoord += dirToNextChunk;
        }
        currCoord = new Coord(currCoord.x < 0 ? MAP_INFO.CHUNK_SIZE.x - 1 : currCoord.x % MAP_INFO.CHUNK_SIZE.x,
                      currCoord.y < 0 ? MAP_INFO.CHUNK_SIZE.y - 1 : currCoord.y % MAP_INFO.CHUNK_SIZE.y);

        int c = currChunk.NextChunkInBetaPath.x + currChunk.NextChunkInBetaPath.y * MAP_INFO.MAP_SIZE.y;
        Chunk nextChunk = MapChunks[c];

        if (nextChunk.hasPreviousChunkPath2) return; // already connected to another chunk, don't interfere

        Door.Direction oppositeDir = lastCoordDoorDirection switch
        {
            Door.Direction.North => Door.Direction.South,
            Door.Direction.South => Door.Direction.North,
            Door.Direction.East => Door.Direction.West,
            Door.Direction.West => Door.Direction.East,
            _ => Door.Direction.North
        };
        nextChunk.PreviousChunkPath2 = new Chunk.DoorSpot(currCoord, oppositeDir);
        nextChunk.hasPreviousChunkPath2 = true;
        MapChunks[c] = nextChunk;

        SetGridPath(path);
        SetDoorGridCell(currChunk.Doors[StartDoorIdx]);
        SetDoorGridCell(new Chunk.DoorSpot(lastCoord, lastCoordDoorDirection));
        currChunk.DoorStates[StartDoorIdx] = 0;

    }

    (Chunk.DoorSpot, int) FindNearestDoors(Coord StartPos, int ID, bool FirstChoice = false, bool ChooseID = false, bool includeExtreme = true)
    {
        Chunk.DoorSpot closestDoor1 = new();
        float dist1 = Mathf.Infinity;
        int door1Index = -1;
        Chunk.DoorSpot closestDoor2 = new();
        float dist2 = Mathf.Infinity;
        int door2Index = -1;
        for (int d = 0; d < currChunk.Doors.Length; d++)
        {
            //Debug.Log(d + " checking: " + currChunk.Doors[d].coord + " ID: " + ID + " " + currChunk.DoorRoomIDs[d] + " .. state: " + currChunk.DoorStates[d]);
            if (currChunk.DoorStates[d] == 0 || currChunk.DoorStates[d] == -1 && !includeExtreme ||
                currChunk.DoorStates[d] == -2) continue;
            if (currChunk.DoorRoomIDs[d] == ID && !ChooseID || currChunk.DoorRoomIDs[d] != ID && ChooseID) continue;
            Chunk.DoorSpot nDoor = currChunk.Doors[d];
            // take manhattan distance
            float dist = Math.Abs(StartPos.x - nDoor.coord.x) + Math.Abs(StartPos.y - nDoor.coord.y);

            if (dist < dist1)
            {
                dist2 = dist1;
                closestDoor2 = closestDoor1;
                door2Index = door1Index;
                dist1 = dist;
                closestDoor1 = nDoor;
                door1Index = d;
            }
            else if (dist < dist2)
            {
                dist2 = dist;
                closestDoor2 = nDoor;
                door2Index = d;
            }
        }

        if (!FirstChoice && door2Index >= 0)
        {
            if (RNG.NextBool()) return (closestDoor1, door1Index);
            else return (closestDoor2, door2Index);
        }
        else
        {
            return (closestDoor1, door1Index);
        }
    }

    int GetGridCell(PlaceConnectorsJob job, int x, int y)
    {
        if (x < 0 || x >= job.MAP_INFO.CHUNK_SIZE.x || y < 0 || y >= job.MAP_INFO.CHUNK_SIZE.y) return int.MaxValue;
        //Debug.Log("curr grid check = " + job.currChunk.Grid[x + y * job.MAP_INFO.CHUNK_SIZE.y]);
        return job.currChunk.Grid[x + y * job.MAP_INFO.CHUNK_SIZE.y];
    }
    void SetGridCell(PlaceConnectorsJob job, Coord c, byte value)
    {
        if (c.x < 0 || c.x >= job.MAP_INFO.CHUNK_SIZE.x || c.y < 0 || c.y >= job.MAP_INFO.CHUNK_SIZE.y) return;
        int currVal = job.currChunk.Grid[c.x + c.y * job.MAP_INFO.CHUNK_SIZE.y];
        job.currChunk.Grid[c.x + c.y * job.MAP_INFO.CHUNK_SIZE.y] = job.currChunk.Grid[c.x + c.y * job.MAP_INFO.CHUNK_SIZE.y] | value;
        //Debug.Log("Changing to value " + currVal + " to " + job.currChunk.Grid[c.x + c.y * job.MAP_INFO.CHUNK_SIZE.y]);
    }

    void SetDoorGridCell(Chunk.DoorSpot door)
    {
        SetGridCell(this, door.coord, door.dir switch
        {
            Door.Direction.North => 4,
            Door.Direction.South => 2,
            Door.Direction.East => 16,
            Door.Direction.West => 8,
            _ => 0
        });
    }
    /// <summary>
    /// Connector Dirs represented as byte: 11110 (West, East, South, North, Padding) in currChunk.Grid
    /// </summary>
    void FindAndPlaceConnectorPath(Chunk.DoorSpot StartPos, Chunk.DoorSpot EndPos)
    {

        UnsafeList<Coord> FindAStarPath(PlaceConnectorsJob job, Coord START, Coord GOAL)
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
                    //Debug.Log("Found goal");
                    break;
                }

                UnsafeList<Coord> neighbors = new(0, Allocator.Persistent);

                if (job.GetGridCell(job, current.x, current.y - 1) < 32) neighbors.Add(new(current.x, current.y - 1));
                if (job.GetGridCell(job, current.x - 1, current.y) < 32) neighbors.Add(new(current.x - 1, current.y));
                if (job.GetGridCell(job, current.x + 1, current.y) < 32) neighbors.Add(new(current.x + 1, current.y));
                if (job.GetGridCell(job, current.x, current.y + 1) < 32) neighbors.Add(new(current.x, current.y + 1));
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
            // from GOAL to START
            //Debug.Log("frontier = " + frontier.Count);
            //Debug.Log("PATH is " + current + " " + GetGridCell(job, current.x, current.y));
            while (current != NONE)
            {
                path.Add(current);
                current = came_from[current];
                //Debug.Log("PATH is " + current + " " + GetGridCell(job, current.x, current.y));
            }

            return path;
        }
        
        UnsafeList<Coord> path = FindAStarPath(this, StartPos.coord, EndPos.coord);
        SetGridPath(path);

        SetDoorGridCell(StartPos);
        SetDoorGridCell(EndPos);
    }

    void SetGridPath(UnsafeList<Coord> path)
    {
        //Debug.Log("Path length = " + path.Length);
        for (int coord = 0, next_coord = 1; next_coord < path.Length; coord++, next_coord++)
        {
            //Debug.Log(path[next_coord] + " " + path[coord]);
            if (path[next_coord].y > path[coord].y) // north
            {
                SetGridCell(this, path[coord], 2);
                SetGridCell(this, path[next_coord], 4);
            }
            else if (path[next_coord].y < path[coord].y) // south
            {
                SetGridCell(this, path[coord], 4);
                SetGridCell(this, path[next_coord], 2);
            }
            else if (path[next_coord].x > path[coord].x) // east
            {
                SetGridCell(this, path[coord], 8);
                SetGridCell(this, path[next_coord], 16);
            }
            else if (path[next_coord].x < path[coord].x) // west
            {
                SetGridCell(this, path[coord], 16);
                SetGridCell(this, path[next_coord], 8);
            }
        }
    }

}