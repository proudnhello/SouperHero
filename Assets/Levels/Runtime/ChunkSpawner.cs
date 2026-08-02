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
        public bool hasBeenInitialized;
    }

    ChunkSpawnInfo[] chunkSpawnInfos;
    public IEnumerator SpawnInitialChunks(NativeArray<Chunk> _MapChunks, Dictionary<int, MapRoom> _UUIDtoRoom, MapInfo _MAP_INFO, GameObject hubRoomObject)
    {
        MapChunks = _MapChunks;
        UUIDtoRoom = _UUIDtoRoom;
        MAP_INFO = _MAP_INFO;

        chunkSpawnInfos = new ChunkSpawnInfo[MAP_INFO.MAP_SIZE.x * MAP_INFO.MAP_SIZE.y];


        // spawn hub manually
        Vector2 hubSpawnPos = new Vector2(3, 3) * MAP_INFO.CHUNK_SIZE * MAP_INFO.GRID_SIZE - new Vector2(2, 2) * MAP_INFO.GRID_SIZE;
        MapRoom spawnRoom = Instantiate(hubRoomObject, hubSpawnPos, Quaternion.identity, transform).GetComponent<MapRoom>();

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

            int door = 0;
            for (int j = 0; j < chunk.Rooms.Length; j++)
            {
                var room = chunk.Rooms[j];

                Vector2 spawnPos = chunkSpawnInfos[i].ChunkBottomLeft + // chunk bottom left
                        (new Vector2Int(room.RoomSpawn.x, room.RoomSpawn.y) * MAP_INFO.GRID_SIZE);

#if UNITY_EDITOR
                if ((chunk.Coordinate.x == 2 || chunk.Coordinate.x == 3) && (chunk.Coordinate.y == 2 || chunk.Coordinate.y == 3)) {
#endif
                  chunkSpawnInfos[i].hasBeenInitialized = true;

                    MapRoom mRoom = room.Type == RoomType.START ? spawnRoom :
                        Instantiate(UUIDtoRoom[room.UUID].gameObject, new Vector3(spawnPos.x, spawnPos.y, 0), Quaternion.identity, ChunkHolder).GetComponent<MapRoom>();
    
                    foreach (var d in mRoom.Doors)
                    {
                        if (chunk.DoorStates[door] == 0) d.isOpen = true;
                        door++;
                    }

                    if (mRoom.entities != null)
                    {
                        mRoom.entities.name = ChunkHolder.name + " - Entities";
                        mRoom.entities.transform.parent = transform;
                    }

                    mRoom.InitializeTiles(0);

                    if (room.Type == RoomType.START) mRoom.transform.parent = ChunkHolder;
#if UNITY_EDITOR
                }
#endif
            }

            StaticBatchingUtility.Combine(ChunkHolder.gameObject);

            chunk.FreeRectangles.Dispose();
            chunk.Grid.Dispose();
            chunk.Doors.Dispose();     
            chunk.DoorRoomIDs.Dispose();

            if (chunk.Coordinate.x >= 2 && chunk.Coordinate.x <= 3 && chunk.Coordinate.y >= 2 && chunk.Coordinate.y <= 3)
            {
                chunk.Rooms.Dispose();
                chunk.DoorStates.Dispose();
            }

        }

        PlayerInChunkTracker tracker = new();
        StartCoroutine(tracker.GetPlayerCurrentChunk(this, MAP_INFO));

        yield return null;
    }


    public void DisplayChunk(Coord chunk, int dir)
    {
        List<int> showChunkIndicies = new()
        {
            chunk.x + chunk.y * MAP_INFO.MAP_SIZE.y
        };

        // dir: 0 = bottom left, 1 = bottom right, 2 = top left, 3 = top right
        if ((dir == 0 || dir == 2) && chunk.x - 1 >= 0) showChunkIndicies.Add(chunk.x - 1 + chunk.y * MAP_INFO.MAP_SIZE.y);
        if ((dir == 1 || dir == 3) && chunk.x + 1 < MAP_INFO.MAP_SIZE.x) showChunkIndicies.Add(chunk.x + 1 + chunk.y * MAP_INFO.MAP_SIZE.y);

        if ((dir == 0 || dir == 1) && chunk.y - 1 >= 0) showChunkIndicies.Add(chunk.x + (chunk.y-1) * MAP_INFO.MAP_SIZE.y);
        if ((dir == 2 || dir == 3) && chunk.y + 1 < MAP_INFO.MAP_SIZE.y) showChunkIndicies.Add(chunk.x + (chunk.y+1) * MAP_INFO.MAP_SIZE.y);

        if (dir == 0 &&  chunk.x - 1 >= 0 && chunk.y - 1 >= 0) showChunkIndicies.Add(chunk.x - 1 + (chunk.y - 1) * MAP_INFO.MAP_SIZE.y);
        if (dir == 1 && chunk.x + 1 < MAP_INFO.MAP_SIZE.x && chunk.y - 1 >= 0) showChunkIndicies.Add(chunk.x + 1 + (chunk.y - 1) * MAP_INFO.MAP_SIZE.y);
        if (dir == 2 && chunk.x - 1 >= 0 && chunk.y + 1 < MAP_INFO.MAP_SIZE.y) showChunkIndicies.Add(chunk.x - 1 + (chunk.y + 1) * MAP_INFO.MAP_SIZE.y);
        if (dir == 3 && chunk.x + 1 < MAP_INFO.MAP_SIZE.x && chunk.y + 1 < MAP_INFO.MAP_SIZE.y) showChunkIndicies.Add(chunk.x + 1 + (chunk.y + 1) * MAP_INFO.MAP_SIZE.y);


        for (int i = 0; i < chunkSpawnInfos.Length; i++)
        {
            if (chunkSpawnInfos[i] == null) continue; // is EMPTY chunk

            if (showChunkIndicies.Contains(i))
            {    
                if (!chunkSpawnInfos[i].hasBeenInitialized) SpawnInChunk(chunkSpawnInfos[i]);
                chunkSpawnInfos[i].hasBeenInitialized = true;
                //chunkSpawnInfos[i].ChunkHolder.gameObject.SetActive(true);
            }
            //else chunkSpawnInfos[i].ChunkHolder.gameObject.SetActive(false);
        }
        foreach (var i in showChunkIndicies)
        {
            
        }
    }

    void SpawnInChunk(ChunkSpawnInfo info)
    {
        int door = 0;
        foreach (var room in info.ChunkInfo.Rooms)
        {
            Vector2 spawnPos = info.ChunkBottomLeft + // chunk bottom left
                new Vector2(room.RoomSpawn.x, room.RoomSpawn.y) * MAP_INFO.GRID_SIZE;


            MapRoom mRoom = Instantiate(UUIDtoRoom[room.UUID].gameObject, spawnPos, Quaternion.identity, info.ChunkHolder).GetComponent<MapRoom>();

            foreach (var d in mRoom.Doors)
            {
                if (info.ChunkInfo.DoorStates[door] == 0) d.isOpen = true;
                door++;
            }
            mRoom.InitializeTiles(0);
            if (door >= info.ChunkInfo.DoorStates.Length) info.ChunkInfo.DoorStates.Dispose();
        }

        StaticBatchingUtility.Combine(info.ChunkHolder.gameObject);

        info.ChunkInfo.Rooms.Dispose();
    }
}