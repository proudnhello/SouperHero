using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using static MapRoom;

public class ChunkSpawner : MonoBehaviour
{
    NativeArray<Chunk> MapChunks;
    Dictionary<int, MapRoom> UUIDtoRoom;
    MapInfo MAP_INFO;

    public class ChunkSpawnInfo
    {
        public Transform ChunkHolder;
        public Vector2 ChunkBottomLeft;
        public Chunk ChunkInfo;
        public List<AsyncInstantiateOperation> asyncInstantiateOperations;
    }

    internal ChunkSpawnInfo[] chunkSpawnInfos;
    MapRoom spawnRoom;
    public void TriggerChunkSpawn(NativeArray<Chunk> _MapChunks, Dictionary<int, MapRoom> _UUIDtoRoom, MapInfo _MAP_INFO, GameObject hubRoomObject)
    {
        MapChunks = _MapChunks;
        UUIDtoRoom = _UUIDtoRoom;
        MAP_INFO = _MAP_INFO;

        chunkSpawnInfos = new ChunkSpawnInfo[MAP_INFO.MAP_SIZE.x * MAP_INFO.MAP_SIZE.y];

        List<AsyncInstantiateOperation> instantiatedRooms = new List<AsyncInstantiateOperation>();

        // spawn hub manually
        Vector2 hubSpawnPos = new Vector2(3, 3) * MAP_INFO.CHUNK_SIZE * MAP_INFO.GRID_SIZE - new Vector2(2, 2) * MAP_INFO.GRID_SIZE;
        spawnRoom = Instantiate(hubRoomObject, hubSpawnPos, Quaternion.identity, transform).GetComponent<MapRoom>();

        PlayerSpawnLocation spawnLocation = spawnRoom.GetComponentInChildren<PlayerSpawnLocation>();
        RunStateManager.Singleton.InitialPlacePlayer(spawnLocation);

        for (int i = 0; i < MapChunks.Length; i++)
        {
            Chunk chunk = MapChunks[i];
            if (chunk.ChunkType == Chunk.Type.Empty) continue;
            //if (chunk.ChunkType != Chunk.Type.Starting) continue;
            Transform ChunkHolder = new GameObject($"{chunk.ChunkType} Chunk {chunk.Coordinate.x},{chunk.Coordinate.y}").transform;
            ChunkHolder.parent = transform;
            ChunkHolder.gameObject.isStatic = true;
            Rigidbody2D chunkRB = ChunkHolder.AddComponent<Rigidbody2D>();
            chunkRB.bodyType = RigidbodyType2D.Static;
            ChunkHolder.AddComponent<CompositeCollider2D>();

            chunkSpawnInfos[i] = new ChunkSpawnInfo();
            chunkSpawnInfos[i].ChunkHolder = ChunkHolder;
            chunkSpawnInfos[i].ChunkBottomLeft = new Vector2(chunk.Coordinate.x, chunk.Coordinate.y) * MAP_INFO.CHUNK_SIZE * MAP_INFO.GRID_SIZE;
            chunkSpawnInfos[i].ChunkInfo = chunk;
            chunkSpawnInfos[i].asyncInstantiateOperations = new();

            for (int j = 0; j < chunk.Rooms.Length; j++)
            {
                var room = chunk.Rooms[j];
                totalRooms++;

                if (room.Type == RoomType.START)
                {
                    chunkSpawnInfos[i].asyncInstantiateOperations.Add(null);
                    continue;
                }

                Vector2 spawnPos = chunkSpawnInfos[i].ChunkBottomLeft + // chunk bottom left
                        (new Vector2Int(room.RoomSpawn.x, room.RoomSpawn.y) * MAP_INFO.GRID_SIZE);

                chunkSpawnInfos[i].asyncInstantiateOperations.Add(
                    InstantiateAsync(UUIDtoRoom[room.UUID].gameObject, ChunkHolder, new Vector3(spawnPos.x, spawnPos.y, 0), Quaternion.identity));
            }
        }
    }

    internal float totalRooms;
    internal float roomsSpawned = 0;
    internal float totalChunks;
    internal float chunksSpawned = 0;
    public IEnumerator HandleSpawnCheck()
    {
        roomsSpawned = 0;
        foreach (var chunk in chunkSpawnInfos)
        {
            if (chunk == null) continue;

            for (int o = 0; o < chunk.asyncInstantiateOperations.Count; o++)
            {
                if (chunk.asyncInstantiateOperations[o] != null) yield return chunk.asyncInstantiateOperations[o];
                roomsSpawned++;
            }
        }


        totalChunks = chunkSpawnInfos.Length; // just reusing these variables whatever
        chunksSpawned = 0;
        foreach (var info in chunkSpawnInfos)
        {
            if (info != null) yield return StartCoroutine(SpawnInChunk(info));
            chunksSpawned++;
        }
    }

    IEnumerator SpawnInChunk(ChunkSpawnInfo info)
    {
        int door = 0;
        for (int r = 0; r < info.ChunkInfo.Rooms.Length; r++) 
        {
            var room = info.ChunkInfo.Rooms[r];

            Vector2 spawnPos = info.ChunkBottomLeft + // chunk bottom left
                new Vector2(room.RoomSpawn.x, room.RoomSpawn.y) * MAP_INFO.GRID_SIZE;

            MapRoom mRoom = room.Type == RoomType.START ? spawnRoom :
                info.asyncInstantiateOperations[r].Result[0].GetComponent<MapRoom>();

            foreach (var d in mRoom.Doors)
            {
                if (info.ChunkInfo.DoorStates[door] == 0) d.isOpen = true;
                door++;
            }

            if (mRoom.entities != null)
            {
                mRoom.entities.name = info.ChunkHolder.name + " - Entities";
                mRoom.entities.transform.parent = transform;
            }

            mRoom.InitializeTiles(0);

            if (room.Type == RoomType.START) mRoom.transform.parent = info.ChunkHolder;

            if (door >= info.ChunkInfo.DoorStates.Length) info.ChunkInfo.DoorStates.Dispose();
        }

        StaticBatchingUtility.Combine(info.ChunkHolder.gameObject);

        info.ChunkInfo.FreeRectangles.Dispose();
        info.ChunkInfo.Grid.Dispose();
        info.ChunkInfo.Doors.Dispose();
        info.ChunkInfo.DoorRoomIDs.Dispose();
        info.ChunkInfo.Rooms.Dispose();

        yield return null;
    }
}