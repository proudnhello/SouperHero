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
    int westBetaChunks, eastBetaChunks;
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
            totalChunks++;

            // get stats for treasure placement later
            if (chunk.ChunkType == Chunk.Type.BetaPath)
            {
                if (chunk.Coordinate.x < 3) westBetaChunks++;
                else eastBetaChunks++;
            }

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
    internal float chunksFilledWithTreasure = 0;
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


        chunksSpawned = 0;
        foreach (var info in chunkSpawnInfos)
        {
            if (info != null)
            {
                yield return StartCoroutine(SpawnInChunk(info));
                chunksSpawned++;
            }
        }

        chunksFilledWithTreasure = 0;
        // i hate all these variables whatever
        int indexWestHighChest = UnityEngine.Random.Range(0, westBetaChunks);
        int indexWestTracker = 0;
        int infoIndexWest = 0;
        int indexEastHighChest = UnityEngine.Random.Range(0, eastBetaChunks);
        int indexEastTracker = 0;
        int infoIndexEast = 0;
        for (int i = 0; i < chunkSpawnInfos.Length; i++)
        {
            var info = chunkSpawnInfos[i];
            if (info == null) continue;
            if (info.ChunkInfo.ChunkType == Chunk.Type.BetaPath)
            {
                if (info.ChunkInfo.Coordinate.x < 3)
                {
                    if (indexWestTracker == -1) continue;
                    if (indexWestHighChest == indexWestTracker)
                    {
                        infoIndexWest = i;
                        indexWestTracker = -1;
                    }
                    else indexWestTracker++;
                }
                else
                {
                    if (indexEastTracker == -1) continue;
                    if (indexEastHighChest == indexEastTracker)
                    {
                        infoIndexEast = i;
                        indexEastTracker = -1;
                    }
                    else indexEastTracker++;
                }
            }
        }

        int medLowTracker = 0;
        for (int i = 0; i < chunkSpawnInfos.Length; i++)
        {
            var info = chunkSpawnInfos[i];
            if (info != null)
            {
                chunksFilledWithTreasure++;

                // use one chest per chunk
                Chest[] chests = info.ChunkHolder.GetComponentsInChildren<Chest>();
                if (chests.Length == 0) continue; // should be impossible

                int usingChest = UnityEngine.Random.Range(0, chests.Length);
                for (int c = 0; c < chests.Length; c++) if (c != usingChest) Destroy(chests[c].gameObject);

                if (info.ChunkInfo.ChunkType == Chunk.Type.BetaPath)
                {
                    if (i == infoIndexWest || i == infoIndexEast)
                    {
                        chests[usingChest].item = RoomGenerator2.Instance.chestDistribution.GetBowl(2);
                        yield return null;
                        continue;
                    }
                }
                chests[usingChest].item = RoomGenerator2.Instance.chestDistribution.GetBowl(medLowTracker);
                medLowTracker = (medLowTracker + 1) % 2;

                yield return null;
            }
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