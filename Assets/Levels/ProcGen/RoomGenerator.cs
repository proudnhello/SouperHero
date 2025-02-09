using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public int TILE_WIDTH;
    public int TILE_HEIGHT;

    public int _mapWidth;
    public int _mapHeight;

    private List<List<char>> _map;

    public List<Block> _startBlocks;
    public List<Block> _connectorBlocks;
    public List<Block> _intermediateBlocks;

    void Start()
    {
        // Start the scene identifying where the player will spawn
        // Have a select number of rooms you want
        _map = new List<List<char>>();
        for(int i = 0; i < _mapWidth; i++)
        {
            _map.Add(new List<char>());
        }
        for(int i = 0; i < _mapWidth; i++) { 
            for (int j = 0; j < _mapHeight; j++)
            {
                _map[i].Add(' ');
            }
        }
        GenerateRoom();
        Gizmos.color = Color.yellow;
    }

    private Vector2 getOffset(int row, int col, Block b)
    {
        return new Vector2((row + (b.BlockWidth() / 2.0f)) * TILE_WIDTH, (col + (b.BlockHeight() / 2.0f)) * TILE_HEIGHT);
    }

    private bool canPlaceIntermediate(int row, int col, Block b)
    {
        b.gameObject.transform.position = getOffset(row, col, b);
        for(int i = 0; i < b.BlockWidth(); ++i)
        {
            if(row + i >= _mapWidth || _map[row + i][col] != ' ')
            {
                return false;
            }
        }
        for (int i = 0; i < b.BlockHeight(); ++i)
        {
            if (col + i >= _mapHeight || _map[row][col + i] != ' ')
            {
                return false;
            }
        }
        return true;
    }

    // BFS Random Placement for intermediates // CHATGPT GAVE ME PSEUDO CODE
    void placeIntermediates(int numIntermediates)
    {
        for (int i = 0; i < numIntermediates; i++)
        {
            int blockType = Random.Range(0, _intermediateBlocks.Count);
            Block b = Instantiate(_intermediateBlocks[blockType]).GetComponent<Block>();

            bool placed = false;
            while (!placed)
            {
                int row = Random.Range(0, _mapWidth);
                int col = Random.Range(0, _mapHeight);

                b.gameObject.transform.rotation = Quaternion.identity;
                if (row + b.BlockWidth() < _mapWidth && canPlaceIntermediate(row, col, b))
                {
                    for (int j = 0; j < b.BlockWidth(); ++j)
                    {
                        _map[row + j][col] = 'I';
                    }
                    for (int j = 0; j < b.BlockHeight(); ++j)
                    {
                        _map[row][col + j] = 'I';
                    }
                    placed = true;
                    Vector2 offset = getOffset(row, col, b);
                    DrawRect(new Vector3(row * TILE_WIDTH, col * TILE_HEIGHT, 0), new Vector3(offset.x, offset.y, 0), Color.yellow);
                }
            }
        }
    }

    void GenerateRoom() {
        // place start
        int midWidth = (_mapWidth - 1) / 2;
        int midHeight = (_mapHeight - 1) / 2;

        _map[midWidth][midHeight] = 'S';
        Block b = Instantiate(_startBlocks[0]).GetComponent<Block>();
        b.gameObject.transform.position = getOffset(midWidth, midHeight, b);

        for (int i = 0; i < _mapWidth; ++i)
        {
            for (int j = 0; j < _mapHeight; ++j)
            {
                DrawRect(new Vector3(i * TILE_WIDTH, j * TILE_WIDTH, 0), new Vector3((i + 1) * TILE_WIDTH, (j + 1) * TILE_HEIGHT, 0), Color.red);
            }
        }

        placeIntermediates(10);

        Print2DArray(_map);
    }

    public static void DrawRect(Vector3 min, Vector3 max, Color color)
    {
        UnityEngine.Debug.DrawLine(min, new Vector3(min.x, max.y), color);
        UnityEngine.Debug.DrawLine(new Vector3(min.x, max.y), max, color);
        UnityEngine.Debug.DrawLine(max, new Vector3(max.x, min.y), color);
        UnityEngine.Debug.DrawLine(min, new Vector3(max.x, min.y), color);
    }

    public static void Print2DArray(List<List<char>> matrix)
    {
        string s = "";
        for (int i = 0; i < matrix.Count; i++)
        {
            for (int j = 0; j < matrix[0].Count; j++)
            {
                if (matrix[i][matrix.Count - j - 1] != ' ')
                {
                    s += '*';
                }
                else
                {
                    s += "_";
                }

            }
            s += '\n';
        }
        Debug.Log(s);
    }
}
