using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static MapRoom;
using Random = Unity.Mathematics.Random;

[BurstCompatible]
public unsafe struct RoomDatabase
{
    // SORTED ROOMS
    GenerationInfo START_ROOM;
    [NoAlias][ReadOnly] NativeList<GenerationInfo> CAVE_INTERMEDIATES;
    [NoAlias][ReadOnly] NativeList<GenerationInfo> CAVE_CONNECTORS;
    [NoAlias][ReadOnly] NativeList<GenerationInfo> CAVE_CAMPFIRES;
    [NoAlias][ReadOnly] NativeList<GenerationInfo> DESERT_INTERMEDIATES;
    [NoAlias][ReadOnly] NativeList<GenerationInfo> DESERT_CONNECTORS;
    [NoAlias][ReadOnly] NativeList<GenerationInfo> DESERT_CAMPFIRES;
    GenerationInfo DESERT_BOSS;
    [NoAlias][ReadOnly] NativeList<GenerationInfo> FOREST_INTERMEDIATES;
    [NoAlias][ReadOnly] NativeList<GenerationInfo> FOREST_CONNECTORS;
    [NoAlias][ReadOnly] NativeList<GenerationInfo> FOREST_CAMPFIRES;
    GenerationInfo FOREST_BOSS;
    static Random rng;

    public void Init(MapRoom[] rooms, uint seed)
    {
        rng = new Random(seed);

        CAVE_INTERMEDIATES = new(0, Allocator.Persistent);
        CAVE_CONNECTORS = new(0, Allocator.Persistent);
        CAVE_CAMPFIRES = new(0, Allocator.Persistent);
        DESERT_INTERMEDIATES = new(0, Allocator.Persistent);
        DESERT_CONNECTORS = new(0, Allocator.Persistent);
        DESERT_CAMPFIRES = new(0, Allocator.Persistent);
        FOREST_INTERMEDIATES = new(0, Allocator.Persistent);
        FOREST_CONNECTORS = new(0, Allocator.Persistent);
        FOREST_CAMPFIRES = new(0, Allocator.Persistent);
        foreach (var room in rooms)
        {
            var info = room.Info.InitInfo(room);
            switch (info.Type)
            {
                case RoomType.START:
                    START_ROOM = info;
                    break;
                case RoomType.INTERMEDIATE:
                    if (info.Biome == Biome.CAVE) CAVE_INTERMEDIATES.Add(info);
                    else if (info.Biome == Biome.DESERT) DESERT_INTERMEDIATES.Add(info);
                    else if (info.Biome == Biome.FOREST) FOREST_INTERMEDIATES.Add(info);
                    break;
                case RoomType.CONNECTOR:
                    if (info.Biome == Biome.CAVE) CAVE_CONNECTORS.Add(info);
                    else if (info.Biome == Biome.DESERT) DESERT_CONNECTORS.Add(info);
                    else if (info.Biome == Biome.FOREST) FOREST_CONNECTORS.Add(info);
                    break;
                case RoomType.CAMPFIRE:
                    if (info.Biome == Biome.CAVE) CAVE_CAMPFIRES.Add(info);
                    else if (info.Biome == Biome.DESERT) DESERT_CAMPFIRES.Add(info);
                    else if (info.Biome == Biome.FOREST) FOREST_CAMPFIRES.Add(info);
                    break;
                case RoomType.BOSS:
                    if (info.Biome == Biome.DESERT) DESERT_BOSS = info;
                    else if (info.Biome == Biome.FOREST) FOREST_BOSS = info;
                    break;
            }
        }
    }

    public readonly GenerationInfo GetRoom(RoomType room, Biome biome, Coord maxSize = default)
    {
        GenerationInfo Select(NativeList<GenerationInfo> rooms)
        {
            if (maxSize.x > 0 && maxSize.y > 0)
            {
                for (int i = 0; i < rooms.Length; i++)
                {
                    var room = rooms[rng.NextInt(0, rooms.Length)];
                    if (room.TotalGridSpace.x <= maxSize.x && room.TotalGridSpace.y <= maxSize.y)
                        return room;
                }
                // otherwise just loop through list until you find the first one
                foreach (var room in rooms) 
                    if (room.TotalGridSpace.x <= maxSize.x && room.TotalGridSpace.y <= maxSize.y) 
                        return room;
                return new GenerationInfo().Null();
            }
            return rooms[rng.NextInt(0, rooms.Length)];
        }

        switch (room)
        {
            case RoomType.START:
                return START_ROOM;
            case RoomType.INTERMEDIATE:
                if (biome == Biome.CAVE) return Select(CAVE_INTERMEDIATES);
                else if (biome == Biome.DESERT) return Select(DESERT_INTERMEDIATES);
                else if (biome == Biome.FOREST) return Select(CAVE_INTERMEDIATES);
                break;
            case RoomType.CAMPFIRE:
                if (biome == Biome.CAVE) return Select(CAVE_CAMPFIRES);
                else if (biome == Biome.DESERT) return Select(DESERT_CAMPFIRES);
                else if (biome == Biome.FOREST) return Select(CAVE_CAMPFIRES);
                break;
            case RoomType.BOSS:
                if (biome == Biome.DESERT) return DESERT_BOSS;
                else return FOREST_BOSS;
        }
        return START_ROOM;
    }

    public readonly GenerationInfo GetConnector(Biome biome, byte value)
    {
        GenerationInfo Select(NativeList<GenerationInfo> rooms)
        {
            value >>= 1; // just want last 4 values
            // value bits correspond to = WESN
            ConnectorType type = value switch
            {
                3 => ConnectorType.Two_NS,
                12 => ConnectorType.Two_EW,
                5 => ConnectorType.Two_NE,
                6 => ConnectorType.Two_SE,
                10 => ConnectorType.Two_SW,
                9 => ConnectorType.Two_NW,
                7 => ConnectorType.Three_NSE,
                14 => ConnectorType.Three_SEW,
                11 => ConnectorType.Three_NSW,
                13 => ConnectorType.Three_NEW,
                15 => ConnectorType.Four,
                _ => ConnectorType.None
            };
            foreach (var room in rooms)
            {
                if (room.ConnectorType == type) return room;
            }
            return rooms[0];
        }

        //return Select(CAVE_CONNECTORS);
        if (biome == Biome.CAVE) return Select(CAVE_CONNECTORS);
        else if (biome == Biome.DESERT) return Select(DESERT_CONNECTORS);
        else if (biome == Biome.FOREST) return Select(CAVE_CONNECTORS);

        return START_ROOM;
    }

    public void Dispose()
    {
        CAVE_INTERMEDIATES.Dispose();
        CAVE_CONNECTORS.Dispose();
        CAVE_CAMPFIRES.Dispose();
        DESERT_INTERMEDIATES.Dispose();
        DESERT_CONNECTORS.Dispose();
        DESERT_CAMPFIRES.Dispose();
        FOREST_INTERMEDIATES.Dispose();
        FOREST_CONNECTORS.Dispose();
        FOREST_CAMPFIRES.Dispose();
    }
}