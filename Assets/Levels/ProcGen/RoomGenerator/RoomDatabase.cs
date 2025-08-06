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

    public readonly GenerationInfo GetRoom(RoomType room, Biome biome, int index = -1)
    {
        GenerationInfo Select(NativeList<GenerationInfo> rooms)
        {
            if (index >= 0 && index < rooms.Length) return rooms[index];
            return rooms[rng.NextInt(0, rooms.Length)];
        }

        switch (room)
        {
            case RoomType.START:
                return START_ROOM;
            case RoomType.INTERMEDIATE:
                if (biome == Biome.CAVE) return Select(CAVE_INTERMEDIATES);
                else if (biome == Biome.DESERT) return Select(DESERT_INTERMEDIATES);
                else if (biome == Biome.FOREST) return Select(FOREST_INTERMEDIATES);
                break;
            case RoomType.CONNECTOR:
                if (biome == Biome.CAVE) return Select(CAVE_CONNECTORS);
                else if (biome == Biome.DESERT) Select(DESERT_CONNECTORS);
                else if (biome == Biome.FOREST) Select(FOREST_CONNECTORS);
                break;
            case RoomType.CAMPFIRE:
                if (biome == Biome.CAVE) return Select(CAVE_CAMPFIRES);
                else if (biome == Biome.DESERT) return Select(DESERT_CAMPFIRES);
                else if (biome == Biome.FOREST) return Select(FOREST_CAMPFIRES);
                break;
            case RoomType.BOSS:
                if (biome == Biome.DESERT) return DESERT_BOSS;
                else if (biome == Biome.FOREST) return FOREST_BOSS;
                break;
        }
        return START_ROOM;
    }
}