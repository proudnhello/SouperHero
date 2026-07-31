using System.Collections;
using UnityEngine;

public class PlayerInChunkTracker
{
    float CHECK_INTERVAL = 0.5f;
    public IEnumerator GetPlayerCurrentChunk(ChunkSpawner _ChunkSpawner, MapInfo MAP_INFO)
    {
        Coord GetCurrentChunk(Vector2 pos)
        {
            return new Coord(pos / MAP_INFO.GRID_SIZE / MAP_INFO.CHUNK_SIZE);
        }

        // 0 = bottom left, 1 = bottom right, 2 = top left, 3 = top right
        int GetCurrentChunkCorner(Vector2 pos)
        {
            Vector2 square = pos / MAP_INFO.GRID_SIZE / MAP_INFO.CHUNK_SIZE;
            Vector2Int square_int = new Vector2Int(Mathf.RoundToInt(square.x % 1), Mathf.RoundToInt(square.y % 1));
            return square_int.x + square_int.y * 2;
        }

        int lastChunkCorner = GetCurrentChunkCorner(PlayerEntityManager.Singleton.transform.position);
        while (true)
        {
            int currChunkCorner = GetCurrentChunkCorner(PlayerEntityManager.Singleton.transform.position);

            if (lastChunkCorner != currChunkCorner)
            {
                yield return new WaitForSeconds(CHECK_INTERVAL);
                currChunkCorner = GetCurrentChunkCorner(PlayerEntityManager.Singleton.transform.position);
                if (lastChunkCorner != currChunkCorner)
                {
                    _ChunkSpawner.DisplayChunk(GetCurrentChunk(PlayerEntityManager.Singleton.transform.position), currChunkCorner);
                    lastChunkCorner = currChunkCorner;
                }
            }

            yield return new WaitForSeconds(CHECK_INTERVAL);
        }
    }
}