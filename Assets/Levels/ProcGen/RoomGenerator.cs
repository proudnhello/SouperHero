using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public int TILE_WIDTH;
    public int TILE_HEIGHT;

    public int _mapWidth;
    public int _mapHeight;

    private List<List<Block>> _map;

    public List<Block> _startBlocks;
    public List<Block> _intermediateBlocks;

    public GameObject connector2;
    public GameObject connector25;
    public GameObject connector3;
    public GameObject connector4;

    void Start()
    {
        // Start the scene identifying where the player will spawn
        // Have a select number of rooms you want
        _map = new List<List<Block>>();
        for(int i = 0; i < _mapWidth; i++)
        {
            _map.Add(new List<Block>());
        }
        for(int i = 0; i < _mapWidth; i++) { 
            for (int j = 0; j < _mapHeight; j++)
            {
                _map[i].Add(null);
            }
        }
        GenerateRoom();
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
            if(row + i >= _mapWidth || _map[row + i][col] != null)
            {
                return false;
            }
        }
        for (int i = 0; i < b.BlockHeight(); ++i)
        {
            if (col + i >= _mapHeight || _map[row][col + i] != null)
            {
                return false;
            }
        }
        return true;
    }

    private void fillBlock(int row, int col, Block b)
    {
        for (int j = 0; j < b.BlockWidth(); ++j)
        {
            _map[row + j][col] = b;
        }
        for (int j = 0; j < b.BlockHeight(); ++j)
        {
            _map[row][col + j] = b;
        }
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
                    fillBlock(row, col, b);
                    placed = true;
                    Vector2 offset = getOffset(row, col, b);
                }
            }
        }
    }

    private bool checkForBlock(int row, int col)
    {
        Block b = _map[row][col];
        return b != null && b.BlockType() != "Connector";
    }

    private string getConnectionsAt(int row, int col)
    {
        int rowPlus = row + 1;
        int rowMinus = row - 1;
        int colPlus = col + 1;
        int colMinus = col - 1;

        string s = "";

        if (colPlus < _mapHeight)
        {
            if (checkForBlock(row, colPlus) && _map[row][colPlus].south)
            {
                s += "N";
            }
        }
        if (colMinus >= 0)
        {
            if (checkForBlock(row, colMinus) && _map[row][colMinus].north)
            {
                s += "S";
            }
        }
        if (rowPlus < _mapWidth)
        {
            if (checkForBlock(rowPlus, col) && _map[rowPlus][col].west)
            {
                s += "E";
            }
        }
        if (rowMinus >= 0)
        {
            if (checkForBlock(rowMinus, col) && _map[rowMinus][col].east)
            {
                s += "W";
            }
        }
        return s;
    }

    private void firstSweepConnect()
    {
        for (int row = 0; row < _mapWidth; row++)
        {
            for (int col = 0; col < _mapHeight; col++)
            {
                if (!_map[row][col])
                {
                    string c = getConnectionsAt(row, col);
                    switch (c.Length)
                    {
                        case 2:
                            float angle = 0.0f;
                            bool connectorType2Need = false;
                            switch(c)
                            {
                                case "NW":
                                    break;
                                case "NE":
                                    angle = -90.0f;
                                    break;
                                case "NS":
                                    angle = 90.0f;
                                    connectorType2Need = true;
                                    break;
                                case "SW":
                                    angle = 90.0f;
                                    break;
                                case "SE":
                                    angle = 180.0f;
                                    break;
                                case "EW":
                                    connectorType2Need = true;
                                    break;
                                default:
                                    break;
                            }
                            Block b = null;
                            if(connectorType2Need)
                            {
                                b = Instantiate(connector2).GetComponent<Block>();
                            } else
                            {
                                b = Instantiate(connector25).GetComponent<Block>();
                            }
                            b.gameObject.transform.Rotate(new Vector3(0.0f, 0.0f, angle));
                            canPlaceIntermediate(row, col, b);
                            fillBlock(row, col, b);
                            _map[row][col] = b;
                            break;
                        case 3:
                            float angle2 = 0.0f;
                            switch(c)
                            {
                                case "NSE":
                                    angle2 = -90.0f;
                                    break;
                                case "NSW":
                                    angle2 = 90.0f;
                                    break;
                                case "SEW":
                                    angle2 = 180.0f;
                                    break;
                                case "NEW":
                                    break;
                            }
                            Block b2 = Instantiate(connector3).GetComponent<Block>();
                            b2.gameObject.transform.Rotate(new Vector3(0.0f, 0.0f, angle2));
                            canPlaceIntermediate(row, col, b2);
                            fillBlock(row, col, b2);
                            _map[row][col] = b2;
                            break;
                        case 4:
                            Block b3 = Instantiate(connector4).GetComponent<Block>();
                            canPlaceIntermediate(row, col, b3);
                            fillBlock(row, col, b3);
                            _map[row][col] = b3;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    void GenerateRoom() {
        // place start
        int midWidth = (_mapWidth - 1) / 2;
        int midHeight = (_mapHeight - 1) / 2;

        Block b = Instantiate(_startBlocks[0]).GetComponent<Block>();
        b.gameObject.transform.position = getOffset(midWidth, midHeight, b);
        _map[midWidth][midHeight] = b;

        placeIntermediates(10);

        firstSweepConnect();

        for (int i = 0; i < _mapWidth; ++i)
        {
            for (int j = 0; j < _mapHeight; ++j)
            {
                DrawRect(new Vector3(i * TILE_WIDTH, j * TILE_WIDTH, 0), new Vector3((i + 1) * TILE_WIDTH, (j + 1) * TILE_HEIGHT, 0), Color.red);
            }
        }
        for (int i = 0; i < _mapWidth; ++i)
        {
            for (int j = 0; j < _mapHeight; ++j)
            {
                if (_map[i][j])
                {
                    Color c;
                    switch (_map[i][j].BlockType())
                    {
                        case "Start":
                            c = Color.yellow;
                            break;
                        case "Connector":
                            c = Color.green;
                            break;
                        case "Intermediate":
                            c = Color.cyan;
                            break;
                        default:
                            c = Color.magenta;
                            break;
                    }
                    DrawRect(new Vector3(i * TILE_WIDTH, j * TILE_WIDTH, 0), new Vector3((i + 1) * TILE_WIDTH, (j + 1) * TILE_HEIGHT, 0), c);
                }
            }
        }
    }

    public static void DrawRect(Vector3 min, Vector3 max, Color color)
    {
        UnityEngine.Debug.DrawLine(min, new Vector3(min.x, max.y), color);
        UnityEngine.Debug.DrawLine(new Vector3(min.x, max.y), max, color);
        UnityEngine.Debug.DrawLine(max, new Vector3(max.x, min.y), color);
        UnityEngine.Debug.DrawLine(min, new Vector3(max.x, min.y), color);
    }

    //public static void Print2DArray(List<List<char>> matrix)
    //{
    //    string s = "";
    //    for (int i = 0; i < matrix.Count; i++)
    //    {
    //        for (int j = 0; j < matrix[0].Count; j++)
    //        {
    //            if (matrix[i][matrix.Count - j - 1] != ' ')
    //            {
    //                s += '*';
    //            }
    //            else
    //            {
    //                s += "_";
    //            }

    //        }
    //        s += '\n';
    //    }
    //    Debug.Log(s);
    //}
}
